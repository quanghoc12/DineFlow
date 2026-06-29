using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{

    private CancelLineDialogResult? ShowCancelLineDialog(BillLinePreview line, int maxCancelQuantity)
    {
        int quantityToCancel = 1;
        Window dialog = new()
        {
            Title = "Xác nhận giảm / Hủy món",
            Owner = Window.GetWindow(this),
            Width = 520,
            Height = 360,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.White
        };

        Grid root = new() { Margin = new Thickness(22) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        TextBlock title = new()
        {
            Text = "Xác nhận giảm / Hủy món",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.Black
        };
        root.Children.Add(title);

        TextBlock message = new()
        {
            Text = $"Bạn có chắc chắn muốn hủy món {line.ItemName} không?",
            FontSize = 15,
            Margin = new Thickness(0, 18, 0, 18),
            Foreground = System.Windows.Media.Brushes.Black
        };
        Grid.SetRow(message, 1);
        root.Children.Add(message);

        Grid quantityRow = new();
        quantityRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        quantityRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        quantityRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        quantityRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        quantityRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        quantityRow.Children.Add(new TextBlock
        {
            Text = "Số lượng hủy",
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.Black
        });

        TextBlock quantityText = new()
        {
            Text = quantityToCancel.ToString(CultureInfo.InvariantCulture),
            Width = 40,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 17
        };
        TextBlock maxText = new()
        {
            Text = $"/{maxCancelQuantity}",
            Margin = new Thickness(6, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.DimGray
        };

        Button minusButton = new() { Content = "-", Width = 34, Height = 34 };
        minusButton.Click += (_, _) =>
        {
            if (quantityToCancel > 1)
            {
                quantityToCancel--;
                quantityText.Text = quantityToCancel.ToString(CultureInfo.InvariantCulture);
            }
        };

        Button plusButton = new() { Content = "+", Width = 34, Height = 34 };
        plusButton.Click += (_, _) =>
        {
            if (quantityToCancel < maxCancelQuantity)
            {
                quantityToCancel++;
                quantityText.Text = quantityToCancel.ToString(CultureInfo.InvariantCulture);
            }
        };

        Grid.SetColumn(minusButton, 1);
        Grid.SetColumn(quantityText, 2);
        Grid.SetColumn(maxText, 3);
        Grid.SetColumn(plusButton, 4);
        quantityRow.Children.Add(minusButton);
        quantityRow.Children.Add(quantityText);
        quantityRow.Children.Add(maxText);
        quantityRow.Children.Add(plusButton);
        Grid.SetRow(quantityRow, 2);
        root.Children.Add(quantityRow);

        Grid reasonGrid = new() { Margin = new Thickness(0, 24, 0, 0) };
        reasonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        reasonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        reasonGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        reasonGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        reasonGrid.Children.Add(new TextBlock
        {
            Text = "Lý do hủy",
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.Black
        });

        ComboBox reasonCombo = new()
        {
            Height = 34,
            ItemsSource = new[] { "Khác", "Khách đổi ý", "Nhập sai món", "Hết món", "Món làm lỗi" },
            SelectedIndex = 0
        };
        Grid.SetColumn(reasonCombo, 1);
        reasonGrid.Children.Add(reasonCombo);

        TextBox reasonTextBox = new()
        {
            Text = "Khác",
            Height = 34,
            Margin = new Thickness(0, 12, 0, 0),
            BorderThickness = new Thickness(0, 0, 0, 2),
            BorderBrush = System.Windows.Media.Brushes.DodgerBlue
        };
        Grid.SetRow(reasonTextBox, 1);
        Grid.SetColumn(reasonTextBox, 1);
        reasonGrid.Children.Add(reasonTextBox);

        reasonCombo.SelectionChanged += (_, _) =>
        {
            reasonTextBox.Text = reasonCombo.SelectedItem?.ToString() ?? string.Empty;
        };

        Grid.SetRow(reasonGrid, 3);
        root.Children.Add(reasonGrid);

        CancelLineDialogResult? result = null;
        StackPanel footer = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 18, 0, 0)
        };

        Button confirmButton = new()
        {
            Content = "Chắc chắn",
            Width = 130,
            Height = 44,
            Background = System.Windows.Media.Brushes.IndianRed,
            Foreground = System.Windows.Media.Brushes.White,
            FontWeight = FontWeights.Bold,
            BorderBrush = System.Windows.Media.Brushes.IndianRed
        };
        confirmButton.Click += (_, _) =>
        {
            string reason = string.IsNullOrWhiteSpace(reasonTextBox.Text)
                ? "Khác"
                : reasonTextBox.Text.Trim();

            result = new CancelLineDialogResult(quantityToCancel, reason);
            dialog.Close();
        };

        Button cancelButton = new()
        {
            Content = "HỦY",
            Width = 110,
            Height = 44,
            Margin = new Thickness(10, 0, 0, 0),
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#64748B"),
            FontWeight = FontWeights.Bold,
            BorderBrush = GetBrush("#CBD5E1")
        };
        cancelButton.Click += (_, _) => dialog.Close();

        footer.Children.Add(confirmButton);
        footer.Children.Add(cancelButton);
        Grid.SetRow(footer, 4);
        root.Children.Add(footer);

        dialog.Content = root;
        ShowBorderlessDialog(dialog);
        return result;
    }

    private void ShowLineDetailDialog(BillLinePreview line)
    {
        Window dialog = new()
        {
            Title = $"Chi tiết {line.ItemName}",
            Owner = Window.GetWindow(this),
            Width = 460,
            Height = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.White
        };

        StackPanel root = new() { Margin = new Thickness(20) };
        root.Children.Add(new TextBlock
        {
            Text = line.ItemName,
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.Black,
            TextWrapping = TextWrapping.Wrap
        });

        root.Children.Add(new TextBlock
        {
            Text = $"Số lượng hiện tại: {line.Quantity}",
            Margin = new Thickness(0, 16, 0, 0),
            Foreground = System.Windows.Media.Brushes.Black
        });

        root.Children.Add(new TextBlock
        {
            Text = $"Đã thông báo bếp: {line.NotifiedQuantity}",
            Margin = new Thickness(0, 6, 0, 0),
            Foreground = System.Windows.Media.Brushes.Black
        });

        root.Children.Add(new TextBlock
        {
            Text = $"Chưa thông báo: {Math.Max(0, line.Quantity - line.NotifiedQuantity)}",
            Margin = new Thickness(0, 6, 0, 12),
            Foreground = System.Windows.Media.Brushes.Black
        });

        root.Children.Add(new TextBlock
        {
            Text = "Option / ghi chú",
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.Black
        });

        root.Children.Add(new TextBlock
        {
            Text = line.ChoiceSummary,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0),
            Foreground = System.Windows.Media.Brushes.DimGray
        });

        Button closeButton = new()
        {
            Content = "Đóng",
            Width = 96,
            Height = 36,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 18, 0, 0)
        };
        closeButton.Click += (_, _) => dialog.Close();
        root.Children.Add(closeButton);

        dialog.Content = root;
        ShowBorderlessDialog(dialog);
    }

    private CancelBillDialogResult? ShowCancelBillDialog(BillPreview bill)
    {
        Window dialog = new()
        {
            Title = $"Hủy {bill.BillName}",
            Owner = Window.GetWindow(this),
            Width = 560,
            Height = 310,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.White
        };

        Grid root = new() { Margin = new Thickness(22) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        TextBlock title = new()
        {
            Text = $"Hủy {bill.BillName}",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.Black
        };
        root.Children.Add(title);

        TextBlock message = new()
        {
            Text = "Đơn hàng này đã được thông báo tới nhà bếp và các thiết bị khác, bạn có chắc chắn muốn hủy?",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 15,
            Margin = new Thickness(0, 22, 0, 22),
            Foreground = System.Windows.Media.Brushes.Black
        };
        Grid.SetRow(message, 1);
        root.Children.Add(message);

        Grid reasonGrid = new();
        reasonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        reasonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        reasonGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        reasonGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        reasonGrid.Children.Add(new TextBlock
        {
            Text = "Lý do hủy",
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.Black
        });

        ComboBox reasonCombo = new()
        {
            Height = 34,
            ItemsSource = new[] { "Khác", "Khách đổi ý", "Nhập sai bill", "Tách nhầm bill", "Hết món" },
            SelectedIndex = 0
        };
        Grid.SetColumn(reasonCombo, 1);
        reasonGrid.Children.Add(reasonCombo);

        TextBox reasonTextBox = new()
        {
            Text = "Khác",
            Height = 34,
            Margin = new Thickness(0, 12, 0, 0),
            BorderThickness = new Thickness(0, 0, 0, 2),
            BorderBrush = System.Windows.Media.Brushes.DodgerBlue
        };
        Grid.SetRow(reasonTextBox, 1);
        Grid.SetColumn(reasonTextBox, 1);
        reasonGrid.Children.Add(reasonTextBox);

        reasonCombo.SelectionChanged += (_, _) =>
        {
            reasonTextBox.Text = reasonCombo.SelectedItem?.ToString() ?? string.Empty;
        };

        Grid.SetRow(reasonGrid, 2);
        root.Children.Add(reasonGrid);

        CancelBillDialogResult? result = null;
        StackPanel footer = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 18, 0, 0)
        };

        Button confirmButton = new()
        {
            Content = "Đồng ý hủy",
            Width = 128,
            Height = 44,
            Background = System.Windows.Media.Brushes.IndianRed,
            Foreground = System.Windows.Media.Brushes.White,
            FontWeight = FontWeights.Bold,
            BorderBrush = System.Windows.Media.Brushes.IndianRed
        };
        confirmButton.Click += (_, _) =>
        {
            string reason = string.IsNullOrWhiteSpace(reasonTextBox.Text)
                ? "Khác"
                : reasonTextBox.Text.Trim();

            result = new CancelBillDialogResult(reason);
            dialog.Close();
        };

        Button cancelButton = new()
        {
            Content = "HỦY",
            Width = 110,
            Height = 44,
            Margin = new Thickness(10, 0, 0, 0),
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#64748B"),
            FontWeight = FontWeights.Bold,
            BorderBrush = GetBrush("#CBD5E1")
        };
        cancelButton.Click += (_, _) => dialog.Close();

        footer.Children.Add(confirmButton);
        footer.Children.Add(cancelButton);
        Grid.SetRow(footer, 3);
        root.Children.Add(footer);

        dialog.Content = root;
        ShowBorderlessDialog(dialog);
        return result;
    }
}
