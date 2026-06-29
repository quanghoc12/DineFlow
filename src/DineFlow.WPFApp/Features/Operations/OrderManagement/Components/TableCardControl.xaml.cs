using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement.Components;

public partial class TableCardControl : UserControl
{
    public TableCardControl()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler? Click;

    private void RootButton_Click(object sender, RoutedEventArgs e)
    {
        Click?.Invoke(this, e);
    }
}
