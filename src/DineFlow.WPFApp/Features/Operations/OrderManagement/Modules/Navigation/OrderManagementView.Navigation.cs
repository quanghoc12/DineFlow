using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DineFlow.Services.Bills;
using DineFlow.Services.Menu;
using DineFlow.Services.Orders;
using DineFlow.Services.Requests;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{
    private void RoomTabButton_Click(object sender, RoutedEventArgs e)
    {
        RoomPanel.Visibility = Visibility.Visible;
        MenuPanel.Visibility = Visibility.Collapsed;
        PendingOrdersPanel.Visibility = Visibility.Collapsed;
        RequestsPanel.Visibility = Visibility.Collapsed;
        RoomTabButton.Tag = "Active";
        MenuTabButton.Tag = null;
        PendingOrdersTabButton.Tag = null;
        RequestsTabButton.Tag = null;
        SearchBox.Text = string.Empty;
        SetSearchContext("Tìm kiếm bàn...");
    }

    private void MenuTabButton_Click(object sender, RoutedEventArgs e)
    {
        RoomPanel.Visibility = Visibility.Collapsed;
        MenuPanel.Visibility = Visibility.Visible;
        PendingOrdersPanel.Visibility = Visibility.Collapsed;
        RequestsPanel.Visibility = Visibility.Collapsed;
        RoomTabButton.Tag = null;
        MenuTabButton.Tag = "Active";
        PendingOrdersTabButton.Tag = null;
        RequestsTabButton.Tag = null;
        SearchBox.Text = string.Empty;
        SetSearchContext("Tìm kiếm món...");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (MenuPanel.Visibility == Visibility.Visible)
        {
            ApplyMenuFilters();
        }
        else if (RoomPanel.Visibility == Visibility.Visible)
        {
            ApplyTableFilters();
        }
        else if (PendingOrdersPanel.Visibility == Visibility.Visible)
        {
            ApplyPendingOrderFilter();
        }
        else if (RequestsPanel.Visibility == Visibility.Visible)
        {
            ApplyServiceRequestFilter();
        }
    }

    private void SetSearchContext(string placeholder)
    {
        SearchPlaceholder.Text = placeholder;
        SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

}
