using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using DineFlow.Services.Orders;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{

        private AddMenuItemDialogResult? ShowAddMenuItemDialog(MenuItemCard item)
    {
        int quantity = 1;
        List<ChoiceGroupCard> groups = item.ChoiceGroups
            .OrderBy(x => x.DisplayOrder)
            .Select(x => x.Clone())
            .ToList();

        Window dialog = new()
        {
            Title = item.Name,
            Owner = Window.GetWindow(this),
            Width = 600,
            Height = 700,
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
            Padding = new Thickness(24)
        };

        Grid root = new();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Grid header = new();
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.Children.Add(new TextBlock
        {
            Text = item.Name,
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A"),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        });

        Border quantityBorder = new()
        {
            Width = 110,
            Height = 38,
            CornerRadius = new CornerRadius(8),
            BorderBrush = GetBrush("#E2E8F0"),
            BorderThickness = new Thickness(1),
            Background = System.Windows.Media.Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid quantityGrid = new();
        quantityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
        quantityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        quantityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });

        TextBlock quantityText = new()
        {
            Text = quantity.ToString(CultureInfo.InvariantCulture),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A"),
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Border quantityTextBorder = new()
        {
            BorderBrush = GetBrush("#E2E8F0"),
            BorderThickness = new Thickness(1, 0, 1, 0)
        };
        quantityTextBorder.Child = quantityText;
        Grid.SetColumn(quantityTextBorder, 1);

        Button minusButton = new()
        {
            Content = "-",
            Background = System.Windows.Media.Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = GetBrush("#0866E5"),
            FontSize = 14,
            FontWeight = FontWeights.Bold
        };
        minusButton.Click += (_, _) =>
        {
            if (quantity > 1)
            {
                quantity--;
                quantityText.Text = quantity.ToString(CultureInfo.InvariantCulture);
            }
        };

        Button plusButton = new()
        {
            Content = "+",
            Background = System.Windows.Media.Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = GetBrush("#0866E5"),
            FontSize = 16,
            FontWeight = FontWeights.Bold
        };
        plusButton.Click += (_, _) =>
        {
            quantity++;
            quantityText.Text = quantity.ToString(CultureInfo.InvariantCulture);
        };

        Grid.SetColumn(minusButton, 0);
        Grid.SetColumn(plusButton, 2);
        quantityGrid.Children.Add(minusButton);
        quantityGrid.Children.Add(quantityTextBorder);
        quantityGrid.Children.Add(plusButton);
        quantityBorder.Child = quantityGrid;
        Grid.SetColumn(quantityBorder, 1);
        header.Children.Add(quantityBorder);
        root.Children.Add(header);

        ScrollViewer scroll = new()
        {
            Margin = new Thickness(0, 16, 0, 0),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        StackPanel body = new();

        void UpdateOptionSelection(Border border, bool isSelected)
        {
            if (isSelected)
            {
                border.BorderBrush = GetBrush("#0866E5");
                border.Background = GetBrush("#F0F7FF");
            }
            else
            {
                border.BorderBrush = GetBrush("#E2E8F0");
                border.Background = System.Windows.Media.Brushes.White;
            }
        }

        foreach (ChoiceGroupCard group in groups)
        {
            Border groupBox = new()
            {
                BorderBrush = GetBrush("#E2E8F0"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 14),
                Background = System.Windows.Media.Brushes.White
            };

            StackPanel groupPanel = new();

            StackPanel titlePanel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
            Border blueBar = new()
            {
                Width = 4,
                Height = 16,
                Background = GetBrush("#0866E5"),
                CornerRadius = new CornerRadius(2),
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            titlePanel.Children.Add(blueBar);

            titlePanel.Children.Add(new TextBlock
            {
                Text = group.GroupName,
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                Foreground = GetBrush("#0F172A"),
                VerticalAlignment = VerticalAlignment.Center
            });

            if (group.IsRequired)
            {
                titlePanel.Children.Add(new TextBlock
                {
                    Text = " *",
                    FontWeight = FontWeights.Bold,
                    FontSize = 15,
                    Foreground = GetBrush("#EF4444"),
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            else if (group.EffectiveMaxSelect > 1)
            {
                titlePanel.Children.Add(new TextBlock
                {
                    Text = $" (Chọn tối đa {group.EffectiveMaxSelect})",
                    FontWeight = FontWeights.Normal,
                    FontSize = 14,
                    Foreground = System.Windows.Media.Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            groupPanel.Children.Add(titlePanel);

            WrapPanel optionsPanel = new() { Orientation = Orientation.Horizontal };



            List<System.Windows.Controls.Primitives.ToggleButton> groupChecks = [];

            foreach (ChoiceOptionCard option in group.Options)
            {
                Border optionBorder = new()
                {
                    Background = System.Windows.Media.Brushes.White,
                    BorderBrush = GetBrush("#E2E8F0"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(14, 10, 14, 10),
                    Margin = new Thickness(0, 0, 12, 10),
                    MinWidth = 150
                };

                System.Windows.Controls.Primitives.ToggleButton toggle;
                if (group.EffectiveMaxSelect == 1)
                {
                    var radio = new RadioButton
                    {
                        Content = $"{option.Name} ({option.ExtraPriceText})",
                        GroupName = group.ChoiceGroupId.ToString(),
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 13,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = GetBrush("#1E293B"),
                        Tag = option
                    };
                    toggle = radio;
                }
                else
                {
                    var check = new CheckBox
                    {
                        Content = $"{option.Name} ({option.ExtraPriceText})",
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 13,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = GetBrush("#1E293B"),
                        Tag = option
                    };
                    toggle = check;
                }

                optionBorder.MouseDown += (s, e) =>
                {
                    if (toggle is RadioButton r)
                    {
                        r.IsChecked = true;
                    }
                    else if (toggle is CheckBox c)
                    {
                        c.IsChecked = !c.IsChecked;
                    }
                };

                toggle.Checked += (s, e) =>
                {
                    if (group.EffectiveMaxSelect == 1)
                    {
                        option.IsSelected = true;
                        UpdateOptionSelection(optionBorder, true);
                        foreach (var sibling in groupChecks.Where(x => x != toggle))
                        {
                            sibling.IsChecked = false;
                            if (sibling.Tag is ChoiceOptionCard siblingOption)
                            {
                                siblingOption.IsSelected = false;
                            }
                        }
                    }
                    else
                    {
                        if (group.Options.Count(x => x.IsSelected) >= group.EffectiveMaxSelect)
                        {
                            toggle.IsChecked = false;
                            return;
                        }
                        option.IsSelected = true;
                        UpdateOptionSelection(optionBorder, true);
                    }
                };

                toggle.Unchecked += (s, e) =>
                {
                    option.IsSelected = false;
                    UpdateOptionSelection(optionBorder, false);
                };

                toggle.IsChecked = option.IsSelected;
                UpdateOptionSelection(optionBorder, option.IsSelected);

                optionBorder.Child = toggle;
                groupChecks.Add(toggle);
                optionsPanel.Children.Add(optionBorder);
            }

            groupPanel.Children.Add(optionsPanel);
            groupBox.Child = groupPanel;
            body.Children.Add(groupBox);
        }

        Border noteGroupBox = new()
        {
            BorderBrush = GetBrush("#E2E8F0"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 14),
            Background = System.Windows.Media.Brushes.White
        };

        StackPanel noteGroupPanel = new();

        StackPanel noteTitlePanel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        Border noteBlueBar = new()
        {
            Width = 4,
            Height = 16,
            Background = GetBrush("#0866E5"),
            CornerRadius = new CornerRadius(2),
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        noteTitlePanel.Children.Add(noteBlueBar);
        noteTitlePanel.Children.Add(new TextBlock
        {
            Text = "Ghi chú",
            FontWeight = FontWeights.Bold,
            FontSize = 15,
            Foreground = GetBrush("#0F172A"),
            VerticalAlignment = VerticalAlignment.Center
        });
        noteGroupPanel.Children.Add(noteTitlePanel);

        Grid noteGrid = new();
        TextBox noteBox = new()
        {
            MinHeight = 80,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(12, 10, 12, 24),
            FontSize = 14,
            Foreground = GetBrush("#0F172A"),
            BorderBrush = GetBrush("#E2E8F0"),
            Background = System.Windows.Media.Brushes.White
        };

        TextBlock counterText = new()
        {
            Text = "0/200",
            FontSize = 11,
            Foreground = GetBrush("#94A3B8"),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 12, 8)
        };

        noteBox.TextChanged += (s, e) =>
        {
            if (noteBox.Text.Length > 200)
            {
                noteBox.Text = noteBox.Text.Substring(0, 200);
                noteBox.CaretIndex = 200;
            }
            counterText.Text = $"{noteBox.Text.Length}/200";
        };

        noteGrid.Children.Add(noteBox);
        noteGrid.Children.Add(counterText);
        noteGroupPanel.Children.Add(noteGrid);
        noteGroupBox.Child = noteGroupPanel;
        body.Children.Add(noteGroupBox);

        scroll.Content = body;
        Grid.SetRow(scroll, 1);
        root.Children.Add(scroll);

        StackPanel footer = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 14, 0, 0)
        };

        AddMenuItemDialogResult? result = null;
        Button confirmButton = new()
        {
            Content = "Thêm món",
            Width = 120,
            Height = 42,
            Background = GetBrush("#0866E5"),
            Foreground = System.Windows.Media.Brushes.White,
            BorderBrush = GetBrush("#0866E5"),
            FontWeight = FontWeights.Bold,
            Style = FindResource("RoundCornerButtonStyle") as Style
        };
        confirmButton.Click += (_, _) =>
        {
            ChoiceGroupCard? invalidGroup = groups.FirstOrDefault(x =>
                x.IsRequired && x.Options.All(option => !option.IsSelected));

            if (invalidGroup is not null)
            {
                ShowCustomMessageBox($"Vui lòng chọn {invalidGroup.GroupName}.", "Thêm món", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string choiceSummary = BuildChoiceSummary(groups);
            decimal unitPrice = item.Price + groups
                .SelectMany(x => x.Options)
                .Where(x => x.IsSelected)
                .Sum(x => x.ExtraPrice);

            List<SelectedChoiceGroupRequest> selectedChoices = groups
                .Select(group => new SelectedChoiceGroupRequest
                {
                    ChoiceGroupId = group.ChoiceGroupId,
                    ChoiceItemIds = group.Options
                        .Where(option => option.IsSelected)
                        .Select(option => option.ChoiceItemId)
                        .Where(id => id > 0)
                        .ToList()
                })
                .Where(group => group.ChoiceItemIds.Count > 0)
                .ToList();

            result = new AddMenuItemDialogResult(
                quantity,
                string.IsNullOrWhiteSpace(noteBox.Text) ? null : noteBox.Text.Trim(),
                choiceSummary,
                unitPrice,
                selectedChoices);
            dialog.Close();
        };

        Button cancelButton = new()
        {
            Content = "HỦY",
            Width = 100,
            Height = 42,
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#475569"),
            BorderBrush = GetBrush("#E2E8F0"),
            FontWeight = FontWeights.Bold,
            Style = FindResource("RoundCornerButtonStyle") as Style,
            Margin = new Thickness(12, 0, 0, 0)
        };
        cancelButton.Click += (_, _) => dialog.Close();

        footer.Children.Add(confirmButton);
        footer.Children.Add(cancelButton);
        Grid.SetRow(footer, 2);
        root.Children.Add(footer);

        mainBorder.Child = root;
        dialog.Content = mainBorder;

        ShowBorderlessDialog(dialog);
        return result;
    }
}
