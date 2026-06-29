using DineFlow.BusinessObjects.Menu;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class ChoiceGroupEditorWindow : Window
{
    private readonly ManagedChoiceGroupDto? _group;
    private readonly IReadOnlyList<ManagedChoiceGroupDto> _existingGroups;
    private EditableChoiceItem? _editingChoiceItem;

    public ChoiceGroupEditorWindow(IEnumerable<ManagedChoiceGroupDto> existingGroups, ManagedChoiceGroupDto? group = null)
    {
        InitializeComponent();
        _group = group;
        _existingGroups = existingGroups.ToList();

        foreach (EditableChoiceItem item in group?.Items.Select(item => new EditableChoiceItem
            {
                ChoiceItemId = item.ChoiceItemId,
                ChoiceName = item.ChoiceName,
                ExtraPrice = item.ExtraPrice,
                IsAvailable = item.IsAvailable
            }) ?? [])
        {
            ChoiceItems.Add(item);
        }

        if (group is null)
        {
            ApplyGroupKindState();
            return;
        }

        HeadingText.Text = "Chỉnh sửa nhóm phụ";
        GroupNameTextBox.Text = group.GroupName;
        TypeRadioButton.IsChecked = group.IsRequired;
        OptionRadioButton.IsChecked = !group.IsRequired;
        MaxSelectTextBox.Text = group.MaxSelectDefault.ToString(CultureInfo.InvariantCulture);
        ApplyGroupKindState();
    }

    public ObservableCollection<EditableChoiceItem> ChoiceItems { get; } = [];
    public EditableChoiceItem? SelectedChoiceItem { get; set; }
    public SaveChoiceGroupRequest Request { get; private set; } = new();
    public IReadOnlyList<SaveChoiceItemRequest> ChoiceRequests { get; private set; } = [];

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string name = GroupNameTextBox.Text.Trim();
        bool isRequired = TypeRadioButton.IsChecked == true;
        int maxSelect = 1;

        if (!ValidateGroupName()) return;

        if (!isRequired &&
            (!int.TryParse(MaxSelectTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out maxSelect) ||
             maxSelect < 1))
        {
            ErrorText.Text = "MaxSelect mặc định phải là số nguyên lớn hơn hoặc bằng 1.";
            return;
        }

        Request = new SaveChoiceGroupRequest
        {
            ChoiceGroupId = _group?.ChoiceGroupId,
            GroupName = name,
            IsRequired = isRequired,
            MaxSelectDefault = isRequired ? 1 : maxSelect
        };
        ChoiceRequests = ChoiceItems.Select(item => new SaveChoiceItemRequest
        {
            ChoiceItemId = item.ChoiceItemId,
            ChoiceGroupId = _group?.ChoiceGroupId ?? 0,
            ChoiceName = item.ChoiceName,
            ExtraPrice = item.ExtraPrice
        }).ToList();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void GroupNameTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ValidateGroupName();
    }

    private bool ValidateGroupName()
    {
        string name = GroupNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            GroupNameValidationText.Text = "Tên nhóm không được để trống.";
            return false;
        }

        bool duplicated = _existingGroups.Any(group =>
            group.ChoiceGroupId != (_group?.ChoiceGroupId ?? 0) &&
            group.GroupName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (duplicated)
        {
            GroupNameValidationText.Text = "Tên nhóm đã tồn tại. Vui lòng chọn tên khác trước khi lưu.";
            return false;
        }

        GroupNameValidationText.Text = string.Empty;
        return true;
    }

    private void GroupKind_Changed(object sender, RoutedEventArgs e) => ApplyGroupKindState();

    private void ApplyGroupKindState()
    {
        if (MaxSelectTextBox is null)
        {
            return;
        }

        if (TypeRadioButton.IsChecked == true)
        {
            MaxSelectTextBox.Text = "1";
            MaxSelectTextBox.IsEnabled = false;
        }
        else
        {
            MaxSelectTextBox.IsEnabled = true;
            if (string.IsNullOrWhiteSpace(MaxSelectTextBox.Text)) MaxSelectTextBox.Text = "1";
        }
    }

    private void AddOrUpdateChoice_Click(object sender, RoutedEventArgs e)
    {
        string name = ChoiceNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name) ||
            !decimal.TryParse(ExtraPriceTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal extraPrice) ||
            extraPrice < 0)
        {
            ErrorText.Text = "Kiểm tra tên lựa chọn và giá cộng thêm. Giá không được âm.";
            return;
        }

        bool duplicated = ChoiceItems.Any(item =>
            !ReferenceEquals(item, _editingChoiceItem) &&
            item.ChoiceName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (duplicated)
        {
            ErrorText.Text = "Tên lựa chọn đã tồn tại trong nhóm.";
            return;
        }

        if (_editingChoiceItem is null)
        {
            ChoiceItems.Add(new EditableChoiceItem
            {
                ChoiceName = name,
                ExtraPrice = extraPrice,
                IsAvailable = true
            });
        }
        else
        {
            _editingChoiceItem.ChoiceName = name;
            _editingChoiceItem.ExtraPrice = extraPrice;
            ChoiceItemsGrid.Items.Refresh();
        }

        ErrorText.Text = string.Empty;
        ResetChoiceForm();
    }

    private void EditChoice_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not EditableChoiceItem item) return;
        _editingChoiceItem = item;
        ChoiceFormTitle.Text = $"Sửa lựa chọn: {item.ChoiceName}";
        AddOrUpdateChoiceButton.Content = "Cập nhật";
        ChoiceNameTextBox.Text = item.ChoiceName;
        ExtraPriceTextBox.Text = item.ExtraPrice.ToString(CultureInfo.InvariantCulture);
        ChoiceNameTextBox.Focus();
    }

    private void ResetChoiceForm_Click(object sender, RoutedEventArgs e) => ResetChoiceForm();

    private void ResetChoiceForm()
    {
        _editingChoiceItem = null;
        ChoiceFormTitle.Text = "Thêm lựa chọn";
        AddOrUpdateChoiceButton.Content = "Thêm";
        ChoiceNameTextBox.Clear();
        ExtraPriceTextBox.Text = "0";
    }
}

public sealed class EditableChoiceItem
{
    public int? ChoiceItemId { get; set; }
    public string ChoiceName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public bool IsAvailable { get; set; }
}
