using System.Globalization;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

internal static class OrderManagementFormatting
{
    public static string Money(decimal value)
    {
        return value.ToString("#,0", CultureInfo.GetCultureInfo("vi-VN"));
    }
}
