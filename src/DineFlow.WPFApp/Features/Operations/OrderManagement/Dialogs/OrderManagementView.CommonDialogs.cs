using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{

    private static bool? ShowBorderlessDialog(Window dialog)
    {
        dialog.WindowStyle = WindowStyle.None;
        dialog.AllowsTransparency = true;
        dialog.Background = System.Windows.Media.Brushes.Transparent;
        dialog.ResizeMode = ResizeMode.NoResize;
        dialog.ShowInTaskbar = false;

        if (dialog.Content is FrameworkElement content)
        {
            if (content is Border border)
            {
                border.BorderBrush = GetBrush("#0866E5");
                border.BorderThickness = new Thickness(2);
                border.CornerRadius = new CornerRadius(16);
            }
            else
            {
                dialog.Content = null;

                Border mainBorder = new()
                {
                    Background = System.Windows.Media.Brushes.White,
                    BorderBrush = GetBrush("#0866E5"),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(16),
                    Child = content
                };

                dialog.Content = mainBorder;
            }
        }

        return dialog.ShowDialog();
    }

    private MessageBoxResult ShowCustomMessageBox(
        string message,
        string title,
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage image = MessageBoxImage.Information)
    {
        MessageBoxResult result = MessageBoxResult.None;

        Window dialog = new()
        {
            Title = title,
            Owner = Window.GetWindow(this),
            Width = 420,
            SizeToContent = SizeToContent.Height,
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
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        TextBlock titleText = new()
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A"),
            Margin = new Thickness(0, 0, 0, 16)
        };
        root.Children.Add(titleText);

        TextBlock msgText = new()
        {
            FontSize = 14,
            Foreground = GetBrush("#1E293B"),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20,
            Margin = new Thickness(0, 0, 0, 20)
        };

        // Local helper to parse simple bold markdown: **text**
        void PopulateFormattedText(TextBlock textBlock, string rawText)
        {
            textBlock.Inlines.Clear();
            int index = 0;
            while (index < rawText.Length)
            {
                int start = rawText.IndexOf("**", index);
                if (start == -1)
                {
                    textBlock.Inlines.Add(new System.Windows.Documents.Run(rawText.Substring(index)));
                    break;
                }

                if (start > index)
                {
                    textBlock.Inlines.Add(new System.Windows.Documents.Run(rawText.Substring(index, start - index)));
                }

                int end = rawText.IndexOf("**", start + 2);
                if (end == -1)
                {
                    textBlock.Inlines.Add(new System.Windows.Documents.Run(rawText.Substring(start)));
                    break;
                }

                string boldText = rawText.Substring(start + 2, end - (start + 2));
                textBlock.Inlines.Add(new System.Windows.Documents.Run(boldText) { FontWeight = FontWeights.Bold });

                index = end + 2;
            }
        }

        PopulateFormattedText(msgText, message);
        Grid.SetRow(msgText, 1);
        root.Children.Add(msgText);

        StackPanel footer = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        if (button == MessageBoxButton.OKCancel || button == MessageBoxButton.YesNo)
        {
            Button confirmBtn = new()
            {
                Content = button == MessageBoxButton.YesNo ? "Có" : "Đồng ý",
                Width = 90,
                Height = 38,
                Background = GetBrush("#0866E5"),
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = GetBrush("#0866E5"),
                FontWeight = FontWeights.Bold,
                Style = FindResource("RoundCornerButtonStyle") as Style
            };
            confirmBtn.Click += (_, _) =>
            {
                result = button == MessageBoxButton.YesNo ? MessageBoxResult.Yes : MessageBoxResult.OK;
                dialog.Close();
            };

            Button cancelBtn = new()
            {
                Content = button == MessageBoxButton.YesNo ? "Không" : "Hủy bỏ",
                Width = 90,
                Height = 38,
                Background = System.Windows.Media.Brushes.White,
                Foreground = GetBrush("#475569"),
                BorderBrush = GetBrush("#E2E8F0"),
                FontWeight = FontWeights.Bold,
                Style = FindResource("RoundCornerButtonStyle") as Style,
                Margin = new Thickness(10, 0, 0, 0)
            };
            cancelBtn.Click += (_, _) =>
            {
                result = button == MessageBoxButton.YesNo ? MessageBoxResult.No : MessageBoxResult.Cancel;
                dialog.Close();
            };

            footer.Children.Add(confirmBtn);
            footer.Children.Add(cancelBtn);
        }
        else
        {
            Button okBtn = new()
            {
                Content = "Đồng ý",
                Width = 90,
                Height = 38,
                Background = GetBrush("#0866E5"),
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = GetBrush("#0866E5"),
                FontWeight = FontWeights.Bold,
                Style = FindResource("RoundCornerButtonStyle") as Style
            };
            okBtn.Click += (_, _) =>
            {
                result = MessageBoxResult.OK;
                dialog.Close();
            };
            footer.Children.Add(okBtn);
        }

        Grid.SetRow(footer, 2);
        root.Children.Add(footer);

        mainBorder.Child = root;
        dialog.Content = mainBorder;

        ShowBorderlessDialog(dialog);
        return result;
    }

    private string? ShowReasonDialog(string title, string message, string defaultReason)
    {
        Window dialog = new()
        {
            Title = title,
            Width = 440,
            Height = 270,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this),
            ResizeMode = ResizeMode.NoResize
        };

        StackPanel root = new()
        {
            Margin = new Thickness(18)
        };

        root.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 14),
            Foreground = System.Windows.Media.Brushes.Black
        });

        ComboBox reasonBox = new()
        {
            Height = 34,
            Margin = new Thickness(0, 0, 0, 10),
            IsEditable = false,
            ItemsSource = new[] { "Khách đổi ý", "Hết món", "Gửi nhầm", "Khác" },
            SelectedItem = defaultReason
        };
        root.Children.Add(reasonBox);

        TextBox reasonText = new()
        {
            MinHeight = 52,
            Text = defaultReason,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            Margin = new Thickness(0, 0, 0, 16)
        };
        root.Children.Add(reasonText);

        reasonBox.SelectionChanged += (_, _) =>
        {
            if (reasonBox.SelectedItem is string selected)
            {
                reasonText.Text = selected;
            }
        };

        StackPanel actions = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        Button confirmButton = new()
        {
            Content = "Đồng ý",
            Width = 96,
            Height = 36,
            Margin = new Thickness(0, 0, 8, 0),
            Background = System.Windows.Media.Brushes.IndianRed,
            Foreground = System.Windows.Media.Brushes.White,
            FontWeight = FontWeights.Bold
        };

        Button cancelButton = new()
        {
            Content = "Quay lại",
            Width = 96,
            Height = 36
        };

        confirmButton.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(reasonText.Text))
            {
                ShowCustomMessageBox("Vui lòng nhập lý do.", title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dialog.DialogResult = true;
        };
        cancelButton.Click += (_, _) => dialog.DialogResult = false;

        actions.Children.Add(confirmButton);
        actions.Children.Add(cancelButton);
        root.Children.Add(actions);

        dialog.Content = root;
        bool? result = ShowBorderlessDialog(dialog);
        return result == true ? reasonText.Text.Trim() : null;
    }
}
