using DineFlow.BusinessObjects.Tables;
using DineFlow.BusinessObjects.Auth;
using DineFlow.Repositories.Tables;
using DineFlow.Services.Auth;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

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
        _customerWebBaseUrl = ResolveCustomerWebBaseUrl(configuration);
    }

    public async Task<IReadOnlyList<ManagedTableDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanViewTables();
        return (await _repository.GetAllAsync(cancellationToken)).Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ManagedAreaDto>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanViewTables();
        return (await _repository.GetAreasAsync(cancellationToken)).Select(MapArea).ToList();
    }

    public async Task<ManagedAreaDto> SaveAreaAsync(
        SaveAreaRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        string name = request.AreaName.Trim();
        if (name.Length is < 1 or > 100)
        {
            throw new InvalidOperationException("Tên khu vực phải từ 1 đến 100 ký tự.");
        }
        if (request.DisplayOrder < 0)
        {
            throw new InvalidOperationException("Thứ tự hiển thị không được âm.");
        }
        if (await _repository.AreaNameExistsAsync(name, request.AreaId, cancellationToken))
        {
            throw new InvalidOperationException("Tên khu vực đã tồn tại.");
        }

        DateTime now = DateTime.UtcNow;
        List<Area> areas = await _repository.GetAreasForUpdateAsync(cancellationToken);
        Area area;
        if (request.AreaId is null)
        {
            area = new Area
            {
                AreaName = name,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _repository.AddAreaAsync(area, cancellationToken);
            areas.Add(area);
        }
        else
        {
            area = await _repository.GetAreaAsync(request.AreaId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy khu vực.");
            area.AreaName = name;
            area.DisplayOrder = request.DisplayOrder;
            area.UpdatedAt = now;
        }

        RebuildAreaOrders(areas, area, request.DisplayOrder, now);
        await _repository.SaveChangesAsync(cancellationToken);
        return MapArea(area);
    }

    public async Task SetAreaActiveAsync(
        int areaId,
        bool active,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        Area area = await _repository.GetAreaAsync(areaId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy khu vực.");
        area.IsActive = active;
        area.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ManagedTableDto> CreateAsync(
        CreateManagedTableRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        (string name, Area? areaEntity, string area) = await ValidateAsync(
            request.TableName, request.AreaId, request.Area, cancellationToken);
        if (await _repository.NameExistsInAreaAsync(name, areaEntity?.AreaId, area, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Tên bàn đã tồn tại trong khu vực này.");
        }

        DateTime now = DateTime.UtcNow;
        DiningTable table = new()
        {
            TableName = name,
            AreaId = areaEntity?.AreaId,
            Area = area,
            DisplayOrder = request.DisplayOrder,
            QrToken = await GenerateUniqueTokenAsync(cancellationToken),
            CurrentOtp = TableOtpGenerator.Generate(),
            OtpUpdatedAt = now,
            Status = TableStatuses.Available,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        await _repository.AddAsync(table, cancellationToken);
        await RebuildTableOrdersAsync(table, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(table);
    }

    public async Task UpdateAsync(
        UpdateManagedTableRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        DiningTable table = await FindAsync(request.TableId, cancellationToken);
        (string name, Area? areaEntity, string area) = await ValidateAsync(
            request.TableName, request.AreaId, request.Area, cancellationToken);
        if (await _repository.NameExistsInAreaAsync(name, areaEntity?.AreaId, area, request.TableId, cancellationToken))
        {
            throw new InvalidOperationException("Tên bàn đã tồn tại trong khu vực này.");
        }

        table.TableName = name;
        table.AreaId = areaEntity?.AreaId;
        table.Area = area;
        table.DisplayOrder = request.DisplayOrder;
        table.UpdatedAt = DateTime.UtcNow;
        await RebuildTableOrdersAsync(table, cancellationToken);
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

    public async Task<ManagedTableDto> ResetOtpAsync(int tableId, CancellationToken cancellationToken = default)
    {
        EnsureOtpResetAdmin();
        DiningTable table = await FindAsync(tableId, cancellationToken);
        DateTime now = DateTime.UtcNow;
        table.CurrentOtp = TableOtpGenerator.Generate();
        table.OtpUpdatedAt = now;
        table.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(table);
    }

    private void EnsureAdmin()
    {
        if (!_currentUser.IsAuthenticated ||
            !AuthRoles.CanManage(_currentUser.User!.Role))
        {
            throw new UnauthorizedAccessException("Chỉ Admin hoặc Chủ nhà hàng được quản lý bàn và mã QR.");
        }
    }

    private void EnsureCanViewTables()
    {
        if (!_currentUser.IsAuthenticated ||
            (!AuthRoles.IsStaff(_currentUser.User!.Role) &&
             !AuthRoles.IsAdmin(_currentUser.User.Role) &&
             !AuthRoles.IsOwner(_currentUser.User.Role)))
        {
            throw new UnauthorizedAccessException("Không có quyền xem danh sách bàn.");
        }
    }

    private void EnsureOtpResetAdmin()
    {
        if (!_currentUser.IsAuthenticated ||
            !AuthRoles.IsAdmin(_currentUser.User!.Role))
        {
            throw new UnauthorizedAccessException("Chỉ Admin được reset OTP bàn.");
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

    private async Task<(string Name, Area? AreaEntity, string AreaName)> ValidateAsync(
        string tableName,
        int? areaId,
        string legacyArea,
        CancellationToken cancellationToken)
    {
        string name = tableName.Trim();
        if (name.Length is < 1 or > 100)
        {
            throw new InvalidOperationException("Tên bàn phải từ 1 đến 100 ký tự.");
        }
        if (areaId.HasValue)
        {
            Area area = await _repository.GetAreaAsync(areaId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy khu vực.");
            if (!area.IsActive)
            {
                throw new InvalidOperationException("Không thể gán bàn vào khu vực đang bị khóa.");
            }
            return (name, area, area.AreaName);
        }

        string normalizedArea = legacyArea.Trim();
        if (normalizedArea.Length is < 1 or > 100)
        {
            throw new InvalidOperationException("Khu vực phải từ 1 đến 100 ký tự.");
        }
        Area? matchingArea = await _repository.GetAreaByNameAsync(normalizedArea, cancellationToken);
        if (matchingArea is not null && !matchingArea.IsActive)
        {
            throw new InvalidOperationException("Không thể gán bàn vào khu vực đang bị khóa.");
        }
        return (name, matchingArea, matchingArea?.AreaName ?? normalizedArea);
    }

    private ManagedTableDto Map(DiningTable table) => new()
    {
        TableId = table.TableId,
        TableName = table.TableName,
        AreaId = table.AreaId,
        Area = table.AreaEntity?.AreaName ?? table.Area,
        QrToken = table.QrToken,
        QrUrl = $"{_customerWebBaseUrl}/table/{Uri.EscapeDataString(table.QrToken)}",
        CurrentOtp = table.CurrentOtp,
        OtpUpdatedAt = table.OtpUpdatedAt,
        Status = table.Status,
        IsActive = table.IsActive,
        DisplayOrder = table.DisplayOrder
    };

    private static ManagedAreaDto MapArea(Area area) => new()
    {
        AreaId = area.AreaId,
        AreaName = area.AreaName,
        DisplayOrder = area.DisplayOrder,
        IsActive = area.IsActive,
        TableCount = area.DiningTables.Count
    };

    private static void RebuildAreaOrders(
        List<Area> areas, Area target, int requestedOrder, DateTime now)
    {
        List<Area> ordered = areas
            .Where(area => !ReferenceEquals(area, target))
            .OrderBy(area => area.DisplayOrder)
            .ThenBy(area => area.AreaName)
            .ToList();
        ordered.Insert(Math.Clamp(requestedOrder, 0, ordered.Count), target);
        for (int index = 0; index < ordered.Count; index++)
        {
            ordered[index].DisplayOrder = index;
            ordered[index].UpdatedAt = now;
        }
    }

    private static string ResolveCustomerWebBaseUrl(IConfiguration configuration)
    {
        string? configuredUrl = configuration["CustomerWeb:BaseUrl"]?.Trim();
        if (!string.IsNullOrWhiteSpace(configuredUrl) &&
            !configuredUrl.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            return configuredUrl.TrimEnd('/');
        }

        int port = int.TryParse(configuration["CustomerWeb:Port"], out int configuredPort)
            ? configuredPort
            : 5173;
        string host = FindLocalNetworkAddress()?.ToString() ?? "localhost";
        return $"http://{host}:{port}";
    }

    private static IPAddress? FindLocalNetworkAddress() =>
        NetworkInterface.GetAllNetworkInterfaces()
            .Where(network =>
                network.OperationalStatus == OperationalStatus.Up &&
                network.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                network.GetIPProperties().GatewayAddresses.Any(gateway =>
                    gateway.Address.AddressFamily == AddressFamily.InterNetwork))
            .OrderBy(network => network.NetworkInterfaceType switch
            {
                NetworkInterfaceType.Wireless80211 => 0,
                NetworkInterfaceType.Ethernet => 1,
                _ => 2
            })
            .SelectMany(network => network.GetIPProperties().UnicastAddresses)
            .Where(address =>
                address.Address.AddressFamily == AddressFamily.InterNetwork &&
                !IPAddress.IsLoopback(address.Address) &&
                !address.Address.ToString().StartsWith("169.254.", StringComparison.Ordinal))
            .Select(address => address.Address)
            .FirstOrDefault();

    private async Task RebuildTableOrdersAsync(
        DiningTable target, CancellationToken cancellationToken)
    {
        List<DiningTable> allTables = await _repository.GetAllForUpdateAsync(cancellationToken);
        if (!allTables.Contains(target))
            allTables.Add(target);

        string targetAreaKey = target.AreaId?.ToString() ?? $"legacy:{target.Area}";
        foreach (IGrouping<string, DiningTable> areaTables in allTables.GroupBy(
                     table => table.AreaId?.ToString() ?? $"legacy:{table.Area}"))
        {
            List<DiningTable> ordered = areaTables
                .Where(table => !ReferenceEquals(table, target))
                .OrderBy(table => table.DisplayOrder)
                .ThenBy(table => table.TableName)
                .ToList();
            if (areaTables.Key == targetAreaKey)
                ordered.Insert(Math.Clamp(target.DisplayOrder, 0, ordered.Count), target);

            for (int index = 0; index < ordered.Count; index++)
            {
                ordered[index].DisplayOrder = index;
                ordered[index].UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
