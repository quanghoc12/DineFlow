namespace DineFlow.BusinessObjects.Tables;

public static class TableStatuses
{
    public const string Available = "Available";
    public const string Occupied = "Occupied";
    public const string WaitingPayment = "WaitingPayment";

    public static bool IsBusy(string status) =>
        status.Equals(Occupied, StringComparison.OrdinalIgnoreCase) ||
        status.Equals(WaitingPayment, StringComparison.OrdinalIgnoreCase);
}

public sealed class ManagedTableDto
{
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int? AreaId { get; set; }
    public string Area { get; set; } = string.Empty;
    public string QrToken { get; set; } = string.Empty;
    public string QrUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string ActiveStatusLabel => IsActive ? "Hoạt động" : "Đã khóa";
    public string StatusLabel => Status switch
    {
        TableStatuses.Available => "Trống",
        TableStatuses.Occupied => "Đang phục vụ",
        TableStatuses.WaitingPayment => "Chờ thanh toán",
        _ => Status
    };
}

public sealed class CreateManagedTableRequest
{
    public string TableName { get; set; } = string.Empty;
    public int? AreaId { get; set; }
    public string Area { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public sealed class UpdateManagedTableRequest
{
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int? AreaId { get; set; }
    public string Area { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public sealed class ManagedAreaDto
{
    public int AreaId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int TableCount { get; set; }
    public string ActiveStatusLabel => IsActive ? "Hoạt động" : "Đã khóa";
}

public sealed class SaveAreaRequest
{
    public int? AreaId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
