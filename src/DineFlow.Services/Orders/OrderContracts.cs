namespace DineFlow.Services.Orders;

public class CreateCustomerOrderRequest
{
    public string TableToken { get; set; } = string.Empty;
    public string ClientToken { get; set; } = string.Empty;
    public string? SalesChannelCode { get; set; }
    public string? ExternalOrderCode { get; set; }
    public string? DisplayName { get; set; }
    public string? CustomerNote { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = [];
}

public class CreateStaffOrderRequest
{
    public int TableId { get; set; }
    public int? TargetBillId { get; set; }
    public string? SalesChannelCode { get; set; }
    public string? ExternalOrderCode { get; set; }
    public string? CustomerNote { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = [];
}

public class CreateOrderItemRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public List<SelectedChoiceGroupRequest> SelectedChoices { get; set; } = [];
}

public class SelectedChoiceGroupRequest
{
    public int ChoiceGroupId { get; set; }
    public List<int> ChoiceItemIds { get; set; } = [];
}

public class CreateOrderResponse
{
    public int? OrderId { get; set; }
    public string? OrderCode { get; set; }
    public int? TableSessionId { get; set; }
    public int? BillId { get; set; }
    public string? PrintStatus { get; set; }
    public List<OrderItemDetailDto> AcceptedItems { get; set; } = [];
    public List<RejectedOrderItemDto> RejectedItems { get; set; } = [];
}

public class ConfirmOrderRequest
{
    public int? TargetBillId { get; set; }
}

public class CancelOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class RejectedOrderItemDto
{
    public int MenuItemId { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string ReasonMessage { get; set; } = string.Empty;
}

public class OrderFilter
{
    public int? TableSessionId { get; set; }
    public string? Status { get; set; }
    public string? PrintStatus { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public int TableSessionId { get; set; }
    public int SalesChannelId { get; set; }
    public string SalesChannelCode { get; set; } = string.Empty;
    public string SalesChannelName { get; set; } = string.Empty;
    public string OrderSource { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PrintStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

public class OrderDetailDto : OrderSummaryDto
{
    public string? CustomerNote { get; set; }
    public string? SystemNote { get; set; }
    public List<OrderItemDetailDto> Items { get; set; } = [];
}

public class OrderItemDetailDto
{
    public int OrderItemId { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemNameSnapshot { get; set; } = string.Empty;
    public decimal BasePriceSnapshot { get; set; }
    public decimal ChannelExtraPriceSnapshot { get; set; }
    public decimal FinalUnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public List<OrderItemSelectedChoiceDto> SelectedChoices { get; set; } = [];
    public decimal LineUnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderItemSelectedChoiceDto
{
    public int ChoiceGroupId { get; set; }
    public int ChoiceItemId { get; set; }
    public string GroupNameSnapshot { get; set; } = string.Empty;
    public string ChoiceNameSnapshot { get; set; } = string.Empty;
    public decimal ExtraPriceSnapshot { get; set; }
    public decimal ChannelExtraPriceSnapshot { get; set; }
    public decimal FinalExtraPriceSnapshot { get; set; }
}

public class MarkPrintFailedRequest
{
    public string PrintError { get; set; } = string.Empty;
}
