namespace DineFlow.Services.Bills;

public class BillSummaryDto
{
    public int BillId { get; set; }
    public string BillCode { get; set; } = string.Empty;
    public int TableSessionId { get; set; }
    public int SalesChannelId { get; set; }
    public string SalesChannelCode { get; set; } = string.Empty;
    public string SalesChannelName { get; set; } = string.Empty;
    public int BillNo { get; set; }
    public string BillName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BillDto : BillSummaryDto
{
    public List<BillDetailDto> Details { get; set; } = [];
    public List<PaymentDto> Payments { get; set; } = [];
}

public class BillDetailDto
{
    public int BillDetailId { get; set; }
    public int BillId { get; set; }
    public int MenuItemId { get; set; }
    public int SalesChannelId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ChoiceSummary { get; set; }
    public string? Note { get; set; }
    public int Quantity { get; set; }
    public int NotifiedQuantity { get; set; }
    public decimal BasePriceSnapshot { get; set; }
    public decimal MenuItemChannelExtraPriceSnapshot { get; set; }
    public decimal ChoiceExtraPriceSnapshot { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class AddOrderItemsToBillRequest
{
    public int TableSessionId { get; set; }
    public int OrderId { get; set; }
    public int? TargetBillId { get; set; }
    public int? CreatedBy { get; set; }
}

public class AdjustBillDetailQuantityRequest
{
    public int BillDetailId { get; set; }
    public int NewQuantity { get; set; }
    public bool RestoreStock { get; set; }
    public string? ChangeReason { get; set; }
}

public class RenameBillRequest
{
    public string BillName { get; set; } = string.Empty;
}

public class CreateBillRequest
{
    public string BillName { get; set; } = string.Empty;
    public int? SalesChannelId { get; set; }
}

public class SplitBillRequest
{
    public int SourceBillId { get; set; }
    public int? TargetBillId { get; set; }
    public string? NewBillName { get; set; }
    public int BillDetailId { get; set; }
    public int QuantityToMove { get; set; }
}

public class SplitBillBatchRequest
{
    public int SourceBillId { get; set; }
    public int? TargetBillId { get; set; }
    public bool CreateNewBill { get; set; }
    public string? NewBillName { get; set; }
    public List<SplitBillItemRequest> Items { get; set; } = [];
}

public class SplitBillItemRequest
{
    public int BillDetailId { get; set; }
    public int QuantityToMove { get; set; }
}

public class MoveBillItemRequest
{
    public int SourceBillId { get; set; }
    public int TargetBillId { get; set; }
    public int BillDetailId { get; set; }
    public int QuantityToMove { get; set; }
}

public class MergeBillRequest
{
    public int SourceBillId { get; set; }
    public int TargetBillId { get; set; }
}

public class ConfirmPaymentRequest
{
    public int BillId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ConfirmCombinedPaymentRequest
{
    public int BillId { get; set; }
    public List<PaymentPartRequest> Payments { get; set; } = [];
}

public class PaymentPartRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PaymentResultDto
{
    public int BillId { get; set; }
    public string BillStatus { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public bool SessionClosed { get; set; }
    public List<PaymentDto> Payments { get; set; } = [];
}

public class PaymentDto
{
    public int PaymentId { get; set; }
    public int BillId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public int ConfirmedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public string? ChangeReason { get; set; }
}
