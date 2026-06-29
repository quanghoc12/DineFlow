using DineFlow.BusinessObjects.Tables;
using DineFlow.Repositories.Tables;
using DineFlow.Services.Auth;
using Microsoft.Extensions.Configuration;

namespace DineFlow.Services.Tables;

public sealed class TableManagementService : ITableManagementService
{
    private readonly ITableManagementRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly string _customerWebBaseUrl;

    public TableManagementService(
        ITableManagementRepository repository,
        ICurrentUserService currentUser,
        IConfiguration configuration)
    {
        _repository = repository;
        _currentUser = currentUser;
        _customerWebBaseUrl = configuration["CustomerWeb:BaseUrl"]?.TrimEnd('/')
            ?? "http://localhost:5173";
    }

    public async Task<IReadOnlyList<ManagedTableDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return (await _repository.GetAllAsync(cancellationToken)).Select(Map).ToList();
    }

    public async Task<ManagedTableDto> CreateAsync(
        CreateManagedTableRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        (string name, string area) = Validate(request.TableName, request.Area);
        if (await _repository.NameExistsInAreaAsync(name, area, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Tên bàn đã tồn tại trong khu vực này.");
        }

        DateTime now = DateTime.UtcNow;
        DiningTable table = new()
        {
            TableName = name,
            Area = area,
            QrToken = await GenerateUniqueTokenAsync(cancellationToken),
            Status = TableStatuses.Available,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        await _repository.AddAsync(table, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(table);
    }

    public async Task UpdateAsync(
        UpdateManagedTableRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        DiningTable table = await FindAsync(request.TableId, cancellationToken);
        (string name, string area) = Validate(request.TableName, request.Area);
        if (await _repository.NameExistsInAreaAsync(name, area, request.TableId, cancellationToken))
        {
            throw new InvalidOperationException("Tên bàn đã tồn tại trong khu vực này.");
        }

        table.TableName = name;
        table.Area = area;
        table.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetActiveAsync(int tableId, bool active, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        DiningTable table = await FindAsync(tableId, cancellationToken);
        if (!active && TableStatuses.IsBusy(table.Status))
        {
            throw new InvalidOperationException("Không thể khóa bàn đang phục vụ hoặc chờ thanh toán.");
        }

        table.IsActive = active;
        if (active && !TableStatuses.IsBusy(table.Status))
        {
            table.Status = TableStatuses.Available;
        }
        table.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ManagedTableDto> ResetQrAsync(int tableId, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        DiningTable table = await FindAsync(tableId, cancellationToken);
        if (TableStatuses.IsBusy(table.Status))
        {
            throw new InvalidOperationException("Không thể tạo lại QR khi bàn đang phục vụ hoặc chờ thanh toán.");
        }

        table.QrToken = await GenerateUniqueTokenAsync(cancellationToken);
        table.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(table);
    }

    private void EnsureAdmin()
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.User!.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Chỉ Admin được quản lý bàn và mã QR.");
        }
    }

    private async Task<DiningTable> FindAsync(int tableId, CancellationToken cancellationToken) =>
        await _repository.GetByIdAsync(tableId, cancellationToken)
        ?? throw new InvalidOperationException("Không tìm thấy bàn.");

    private async Task<string> GenerateUniqueTokenAsync(CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            string token = Guid.NewGuid().ToString("N");
            if (!await _repository.QrTokenExistsAsync(token, cancellationToken))
            {
                return token;
            }
        }
        throw new InvalidOperationException("Không thể sinh QR token duy nhất. Vui lòng thử lại.");
    }

    private static (string Name, string Area) Validate(string tableName, string area)
    {
        string name = tableName.Trim();
        string normalizedArea = area.Trim();
        if (name.Length is < 1 or > 100)
        {
            throw new InvalidOperationException("Tên bàn phải từ 1 đến 100 ký tự.");
        }
        if (normalizedArea.Length is < 1 or > 100)
        {
            throw new InvalidOperationException("Khu vực phải từ 1 đến 100 ký tự.");
        }
        return (name, normalizedArea);
    }

    private ManagedTableDto Map(DiningTable table) => new()
    {
        TableId = table.TableId,
        TableName = table.TableName,
        Area = table.Area,
        QrToken = table.QrToken,
        QrUrl = $"{_customerWebBaseUrl}?t={Uri.EscapeDataString(table.QrToken)}",
        Status = table.Status,
        IsActive = table.IsActive
    };
}
