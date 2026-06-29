using DineFlow.BusinessObjects.Menu;
using System.Globalization;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class CategoryEditorWindow : Window
{
    private readonly ManagedCategoryDto? _category;
    private readonly List<ManagedCategoryDto> _existingCategories;

    public CategoryEditorWindow(IEnumerable<ManagedCategoryDto> existingCategories, ManagedCategoryDto? category = null)
    {
        InitializeComponent();
        _category = category;
        // Filter out the default "Tất cả danh mục" if present (which usually has CategoryId = 0)
        _existingCategories = existingCategories.Where(x => x.CategoryId > 0).ToList();

        if (category is null)
        {
            OrderTextBox.Text = _existingCategories.Count.ToString(CultureInfo.InvariantCulture);
            return;
        }

        HeadingText.Text = "Chỉnh sửa danh mục";
        DeleteButton.Visibility = Visibility.Visible;

        NameTextBox.Text = category.CategoryName;
        DescriptionTextBox.Text = category.Description ?? string.Empty;
        OrderTextBox.Text = category.DisplayOrder.ToString(CultureInfo.InvariantCulture);
        IsActiveCheckBox.IsChecked = category.IsActive;
    }

    public SaveCategoryRequest Request { get; private set; } = new();
    public bool DeleteRequested { get; private set; }
    public bool ToggleActiveRequested { get; private set; }
    public bool TargetActiveState { get; private set; }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string name = NameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorText.Text = "Tên danh mục không được để trống.";
            return;
        }

        if (!int.TryParse(OrderTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int displayOrder) ||
            displayOrder < 0)
        {
            ErrorText.Text = "Thứ tự hiển thị không hợp lệ.";
            return;
        }

        // Validate display order max limit
        int maxAllowedOrder = _category == null ? _existingCategories.Count : _existingCategories.Count - 1;
        if (displayOrder > maxAllowedOrder)
        {
            ErrorText.Text = $"Thứ tự không được vượt quá {maxAllowedOrder} (danh sách có {_existingCategories.Count} danh mục, bắt đầu từ 0).";
            return;
        }

        // Check duplicated name
        bool isDuplicated = _existingCategories.Any(x =>
            x.CategoryId != (_category?.CategoryId ?? 0) &&
            x.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (isDuplicated)
        {
            ErrorText.Text = "Tên danh mục đã tồn tại.";
            return;
        }

        Request = new SaveCategoryRequest
        {
            CategoryId = _category?.CategoryId,
            CategoryName = name,
            Description = DescriptionTextBox.Text.Trim(),
            DisplayOrder = displayOrder
        };

        // If the active state changed, we also trigger toggle requested
        if (_category != null && IsActiveCheckBox.IsChecked != _category.IsActive)
        {
            ToggleActiveRequested = true;
            TargetActiveState = IsActiveCheckBox.IsChecked == true;
        }
        else if (_category == null)
        {
            // For new category, active defaults to true. If unchecked, we can handle it later or default it.
            // (The SaveCategoryRequest itself doesn't have IsActive, but we can set it via service later if needed,
            // however the service default is active=true, which is fine).
        }

        DialogResult = true;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_category is null) return;
        MessageBoxResult result = MessageBox.Show(
            $"Bạn chắc chắn muốn xóa danh mục '{_category.CategoryName}'? Hành động này không thể hoàn tác.",
            "Xóa danh mục",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        DeleteRequested = true;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }
}
