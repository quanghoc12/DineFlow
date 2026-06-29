using System.Globalization;
using System.Windows.Data;
using DineFlow.BusinessObjects.Menu;

namespace DineFlow.WPFApp.Features.Management.Menu;

public class ChannelPriceConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not IEnumerable<ManagedChannelPriceDto> prices ||
            values[1] is not ManagedSalesChannelDto channel)
        {
            return "0 ₫";
        }

        ManagedChannelPriceDto? match = prices.FirstOrDefault(p => p.SalesChannelId == channel.SalesChannelId);
        decimal price = match?.ChannelExtraPrice ?? 0m;
        return $"{price:N0} ₫";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
