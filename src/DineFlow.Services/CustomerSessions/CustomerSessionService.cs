using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Requests;
using DineFlow.BusinessObjects.Tables;
using DineFlow.Repositories.Common;
using DineFlow.Repositories.Orders;
using DineFlow.Repositories.Requests;
using DineFlow.Services.Common;
using DineFlow.Services.Orders;

namespace DineFlow.Services.CustomerSessions;

public class CustomerSessionService : ICustomerSessionService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly ITableSessionRepository _tableSessionRepository;
    private readonly ITableSessionService _tableSessionService;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerSessionService(
        IOrderRepository orderRepository,
        IServiceRequestRepository serviceRequestRepository,
        ITableSessionRepository tableSessionRepository,
        ITableSessionService tableSessionService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _serviceRequestRepository = serviceRequestRepository;
        _tableSessionRepository = tableSessionRepository;
        _tableSessionService = tableSessionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerSessionDto> ScanAsync(
        ScanCustomerSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        await _tableSessionService.ExpireInactiveBrowsingSessionsAsync(cancellationToken);

        string qrToken = NormalizeRequired(request.QrToken, "QR_TOKEN_REQUIRED", "QR token is required.");
        DiningTable table = await _tableSessionRepository.GetActiveTableByQrTokenAsync(qrToken, cancellationToken)
            ?? throw new BusinessException("TABLE_NOT_FOUND", "Dining table does not exist or is inactive.");

        TableSessionDto session = await _tableSessionService.GetOrCreateActiveSessionByQrTokenAsync(qrToken, null, cancellationToken);
        TableSessionCustomer? customer = null;

        if (!string.IsNullOrWhiteSpace(request.ClientToken))
        {
            customer = await _tableSessionRepository.GetSessionCustomerAsync(
                session.TableSessionId,
                request.ClientToken.Trim(),
                cancellationToken);
        }

        if (customer is null)
        {
            customer = await CreateCustomerAsync(session.TableSessionId, cancellationToken);
        }

        return Map(table, session, customer);
    }

    public async Task<CustomerSessionDto> UpdateCustomerNameAsync(
        UpdateCustomerNameRequest request,
        CancellationToken cancellationToken = default)
    {
        string clientToken = NormalizeRequired(request.ClientToken, "CLIENT_TOKEN_REQUIRED", "Client token is required.");
        string displayName = NormalizeRequired(request.DisplayName, "DISPLAY_NAME_REQUIRED", "Display name is required.");

        if (displayName.Length > 100)
        {
            throw new BusinessException("DISPLAY_NAME_TOO_LONG", "Display name must be 100 characters or fewer.");
        }

        TableSessionCustomer customer = await GetActiveCustomerAsync(clientToken, cancellationToken);
        customer.DisplayName = displayName;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapFromCustomer(customer);
    }

    public async Task<CustomerSessionDto> VerifyOtpAsync(
        VerifyCustomerOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        await _tableSessionService.ExpireInactiveBrowsingSessionsAsync(cancellationToken);

        string qrToken = NormalizeRequired(request.QrToken, "QR_TOKEN_REQUIRED", "QR token is required.");
        string clientToken = NormalizeRequired(request.ClientToken, "CLIENT_TOKEN_REQUIRED", "Client token is required.");
        string otp = NormalizeRequired(request.Otp, "OTP_REQUIRED", "OTP is required.");

        DiningTable table = await _tableSessionRepository.GetActiveTableByQrTokenAsync(qrToken, cancellationToken)
            ?? throw new BusinessException("TABLE_NOT_FOUND", "Dining table does not exist or is inactive.");

        if (!string.Equals(table.CurrentOtp, otp, StringComparison.Ordinal))
        {
            throw new BusinessException("TABLE_OTP_INVALID", "Mã bàn không đúng. Vui lòng hỏi nhân viên.");
        }

        string? displayName = request.DisplayName?.Trim();
        if (displayName?.Length > 100)
        {
            throw new BusinessException("DISPLAY_NAME_TOO_LONG", "Display name must be 100 characters or fewer.");
        }

        TableSessionDto session = await _tableSessionService.GetOrCreateActiveSessionByTableIdAsync(
            table.TableId,
            openedBy: null,
            cancellationToken);

        TableSessionCustomer? customer = await _tableSessionRepository.GetSessionCustomerAsync(
            session.TableSessionId,
            clientToken,
            cancellationToken);

        if (customer is null)
        {
            customer = new TableSessionCustomer
            {
                TableSessionId = session.TableSessionId,
                ClientToken = clientToken,
                CreatedAt = DateTime.UtcNow
            };
            await _tableSessionRepository.AddSessionCustomerAsync(customer, cancellationToken);
        }

        customer.IsVerified = true;
        customer.VerifiedAt ??= DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            customer.DisplayName = displayName;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(table, session, customer);
    }

    public async Task<CustomerSessionDto> GetCurrentAsync(
        string clientToken,
        CancellationToken cancellationToken = default)
    {
        TableSessionCustomer customer = await GetActiveCustomerAsync(
            NormalizeRequired(clientToken, "CLIENT_TOKEN_REQUIRED", "Client token is required."),
            cancellationToken);

        return MapFromCustomer(customer);
    }

    public async Task<IReadOnlyList<CustomerMessageDto>> GetMessagesAsync(
        string clientToken,
        CancellationToken cancellationToken = default)
    {
        TableSessionCustomer customer = await GetActiveCustomerAsync(
            NormalizeRequired(clientToken, "CLIENT_TOKEN_REQUIRED", "Client token is required."),
            cancellationToken);

        IReadOnlyList<Order> orders = await _orderRepository.GetOrdersBySessionAsync(customer.TableSessionId, cancellationToken);
        IReadOnlyList<ServiceRequest> requests = await _serviceRequestRepository.GetBySessionAsync(customer.TableSessionId, cancellationToken);

        List<CustomerMessageDto> messages = orders
            .Where(x => x.ClientToken == customer.ClientToken || x.SessionCustomerId == customer.SessionCustomerId)
            .Select(CustomerMessageMapper.MapOrderMessage)
            .Concat(requests
                .Where(x => x.ClientToken == customer.ClientToken || x.SessionCustomerId == customer.SessionCustomerId)
                .Select(CustomerMessageMapper.MapRequestMessage))
            .OrderBy(x => x.CreatedAt)
            .ToList();

        return messages;
    }

    private async Task<TableSessionCustomer> CreateCustomerAsync(int tableSessionId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            TableSessionCustomer customer = new()
            {
                TableSessionId = tableSessionId,
                ClientToken = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            };

            await _tableSessionRepository.AddSessionCustomerAsync(customer, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return customer;
        }, cancellationToken);
    }

    private async Task<TableSessionCustomer> GetActiveCustomerAsync(string clientToken, CancellationToken cancellationToken)
    {
        await _tableSessionService.ExpireInactiveBrowsingSessionsAsync(cancellationToken);

        TableSessionCustomer customer = await _tableSessionRepository.GetSessionCustomerByTokenAsync(clientToken, cancellationToken)
            ?? throw new BusinessException("CUSTOMER_SESSION_NOT_FOUND", "Customer session does not exist.");

        if (customer.TableSession is null ||
            customer.TableSession.Status is not ("Browsing" or "Open" or "WaitingPayment"))
        {
            throw new BusinessException("CUSTOMER_SESSION_NOT_ACTIVE", "Customer session is not active.");
        }

        return customer;
    }

    private static CustomerSessionDto MapFromCustomer(TableSessionCustomer customer)
    {
        TableSession session = customer.TableSession
            ?? throw new BusinessException("SESSION_NOT_FOUND", "Table session does not exist.");
        DiningTable table = session.Table
            ?? throw new BusinessException("TABLE_NOT_FOUND", "Dining table does not exist.");

        return Map(table, new TableSessionDto
        {
            TableId = session.TableId,
            TableSessionId = session.TableSessionId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            Status = session.Status
        }, customer);
    }

    private static CustomerSessionDto Map(DiningTable table, TableSessionDto session, TableSessionCustomer customer)
    {
        return new CustomerSessionDto
        {
            TableId = table.TableId,
            TableName = table.TableName,
            Area = table.Area,
            TableSessionId = session.TableSessionId,
            SessionCustomerId = customer.SessionCustomerId,
            ClientToken = customer.ClientToken,
            DisplayName = customer.DisplayName,
            CurrentOtp = table.CurrentOtp,
            OtpUpdatedAt = table.OtpUpdatedAt,
            RequiresName = string.IsNullOrWhiteSpace(customer.DisplayName),
            IsVerified = customer.IsVerified,
            RequiresOtp = !customer.IsVerified,
            CanOrder = customer.IsVerified && session.Status is "Open" or "WaitingPayment",
            SessionStatus = session.Status
        };
    }

    private static string NormalizeRequired(string? value, string code, string message)
    {
        string? normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new BusinessException(code, message);
        }

        return normalized;
    }
}
