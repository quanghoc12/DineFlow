using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Orders;
using DineFlow.Repositories.Common;
using DineFlow.Repositories.Menu;
using DineFlow.Repositories.Orders;
using DineFlow.Services.Bills;
using DineFlow.Services.Common;
using DineFlow.Services.CustomerSessions;
using DineFlow.Services.Realtime;

namespace DineFlow.Services.Orders;

public class OrderService : IOrderService
{
    private readonly IBillService _billService;
    private readonly IMenuReadRepository _menuReadRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ITableSessionRepository _tableSessionRepository;
    private readonly ITableSessionService _tableSessionService;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IBillService billService,
        IMenuReadRepository menuReadRepository,
        IOrderRepository orderRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ITableSessionRepository tableSessionRepository,
        ITableSessionService tableSessionService,
        IUnitOfWork unitOfWork)
    {
        _billService = billService;
        _menuReadRepository = menuReadRepository;
        _orderRepository = orderRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _tableSessionRepository = tableSessionRepository;
        _tableSessionService = tableSessionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateOrderResponse> CreateCustomerOrderAsync(
        CreateCustomerOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ClientToken))
        {
            throw new BusinessException("CLIENT_TOKEN_REQUIRED", "Client token is required.");
        }

        TableSessionDto session = await _tableSessionService.GetOrCreateActiveSessionByQrTokenAsync(
            request.TableToken,
            openedBy: null,
            cancellationToken);

        CreateOrderResponse response = await CreateOrderCoreAsync(
            tableSessionId: session.TableSessionId,
            salesChannelCode: request.SalesChannelCode ?? "CUSTOMER_WEB",
            externalOrderCode: request.ExternalOrderCode,
            clientToken: request.ClientToken,
            displayName: request.DisplayName,
            customerNote: request.CustomerNote,
            items: request.Items,
            createdBy: null,
            targetBillId: null,
            addToBillImmediately: false,
            cancellationToken);

        await NotifyCustomerOrderCreatedAsync(response.OrderId, cancellationToken);
        return response;
    }

    public async Task<CreateOrderResponse> CreateStaffOrderAsync(
        CreateStaffOrderRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        TableSessionDto session = await _tableSessionService.GetOrCreateActiveSessionByTableIdAsync(
            request.TableId,
            currentUserId,
            cancellationToken);

        CreateOrderResponse response = await CreateOrderCoreAsync(
            tableSessionId: session.TableSessionId,
            salesChannelCode: request.SalesChannelCode ?? "DINE_IN",
            externalOrderCode: request.ExternalOrderCode,
            clientToken: null,
            displayName: null,
            customerNote: request.CustomerNote,
            items: request.Items,
            createdBy: currentUserId,
            targetBillId: request.TargetBillId,
            addToBillImmediately: true,
            cancellationToken);

        await NotifyBillChangedAsync(response.TableSessionId, response.OrderId, response.BillId, cancellationToken);
        return response;
    }

    public async Task<BillDto> ConfirmOrderAsync(
        int orderId,
        ConfirmOrderRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        Order order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken)
            ?? throw new BusinessException("ORDER_NOT_FOUND", "Order does not exist.");

        if (order.Status != "PendingConfirmation")
        {
            throw new BusinessException("ORDER_CONFIRM_STATUS_INVALID", "Only pending order can be confirmed.");
        }

        await _tableSessionService.ActivateBrowsingSessionAsync(
            order.TableSessionId,
            currentUserId,
            cancellationToken);

        order.Status = "Accepted";
        order.UpdatedAt = DateTime.UtcNow;
        order.CreatedBy ??= currentUserId;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        BillDto bill = await _billService.AddOrderItemsToDefaultBillAsync(new AddOrderItemsToBillRequest
        {
            TableSessionId = order.TableSessionId,
            OrderId = order.OrderId,
            TargetBillId = request.TargetBillId,
            CreatedBy = currentUserId
        }, cancellationToken);

        await NotifyCustomerOrderStatusChangedAsync(order.OrderId, cancellationToken);
        await NotifyBillChangedAsync(order.TableSessionId, order.OrderId, bill.BillId, cancellationToken);
        return bill;
    }

    public async Task<OrderDetailDto> CancelPendingOrderAsync(
        int orderId,
        CancelOrderRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        string reason = request.Reason?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessException("ORDER_CANCEL_REASON_REQUIRED", "Cancel reason is required.");
        }

        Order order = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Order entity = await _orderRepository.GetOrderByIdAsync(orderId, ct)
                ?? throw new BusinessException("ORDER_NOT_FOUND", "Order does not exist.");

            if (entity.Status != "PendingConfirmation")
            {
                throw new BusinessException("ORDER_CANCEL_STATUS_INVALID", "Only pending order can be cancelled.");
            }

            entity.Status = "Cancelled";
            entity.CancelReason = reason;
            entity.SystemNote = reason;
            entity.CancelledAt = DateTime.UtcNow;
            entity.CancelledBy = currentUserId;
            entity.UpdatedAt = DateTime.UtcNow;

            foreach (OrderItem orderItem in entity.OrderItems)
            {
                await RestoreStockAsync(orderItem.MenuItemId, orderItem.Quantity, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return entity;
        }, cancellationToken);

        await NotifyCustomerOrderStatusChangedAsync(order.OrderId, cancellationToken);
        await NotifyStaffOrderStatusChangedAsync(order, cancellationToken);
        return MapOrderDetail(order);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(
        OrderFilter filter,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OrderSummaryDto> orders = (await _orderRepository.GetOrdersAsync(
                filter.TableSessionId,
                filter.Status,
                filter.PrintStatus,
                filter.From,
                filter.To,
                cancellationToken))
            .GroupBy(x => x.OrderId)
            .Select(group => group.First())
            .Select(MapOrderSummary)
            .ToList();

        return orders;
    }

    public async Task<OrderDetailDto?> GetOrderDetailAsync(int orderId, CancellationToken cancellationToken = default)
    {
        Order? order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        return order is null ? null : MapOrderDetail(order);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetOrdersBySessionAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OrderSummaryDto> orders = (await _orderRepository.GetOrdersBySessionAsync(tableSessionId, cancellationToken))
            .Select(MapOrderSummary)
            .ToList();

        return orders;
    }

    public async Task SystemCancelOrderBeforeBillMergeAsync(
        int orderId,
        string systemReason,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Order order = await _orderRepository.GetOrderByIdAsync(orderId, ct)
                ?? throw new BusinessException("ORDER_NOT_FOUND", "Order does not exist.");

            if (order.Status != "Accepted")
            {
                return;
            }

            order.Status = "Cancelled";
            order.CancelReason = string.IsNullOrWhiteSpace(systemReason) ? "System cancelled before bill merge." : systemReason.Trim();
            order.CancelledAt = DateTime.UtcNow;
            order.SystemNote = order.CancelReason;

            foreach (OrderItem orderItem in order.OrderItems)
            {
                await RestoreStockAsync(orderItem.MenuItemId, orderItem.Quantity, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
    }

    private async Task<CreateOrderResponse> CreateOrderCoreAsync(
        int tableSessionId,
        string salesChannelCode,
        string? externalOrderCode,
        string? clientToken,
        string? displayName,
        string? customerNote,
        IReadOnlyCollection<CreateOrderItemRequest> items,
        int? createdBy,
        int? targetBillId,
        bool addToBillImmediately,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            throw new BusinessException("ORDER_EMPTY", "Order must contain at least one item.");
        }

        TableSessionCustomer? customer = null;
        SalesChannel salesChannel = await ResolveSalesChannelAsync(salesChannelCode, cancellationToken);

        if (!string.IsNullOrWhiteSpace(clientToken))
        {
            customer = await GetOrCreateSessionCustomerAsync(tableSessionId, clientToken, displayName, cancellationToken);
        }

        List<ValidatedOrderItem> acceptedItems = [];
        List<RejectedOrderItemDto> rejectedItems = [];

        foreach (CreateOrderItemRequest item in items)
        {
            ValidatedOrderItemResult result = await ValidateOrderItemAsync(item, salesChannel.SalesChannelId, cancellationToken);

            if (result.ValidatedItem is null)
            {
                rejectedItems.Add(new RejectedOrderItemDto
                {
                    MenuItemId = item.MenuItemId,
                    ReasonCode = result.ReasonCode,
                    ReasonMessage = result.ReasonMessage
                });
            }
            else
            {
                acceptedItems.Add(result.ValidatedItem);
            }
        }

        List<ValidatedOrderItem> mergedItems = MergeAcceptedItems(acceptedItems);

        foreach (ValidatedOrderItem item in mergedItems)
        {
            if (item.MenuItem.Stock is not null && item.MenuItem.Stock < item.Quantity)
            {
                rejectedItems.Add(new RejectedOrderItemDto
                {
                    MenuItemId = item.MenuItem.MenuItemId,
                    ReasonCode = "STOCK_NOT_ENOUGH",
                    ReasonMessage = "Stock is not enough after merging requested quantities."
                });
            }
        }

        mergedItems = mergedItems
            .Where(x => x.MenuItem.Stock is null || x.MenuItem.Stock >= x.Quantity)
            .ToList();

        if (mergedItems.Count == 0)
        {
            return new CreateOrderResponse
            {
                TableSessionId = tableSessionId,
                RejectedItems = rejectedItems
            };
        }

        Order order = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            ReserveStock(mergedItems);

            Order newOrder = new()
            {
                TableSessionId = tableSessionId,
                SessionCustomerId = customer?.SessionCustomerId,
                SalesChannelId = salesChannel.SalesChannelId,
                OrderCode = NewOrderCode(),
                OrderSource = salesChannel.ChannelCode,
                ExternalOrderCode = Normalize(externalOrderCode),
                ClientToken = clientToken,
                Status = addToBillImmediately ? "Accepted" : "PendingConfirmation",
                PrintStatus = null,
                CustomerNote = customerNote,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            foreach (ValidatedOrderItem acceptedItem in mergedItems)
            {
                OrderItem orderItem = new()
                {
                    MenuItemId = acceptedItem.MenuItem.MenuItemId,
                    MenuItemNameSnapshot = acceptedItem.MenuItem.Name,
                    BasePriceSnapshot = acceptedItem.MenuItem.BasePrice,
                    ChannelExtraPriceSnapshot = acceptedItem.ChannelExtraPrice,
                    FinalUnitPriceSnapshot = acceptedItem.FinalUnitPrice,
                    Quantity = acceptedItem.Quantity,
                    Note = Normalize(acceptedItem.Note),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                foreach (ValidatedChoice selectedChoice in acceptedItem.SelectedChoices)
                {
                    orderItem.SelectedChoices.Add(new OrderItemSelectedChoice
                    {
                        ChoiceGroupId = selectedChoice.ChoiceGroup.ChoiceGroupId,
                        ChoiceItemId = selectedChoice.ChoiceItem.ChoiceItemId,
                        GroupNameSnapshot = selectedChoice.ChoiceGroup.GroupName,
                        ChoiceNameSnapshot = selectedChoice.ChoiceItem.ChoiceName,
                        ExtraPriceSnapshot = selectedChoice.ChoiceItem.ExtraPrice,
                        ChannelExtraPriceSnapshot = selectedChoice.ChannelExtraPrice,
                        FinalExtraPriceSnapshot = selectedChoice.FinalExtraPrice,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                newOrder.OrderItems.Add(orderItem);
            }

            await _orderRepository.AddOrderAsync(newOrder, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return newOrder;
        }, cancellationToken);

        int? billId = null;
        if (addToBillImmediately)
        {
            BillDto bill = await _billService.AddOrderItemsToDefaultBillAsync(new AddOrderItemsToBillRequest
            {
                TableSessionId = tableSessionId,
                OrderId = order.OrderId,
                TargetBillId = targetBillId,
                CreatedBy = createdBy
            }, cancellationToken);
            billId = bill.BillId;
        }

        Order detailOrder = await _orderRepository.GetOrderByIdAsync(order.OrderId, cancellationToken) ?? order;

        return new CreateOrderResponse
        {
            OrderId = detailOrder.OrderId,
            OrderCode = detailOrder.OrderCode,
            TableSessionId = detailOrder.TableSessionId,
            BillId = billId,
            PrintStatus = detailOrder.PrintStatus,
            AcceptedItems = detailOrder.OrderItems.Select(MapOrderItem).ToList(),
            RejectedItems = rejectedItems
        };
    }

    private async Task<TableSessionCustomer> GetOrCreateSessionCustomerAsync(
        int tableSessionId,
        string clientToken,
        string? displayName,
        CancellationToken cancellationToken)
    {
        TableSessionCustomer? existing = await _tableSessionRepository.GetSessionCustomerAsync(
            tableSessionId,
            clientToken,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            TableSessionCustomer customer = new()
            {
                TableSessionId = tableSessionId,
                ClientToken = clientToken,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow
            };

            await _tableSessionRepository.AddSessionCustomerAsync(customer, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return customer;
        }, cancellationToken);
    }

    private async Task<SalesChannel> ResolveSalesChannelAsync(string channelCode, CancellationToken cancellationToken)
    {
        SalesChannel? salesChannel = await _menuReadRepository.GetSalesChannelByCodeAsync(channelCode, cancellationToken);

        if (salesChannel is null)
        {
            throw new BusinessException("SALES_CHANNEL_INVALID", "Sales channel does not exist or is inactive.");
        }

        return salesChannel;
    }

    private async Task<ValidatedOrderItemResult> ValidateOrderItemAsync(
        CreateOrderItemRequest request,
        int salesChannelId,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return Rejected(request.MenuItemId, "QUANTITY_INVALID", "Quantity must be greater than zero.");
        }

        MenuItem? menuItem = await _menuReadRepository.GetMenuItemByIdAsync(request.MenuItemId, cancellationToken);

        if (menuItem is null || !menuItem.IsAvailable)
        {
            return Rejected(request.MenuItemId, "MENU_ITEM_UNAVAILABLE", "Menu item does not exist or is unavailable.");
        }

        if (menuItem.IsOutOfStock || menuItem.Stock == 0)
        {
            return Rejected(request.MenuItemId, "MENU_ITEM_OUT_OF_STOCK", "Menu item is out of stock.");
        }

        decimal menuItemChannelExtraPrice = await _menuReadRepository.GetMenuItemChannelExtraPriceAsync(
            menuItem.MenuItemId,
            salesChannelId,
            cancellationToken);

        List<MenuItemChoiceGroup> assignments = (await _menuReadRepository.GetChoiceGroupAssignmentsByMenuItemIdAsync(
                request.MenuItemId,
                cancellationToken))
            .ToList();

        Dictionary<int, List<int>> selectedByGroup = request.SelectedChoices
            .GroupBy(x => x.ChoiceGroupId)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(x => x.ChoiceItemIds).Distinct().OrderBy(x => x).ToList());

        List<ValidatedChoice> selectedChoices = [];

        foreach (MenuItemChoiceGroup assignment in assignments.OrderBy(x => x.DisplayOrder))
        {
            ChoiceGroup? group = await _menuReadRepository.GetAvailableChoiceGroupByIdAsync(
                assignment.ChoiceGroupId,
                cancellationToken);

            if (group is null)
            {
                continue;
            }

            selectedByGroup.TryGetValue(group.ChoiceGroupId, out List<int>? selectedChoiceIds);
            selectedChoiceIds ??= [];

            int effectiveMaxSelect = assignment.MaxSelect ?? group.MaxSelectDefault;

            if (group.IsRequired && selectedChoiceIds.Count != 1)
            {
                return Rejected(request.MenuItemId, "REQUIRED_CHOICE_INVALID", $"Choice group {group.GroupName} requires exactly one choice.");
            }

            if (!group.IsRequired && selectedChoiceIds.Count > effectiveMaxSelect)
            {
                return Rejected(request.MenuItemId, "MAX_SELECT_EXCEEDED", $"Choice group {group.GroupName} allows at most {effectiveMaxSelect} choices.");
            }

            foreach (int choiceItemId in selectedChoiceIds)
            {
                ChoiceItem? choiceItem = await _menuReadRepository.GetAvailableChoiceItemAsync(
                    group.ChoiceGroupId,
                    choiceItemId,
                    cancellationToken);

                if (choiceItem is null)
                {
                    return Rejected(request.MenuItemId, "CHOICE_ITEM_INVALID", "Selected choice item is invalid.");
                }

                decimal choiceChannelExtraPrice = await _menuReadRepository.GetChoiceItemChannelExtraPriceAsync(
                    choiceItem.ChoiceItemId,
                    salesChannelId,
                    cancellationToken);

                selectedChoices.Add(new ValidatedChoice(
                    group,
                    choiceItem,
                    choiceChannelExtraPrice,
                    choiceItem.ExtraPrice + choiceChannelExtraPrice));
            }
        }

        List<int> assignedGroupIds = assignments.Select(x => x.ChoiceGroupId).ToList();
        bool hasUnassignedGroup = selectedByGroup.Keys.Any(groupId => !assignedGroupIds.Contains(groupId));

        if (hasUnassignedGroup)
        {
            return Rejected(request.MenuItemId, "CHOICE_GROUP_NOT_ASSIGNED", "Selected choice group is not assigned to menu item.");
        }

        if (menuItem.Stock is not null && menuItem.Stock < request.Quantity)
        {
            return Rejected(request.MenuItemId, "STOCK_NOT_ENOUGH", "Stock is not enough.");
        }

        return new ValidatedOrderItemResult(
            new ValidatedOrderItem(
                menuItem,
                request.Quantity,
                Normalize(request.Note),
                menuItemChannelExtraPrice,
                menuItem.BasePrice + menuItemChannelExtraPrice,
                selectedChoices),
            string.Empty,
            string.Empty);
    }

    private static ValidatedOrderItemResult Rejected(int menuItemId, string code, string message)
    {
        return new ValidatedOrderItemResult(null, code, message);
    }

    private static List<ValidatedOrderItem> MergeAcceptedItems(IEnumerable<ValidatedOrderItem> items)
    {
        return items
            .GroupBy(x => new
            {
                x.MenuItem.MenuItemId,
                Note = Normalize(x.Note),
                ChoiceKey = BuildChoiceKey(x.SelectedChoices)
            })
            .Select(group =>
            {
                ValidatedOrderItem first = group.First();
                return first with { Quantity = group.Sum(x => x.Quantity) };
            })
            .ToList();
    }

    private static string BuildChoiceKey(IEnumerable<ValidatedChoice> choices)
    {
        return string.Join("|", choices
            .OrderBy(x => x.ChoiceGroup.ChoiceGroupId)
            .ThenBy(x => x.ChoiceItem.ChoiceItemId)
            .Select(x => $"{x.ChoiceGroup.ChoiceGroupId}:{x.ChoiceItem.ChoiceItemId}"));
    }

    private static void ReserveStock(IEnumerable<ValidatedOrderItem> items)
    {
        foreach (ValidatedOrderItem item in items)
        {
            if (item.MenuItem.Stock is null)
            {
                continue;
            }

            item.MenuItem.Stock -= item.Quantity;

            if (item.MenuItem.Stock <= 0)
            {
                item.MenuItem.Stock = 0;
                item.MenuItem.IsOutOfStock = true;
            }

            item.MenuItem.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task RestoreStockAsync(int menuItemId, int quantity, CancellationToken cancellationToken)
    {
        MenuItem? menuItem = await _menuReadRepository.GetMenuItemByIdAsync(menuItemId, cancellationToken);

        if (menuItem?.Stock is null)
        {
            return;
        }

        menuItem.Stock += quantity;
        menuItem.UpdatedAt = DateTime.UtcNow;
    }

    private static OrderSummaryDto MapOrderSummary(Order order)
    {
        return new OrderSummaryDto
        {
            OrderId = order.OrderId,
            OrderCode = order.OrderCode,
            TableSessionId = order.TableSessionId,
            SalesChannelId = order.SalesChannelId,
            SalesChannelCode = order.SalesChannel?.ChannelCode ?? order.OrderSource,
            SalesChannelName = order.SalesChannel?.ChannelName ?? order.OrderSource,
            OrderSource = order.OrderSource,
            Status = order.Status,
            PrintStatus = order.PrintStatus,
            CreatedAt = order.CreatedAt,
            ItemCount = order.OrderItems.Sum(x => x.Quantity),
            TableName = order.TableSession?.Table?.TableName
        };
    }

    private static OrderDetailDto MapOrderDetail(Order order)
    {
        OrderSummaryDto summary = MapOrderSummary(order);
        return new OrderDetailDto
        {
            OrderId = summary.OrderId,
            OrderCode = summary.OrderCode,
            TableSessionId = summary.TableSessionId,
            OrderSource = summary.OrderSource,
            Status = summary.Status,
            PrintStatus = summary.PrintStatus,
            CreatedAt = summary.CreatedAt,
            ItemCount = summary.ItemCount,
            CustomerNote = order.CustomerNote,
            SystemNote = order.SystemNote,
            Items = order.OrderItems.Select(MapOrderItem).ToList()
        };
    }

    private static OrderItemDetailDto MapOrderItem(OrderItem orderItem)
    {
        decimal selectedChoiceExtraTotal = orderItem.SelectedChoices.Sum(x => x.FinalExtraPriceSnapshot);
        decimal lineUnitPrice = orderItem.FinalUnitPriceSnapshot + selectedChoiceExtraTotal;

        return new OrderItemDetailDto
        {
            OrderItemId = orderItem.OrderItemId,
            MenuItemId = orderItem.MenuItemId,
            MenuItemNameSnapshot = orderItem.MenuItemNameSnapshot,
            BasePriceSnapshot = orderItem.BasePriceSnapshot,
            ChannelExtraPriceSnapshot = orderItem.ChannelExtraPriceSnapshot,
            FinalUnitPriceSnapshot = orderItem.FinalUnitPriceSnapshot,
            Quantity = orderItem.Quantity,
            Note = orderItem.Note,
            SelectedChoices = orderItem.SelectedChoices.Select(x => new OrderItemSelectedChoiceDto
            {
                ChoiceGroupId = x.ChoiceGroupId,
                ChoiceItemId = x.ChoiceItemId,
                GroupNameSnapshot = x.GroupNameSnapshot,
                ChoiceNameSnapshot = x.ChoiceNameSnapshot,
                ExtraPriceSnapshot = x.ExtraPriceSnapshot,
                ChannelExtraPriceSnapshot = x.ChannelExtraPriceSnapshot,
                FinalExtraPriceSnapshot = x.FinalExtraPriceSnapshot
            }).ToList(),
            LineUnitPrice = lineUnitPrice,
            LineTotal = lineUnitPrice * orderItem.Quantity
        };
    }

    private static string NewOrderCode()
    {
        return $"ORD{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static string? Normalize(string? value)
    {
        string? normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private async Task NotifyCustomerOrderCreatedAsync(int? orderId, CancellationToken cancellationToken)
    {
        if (!orderId.HasValue)
        {
            return;
        }

        Order? order = await _orderRepository.GetOrderByIdAsync(orderId.Value, cancellationToken);
        if (order is null)
        {
            return;
        }

        RealtimeEventDto staffPayload = new()
        {
            TableSessionId = order.TableSessionId,
            OrderId = order.OrderId
        };

        if (!string.IsNullOrWhiteSpace(order.ClientToken))
        {
            await _realtimeNotificationService.NotifyCustomerAsync(
                order.ClientToken,
                RealtimeEvents.CustomerMessageCreated,
                CustomerMessageMapper.MapOrderMessage(order),
                cancellationToken);
        }

        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.CustomerOrderCreated,
            staffPayload,
            cancellationToken);
    }

    private async Task NotifyCustomerOrderStatusChangedAsync(int orderId, CancellationToken cancellationToken)
    {
        Order? order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
        if (order is null || string.IsNullOrWhiteSpace(order.ClientToken))
        {
            return;
        }

        await _realtimeNotificationService.NotifyCustomerAsync(
            order.ClientToken,
            RealtimeEvents.CustomerOrderStatusChanged,
            CustomerMessageMapper.MapOrderMessage(order),
            cancellationToken);
    }

    private async Task NotifyStaffOrderStatusChangedAsync(Order order, CancellationToken cancellationToken)
    {
        RealtimeEventDto staffPayload = new()
        {
            TableSessionId = order.TableSessionId,
            OrderId = order.OrderId
        };

        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.CustomerOrderStatusChanged,
            staffPayload,
            cancellationToken);
    }

    private async Task NotifyBillChangedAsync(
        int? tableSessionId,
        int? orderId,
        int? billId,
        CancellationToken cancellationToken)
    {
        if (!tableSessionId.HasValue)
        {
            return;
        }

        RealtimeEventDto payload = new()
        {
            TableSessionId = tableSessionId.Value,
            OrderId = orderId,
            BillId = billId
        };

        await _realtimeNotificationService.NotifyStaffAsync(RealtimeEvents.BillChanged, payload, cancellationToken);
        await _realtimeNotificationService.NotifySessionAsync(tableSessionId.Value, RealtimeEvents.BillChanged, payload, cancellationToken);
    }

    private sealed record ValidatedChoice(
        ChoiceGroup ChoiceGroup,
        ChoiceItem ChoiceItem,
        decimal ChannelExtraPrice,
        decimal FinalExtraPrice);

    private sealed record ValidatedOrderItem(
        MenuItem MenuItem,
        int Quantity,
        string? Note,
        decimal ChannelExtraPrice,
        decimal FinalUnitPrice,
        List<ValidatedChoice> SelectedChoices);

    private sealed record ValidatedOrderItemResult(
        ValidatedOrderItem? ValidatedItem,
        string ReasonCode,
        string ReasonMessage);
}
