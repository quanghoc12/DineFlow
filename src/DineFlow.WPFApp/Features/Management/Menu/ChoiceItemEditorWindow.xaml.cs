using DineFlow.BusinessObjects.Menu;
using System.Globalization;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class ChoiceItemEditorWindow : Window
{
    private readonly ManagedChoiceGroupDto _group;
    private readonly ManagedChoiceItemDto? _item;

    public ChoiceItemEditorWindow(ManagedChoiceGroupDto group, ManagedChoiceItemDto? item = null)
    {
        InitializeComponent();
        _group = group;
        _item = item;
        HintText.Text = $"Nhóm: {group.GroupName}";

        if (item is null) return;

        HeadingText.Text = "Chỉnh sửa lựa chọn";
        ChoiceNameTextBox.Text = item.ChoiceName;
        ExtraPriceTextBox.Text = item.ExtraPrice.ToString(CultureInfo.InvariantCulture);
    }

    public SaveChoiceItemRequest Request { get; private set; } = new();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string name = ChoiceNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name) ||
            !decimal.TryParse(ExtraPriceTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal extraPrice) ||
            extraPrice < 0)
        {
            ErrorText.Text = "Kiểm tra tên lựa chọn và giá cộng thêm. Giá không được âm.";
            return;
        }

        Request = new SaveChoiceItemRequest
        {
            ChoiceItemId = _item?.ChoiceItemId,
            ChoiceGroupId = _group.ChoiceGroupId,
            ChoiceName = name,
            ExtraPrice = extraPrice
        };
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
