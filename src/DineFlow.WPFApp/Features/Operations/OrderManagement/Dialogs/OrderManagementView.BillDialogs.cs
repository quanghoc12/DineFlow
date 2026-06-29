using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DineFlow.Services.Bills;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{
    private Window BuildMoveTableDialog(IReadOnlyList<TableCard> emptyTables)
    {
        Window dialog = new()
        {
            Title = "Chọn bàn trống để chuyển",
            Owner = Window.GetWindow(this),
            Width = 460,
            Height = 460,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        Grid root = new() { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        StackPanel titlePanel = new() { Margin = new Thickness(0, 0, 0, 16) };
        titlePanel.Children.Add(new TextBlock
        {
            Text = "Chuyển bàn ăn",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A")
        });
        titlePanel.Children.Add(new TextBlock
        {
            Text = "Chọn bàn trống muốn chuyển đến:",
            FontSize = 13,
            Foreground = GetBrush("#64748B"),
            Margin = new Thickness(0, 6, 0, 0)
        });
        root.Children.Add(titlePanel);

        ScrollViewer scroll = new() { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        WrapPanel panel = new() { Margin = new Thickness(0, 0, 0, 10) };

        foreach (TableCard table in emptyTables)
        {
            Button button = new()
            {
                Content = $"{table.TableName}\n{DisplayArea(table.Area)}",
                Tag = table,
                Width = 124,
                Height = 80,
                Margin = new Thickness(0, 0, 12, 12),
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Background = System.Windows.Media.Brushes.White,
                Foreground = GetBrush("#1E293B"),
                BorderBrush = GetBrush("#E2E8F0"),
                Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
            };

            button.Click += async (_, _) =>
            {
                await MoveSelectedSessionToTableAsync(table);
                dialog.Close();
            };

            panel.Children.Add(button);
        }

        scroll.Content = panel;
        Grid.SetRow(scroll, 1);
        root.Children.Add(scroll);

        Button cancelBtn = new()
        {
            Content = "HỦY",
            Height = 40,
            HorizontalAlignment = HorizontalAlignment.Right,
            Width = 100,
            Margin = new Thickness(0, 14, 0, 0),
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#64748B"),
            BorderBrush = GetBrush("#CBD5E1"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        cancelBtn.Click += (_, _) => dialog.Close();
        Grid.SetRow(cancelBtn, 2);
        root.Children.Add(cancelBtn);

        dialog.Content = root;
        // NOTE: caller (Tables.cs) calls ShowBorderlessDialog — do NOT call it here.
        return dialog;
    }

    private void ShowSplitBillDialog()
    {
        if (_selectedTable is null || _selectedBill is null || _selectedBill.Lines.Count == 0)
        {
            ShowCustomMessageBox("Bill hiện tại chưa có món để tách.", "Tách bill", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Window dialog = new()
        {
            Title = "Tách bill",
            Owner = Window.GetWindow(this),
            Width = 580,
            Height = 540,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        Grid root = new() { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        StackPanel headerPanel = new() { Margin = new Thickness(0, 0, 0, 16) };
        headerPanel.Children.Add(new TextBlock
        {
            Text = $"Tách bill • {_selectedTable.TableName}",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A")
        });
        headerPanel.Children.Add(new TextBlock
        {
            Text = "Chọn bill đích cần chuyển món sang:",
            FontSize = 13,
            Foreground = GetBrush("#64748B"),
            Margin = new Thickness(0, 12, 0, 6)
        });

        ComboBox targetCombo = new()
        {
            Height = 38,
            DisplayMemberPath = nameof(SplitTargetOption.Label),
            SelectedValuePath = nameof(SplitTargetOption.Bill),
            Margin = new Thickness(0, 0, 0, 4),
            Padding = new Thickness(10, 4, 10, 4)
        };
        targetCombo.Items.Add(new SplitTargetOption("Bill mới", null));
        foreach (BillPreview bill in CurrentBills.Where(x => x != _selectedBill))
        {
            targetCombo.Items.Add(new SplitTargetOption(bill.DisplayName, bill));
        }
        targetCombo.SelectedIndex = 0;
        
        headerPanel.Children.Add(targetCombo);
        root.Children.Add(headerPanel);

        List<SplitLineSelection> selections = _selectedBill.Lines.Select(x => new SplitLineSelection(x)).ToList();
        StackPanel linePanel = new();

        foreach (SplitLineSelection selection in selections)
        {
            Border rowCard = BuildSplitLineRowCard(selection);
            linePanel.Children.Add(rowCard);
        }

        ScrollViewer scroll = new() { Content = linePanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(0, 0, 0, 10) };
        Grid.SetRow(scroll, 1);
        root.Children.Add(scroll);

        StackPanel footer = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
        Button cancelButton = new() 
        { 
            Content = "HỦY BỎ", 
            Width = 96, 
            Height = 40, 
            Margin = new Thickness(0, 0, 10, 0),
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#64748B"),
            BorderBrush = GetBrush("#CBD5E1"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        cancelButton.Click += (_, _) => dialog.Close();

        Button saveButton = new() 
        { 
            Content = "XÁC NHẬN TÁCH", 
            Width = 130, 
            Height = 40, 
            FontWeight = FontWeights.Bold,
            Background = GetBrush("#0866E5"),
            Foreground = System.Windows.Media.Brushes.White,
            BorderBrush = GetBrush("#0866E5"),
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        saveButton.Click += async (_, _) =>
        {
            int moveQuantity = selections.Sum(x => x.SelectedQuantity);
            int sourceQuantity = _selectedBill.Lines.Sum(x => x.Quantity);

            if (moveQuantity <= 0)
            {
                ShowCustomMessageBox("Chọn ít nhất 1 món hoặc 1 số lượng để tách.", "Tách bill", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (sourceQuantity - moveQuantity <= 0)
            {
                ShowCustomMessageBox("Không được tách hết bill. Bill gốc phải còn ít nhất 1 số lượng.", "Tách bill", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_usesApiData)
                {
                    BillPreview? selectedTargetBill = targetCombo.SelectedValue as BillPreview;
                    SplitBillBatchRequest request = new()
                    {
                        SourceBillId = _selectedBill.BillId,
                        TargetBillId = selectedTargetBill?.BillId,
                        CreateNewBill = selectedTargetBill is null,
                        NewBillName = selectedTargetBill is null ? $"Bill {CurrentBills.Count + 1}" : null,
                        Items = selections
                            .Where(x => x.SelectedQuantity > 0)
                            .Select(x => new SplitBillItemRequest
                            {
                                BillDetailId = x.Line.BillDetailId,
                                QuantityToMove = x.SelectedQuantity
                            })
                            .ToList()
                    };

                    BillDto result = await _apiClient.SplitBillBatchAsync(request);
                    await ReloadTableFromApiAsync(_selectedTable.TableId, result.BillId);
                }
                else
                {
                    BillPreview targetBill = targetCombo.SelectedValue as BillPreview ?? _selectedTable.CreateNextBill();
                    ApplySplit(_selectedBill, targetBill, selections);
                    ReloadCurrentBills(_selectedTable);
                    SetSelectedBill(targetBill);
                    RefreshBill();
                }

                dialog.Close();
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox(GetFriendlyError(ex), "Tách bill", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        };

        footer.Children.Add(cancelButton);
        footer.Children.Add(saveButton);
        Grid.SetRow(footer, 2);
        root.Children.Add(footer);

        dialog.Content = root;
        ShowBorderlessDialog(dialog);
    }

    private static Border BuildSplitLineRowCard(SplitLineSelection selection)
    {
        Border card = new()
        {
            Background = System.Windows.Media.Brushes.White,
            BorderBrush = GetBrush("#E2E8F0"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 0, 10)
        };

        Grid row = new();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        StackPanel textPanel = new();
        textPanel.Children.Add(new TextBlock 
        { 
            Text = selection.Line.ItemName, 
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Foreground = GetBrush("#0F172A")
        });
        textPanel.Children.Add(new TextBlock
        {
            Text = $"{selection.Line.ChoiceSummary} • Đang có: {selection.Line.Quantity}",
            Foreground = GetBrush("#64748B"),
            FontSize = 12,
            Margin = new Thickness(0, 4, 0, 0)
        });

        StackPanel stepper = new() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        TextBlock quantityText = new()
        {
            Text = "0",
            Width = 36,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#0F172A")
        };

        Button minusButton = new() 
        { 
            Content = "-", 
            Width = 32, 
            Height = 32,
            Background = GetBrush("#F1F5F9"),
            BorderBrush = GetBrush("#CBD5E1"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        minusButton.Click += (_, _) =>
        {
            if (selection.SelectedQuantity > 0)
            {
                selection.SelectedQuantity--;
                quantityText.Text = selection.SelectedQuantity.ToString(CultureInfo.InvariantCulture);
            }
        };

        Button plusButton = new() 
        { 
            Content = "+", 
            Width = 32, 
            Height = 32,
            Background = GetBrush("#F1F5F9"),
            BorderBrush = GetBrush("#CBD5E1"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        plusButton.Click += (_, _) =>
        {
            if (selection.SelectedQuantity < selection.Line.Quantity)
            {
                selection.SelectedQuantity++;
                quantityText.Text = selection.SelectedQuantity.ToString(CultureInfo.InvariantCulture);
            }
        };

        stepper.Children.Add(minusButton);
        stepper.Children.Add(quantityText);
        stepper.Children.Add(plusButton);

        row.Children.Add(textPanel);
        Grid.SetColumn(stepper, 1);
        row.Children.Add(stepper);
        card.Child = row;
        return card;
    }

    private void ShowMergeBillDialog()
    {
        if (_selectedTable is null || CurrentBills.Count < 2)
        {
            ShowCustomMessageBox("Bàn cần có ít nhất 2 bill để gộp.", "Gộp bill", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Window dialog = new()
        {
            Title = "Gộp bill",
            Owner = Window.GetWindow(this),
            Width = 440,
            Height = 280,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize
        };

        Grid root = new() { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        ComboBox sourceCombo = BuildBillComboBox();
        sourceCombo.SelectedItem = _selectedBill ?? CurrentBills.First();
        ComboBox targetCombo = BuildBillComboBox();
        targetCombo.SelectedItem = CurrentBills.FirstOrDefault(x => x != sourceCombo.SelectedItem);

        root.Children.Add(BuildLabeledControl("Bill nguồn (chuyển đi)", sourceCombo));
        Grid.SetRow(root.Children[^1], 0);
        root.Children.Add(BuildLabeledControl("Gộp tới bill (đích)", targetCombo));
        Grid.SetRow(root.Children[^1], 1);

        StackPanel footer = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 18, 0, 0) };
        Button cancelButton = new() 
        { 
            Content = "HỦY BỎ", 
            Width = 96, 
            Height = 40, 
            Margin = new Thickness(0, 0, 10, 0),
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#64748B"),
            BorderBrush = GetBrush("#CBD5E1"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        cancelButton.Click += (_, _) => dialog.Close();

        Button saveButton = new() 
        { 
            Content = "XÁC NHẬN GỘP", 
            Width = 130, 
            Height = 40, 
            FontWeight = FontWeights.Bold,
            Background = GetBrush("#0866E5"),
            Foreground = System.Windows.Media.Brushes.White,
            BorderBrush = GetBrush("#0866E5"),
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        saveButton.Click += async (_, _) =>
        {
            if (sourceCombo.SelectedItem is not BillPreview sourceBill ||
                targetCombo.SelectedItem is not BillPreview targetBill ||
                sourceBill == targetBill)
            {
                ShowCustomMessageBox("Chọn 2 bill khác nhau để gộp.", "Gộp bill", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_usesApiData)
                {
                    BillDto result = await _apiClient.MergeBillAsync(new MergeBillRequest
                    {
                        SourceBillId = sourceBill.BillId,
                        TargetBillId = targetBill.BillId
                    });

                    await ReloadTableFromApiAsync(_selectedTable.TableId, result.BillId);
                }
                else
                {
                    ApplyMerge(sourceBill, targetBill);
                    ReloadCurrentBills(_selectedTable);
                    SetSelectedBill(targetBill);
                    RefreshBill();
                }

                dialog.Close();
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox(GetFriendlyError(ex), "Gộp bill", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        };

        footer.Children.Add(cancelButton);
        footer.Children.Add(saveButton);
        Grid.SetRow(footer, 2);
        root.Children.Add(footer);

        dialog.Content = root;
        ShowBorderlessDialog(dialog);
    }

    private ComboBox BuildBillComboBox()
    {
        ComboBox comboBox = new()
        {
            Height = 38,
            DisplayMemberPath = nameof(BillPreview.DisplayName),
            Margin = new Thickness(0, 6, 0, 14),
            Padding = new Thickness(10, 4, 10, 4)
        };

        foreach (BillPreview bill in CurrentBills)
        {
            comboBox.Items.Add(bill);
        }

        return comboBox;
    }

    private static FrameworkElement BuildLabeledControl(string label, Control control)
    {
        StackPanel panel = new();
        panel.Children.Add(new TextBlock 
        { 
            Text = label, 
            FontWeight = FontWeights.Bold,
            Foreground = GetBrush("#475569")
        });
        panel.Children.Add(control);
        return panel;
    }
}
