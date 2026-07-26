using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Requests;

namespace DineFlow.Services.CustomerSessions;

public static class CustomerMessageMapper
{
    public static CustomerMessageDto MapOrderMessage(Order order)
    {
        return new CustomerMessageDto
        {
            MessageType = "Order",
            SourceId = order.OrderId,
            Title = $"Gọi món {FormatVietnamTime(order.CreatedAt)}",
            Status = order.Status,
            Message = order.Status == "Cancelled"
                ? order.CancelReason ?? order.SystemNote ?? order.CustomerNote
                : order.CustomerNote,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(item =>
            {
                decimal choiceExtra = item.SelectedChoices.Sum(choice => choice.FinalExtraPriceSnapshot);
                return new CustomerMessageItemDto
                {
                    Name = item.MenuItemNameSnapshot,
                    Quantity = item.Quantity,
                    Note = item.Note,
                    LineTotal = (item.FinalUnitPriceSnapshot + choiceExtra) * item.Quantity,
                    Choices = item.SelectedChoices
                        .Select(choice => $"{choice.GroupNameSnapshot}: {choice.ChoiceNameSnapshot}")
                        .ToList()
                };
            }).ToList()
        };
    }

    public static CustomerMessageDto MapRequestMessage(ServiceRequest request)
    {
        string title = request.RequestType == "PaymentRequest" ? "Gọi thanh toán" : "Gọi nhân viên";
        return new CustomerMessageDto
        {
            MessageType = "ServiceRequest",
            SourceId = request.RequestId,
            Title = $"{title} {FormatVietnamTime(request.CreatedAt)}",
            Status = request.Status,
            Message = request.Message ?? request.Reason ?? request.PaymentMethod,
            CreatedAt = request.CreatedAt
        };
    }

    private static string FormatVietnamTime(DateTime value)
    {
        DateTime utc = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        TimeZoneInfo zone = FindVietnamTimeZone();
        return TimeZoneInfo.ConvertTimeFromUtc(utc, zone).ToString("HH:mm");
    }

    private static TimeZoneInfo FindVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
    }
}
