using DineFlow.BusinessObjects.Tables;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Tables;

public partial class TableEditorWindow : Window
{
    public TableEditorWindow(ManagedTableDto? table = null)
    {
        InitializeComponent();
        if (table is null) return;
        HeadingText.Text = "Chỉnh sửa bàn";
        TableNameTextBox.Text = table.TableName;
        AreaTextBox.Text = table.Area;
    }

    public string TableNameValue => TableNameTextBox.Text;
    public string AreaValue => AreaTextBox.Text;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TableNameValue) || string.IsNullOrWhiteSpace(AreaValue))
        {
            ErrorText.Text = "Tên bàn và khu vực không được để trống.";
            return;
        }
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
