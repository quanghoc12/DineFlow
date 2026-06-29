using System.Globalization;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class MenuItemChannelPriceEditorWindow : Window
{
    public MenuItemChannelPriceEditorWindow(string itemName, decimal basePrice, decimal currentExtraPrice, string channelName)
    {
        InitializeComponent();

        TitleText.Text = $"Món: {itemName}";
        SubtitleText.Text = $"Cấu hình giá phụ thu trên kênh: {channelName}";
        BasePriceText.Text = $"{basePrice:N0} ₫";
        ExtraPriceTextBox.Text = currentExtraPrice.ToString(CultureInfo.InvariantCulture);
    }

    public decimal ResultPrice { get; private set; }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string text = ExtraPriceTextBox.Text.Trim();
        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) || price < 0)
        {
            ErrorText.Text = "Giá trị phụ thu không hợp lệ (phải là số không âm).";
            return;
        }

        ResultPrice = price;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }
}
