using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{
    private static Border CreateMethodCard(string text, string tag)
    {
        return new Border
        {
            Height = 46,
            CornerRadius = new CornerRadius(10),
            BorderThickness = new Thickness(1.5),
            Margin = new Thickness(0, 0, 8, 0),
            Background = System.Windows.Media.Brushes.White,
            BorderBrush = GetBrush("#CBD5E1"),
            Cursor = System.Windows.Input.Cursors.Hand,
            Child = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetBrush("#475569"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
    }

    private static void ApplyCardStyle(Border border, bool isSelected)
    {
        border.Background = isSelected ? GetBrush("#F0F7FF") : System.Windows.Media.Brushes.White;
        border.BorderBrush = isSelected ? GetBrush("#0866E5") : GetBrush("#CBD5E1");
        if (border.Child is TextBlock text)
        {
            text.Foreground = isSelected ? GetBrush("#0866E5") : GetBrush("#475569");
        }
    }

    private PaymentDialogResult? ShowPaymentDialog(BillPreview bill)
    {
        decimal total = bill.Total;
        PaymentDialogResult? result = null;
        Window dialog = new()
        {
            Title = $"Thanh toán - {bill.BillName}",
            Owner = Window.GetWindow(this),
            Width = 960,
            Height = 650,
            MinWidth = 800,
            MinHeight = 560,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        Grid root = new() { Background = System.Windows.Media.Brushes.White };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

        Grid details = new() { Margin = new Thickness(24) };
        details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        details.RowDefinitions.Add(new RowDefinition());
        details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        
        details.Children.Add(new TextBlock
        {
            Text = $"Thanh toán #{bill.BillNo} • {bill.BillName}",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A"),
            Margin = new Thickness(0, 0, 0, 18)
        });

        DataGrid itemGrid = new()
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            CanUserAddRows = false,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            BorderThickness = new Thickness(0),
            RowHeight = 42,
            Background = System.Windows.Media.Brushes.White,
            RowBackground = System.Windows.Media.Brushes.White,
            AlternatingRowBackground = GetBrush("#F8FAFC"),
            HorizontalGridLinesBrush = GetBrush("#E2E8F0"),
            ItemsSource = bill.Lines
        };

        Style headerStyle = new(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
        headerStyle.Setters.Add(new Setter(Control.BackgroundProperty, GetBrush("#F1F5F9")));
        headerStyle.Setters.Add(new Setter(Control.ForegroundProperty, GetBrush("#475569")));
        headerStyle.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
        headerStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 10, 12, 10)));
        headerStyle.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 0, 1)));
        headerStyle.Setters.Add(new Setter(Control.BorderBrushProperty, GetBrush("#CBD5E1")));
        itemGrid.ColumnHeaderStyle = headerStyle;

        Style cellStyle = new(typeof(DataGridCell));
        cellStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 8, 12, 8)));
        cellStyle.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        cellStyle.Setters.Add(new Setter(Control.VerticalAlignmentProperty, VerticalAlignment.Center));
        itemGrid.CellStyle = cellStyle;

        itemGrid.Columns.Add(new DataGridTextColumn { Header = "MÓN ĂN", Binding = new System.Windows.Data.Binding(nameof(BillLinePreview.ItemName)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        itemGrid.Columns.Add(new DataGridTextColumn { Header = "SL", Binding = new System.Windows.Data.Binding(nameof(BillLinePreview.Quantity)), Width = 60 });
        itemGrid.Columns.Add(new DataGridTextColumn { Header = "ĐƠN GIÁ", Binding = new System.Windows.Data.Binding(nameof(BillLinePreview.UnitPriceText)), Width = 110 });
        itemGrid.Columns.Add(new DataGridTextColumn { Header = "THÀNH TIỀN", Binding = new System.Windows.Data.Binding(nameof(BillLinePreview.TotalText)), Width = 120 });
        
        Grid.SetRow(itemGrid, 1);
        details.Children.Add(itemGrid);

        TextBlock detailTotal = new()
        {
            Text = $"Tổng tiền hàng:   {Money(total)}",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A"),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 18, 0, 0)
        };
        Grid.SetRow(detailTotal, 2);
        details.Children.Add(detailTotal);
        root.Children.Add(details);

        Border paymentPanel = new()
        {
            Background = GetBrush("#F8FAFC"),
            BorderBrush = GetBrush("#E2E8F0"),
            BorderThickness = new Thickness(1, 0, 0, 0),
            Padding = new Thickness(26)
        };
        Grid.SetColumn(paymentPanel, 1);
        StackPanel paymentStack = new();
        paymentStack.Children.Add(new TextBlock 
        { 
            Text = "Chi tiết giao dịch", 
            FontSize = 20, 
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A")
        });

        Border amountCard = new()
        {
            Background = GetBrush("#F0F7FF"),
            BorderBrush = GetBrush("#D1E9FF"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 24, 0, 20)
        };
        StackPanel amountStack = new();
        amountStack.Children.Add(new TextBlock 
        { 
            Text = "KHÁCH CẦN TRẢ", 
            FontSize = 11, 
            FontWeight = FontWeights.Bold, 
            Foreground = GetBrush("#0B55A0"),
            Margin = new Thickness(0, 0, 0, 4)
        });
        amountStack.Children.Add(new TextBlock 
        { 
            Text = Money(total), 
            FontSize = 26, 
            FontWeight = FontWeights.ExtraBold, 
            Foreground = GetBrush("#0866E5") 
        });
        amountCard.Child = amountStack;
        paymentStack.Children.Add(amountCard);

        paymentStack.Children.Add(new TextBlock 
        { 
            Text = "Phương thức thanh toán", 
            FontWeight = FontWeights.Bold, 
            Foreground = GetBrush("#475569"),
            Margin = new Thickness(0, 0, 0, 10)
        });

        Grid methodsGrid = new();
        methodsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        methodsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        methodsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        
        string selectedMethod = "Cash";

        Border cashCard = CreateMethodCard("Tiền mặt", "Cash");
        Border transferCard = CreateMethodCard("Chuyển khoản", "BankTransfer");
        Border cardCard = CreateMethodCard("Thẻ", "Card");

        Grid.SetColumn(cashCard, 0);
        Grid.SetColumn(transferCard, 1);
        Grid.SetColumn(cardCard, 2);

        methodsGrid.Children.Add(cashCard);
        methodsGrid.Children.Add(transferCard);
        methodsGrid.Children.Add(cardCard);
        paymentStack.Children.Add(methodsGrid);

        Button combined = new()
        {
            Content = "Kết hợp nhiều phương thức thanh toán",
            Height = 38,
            Margin = new Thickness(0, 12, 0, 0),
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#0866E5"),
            BorderBrush = GetBrush("#D1E9FF"),
            FontWeight = FontWeights.SemiBold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        paymentStack.Children.Add(combined);

        TextBlock cashLabel = new() 
        { 
            Text = "Tiền khách đưa", 
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#475569"),
            Margin = new Thickness(0, 24, 0, 8) 
        };
        TextBox cashReceived = new() 
        { 
            Text = total.ToString("0", CultureInfo.InvariantCulture), 
            Height = 44, 
            FontSize = 18, 
            FontWeight = FontWeights.Bold,
            Padding = new Thickness(10),
            BorderThickness = new Thickness(1),
            BorderBrush = GetBrush("#CBD5E1"),
            Background = System.Windows.Media.Brushes.White
        };
        cashReceived.GotFocus += (s, e) => cashReceived.BorderBrush = GetBrush("#0866E5");
        cashReceived.LostFocus += (s, e) => cashReceived.BorderBrush = GetBrush("#CBD5E1");

        paymentStack.Children.Add(cashLabel);
        paymentStack.Children.Add(cashReceived);

        void UpdateCardSelection()
        {
            ApplyCardStyle(cashCard, selectedMethod == "Cash");
            ApplyCardStyle(transferCard, selectedMethod == "BankTransfer");
            ApplyCardStyle(cardCard, selectedMethod == "Card");

            bool isCash = selectedMethod == "Cash";
            cashLabel.Visibility = isCash ? Visibility.Visible : Visibility.Collapsed;
            cashReceived.Visibility = isCash ? Visibility.Visible : Visibility.Collapsed;
        }

        cashCard.MouseLeftButtonDown += (_, _) => { selectedMethod = "Cash"; UpdateCardSelection(); };
        transferCard.MouseLeftButtonDown += (_, _) => { selectedMethod = "BankTransfer"; UpdateCardSelection(); };
        cardCard.MouseLeftButtonDown += (_, _) => { selectedMethod = "Card"; UpdateCardSelection(); };

        UpdateCardSelection();

        Button pay = new()
        {
            Content = "XÁC NHẬN THANH TOÁN",
            Height = 52,
            Margin = new Thickness(0, 30, 0, 0),
            Background = GetBrush("#16A34A"),
            Foreground = System.Windows.Media.Brushes.White,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            BorderBrush = GetBrush("#16A34A"),
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        pay.Click += (_, _) =>
        {
            decimal received = selectedMethod == "Cash" ? ParseMoney(cashReceived.Text) : total;
            if (received < total)
            {
                ShowCustomMessageBox("Tiền khách đưa chưa đủ.", "Thanh toán", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            result = new PaymentDialogResult([new PaymentPart(selectedMethod, total)], received, selectedMethod == "Cash" ? total : 0);
            dialog.DialogResult = true;
        };

        combined.Click += (_, _) =>
        {
            PaymentDialogResult? combinedResult = ShowCombinedPaymentDialog(dialog, total);
            if (combinedResult is null)
            {
                return;
            }

            result = combinedResult;
            dialog.DialogResult = true;
        };

        paymentStack.Children.Add(pay);

        Button cancel = new()
        {
            Content = "BỎ QUA",
            Height = 44,
            Margin = new Thickness(0, 10, 0, 0),
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#64748B"),
            BorderBrush = GetBrush("#CBD5E1"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        cancel.Click += (_, _) => dialog.Close();
        paymentStack.Children.Add(cancel);

        paymentPanel.Child = paymentStack;
        root.Children.Add(paymentPanel);
        dialog.Content = root;
        ShowBorderlessDialog(dialog);
        return result;
    }

    private PaymentDialogResult? ShowCombinedPaymentDialog(Window owner, decimal total)
    {
        PaymentDialogResult? result = null;
        Window dialog = new()
        {
            Title = "Kết hợp phương thức",
            Owner = owner,
            Width = 560,
            Height = 520,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        StackPanel root = new() { Margin = new Thickness(28) };
        root.Children.Add(new TextBlock 
        { 
            Text = "Kết hợp phương thức", 
            FontSize = 22, 
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A")
        });
        root.Children.Add(new TextBlock 
        { 
            Text = $"Tổng thanh toán: {Money(total)}", 
            FontSize = 18, 
            FontWeight = FontWeights.SemiBold, 
            Foreground = GetBrush("#0866E5"),
            Margin = new Thickness(0, 20, 0, 16) 
        });

        List<(string Method, CheckBox CheckBox, TextBox Amount)> controls = [];
        foreach ((string method, string label) in new[] { ("Cash", "Tiền mặt"), ("BankTransfer", "Chuyển khoản"), ("Card", "Thẻ") })
        {
            Grid row = new() { Margin = new Thickness(0, 8, 0, 8) };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
            
            CheckBox checkBox = new() 
            { 
                Content = label, 
                FontSize = 16, 
                FontWeight = FontWeights.SemiBold,
                Foreground = GetBrush("#475569"),
                VerticalAlignment = VerticalAlignment.Center 
            };
            
            TextBox amount = new()
            {
                Height = 40,
                Padding = new Thickness(10, 8, 10, 8),
                IsEnabled = false,
                Text = string.Empty,
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                BorderThickness = new Thickness(1),
                BorderBrush = GetBrush("#CBD5E1"),
                ToolTip = "Nhập số tiền"
            };
            
            amount.GotFocus += (s, e) => amount.BorderBrush = GetBrush("#0866E5");
            amount.LostFocus += (s, e) => amount.BorderBrush = GetBrush("#CBD5E1");

            checkBox.Checked += (_, _) =>
            {
                amount.IsEnabled = true;
                amount.Focus();
            };
            checkBox.Unchecked += (_, _) =>
            {
                amount.IsEnabled = false;
                amount.Text = string.Empty;
            };
            
            Grid.SetColumn(amount, 1);
            row.Children.Add(checkBox);
            row.Children.Add(amount);
            root.Children.Add(row);
            controls.Add((method, checkBox, amount));
        }

        TextBlock remaining = new() 
        { 
            Margin = new Thickness(0, 18, 0, 16), 
            FontWeight = FontWeights.Bold,
            FontSize = 16,
            Foreground = GetBrush("#D97706")
        };
        
        void RefreshRemaining()
        {
            decimal entered = controls.Where(x => x.CheckBox.IsChecked == true).Sum(x => ParseMoney(x.Amount.Text));
            remaining.Text = $"Còn lại: {Money(total - entered)}";
        }
        
        foreach (var control in controls)
        {
            control.Amount.TextChanged += (_, _) => RefreshRemaining();
        }
        
        RefreshRemaining();
        root.Children.Add(remaining);

        StackPanel buttons = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
        Button back = new() 
        { 
            Content = "Quay lại", 
            Width = 110, 
            Height = 44,
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#64748B"),
            BorderBrush = GetBrush("#CBD5E1"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        
        Button confirm = new() 
        { 
            Content = "Xác nhận", 
            Width = 120, 
            Height = 44, 
            Margin = new Thickness(10, 0, 0, 0), 
            Background = GetBrush("#0866E5"), 
            Foreground = System.Windows.Media.Brushes.White,
            FontWeight = FontWeights.Bold,
            BorderBrush = GetBrush("#0866E5"),
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        
        back.Click += (_, _) => dialog.Close();
        confirm.Click += (_, _) =>
        {
            var invalidSelection = controls.FirstOrDefault(x =>
                x.CheckBox.IsChecked == true && ParseMoney(x.Amount.Text) <= 0);
            if (invalidSelection.CheckBox is not null)
            {
                ShowCustomMessageBox(
                    $"Phương thức \"{invalidSelection.CheckBox.Content}\" chưa có số tiền hợp lệ. Hãy bỏ tích hoặc nhập số tiền.",
                    "Kết hợp phương thức",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                invalidSelection.Amount.Focus();
                return;
            }

            List<PaymentPart> parts = controls
                .Where(x => x.CheckBox.IsChecked == true)
                .Select(x => new PaymentPart(x.Method, ParseMoney(x.Amount.Text)))
                .Where(x => x.Amount > 0)
                .ToList();
            
            if (parts.Sum(x => x.Amount) != total)
            {
                ShowCustomMessageBox("Tổng các phương thức phải bằng số tiền cần trả.", "Kết hợp phương thức", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal cashAmount = parts.Where(x => x.Method == "Cash").Sum(x => x.Amount);
            result = new PaymentDialogResult(parts, cashAmount, cashAmount);
            dialog.DialogResult = true;
        };
        
        buttons.Children.Add(back);
        buttons.Children.Add(confirm);
        root.Children.Add(buttons);
        dialog.Content = root;
        ShowBorderlessDialog(dialog);
        return result;
    }
}
