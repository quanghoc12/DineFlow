using DineFlow.BusinessObjects.Tables;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Tables;

public partial class AreaEditorWindow : Window
{
    public AreaEditorWindow(ManagedAreaDto? area = null)
    {
        InitializeComponent();
        if (area is null) return;
        HeadingText.Text = "Chỉnh sửa khu vực";
        NameBox.Text = area.AreaName;
        OrderBox.Text = area.DisplayOrder.ToString();
    }

    public string AreaNameValue => NameBox.Text;
    public int DisplayOrderValue { get; private set; }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AreaNameValue))
        {
            ErrorText.Text = "Tên khu vực không được để trống.";
            return;
        }
        if (!int.TryParse(OrderBox.Text, out int order) || order < 0)
        {
            ErrorText.Text = "Thứ tự phải là số không âm.";
            return;
        }
        DisplayOrderValue = order;
        DialogResult = true;
    }
}
