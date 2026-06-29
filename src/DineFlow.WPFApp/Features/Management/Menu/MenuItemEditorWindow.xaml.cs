using DineFlow.BusinessObjects.Menu;
using System.Globalization;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class MenuItemEditorWindow : Window
{
    private readonly ManagedMenuItemDto? _item;

    public MenuItemEditorWindow(IEnumerable<ManagedCategoryDto> categories, ManagedMenuItemDto? item = null)
    {
        InitializeComponent();
        _item = item;
        CategoryComboBox.ItemsSource = categories.ToList();
        if (item is null)
        {
            CategoryComboBox.SelectedIndex = 0;
            return;
        }
        HeadingText.Text = "Chỉnh sửa món";
        NameTextBox.Text = item.Name;
        CategoryComboBox.SelectedValue = item.CategoryId;
        PriceTextBox.Text = item.BasePrice.ToString(CultureInfo.InvariantCulture);
        StockTextBox.Text = item.Stock?.ToString() ?? string.Empty;
        ImageTextBox.Text = item.ImageUrl ?? string.Empty;
        DescriptionTextBox.Text = item.Description ?? string.Empty;
        AvailableCheckBox.IsChecked = item.IsAvailable;
    }

    public SaveMenuItemRequest Request { get; private set; } = new();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (CategoryComboBox.SelectedValue is not int categoryId ||
            string.IsNullOrWhiteSpace(NameTextBox.Text) ||
            !decimal.TryParse(PriceTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) ||
            (!string.IsNullOrWhiteSpace(StockTextBox.Text) && !int.TryParse(StockTextBox.Text, out _)))
        {
            ErrorText.Text = "Kiểm tra tên món, danh mục, giá và tồn kho.";
            return;
        }
        int? stock = string.IsNullOrWhiteSpace(StockTextBox.Text) ? null : int.Parse(StockTextBox.Text);
        Request = new SaveMenuItemRequest
        {
            MenuItemId = _item?.MenuItemId,
            CategoryId = categoryId,
            Name = NameTextBox.Text,
            Description = DescriptionTextBox.Text,
            BasePrice = price,
            Stock = stock,
            ImageUrl = ImageTextBox.Text,
            IsAvailable = AvailableCheckBox.IsChecked == true
        };
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
