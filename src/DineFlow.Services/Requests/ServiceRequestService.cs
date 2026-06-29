using DineFlow.BusinessObjects.Requests;
using DineFlow.Repositories.Common;
using DineFlow.Repositories.Orders;
using DineFlow.Repositories.Requests;
using DineFlow.Services.Common;
using DineFlow.Services.CustomerSessions;
using DineFlow.Services.Orders;
using DineFlow.Services.Realtime;

namespace DineFlow.Services.Requests;

public class ServiceRequestService : IServiceRequestService
{
    private static readonly string[] ValidPaymentMethods = ["Cash", "BankTransfer", "Card", "Combined"];

    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ITableSessionRepository _tableSessionRepository;
    private readonly ITableSessionService _tableSessionService;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceRequestService(
        IServiceRequestRepository serviceRequestRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ITableSessionRepository tableSessionRepository,
        ITableSessionService tableSessionService,
        IUnitOfWork unitOfWork)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _tableSessionRepository = tableSessionRepository;
        _tableSessionService = tableSessionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceRequestDto> CreateServiceRequestAsync(
        CreateServiceRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);
        await ResolveCustomerSessionAsync(request, cancellationToken);

        ServiceRequest serviceRequest = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            ServiceRequest entity = new()
            {
                TableSessionId = request.TableSessionId,
                SessionCustomerId = request.SessionCustomerId,
                ClientToken = request.ClientToken,
                RequestType = request.RequestType,
                Reason = request.Reason,
                PaymentMethod = request.PaymentMethod,
                Message = request.Message,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _serviceRequestRepository.AddServiceRequestAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return entity;
        }, cancellationToken);

        if (request.RequestType == "PaymentRequest")
        {
            await _tableSessionService.MarkWaitingPaymentAsync(request.TableSessionId, cancellationToken);
        }

        await NotifyServiceRequestCreatedAsync(serviceRequest, cancellationToken);

        return Map(serviceRequest);
    }

    public async Task<IReadOnlyList<ServiceRequestDto>> GetConfirmedRequestsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ServiceRequestDto> requests = (await _serviceRequestRepository.GetConfirmedRequestsAsync(cancellationToken))
            .Select(Map)
            .ToList();

        return requests;
    }

    public async Task<IReadOnlyList<ServiceRequestDto>> GetRequestsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ServiceRequest> entities = await _serviceRequestRepository.GetRequestsAsync(from, to, status, cancellationToken);
        return entities.Select(Map).ToList();
    }

    public async Task<ServiceRequestDto> ConfirmRequestAsync(
        int requestId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        ServiceRequest serviceRequest = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            ServiceRequest entity = await _serviceRequestRepository.GetByIdAsync(requestId, ct)
                ?? throw new BusinessException("SERVICE_REQUEST_NOT_FOUND", "Service request does not exist.");

            if (entity.Status != "Pending")
            {
                throw new BusinessException("SERVICE_REQUEST_STATUS_INVALID", "Only pending request can be confirmed.");
            }

            entity.Status = "Confirmed";
            entity.ConfirmedAt = DateTime.UtcNow;
            entity.ConfirmedBy = currentUserId;

            await _unitOfWork.SaveChangesAsync(ct);
            return entity;
        }, cancellationToken);

        await NotifyServiceRequestConfirmedAsync(serviceRequest, cancellationToken);
        return Map(serviceRequest);
    }

    private static void ValidateCreateRequest(CreateServiceRequestRequest request)
    {
        if (request.RequestType != "CallStaff" && request.RequestType != "PaymentRequest")
        {
            throw new BusinessException("SERVICE_REQUEST_TYPE_INVALID", "Service request type is invalid.");
        }

        if (request.RequestType == "CallStaff" && !string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            throw new BusinessException("PAYMENT_METHOD_NOT_ALLOWED", "Call staff request must not include payment method.");
        }

        if (request.RequestType == "PaymentRequest" && !ValidPaymentMethods.Contains(request.PaymentMethod))
        {
            throw new BusinessException("PAYMENT_METHOD_INVALID", "Payment request must include a valid payment method.");
        }
    }

    private async Task ResolveCustomerSessionAsync(
        CreateServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TableSessionId > 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(request.ClientToken))
        {
            throw new BusinessException("CLIENT_TOKEN_REQUIRED", "Client token is required.");
        }

        DineFlow.BusinessObjects.Orders.TableSessionCustomer? customer =
            await _tableSessionRepository.GetSessionCustomerByTokenAsync(request.ClientToken.Trim(), cancellationToken);

        if (customer?.TableSession is null ||
            customer.TableSession.Status is not ("Browsing" or "Open" or "WaitingPayment"))
        {
            throw new BusinessException("CUSTOMER_SESSION_NOT_ACTIVE", "Customer session is not active.");
        }

        if (request.RequestType == "PaymentRequest" && customer.TableSession.Status == "Browsing")
        {
            throw new BusinessException(
                "PAYMENT_REQUEST_REQUIRES_BILL",
                "Payment can only be requested after the first order has been confirmed.");
        }

        request.TableSessionId = customer.TableSessionId;
        request.SessionCustomerId = customer.SessionCustomerId;
        request.ClientToken = customer.ClientToken;
    }

    private static ServiceRequestDto Map(ServiceRequest request)
    {
        return new ServiceRequestDto
        {
            RequestId = request.RequestId,
            TableSessionId = request.TableSessionId,
            SessionCustomerId = request.SessionCustomerId,
            RequestType = request.RequestType,
            Reason = request.Reason,
            PaymentMethod = request.PaymentMethod,
            Message = request.Message,
            Status = request.Status,
            CreatedAt = request.CreatedAt
        };
    }

    private async Task NotifyServiceRequestCreatedAsync(
        ServiceRequest request,
        CancellationToken cancellationToken)
    {
        RealtimeEventDto payload = new()
        {
            TableSessionId = request.TableSessionId,
            RequestId = request.RequestId
        };

        if (!string.IsNullOrWhiteSpace(request.ClientToken))
        {
            await _realtimeNotificationService.NotifyCustomerAsync(
                request.ClientToken,
                RealtimeEvents.CustomerMessageCreated,
                CustomerMessageMapper.MapRequestMessage(request),
                cancellationToken);
        }

        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.ServiceRequestCreated,
            payload,
            cancellationToken);

        if (request.RequestType == "PaymentRequest")
        {
            await _realtimeNotificationService.NotifyStaffAsync(
                RealtimeEvents.TableSessionChanged,
                payload,
                cancellationToken);
            await _realtimeNotificationService.NotifySessionAsync(
                request.TableSessionId,
                RealtimeEvents.TableSessionChanged,
                payload,
                cancellationToken);
        }
    }

    private async Task NotifyServiceRequestConfirmedAsync(
        ServiceRequest request,
        CancellationToken cancellationToken)
    {
        RealtimeEventDto payload = new()
        {
            TableSessionId = request.TableSessionId,
            RequestId = request.RequestId
        };

        if (!string.IsNullOrWhiteSpace(request.ClientToken))
        {
            await _realtimeNotificationService.NotifyCustomerAsync(
                request.ClientToken,
                RealtimeEvents.CustomerMessageCreated,
                CustomerMessageMapper.MapRequestMessage(request),
                cancellationToken);
        }

        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.ServiceRequestConfirmed,
            payload,
            cancellationToken);
    }
}
