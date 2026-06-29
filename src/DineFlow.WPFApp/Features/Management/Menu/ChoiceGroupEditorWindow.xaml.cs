using DineFlow.BusinessObjects.Menu;
using System.Globalization;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class ChoiceGroupEditorWindow : Window
{
    private readonly ManagedChoiceGroupDto? _group;

    public ChoiceGroupEditorWindow(ManagedChoiceGroupDto? group = null)
    {
        InitializeComponent();
        _group = group;

        if (group is null)
        {
            ApplyRequiredState();
            return;
        }

        HeadingText.Text = "Chỉnh sửa nhóm phụ";
        GroupNameTextBox.Text = group.GroupName;
        RequiredCheckBox.IsChecked = group.IsRequired;
        MaxSelectTextBox.Text = group.MaxSelectDefault.ToString(CultureInfo.InvariantCulture);
        ApplyRequiredState();
    }

    public SaveChoiceGroupRequest Request { get; private set; } = new();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string name = GroupNameTextBox.Text.Trim();
        bool isRequired = RequiredCheckBox.IsChecked == true;
        int maxSelect = 1;

        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorText.Text = "Tên nhóm không được để trống.";
            return;
        }

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
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void RequiredCheckBox_Changed(object sender, RoutedEventArgs e) => ApplyRequiredState();

    private void ApplyRequiredState()
    {
        if (RequiredCheckBox.IsChecked == true)
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
}
