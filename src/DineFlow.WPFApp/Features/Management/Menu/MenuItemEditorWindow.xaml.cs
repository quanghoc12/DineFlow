using DineFlow.BusinessObjects.Menu;
using DineFlow.WPFApp.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class MenuItemEditorWindow : Window
{
    private readonly ManagedMenuItemDto? _item;
    private readonly List<ManagedChoiceGroupDto> _choiceGroups;
    private EditableMenuItemChoiceGroup? _editingAssignment;

    // Independent out-of-stock flag — not tied to TrackStock (one-way: turning off TrackStock resets this)
    private bool _isOutOfStock;

    // Stores pending new ChoiceGroup creation requests (to be saved by the caller after dialog closes)
    private readonly List<(SaveChoiceGroupRequest Group, List<SaveChoiceItemRequest> Items)> _pendingNewGroups = [];


    public MenuItemEditorWindow(
        IEnumerable<ManagedCategoryDto> categories,
        IEnumerable<ManagedChoiceGroupDto> choiceGroups,
        ManagedMenuItemDto? item = null)
    {
        InitializeComponent();
        _item = item;
        _choiceGroups = choiceGroups.Where(group => group.IsAvailable).ToList();

        CategoryComboBox.ItemsSource = categories.Where(category => category.CategoryId > 0 && category.IsActive).ToList();
        ChoiceGroupComboBox.ItemsSource = _choiceGroups;

        if (item is null)
        {
            CategoryComboBox.SelectedIndex = 0;
            ChoiceGroupComboBox.SelectedIndex = 0;
            PriceTextBox.Text = "0";
            TrackStockCheckBox.IsChecked = false;
            _isOutOfStock = false;
            ApplyTrackStockState();
            ApplyChoiceGroupDefaults();
            return;
        }

        HeadingText.Text = "Chỉnh sửa món";
        EditActionPanel.Visibility = Visibility.Visible;
        NameTextBox.Text = item.Name;
        CategoryComboBox.SelectedValue = item.CategoryId;
        PriceTextBox.Text = item.BasePrice.ToString(CultureInfo.InvariantCulture);
        TrackStockCheckBox.IsChecked = item.Stock.HasValue;
        StockTextBox.Text = item.Stock?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        ImageTextBox.Text = item.ImageUrl ?? string.Empty;
        DescriptionTextBox.Text = item.Description ?? string.Empty;
        AvailableCheckBox.IsChecked = item.IsAvailable;
        // Load IsOutOfStock: if tracking stock → derive from stock==0; otherwise from stored flag
        _isOutOfStock = item.IsOutOfStock;

        foreach (ManagedMenuItemChoiceGroupDto assignment in item.ChoiceGroups)
        {
            ManagedChoiceGroupDto? group = _choiceGroups.FirstOrDefault(x => x.ChoiceGroupId == assignment.ChoiceGroupId);
            Assignments.Add(new EditableMenuItemChoiceGroup
            {
                ChoiceGroupId = assignment.ChoiceGroupId,
                GroupName = assignment.GroupName,
                IsRequired = assignment.IsRequired,
                MaxSelectDefault = group?.MaxSelectDefault ?? assignment.EffectiveMaxSelect,
                MaxSelect = assignment.MaxSelect,
                DisplayOrder = assignment.DisplayOrder
            });
        }

        ChoiceGroupComboBox.SelectedIndex = 0;
        ApplyTrackStockState();
        ApplyChoiceGroupDefaults();
        SortAssignments();
        RefreshActionButtons();
    }

    public ObservableCollection<EditableMenuItemChoiceGroup> Assignments { get; } = [];
    public SaveMenuItemRequest Request { get; private set; } = new();
    public IReadOnlyList<AssignChoiceGroupRequest> AssignmentRequests { get; private set; } = [];
    public IReadOnlyList<int> RemovedChoiceGroupIds => _removedChoiceGroupIds;
    public bool DeleteRequested { get; private set; }

    // Pending new groups to be saved by MenuManagementView before assigning
    public IReadOnlyList<(SaveChoiceGroupRequest Group, List<SaveChoiceItemRequest> Items)> PendingNewGroups => _pendingNewGroups;


    private readonly List<int> _removedChoiceGroupIds = [];

    private void CreateChoiceGroupButton_Click(object sender, RoutedEventArgs e)
    {
        // Open ChoiceGroupEditorWindow with current known groups + any pending new ones as context
        IEnumerable<ManagedChoiceGroupDto> allKnownGroups = _choiceGroups.Concat(
            _pendingNewGroups.Select(pending => new ManagedChoiceGroupDto
            {
                ChoiceGroupId = -1,
                GroupName = pending.Group.GroupName,
                IsRequired = pending.Group.IsRequired,
                MaxSelectDefault = pending.Group.MaxSelectDefault,
                IsAvailable = true
            }));

        ChoiceGroupEditorWindow dialog = new(allKnownGroups)
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true) return;

        // Add to pending list
        _pendingNewGroups.Add((dialog.Request, dialog.ChoiceRequests.ToList()));

        // Add a temporary dto so it appears in the combobox immediately
        ManagedChoiceGroupDto tempGroup = new()
        {
            ChoiceGroupId = -(100 + _pendingNewGroups.Count), // Negative = pending
            GroupName = dialog.Request.GroupName,
            IsRequired = dialog.Request.IsRequired,
            MaxSelectDefault = dialog.Request.MaxSelectDefault,
            IsAvailable = true
        };
        _choiceGroups.Add(tempGroup);
        ChoiceGroupComboBox.ItemsSource = null;
        ChoiceGroupComboBox.ItemsSource = _choiceGroups;
        ChoiceGroupComboBox.SelectedItem = tempGroup;
    }



    private async void PickImageButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Title = "Chọn ảnh món",
            Filter = "Image files|*.png;*.jpg;*.jpeg;*.webp|All files|*.*"
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        string previousImageUrl = ImageTextBox.Text;
        PickImageButton.IsEnabled = false;
        ImageUploadStatusText.Text = "Đang tải ảnh...";
        ErrorText.Text = string.Empty;

        try
        {
            using StaffOrderApiClient apiClient = new();
            string imageUrl = await apiClient.UploadMenuImageAsync(dialog.FileName);
            ImageTextBox.Text = imageUrl;
            ImageUploadStatusText.Text = "Đã tải ảnh.";
        }
        catch (Exception ex)
        {
            ImageTextBox.Text = previousImageUrl;
            ImageUploadStatusText.Text = string.Empty;
            ErrorText.Text = string.IsNullOrWhiteSpace(ex.Message)
                ? "Không thể tải ảnh lên. Vui lòng thử lại."
                : ex.Message;
        }
        finally
        {
            PickImageButton.IsEnabled = true;
        }
    }

    private void TrackStockCheckBox_Changed(object sender, RoutedEventArgs e) => ApplyTrackStockState();

    private void ApplyTrackStockState()
    {
        if (StockTextBox is null) return;
        bool isTracking = TrackStockCheckBox.IsChecked == true;
        StockTextBox.IsEnabled = isTracking;
        if (!isTracking)
        {
            StockTextBox.Clear();
            // One-way link: turning off TrackStock resets the derived stock-count out-of-stock,
            // but the independent _isOutOfStock flag is preserved (user may still want to mark OOS manually).
        }
        else if (string.IsNullOrWhiteSpace(StockTextBox.Text))
        {
            StockTextBox.Text = "0";
        }
        RefreshActionButtons();
    }

    private void ToggleActivityButton_Click(object sender, RoutedEventArgs e)
    {
        AvailableCheckBox.IsChecked = AvailableCheckBox.IsChecked != true;
        RefreshActionButtons();
    }

    private void ToggleOutOfStockButton_Click(object sender, RoutedEventArgs e)
    {
        bool isTracking = TrackStockCheckBox.IsChecked == true;
        if (isTracking)
        {
            // When tracking stock: toggle between stock=0 (hết món) and stock=1 (mở bán).
            // One-way link: stock==0 → _isOutOfStock. Toggling off sets stock=1 + clears flag.
            bool stockIsZero = int.TryParse(StockTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int s) && s == 0;
            if (stockIsZero)
            {
                StockTextBox.Text = "1";
                _isOutOfStock = false;
            }
            else
            {
                StockTextBox.Text = "0";
                _isOutOfStock = true;
            }
        }
        else
        {
            // NOT tracking stock: toggle the independent _isOutOfStock flag only.
            // TrackStock stays off — no forced enable.
            _isOutOfStock = !_isOutOfStock;
        }
        RefreshActionButtons();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_item is null) return;
        MessageBoxResult result = MessageBox.Show(
            "Xóa mềm món này? Món sẽ không còn hiển thị trong thực đơn và menu khách.",
            "Xóa món",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        DeleteRequested = true;
        DialogResult = true;
    }

    private void RefreshActionButtons()
    {
        if (ToggleActivityButton is null || ToggleOutOfStockButton is null) return;
        ToggleActivityButton.Content = AvailableCheckBox.IsChecked == true ? "Tạm ngưng bán" : "Mở hoạt động";

        // Effective out-of-stock: if tracking stock AND stock==0 → always OOS regardless of _isOutOfStock.
        // If not tracking: rely on _isOutOfStock flag alone.
        bool trackingAndEmpty = TrackStockCheckBox.IsChecked == true &&
                                int.TryParse(StockTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int s) &&
                                s == 0;
        bool effectiveOos = trackingAndEmpty || _isOutOfStock;
        ToggleOutOfStockButton.Content = effectiveOos ? "Mở bán" : "Hết món";
    }

    private void ChoiceGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyChoiceGroupDefaults();

    private void ApplyChoiceGroupDefaults()
    {
        if (ChoiceGroupComboBox?.SelectedItem is not ManagedChoiceGroupDto group ||
            AssignMaxTextBox is null ||
            AssignOrderTextBox is null) return;

        AssignMaxTextBox.Text = group.IsRequired ? "1" : group.MaxSelectDefault.ToString(CultureInfo.InvariantCulture);
        AssignMaxTextBox.IsEnabled = !group.IsRequired;
        if (_editingAssignment is null)
        {
            AssignOrderTextBox.Text = Assignments.Count.ToString(CultureInfo.InvariantCulture);
        }
    }

    private void SortAssignments()
    {
        var sorted = Assignments.OrderBy(x => x.DisplayOrder).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            int oldIndex = Assignments.IndexOf(sorted[i]);
            if (oldIndex != i)
            {
                Assignments.Move(oldIndex, i);
            }
        }
        AssignedGroupsGrid.Items.Refresh();
    }

    private void AddOrUpdateAssignment_Click(object sender, RoutedEventArgs e)
    {
        if (ChoiceGroupComboBox.SelectedItem is not ManagedChoiceGroupDto group)
        {
            ErrorText.Text = "Vui lòng chọn nhóm phụ.";
            return;
        }
        if (!int.TryParse(AssignOrderTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int displayOrder) ||
            displayOrder < 0)
        {
            ErrorText.Text = "Thứ tự nhóm phụ không hợp lệ.";
            return;
        }

        // Limit display order based on item count
        int maxAllowedOrder = _editingAssignment == null ? Assignments.Count : Assignments.Count - 1;
        if (displayOrder > maxAllowedOrder)
        {
            ErrorText.Text = $"Thứ tự không được lớn hơn {maxAllowedOrder} (danh sách có {Assignments.Count} nhóm phụ, bắt đầu từ 0).";
            return;
        }

        int? maxSelect = null;
        if (!group.IsRequired)
        {
            if (!int.TryParse(AssignMaxTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedMax) ||
                parsedMax < 1)
            {
                ErrorText.Text = "MaxSelect phải lớn hơn hoặc bằng 1.";
                return;
            }
            maxSelect = parsedMax == group.MaxSelectDefault ? null : parsedMax;
        }

        EditableMenuItemChoiceGroup? existing = Assignments.FirstOrDefault(x => x.ChoiceGroupId == group.ChoiceGroupId);
        if (existing is not null && !ReferenceEquals(existing, _editingAssignment))
        {
            ErrorText.Text = "Món đã có nhóm phụ này.";
            return;
        }

        if (_editingAssignment is null)
        {
            // Shift down assignments whose order >= the new order to avoid duplicates
            foreach (EditableMenuItemChoiceGroup a in Assignments
                         .Where(x => x.DisplayOrder >= displayOrder)
                         .OrderByDescending(x => x.DisplayOrder))
            {
                a.DisplayOrder++;
            }

            Assignments.Add(new EditableMenuItemChoiceGroup
            {
                ChoiceGroupId = group.ChoiceGroupId,
                GroupName = group.GroupName,
                IsRequired = group.IsRequired,
                MaxSelectDefault = group.MaxSelectDefault,
                MaxSelect = group.IsRequired ? null : maxSelect,
                DisplayOrder = displayOrder
            });
        }
        else
        {
            int oldOrder = _editingAssignment.DisplayOrder;
            if (oldOrder != displayOrder)
            {
                // Reorder: shift items between old and new positions
                foreach (EditableMenuItemChoiceGroup a in Assignments.Where(x => !ReferenceEquals(x, _editingAssignment)))
                {
                    if (displayOrder < oldOrder && a.DisplayOrder >= displayOrder && a.DisplayOrder < oldOrder)
                        a.DisplayOrder++;
                    else if (displayOrder > oldOrder && a.DisplayOrder > oldOrder && a.DisplayOrder <= displayOrder)
                        a.DisplayOrder--;
                }
            }
            _editingAssignment.ChoiceGroupId = group.ChoiceGroupId;
            _editingAssignment.GroupName = group.GroupName;
            _editingAssignment.IsRequired = group.IsRequired;
            _editingAssignment.MaxSelectDefault = group.MaxSelectDefault;
            _editingAssignment.MaxSelect = group.IsRequired ? null : maxSelect;
            _editingAssignment.DisplayOrder = displayOrder;
        }

        SortAssignments();
        ErrorText.Text = string.Empty;
        ResetAssignmentForm();
    }

    private void EditAssignment_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not EditableMenuItemChoiceGroup assignment) return;
        _editingAssignment = assignment;
        ChoiceGroupComboBox.SelectedItem = _choiceGroups.FirstOrDefault(group => group.ChoiceGroupId == assignment.ChoiceGroupId);
        AssignMaxTextBox.Text = assignment.EffectiveMaxSelect.ToString(CultureInfo.InvariantCulture);
        AssignMaxTextBox.IsEnabled = !assignment.IsRequired;
        AssignOrderTextBox.Text = assignment.DisplayOrder.ToString(CultureInfo.InvariantCulture);
        AddOrUpdateAssignmentButton.Content = "Sửa nhóm";
    }

    private void RemoveAssignment_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not EditableMenuItemChoiceGroup assignment) return;
        int removedOrder = assignment.DisplayOrder;
        Assignments.Remove(assignment);
        if (_item?.ChoiceGroups.Any(group => group.ChoiceGroupId == assignment.ChoiceGroupId) == true &&
            !_removedChoiceGroupIds.Contains(assignment.ChoiceGroupId))
        {
            _removedChoiceGroupIds.Add(assignment.ChoiceGroupId);
        }

        // Decrement order for items that had higher order than removed item
        foreach (EditableMenuItemChoiceGroup a in Assignments.Where(x => x.DisplayOrder > removedOrder))
        {
            a.DisplayOrder--;
        }

        SortAssignments();
        ResetAssignmentForm();
    }

    private void ResetAssignmentForm()
    {
        _editingAssignment = null;
        ChoiceGroupComboBox.SelectedIndex = ChoiceGroupComboBox.Items.Count > 0 ? 0 : -1;
        ApplyChoiceGroupDefaults();
        AddOrUpdateAssignmentButton.Content = "Thêm nhóm";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (CategoryComboBox.SelectedValue is not int categoryId ||
            string.IsNullOrWhiteSpace(NameTextBox.Text) ||
            !decimal.TryParse(PriceTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) ||
            price < 0)
        {
            ErrorText.Text = "Kiểm tra tên món, danh mục và giá bán.";
            return;
        }

        int? stock = null;
        if (TrackStockCheckBox.IsChecked == true)
        {
            if (!int.TryParse(StockTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedStock) ||
                parsedStock < 0)
            {
                ErrorText.Text = "Tồn kho phải là số nguyên không âm.";
                return;
            }
            stock = parsedStock;
        }

        // Effective out-of-stock: tracking stock with stock==0 is always OOS (one-way link).
        bool trackingAndEmpty = stock == 0;
        bool effectiveOos = (TrackStockCheckBox.IsChecked == true && trackingAndEmpty) || _isOutOfStock;

        Request = new SaveMenuItemRequest
        {
            MenuItemId = _item?.MenuItemId,
            CategoryId = categoryId,
            Name = NameTextBox.Text,
            Description = DescriptionTextBox.Text,
            BasePrice = price,
            Stock = stock,
            IsOutOfStock = effectiveOos,
            ImageUrl = ImageTextBox.Text,
            IsAvailable = AvailableCheckBox.IsChecked == true
        };
        AssignmentRequests = Assignments.Select(assignment => new AssignChoiceGroupRequest
        {
            MenuItemId = _item?.MenuItemId ?? 0,
            ChoiceGroupId = assignment.ChoiceGroupId,
            DisplayOrder = assignment.DisplayOrder,
            MaxSelect = assignment.IsRequired ? null : assignment.MaxSelect
        }).ToList();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }
}

public sealed class EditableMenuItemChoiceGroup
{
    public int ChoiceGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string GroupKindLabel => IsRequired ? "Loại" : "Lựa chọn";
    public int MaxSelectDefault { get; set; } = 1;
    public int? MaxSelect { get; set; }
    public int EffectiveMaxSelect => MaxSelect ?? MaxSelectDefault;
    public int DisplayOrder { get; set; }
}
