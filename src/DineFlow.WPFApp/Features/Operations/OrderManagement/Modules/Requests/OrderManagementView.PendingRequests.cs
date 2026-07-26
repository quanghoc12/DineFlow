using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DineFlow.Services.Bills;
using DineFlow.Services.Orders;
using DineFlow.Services.Requests;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{
    private async Task LoadPendingOrdersAsync()
    {
        await _pendingOrdersReloadLock.WaitAsync();
        try
        {
            if (!_usesApiData)
            {
                _allPendingOrders.Clear();
                PendingOrders.Clear();
                UpdateNotificationBadges();
                return;
            }

            IReadOnlyList<OrderSummaryDto> summaries = await _apiClient.GetPendingOrdersAsync();
            IReadOnlyList<DiningTableDto> tables = await _apiClient.GetTablesAsync();

            _allPendingOrders.Clear();

            foreach (OrderSummaryDto summary in summaries
                .GroupBy(x => x.OrderId)
                .Select(group => group.First())
                .OrderBy(x => x.CreatedAt))
            {
                OrderDetailDto? detail = await _apiClient.GetOrderAsync(summary.OrderId);
                if (detail is null)
                {
                    continue;
                }

                DiningTableDto? table = tables.FirstOrDefault(x => x.CurrentTableSessionId == detail.TableSessionId);
                string tableName = detail.TableName
                    ?? table?.TableName
                    ?? $"Bàn #{detail.TableSessionId}";
                _allPendingOrders.Add(new PendingOrderCard(detail, tableName));
            }

            ApplyPendingOrderFilter();
            UpdateNotificationBadges();
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Đơn chờ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            _pendingOrdersReloadLock.Release();
        }
    }

    private async Task LoadServiceRequestsAsync()
    {
        await _serviceRequestsReloadLock.WaitAsync();
        try
        {
            if (!_usesApiData)
            {
                _allServiceRequests.Clear();
                ServiceRequests.Clear();
                UpdateNotificationBadges();
                return;
            }

            IReadOnlyList<ServiceRequestDto> requests = await _apiClient.GetServiceRequestsAsync();
            _allServiceRequests.Clear();

            foreach (ServiceRequestDto request in requests
                .GroupBy(x => x.RequestId)
                .Select(group => group.First())
                .OrderBy(x => x.CreatedAt))
            {
                string tableName = _tables
                    .FirstOrDefault(x => x.TableSessionId == request.TableSessionId)?.TableName
                    ?? $"Session {request.TableSessionId}";

                _allServiceRequests.Add(new ServiceRequestCard(request, tableName));
            }

            ApplyServiceRequestFilter();
            UpdateNotificationBadges();
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Yêu cầu", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            _serviceRequestsReloadLock.Release();
        }
    }

    private void UpdateNotificationBadges()
    {
        UpdateBadge(PendingOrdersBadge, PendingOrdersBadgeText, _allPendingOrders.Count);
        UpdateBadge(ServiceRequestsBadge, ServiceRequestsBadgeText, _allServiceRequests.Count);
        SidebarNotificationCountChanged?.Invoke(_allPendingOrders.Count + _allServiceRequests.Count);
    }

    private static void UpdateBadge(Border badge, TextBlock text, int count)
    {
        badge.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        text.Text = count > 99 ? "99+" : count.ToString(CultureInfo.InvariantCulture);
    }

    private async void PendingOrdersTabButton_Click(object sender, RoutedEventArgs e)
    {
        RoomPanel.Visibility = Visibility.Collapsed;
        MenuPanel.Visibility = Visibility.Collapsed;
        PendingOrdersPanel.Visibility = Visibility.Visible;
        RequestsPanel.Visibility = Visibility.Collapsed;
        RoomTabButton.Tag = null;
        MenuTabButton.Tag = null;
        PendingOrdersTabButton.Tag = "Active";
        RequestsTabButton.Tag = null;
        SearchBox.Text = string.Empty;
        SetSearchContext("Tìm kiếm đơn chờ...");
        await LoadPendingOrdersAsync();
    }

    private async void RefreshPendingOrdersButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadPendingOrdersAsync();
    }

    private async void PendingOrderHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        Window dialog = new()
        {
            Title = "Lịch sử đơn chờ",
            Owner = Window.GetWindow(this),
            Width = 650,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent
        };

        Border mainBorder = new()
        {
            Background = System.Windows.Media.Brushes.White,
            BorderBrush = GetBrush("#0866E5"),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(20)
        };

        Grid root = new();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        TextBlock titleText = new()
        {
            Text = "Lịch sử đơn chờ",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A"),
            VerticalAlignment = VerticalAlignment.Center
        };
        root.Children.Add(titleText);

        StackPanel filterPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 16, 0, 12),
            VerticalAlignment = VerticalAlignment.Center
        };

        filterPanel.Children.Add(new TextBlock
        {
            Text = "Chọn ngày:",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = GetBrush("#475569"),
            VerticalAlignment = VerticalAlignment.Center
        });

        DatePicker datePicker = new()
        {
            SelectedDate = DateTime.Today,
            Width = 150,
            Height = 32,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };
        filterPanel.Children.Add(datePicker);
        Grid.SetRow(filterPanel, 1);
        root.Children.Add(filterPanel);

        ScrollViewer scroll = new()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 4, 0, 16)
        };
        StackPanel itemsPanel = new();
        scroll.Content = itemsPanel;
        Grid.SetRow(scroll, 2);
        root.Children.Add(scroll);

        async Task LoadHistoryAsync(DateTime? date)
        {
            itemsPanel.Children.Clear();
            itemsPanel.Children.Add(new TextBlock
            {
                Text = "Đang tải dữ liệu...",
                Foreground = GetBrush("#64748B"),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            });

            try
            {
                DateTime? from = date?.Date;
                DateTime? to = date?.Date.AddDays(1).AddTicks(-1);

                var histories = await _apiClient.GetOrdersAsync(from, to);

                itemsPanel.Children.Clear();

                if (histories.Count == 0)
                {
                    itemsPanel.Children.Add(new TextBlock
                    {
                        Text = "Không có lịch sử đơn chờ trong ngày này.",
                        Foreground = GetBrush("#64748B"),
                        FontSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 40, 0, 0)
                    });
                    return;
                }

                foreach (var order in histories.OrderByDescending(x => x.OrderId))
                {
                    var table = _tables.FirstOrDefault(x => x.TableSessionId == order.TableSessionId);
                    string tableName = table?.TableName ?? $"Bàn {order.TableSessionId}";

                    Border card = new()
                    {
                        Background = System.Windows.Media.Brushes.White,
                        BorderBrush = GetBrush("#E2E8F0"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(14, 12, 14, 12),
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    Grid cardGrid = new();
                    cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
                    cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    StackPanel col0 = new() { VerticalAlignment = VerticalAlignment.Center };
                    col0.Children.Add(new TextBlock
                    {
                        Text = tableName,
                        FontWeight = FontWeights.Bold,
                        FontSize = 14,
                        Foreground = GetBrush("#0F172A")
                    });
                    col0.Children.Add(new TextBlock
                    {
                        Text = order.OrderCode,
                        FontSize = 11,
                        Foreground = GetBrush("#64748B"),
                        Margin = new Thickness(0, 2, 0, 0)
                    });
                    cardGrid.Children.Add(col0);

                    StackPanel col1 = new() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
                    col1.Children.Add(new TextBlock
                    {
                        Text = $"{order.CreatedAt:HH:mm dd/MM}",
                        FontSize = 13,
                        Foreground = GetBrush("#475569"),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 12, 0)
                    });

                    var statusStyle = GetOrderStatusStyle(order.Status);
                    Border badge = new()
                    {
                        Background = GetBrush(statusStyle.Bg),
                        BorderBrush = GetBrush(statusStyle.Color),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(8, 2, 8, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    badge.Child = new TextBlock
                    {
                        Text = statusStyle.Label,
                        FontSize = 11,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = GetBrush(statusStyle.Color)
                    };
                    col1.Children.Add(badge);
                    Grid.SetColumn(col1, 1);
                    cardGrid.Children.Add(col1);

                    Button detailBtn = new()
                    {
                        Content = "Xem chi tiết",
                        Width = 96,
                        Height = 30,
                        Background = GetBrush("#0866E5"),
                        Foreground = System.Windows.Media.Brushes.White,
                        BorderBrush = GetBrush("#0866E5"),
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 12,
                        Style = FindResource("RoundCornerButtonStyle") as Style,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    detailBtn.Click += async (_, _) =>
                    {
                        try
                        {
                            var detail = await _apiClient.GetOrderAsync(order.OrderId);
                            if (detail != null)
                            {
                                string detailMsg = string.Join(Environment.NewLine + Environment.NewLine, detail.Items.Select(PendingOrderCard.BuildLineDetail));
                                ShowCustomMessageBox(detailMsg, $"{tableName} - {detail.OrderCode}", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowCustomMessageBox(GetFriendlyError(ex), "Lỗi xem chi tiết", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    };

                    Grid.SetColumn(detailBtn, 2);
                    cardGrid.Children.Add(detailBtn);

                    card.Child = cardGrid;
                    itemsPanel.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                itemsPanel.Children.Clear();
                itemsPanel.Children.Add(new TextBlock
                {
                    Text = $"Lỗi tải dữ liệu: {GetFriendlyError(ex)}",
                    Foreground = GetBrush("#EF4444"),
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 0)
                });
            }
        }

        datePicker.SelectedDateChanged += async (s, e) =>
        {
            await LoadHistoryAsync(datePicker.SelectedDate);
        };

        Button closeButton = new()
        {
            Content = "Đóng",
            Width = 100,
            Height = 40,
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#475569"),
            BorderBrush = GetBrush("#E2E8F0"),
            FontWeight = FontWeights.Bold,
            Style = FindResource("RoundCornerButtonStyle") as Style,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        closeButton.Click += (_, _) => dialog.Close();
        Grid.SetRow(closeButton, 3);
        root.Children.Add(closeButton);

        await LoadHistoryAsync(datePicker.SelectedDate);

        mainBorder.Child = root;
        dialog.Content = mainBorder;

        ShowBorderlessDialog(dialog);
    }

    private static (string Label, string Color, string Bg) GetOrderStatusStyle(string status)
    {
        return status switch
        {
            "PendingConfirmation" => ("Đơn chờ", "#0866E5", "#F0F7FF"),
            "Confirmed" => ("Đã xác nhận", "#16A34A", "#F0FDF4"),
            "Cancelled" => ("Đã hủy", "#EF4444", "#FEF2F2"),
            _ => (status, "#64748B", "#F1F5F9")
        };
    }

    private async void RefreshRequestsButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadServiceRequestsAsync();
    }

    private async void ServiceRequestHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        Window dialog = new()
        {
            Title = "Lịch sử yêu cầu",
            Owner = Window.GetWindow(this),
            Width = 650,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent
        };

        Border mainBorder = new()
        {
            Background = System.Windows.Media.Brushes.White,
            BorderBrush = GetBrush("#0866E5"),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(20)
        };

        Grid root = new();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        TextBlock titleText = new()
        {
            Text = "Lịch sử yêu cầu",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A"),
            VerticalAlignment = VerticalAlignment.Center
        };
        root.Children.Add(titleText);

        StackPanel filterPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 16, 0, 12),
            VerticalAlignment = VerticalAlignment.Center
        };

        filterPanel.Children.Add(new TextBlock
        {
            Text = "Chọn ngày:",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = GetBrush("#475569"),
            VerticalAlignment = VerticalAlignment.Center
        });

        DatePicker datePicker = new()
        {
            SelectedDate = DateTime.Today,
            Width = 150,
            Height = 32,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };
        filterPanel.Children.Add(datePicker);
        Grid.SetRow(filterPanel, 1);
        root.Children.Add(filterPanel);

        ScrollViewer scroll = new()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 4, 0, 16)
        };
        StackPanel itemsPanel = new();
        scroll.Content = itemsPanel;
        Grid.SetRow(scroll, 2);
        root.Children.Add(scroll);

        async Task LoadHistoryAsync(DateTime? date)
        {
            itemsPanel.Children.Clear();
            itemsPanel.Children.Add(new TextBlock
            {
                Text = "Đang tải dữ liệu...",
                Foreground = GetBrush("#64748B"),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            });

            try
            {
                DateTime? from = date?.Date;
                DateTime? to = date?.Date.AddDays(1).AddTicks(-1);

                var requests = await _apiClient.GetServiceRequestsAsync(from, to);

                itemsPanel.Children.Clear();

                if (requests.Count == 0)
                {
                    itemsPanel.Children.Add(new TextBlock
                    {
                        Text = "Không có lịch sử yêu cầu trong ngày này.",
                        Foreground = GetBrush("#64748B"),
                        FontSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 40, 0, 0)
                    });
                    return;
                }

                foreach (var req in requests.OrderByDescending(x => x.RequestId))
                {
                    var table = _tables.FirstOrDefault(x => x.TableSessionId == req.TableSessionId);
                    string tableName = table?.TableName ?? $"Bàn {req.TableSessionId}";

                    ServiceRequestCard cardData = new(req, tableName);

                    Border card = new()
                    {
                        Background = System.Windows.Media.Brushes.White,
                        BorderBrush = GetBrush("#E2E8F0"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(14, 12, 14, 12),
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    Grid cardGrid = new();
                    cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    StackPanel col0 = new() { VerticalAlignment = VerticalAlignment.Center };
                    col0.Children.Add(new TextBlock
                    {
                        Text = cardData.HeaderText,
                        FontWeight = FontWeights.Bold,
                        FontSize = 14,
                        Foreground = GetBrush("#0F172A")
                    });
                    col0.Children.Add(new TextBlock
                    {
                        Text = cardData.DetailText,
                        FontSize = 12,
                        Foreground = GetBrush("#475569"),
                        Margin = new Thickness(0, 4, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    });
                    cardGrid.Children.Add(col0);

                    StackPanel col1 = new() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(16, 0, 0, 0) };
                    col1.Children.Add(new TextBlock
                    {
                        Text = $"{req.CreatedAt.ToLocalTime():HH:mm dd/MM}",
                        FontSize = 13,
                        Foreground = GetBrush("#64748B"),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 12, 0)
                    });

                    var statusStyle = GetRequestStatusStyle(req.Status);
                    Border badge = new()
                    {
                        Background = GetBrush(statusStyle.Bg),
                        BorderBrush = GetBrush(statusStyle.Color),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(8, 2, 8, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    badge.Child = new TextBlock
                    {
                        Text = statusStyle.Label,
                        FontSize = 11,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = GetBrush(statusStyle.Color)
                    };
                    col1.Children.Add(badge);
                    Grid.SetColumn(col1, 1);
                    cardGrid.Children.Add(col1);

                    card.Child = cardGrid;
                    itemsPanel.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                itemsPanel.Children.Clear();
                itemsPanel.Children.Add(new TextBlock
                {
                    Text = $"Lỗi tải dữ liệu: {GetFriendlyError(ex)}",
                    Foreground = GetBrush("#EF4444"),
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 0)
                });
            }
        }

        datePicker.SelectedDateChanged += async (s, e) =>
        {
            await LoadHistoryAsync(datePicker.SelectedDate);
        };

        Button closeButton = new()
        {
            Content = "Đóng",
            Width = 100,
            Height = 40,
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#475569"),
            BorderBrush = GetBrush("#E2E8F0"),
            FontWeight = FontWeights.Bold,
            Style = FindResource("RoundCornerButtonStyle") as Style,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        closeButton.Click += (_, _) => dialog.Close();
        Grid.SetRow(closeButton, 3);
        root.Children.Add(closeButton);

        await LoadHistoryAsync(datePicker.SelectedDate);

        mainBorder.Child = root;
        dialog.Content = mainBorder;

        ShowBorderlessDialog(dialog);
    }

    private static (string Label, string Color, string Bg) GetRequestStatusStyle(string status)
    {
        return status switch
        {
            "Pending" => ("Chờ xử lý", "#D97706", "#FEF3C7"),
            "Confirmed" => ("Đã xử lý", "#16A34A", "#F0FDF4"),
            _ => (status, "#64748B", "#F1F5F9")
        };
    }

    private async void ConfirmPendingOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: PendingOrderCard order })
        {
            return;
        }

        try
        {
            int? targetBillId = _selectedTable?.TableSessionId == order.TableSessionId
                ? GetSelectedBillIdForCurrentTable()
                : null;

            BillDto bill = await _apiClient.ConfirmOrderAsync(order.OrderId, targetBillId);
            await LoadPendingOrdersAsync();
            await LoadFromApiAsync();
            ShowCustomMessageBox("Đã xác nhận order và gửi phản hồi cho khách.", "Xác nhận order", MessageBoxButton.OK, MessageBoxImage.Information);
            TableCard? table = _tables.FirstOrDefault(x => x.TableSessionId == bill.TableSessionId);
            if (table is not null)
            {
                SelectTable(table, table.Bills.FirstOrDefault(x => x.BillId == bill.BillId));
            }
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Xác nhận order", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ViewPendingOrderDetailButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: PendingOrderCard order })
        {
            return;
        }

        ShowCustomMessageBox(
            order.DetailMessage,
            order.HeaderText,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private async void CancelPendingOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: PendingOrderCard order })
        {
            return;
        }

        string? reason = ShowReasonDialog(
            title: $"Hủy {order.OrderCode}",
            message: "Chọn hoặc nhập lý do hủy order để gửi phản hồi cho khách.",
            defaultReason: "Khác");

        if (string.IsNullOrWhiteSpace(reason))
        {
            return;
        }

        try
        {
            await _apiClient.CancelOrderAsync(order.OrderId, reason);
            await LoadPendingOrdersAsync();
            ShowCustomMessageBox("Đã hủy order và gửi phản hồi cho khách.", "Hủy order", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Hủy order", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void ConfirmServiceRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ServiceRequestCard request })
        {
            return;
        }

        try
        {
            await _apiClient.ConfirmServiceRequestAsync(request.RequestId);
            await LoadServiceRequestsAsync();
            ShowCustomMessageBox("Đã xác nhận yêu cầu và gửi phản hồi cho khách.", "Yêu cầu", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Yêu cầu", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ApplyPendingOrderFilter()
    {
        PendingOrders.Clear();
        string keyword = SearchBox.Text.Trim();
        IEnumerable<PendingOrderCard> orders = _allPendingOrders;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            orders = orders.Where(x =>
                x.HeaderText.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.DetailText.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.ItemSummary.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        foreach (PendingOrderCard order in orders)
        {
            PendingOrders.Add(order);
        }
    }

    private void ApplyServiceRequestFilter()
    {
        ServiceRequests.Clear();
        string keyword = SearchBox.Text.Trim();
        IEnumerable<ServiceRequestCard> requests = _allServiceRequests;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            requests = requests.Where(x =>
                x.HeaderText.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.DetailText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        foreach (ServiceRequestCard request in requests)
        {
            ServiceRequests.Add(request);
        }
    }

}
