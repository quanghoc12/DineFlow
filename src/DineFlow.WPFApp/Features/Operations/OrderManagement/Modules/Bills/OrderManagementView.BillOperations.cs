using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Bills;
using DineFlow.Services.Menu;
using DineFlow.Services.Orders;
using DineFlow.Services.Requests;
using DineFlow.WPFApp.Services.Api;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{
    private static BillPreview MapBill(BillDto bill)
    {
        BillPreview preview = new(bill.BillId, bill.BillNo, bill.BillName, bill.IsDefault);
        preview.SelectedChannelId = bill.SalesChannelId;
        preview.SelectedChannelCode = bill.SalesChannelCode;
        preview.SelectedChannelName = string.IsNullOrWhiteSpace(bill.SalesChannelName)
            ? "Bảng giá chung"
            : bill.SalesChannelName;
        foreach (BillDetailDto detail in bill.Details)
        {
            preview.Lines.Add(new BillLinePreview(
                detail.BillDetailId,
                detail.MenuItemId,
                detail.ItemName,
                BuildBillLineDescription(detail.ChoiceSummary, detail.Note),
                detail.Quantity,
                detail.NotifiedQuantity,
                detail.UnitPrice));
        }

        return preview;
    }

    private static string BuildBillLineDescription(string? choiceSummary, string? note)
    {
        string description = string.IsNullOrWhiteSpace(choiceSummary)
            ? "Không có ghi chú/Món thêm"
            : choiceSummary.Trim();

        return string.IsNullOrWhiteSpace(note)
            ? description
            : $"{description} | Ghi chú: {note.Trim()}";
    }

    private void MarkBillQuantityChanged()
    {
        RefreshNotifyButtonState();
    }

    private void ClearBillQuantityChanged()
    {
        SetNotifyButtonState(false);
    }

    private void RefreshNotifyButtonState()
    {
        bool hasUnnotifiedChanges = _selectedBill?.Lines.Any(x => x.Quantity > x.NotifiedQuantity) == true;
        SetNotifyButtonState(hasUnnotifiedChanges);
        UnnotifiedWarning.Visibility = hasUnnotifiedChanges ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetNotifyButtonState(bool isEnabled)
    {
        NotifyButton.IsEnabled = isEnabled;
        NotifyButton.Opacity = isEnabled ? 1 : 0.55;
        NotifyButton.Background = new System.Windows.Media.SolidColorBrush(
            isEnabled
                ? System.Windows.Media.Color.FromRgb(47, 128, 237)
                : System.Windows.Media.Color.FromRgb(241, 245, 249));
        NotifyButton.BorderBrush = new System.Windows.Media.SolidColorBrush(
            isEnabled
                ? System.Windows.Media.Color.FromRgb(47, 128, 237)
                : System.Windows.Media.Color.FromRgb(203, 213, 225));
        NotifyButton.Foreground = new System.Windows.Media.SolidColorBrush(
            isEnabled
                ? System.Windows.Media.Colors.White
                : System.Windows.Media.Color.FromRgb(148, 163, 184));
    }

    private async void AddBillButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            TableCard table = EnsureSelectedTable();

            if (_usesApiData)
            {
                TableSessionDto session = table.TableSessionId.HasValue
                    ? new TableSessionDto { TableSessionId = table.TableSessionId.Value, TableId = table.TableId }
                    : await _apiClient.GetOrCreateTableSessionAsync(table.TableId);

                BillDto billDto = await _apiClient.CreateEmptyBillAsync(session.TableSessionId, $"Bill {table.Bills.Count + 1}");
                table.TableSessionId = session.TableSessionId;
                table.MarkServing();
                await ReloadTableFromApiAsync(table.TableId, billDto.BillId);
                return;
            }

            throw new InvalidOperationException("Không thể tạo bill bằng dữ liệu tạm. Vui lòng kết nối database.");
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Thêm bill", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BillTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: BillPreview bill })
        {
            SetSelectedBill(bill);
            RefreshBill();
        }
    }

    private async void CancelBillButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: BillPreview bill } || _selectedTable is null)
        {
            return;
        }

        string reason;
        if (bill.Lines.Count == 0)
        {
            reason = "Bill trống";
        }
        else
        {
            CancelBillDialogResult? result = ShowCancelBillDialog(bill);
            if (result is null)
            {
                return;
            }

            reason = result.Reason;
        }

        try
        {
            if (bill.Lines.Count > 0)
            {
                PrintKitchenBillCancelPdf(bill, reason);
            }

            if (_usesApiData)
            {
                await _apiClient.CancelBillAsync(bill.BillId, reason);
                await ReloadTableFromApiAsync(_selectedTable.TableId);
                return;
            }

            bool wasDefault = bill.IsDefault;
            _selectedTable.Bills.Remove(bill);

            if (wasDefault && _selectedTable.Bills.Count > 0)
            {
                BillPreview nextDefaultBill = _selectedTable.Bills.OrderBy(x => x.BillNo).First();
                nextDefaultBill.IsDefault = true;
                nextDefaultBill.NotifyChanged();
            }

            SetSelectedBill(_selectedTable.Bills.FirstOrDefault(x => x.IsDefault) ?? _selectedTable.Bills.FirstOrDefault());
            ReloadCurrentBills(_selectedTable);
            _selectedTable.NotifyChanged();
            ApplyTableFilters();
            RefreshBill();
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Hủy bill", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void NotifyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBill is null)
        {
            return;
        }

        try
        {
            List<(BillLinePreview Line, int Quantity)> kitchenLines = _selectedBill.Lines
                .Select(line => (Line: line, Quantity: Math.Max(0, line.Quantity - line.NotifiedQuantity)))
                .Where(x => x.Quantity > 0)
                .ToList();

            if (_usesApiData)
            {
                BillDto billDto = await _apiClient.NotifyBillAsync(_selectedBill.BillId);
                UpdateSelectedBillFromDto(billDto);
                PrintKitchenOrderPdfIfNeeded(kitchenLines);
                ClearBillQuantityChanged();
                return;
            }

            foreach (BillLinePreview line in _selectedBill.Lines)
            {
                line.NotifiedQuantity = line.Quantity;
                line.NotifyChanged();
            }

            PrintKitchenOrderPdfIfNeeded(kitchenLines);
            ClearBillQuantityChanged();
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void PaymentButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBill is null || _selectedBill.Lines.Count == 0)
        {
            ShowCustomMessageBox("Bill chưa có món để thanh toán.", "Thanh toán", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        bool hasUnnotifiedChanges = _selectedBill.Lines.Any(x => x.Quantity > x.NotifiedQuantity);
        if (hasUnnotifiedChanges)
        {
            MessageBoxResult confirmation = ShowCustomMessageBox(
                "Có đơn hàng chưa thông báo, bạn chắc sẽ thanh toán chứ?",
                "Đơn chưa thông báo",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.OK)
            {
                return;
            }

            try
            {
                if (_usesApiData)
                {
                    BillDto notifiedBill = await _apiClient.NotifyBillAsync(_selectedBill.BillId);
                    UpdateSelectedBillFromDto(notifiedBill);
                }
                else
                {
                    foreach (BillLinePreview line in _selectedBill.Lines)
                    {
                        line.NotifiedQuantity = line.Quantity;
                        line.NotifyChanged();
                    }
                    ClearBillQuantityChanged();
                }
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox(
                    GetFriendlyError(ex),
                    "Không thể thông báo đơn",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
        }

        PaymentDialogResult? payment = ShowPaymentDialog(_selectedBill);
        if (payment is null)
        {
            return;
        }

        try
        {
            BillPreview paidBill = _selectedBill;
            string paymentMethods = string.Join(", ", payment.Parts.Select(x => x.Method));
            if (_usesApiData)
            {
                PaymentResultDto result = await _apiClient.ConfirmCombinedPaymentAsync(
                    new ConfirmCombinedPaymentRequest
                    {
                        BillId = _selectedBill.BillId,
                        Payments = payment.Parts
                            .Select(x => new PaymentPartRequest
                            {
                                PaymentMethod = x.Method,
                                Amount = x.Amount
                            })
                            .ToList()
                    });

                PrintPaymentReceiptPdf(paidBill, payment, paymentMethods);
                ShowCustomMessageBox(
                    "Thanh toán thành công.",
                    "Thanh toán",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                if (_selectedTable is not null)
                {
                    await ReloadTableFromApiAsync(_selectedTable.TableId);
                }
                return;
            }

            PrintPaymentReceiptPdf(paidBill, payment, paymentMethods);
            _selectedTable?.Bills.Remove(_selectedBill);
            SetSelectedBill(_selectedTable?.Bills.FirstOrDefault());
            if (_selectedTable is not null)
            {
                ReloadCurrentBills(_selectedTable);
            }
            RefreshBill();
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Thanh toán", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void RenameBillButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBill is null)
        {
            ShowCustomMessageBox("Hãy chọn bill cần đổi tên.", "Đổi tên bill", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Window dialog = new()
        {
            Title = "Đổi tên bill",
            Owner = Window.GetWindow(this),
            Width = 420,
            Height = 210,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(new TextBlock
        {
            Text = "Tên bill",
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        TextBox nameBox = new()
        {
            Text = _selectedBill.BillName,
            Height = 38,
            MaxLength = 100,
            Padding = new Thickness(8)
        };
        root.Children.Add(nameBox);

        StackPanel actions = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 18, 0, 0)
        };
        Button cancel = new() { Content = "HỦY", Width = 90, Height = 38,
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#64748B"),
            BorderBrush = GetBrush("#CBD5E1"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        Button save = new()
        {
            Content = "Lưu",
            Width = 90,
            Height = 38,
            Margin = new Thickness(8, 0, 0, 0),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(11, 103, 208)),
            Foreground = System.Windows.Media.Brushes.White
        };
        cancel.Click += (_, _) => dialog.Close();
        save.Click += async (_, _) =>
        {
            string newName = nameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                ShowCustomMessageBox("Tên bill không được để trống.", "Đổi tên bill", MessageBoxButton.OK, MessageBoxImage.Warning);
                nameBox.Focus();
                return;
            }

            try
            {
                if (_usesApiData)
                {
                    BillDto billDto = await _apiClient.RenameBillAsync(_selectedBill.BillId, newName);
                    UpdateSelectedBillFromDto(billDto);
                }
                else
                {
                    _selectedBill.Rename(newName);
                    ReloadCurrentBills(_selectedTable!);
                }

                dialog.DialogResult = true;
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox(GetFriendlyError(ex), "Đổi tên bill", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        };
        actions.Children.Add(cancel);
        actions.Children.Add(save);
        root.Children.Add(actions);
        dialog.Content = root;
        dialog.Loaded += (_, _) =>
        {
            nameBox.Focus();
            nameBox.SelectAll();
        };
        ShowBorderlessDialog(dialog);
    }

    private static decimal ParseMoney(string? value)
    {
        string normalized = (value ?? string.Empty).Replace(",", string.Empty).Replace(".", string.Empty).Trim();
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) ? amount : 0;
    }

    private void SplitMergeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTable is null || CurrentBills.Count == 0)
        {
            ShowCustomMessageBox("Chọn bàn có bill trước khi tách/gộp.", "Tách ghép", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Window dialog = new()
        {
            Title = "Tách ghép bill",
            Owner = Window.GetWindow(this),
            Width = 300,
            Height = 220,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize
        };

        StackPanel panel = new() { Margin = new Thickness(22) };
        panel.Children.Add(new TextBlock
        {
            Text = $"{_selectedTable.TableName} • Chọn thao tác",
            FontWeight = FontWeights.Bold,
            FontSize = 15,
            Foreground = GetBrush("#0F172A"),
            Margin = new Thickness(0, 0, 0, 16)
        });

        Button splitButton = new()
        {
            Content = "Tách bill",
            Height = 44,
            Margin = new Thickness(0, 0, 0, 10),
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#0866E5"),
            BorderBrush = GetBrush("#D1E9FF"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        splitButton.Click += (_, _) =>
        {
            dialog.Close();
            ShowSplitBillDialog();
        };

        Button mergeButton = new()
        {
            Content = "Gộp bill",
            Height = 44,
            Background = System.Windows.Media.Brushes.White,
            Foreground = GetBrush("#475569"),
            BorderBrush = GetBrush("#CBD5E1"),
            FontWeight = FontWeights.Bold,
            Style = (Style)Application.Current.TryFindResource("RoundCornerButtonStyle")
        };
        mergeButton.Click += (_, _) =>
        {
            dialog.Close();
            ShowMergeBillDialog();
        };

        panel.Children.Add(splitButton);
        panel.Children.Add(mergeButton);
        dialog.Content = panel;
        ShowBorderlessDialog(dialog);
    }

    private async void IncreaseLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: BillLinePreview line })
        {
            return;
        }

        if (_usesApiData)
        {
            try
            {
                BillDto billDto = await _apiClient.AdjustBillDetailQuantityAsync(line.BillDetailId, line.Quantity + 1);
                UpdateSelectedBillFromDto(billDto);
                MarkBillQuantityChanged();
                return;
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox(GetFriendlyError(ex), "Tăng số lượng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        line.Quantity++;
        line.NotifyChanged();
        _selectedTable?.NotifyChanged();
        RefreshBill();
        MarkBillQuantityChanged();
    }

    private async void DecreaseLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: BillLinePreview line } button || _selectedBill is null)
        {
            return;
        }

        bool isDeleteButton = string.Equals(button.ToolTip?.ToString(), "Giảm hoặc hủy món", StringComparison.Ordinal) ||
            string.Equals(button.Content?.ToString(), "\uE74D", StringComparison.Ordinal);
        bool isMinusButton = !isDeleteButton;

        if (isMinusButton && line.Quantity - 1 >= line.NotifiedQuantity)
        {
            await AdjustLineQuantityWithoutCancelReasonAsync(line, line.Quantity - 1);
            return;
        }

        if (!isMinusButton && line.NotifiedQuantity == 0)
        {
            await AdjustLineQuantityWithoutCancelReasonAsync(line, 0);
            return;
        }

        int maxCancelQuantity = isMinusButton ? line.NotifiedQuantity : Math.Max(1, line.NotifiedQuantity);
        CancelLineDialogResult? result = ShowCancelLineDialog(line, maxCancelQuantity);
        if (result is null)
        {
            return;
        }

        int newQuantity = isMinusButton
            ? line.Quantity - result.Quantity
            : line.NotifiedQuantity - result.Quantity;

        if (_usesApiData)
        {
            try
            {
                BillDto billDto = await _apiClient.AdjustBillDetailQuantityAsync(
                    line.BillDetailId,
                    newQuantity,
                    result.Reason);
                PrintKitchenCancelPdf(line, result.Quantity, result.Reason);
                UpdateSelectedBillFromDto(billDto);
                MarkBillQuantityChanged();
                return;
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox(GetFriendlyError(ex), "Giảm số lượng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        line.Quantity = newQuantity;
        line.NotifiedQuantity = Math.Min(line.NotifiedQuantity, newQuantity);
        PrintKitchenCancelPdf(line, result.Quantity, result.Reason);

        if (line.Quantity <= 0)
        {
            _selectedBill.Lines.Remove(line);
        }
        else
        {
            line.NotifyChanged();
        }

        _selectedTable?.NotifyChanged();
        RefreshBill();
        MarkBillQuantityChanged();
    }

    private void LineDetailButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: BillLinePreview line })
        {
            ShowLineDetailDialog(line);
        }
    }

    private async Task AdjustLineQuantityWithoutCancelReasonAsync(BillLinePreview line, int newQuantity)
    {
        if (_selectedBill is null)
        {
            return;
        }

        if (_usesApiData)
        {
            try
            {
                BillDto billDto = await _apiClient.AdjustBillDetailQuantityAsync(line.BillDetailId, newQuantity);
                UpdateSelectedBillFromDto(billDto);
                MarkBillQuantityChanged();
                return;
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox(GetFriendlyError(ex), "Giảm số lượng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        line.Quantity = newQuantity;
        if (line.Quantity <= 0)
        {
            _selectedBill.Lines.Remove(line);
        }
        else
        {
            line.NotifyChanged();
        }

        _selectedTable?.NotifyChanged();
        RefreshBill();
        MarkBillQuantityChanged();
    }

    private void SetSelectedBill(BillPreview? bill)
    {
        if (_selectedTable is not null)
        {
            foreach (BillPreview item in _selectedTable.Bills)
            {
                item.IsSelected = item == bill;
                item.NotifyChanged();
            }
        }

        _selectedBill = bill;
        _ = ReloadMenuCatalogForSelectedBillAsync();
    }

    private int? GetSelectedBillIdForCurrentTable()
    {
        if (_selectedTable is null ||
            _selectedBill is null ||
            _selectedBill.BillId <= 0)
        {
            return null;
        }

        return _selectedTable.Bills.Any(x => x.BillId == _selectedBill.BillId)
            ? _selectedBill.BillId
            : null;
    }

    private BillPreview EnsureSelectedBill()
    {
        TableCard table = EnsureSelectedTable();

        if (_selectedBill is not null && table.Bills.Contains(_selectedBill))
        {
            return _selectedBill;
        }

        BillPreview bill = table.Bills.FirstOrDefault(x => x.IsDefault)
            ?? table.Bills.FirstOrDefault()
            ?? throw new InvalidOperationException("Bàn chưa có bill trong database. Vui lòng tạo bill trước.");

        SelectTable(table, bill);
        return bill;
    }

    private void RefreshBill()
    {
        CurrentBillLines.Clear();

        if (_selectedBill is not null)
        {
            foreach (BillLinePreview line in _selectedBill.Lines)
            {
                CurrentBillLines.Add(line);
            }
            CommonPriceListButtonText.Text = _selectedBill.SelectedChannelName;
        }
        else
        {
            CommonPriceListButtonText.Text = "Bảng giá chung";
        }

        BillLinesList.ItemsSource = CurrentBillLines;
        EmptyBillPanel.Visibility = CurrentBillLines.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        BillLinesScroll.Visibility = CurrentBillLines.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        BillTotalText.Text = Money(CurrentBillLines.Sum(x => x.Total));
        RefreshNotifyButtonState();
        _selectedTable?.NotifyChanged();
    }

    private async Task MoveSelectedSessionToTableAsync(TableCard targetTable)
    {
        if (_selectedTable is null || !_selectedTable.HasSession || targetTable.HasSession)
        {
            return;
        }

        if (_usesApiData)
        {
            try
            {
                int sourceSessionId = _selectedTable.TableSessionId
                    ?? throw new InvalidOperationException("Bàn nguồn chưa có session.");

                await _apiClient.MoveTableAsync(sourceSessionId, targetTable.TableId);
                int targetTableId = targetTable.TableId;
                await LoadFromApiAsync();
                TableCard? movedTable = _tables.FirstOrDefault(x => x.TableId == targetTableId);
                if (movedTable is not null)
                {
                    SelectTable(movedTable);
                }

                return;
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox(GetFriendlyError(ex), "Chuyển bàn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        TableCard sourceTable = _selectedTable;
        List<BillPreview> movingBills = sourceTable.Bills.ToList();
        int? movingSessionId = sourceTable.TableSessionId ?? 1;
        sourceTable.Bills.Clear();
        sourceTable.TableSessionId = null;
        sourceTable.Status = "Available";

        targetTable.Bills.Clear();
        targetTable.TableSessionId = movingSessionId;
        foreach (BillPreview bill in movingBills)
        {
            targetTable.Bills.Add(bill);
        }

        targetTable.Status = movingBills.Sum(x => x.Total) > 0 ? "Occupied" : "Available";
        sourceTable.NotifyChanged();
        targetTable.NotifyChanged();
        ApplyTableFilters();
        SelectTable(targetTable);
    }

    private void ApplySplit(BillPreview sourceBill, BillPreview targetBill, IReadOnlyList<SplitLineSelection> selections)
    {
        foreach (SplitLineSelection selection in selections.Where(x => x.SelectedQuantity > 0))
        {
            BillLinePreview sourceLine = selection.Line;
            BillLinePreview movingLine = sourceLine.CloneWithQuantity(selection.SelectedQuantity);
            BillLinePreview? existingTargetLine = targetBill.Lines.FirstOrDefault(x => x.CanMergeWith(movingLine));

            if (existingTargetLine is null)
            {
                targetBill.Lines.Add(movingLine);
            }
            else
            {
                existingTargetLine.Quantity += movingLine.Quantity;
                existingTargetLine.NotifiedQuantity += movingLine.NotifiedQuantity;
                existingTargetLine.NotifyChanged();
            }

            sourceLine.Quantity -= selection.SelectedQuantity;
            sourceLine.NotifiedQuantity = Math.Max(0, sourceLine.NotifiedQuantity - movingLine.NotifiedQuantity);
            if (sourceLine.Quantity <= 0)
            {
                sourceBill.Lines.Remove(sourceLine);
            }
            else
            {
                sourceLine.NotifyChanged();
            }
        }

        _selectedTable?.NotifyChanged();
    }

    private void ApplyMerge(BillPreview sourceBill, BillPreview targetBill)
    {
        foreach (BillLinePreview line in sourceBill.Lines.ToList())
        {
            BillLinePreview? existingLine = targetBill.Lines.FirstOrDefault(x => x.CanMergeWith(line));
            if (existingLine is null)
            {
                targetBill.Lines.Add(line);
            }
            else
            {
                existingLine.Quantity += line.Quantity;
                existingLine.NotifiedQuantity += line.NotifiedQuantity;
                existingLine.NotifyChanged();
            }
        }

        bool targetShouldBeDefault = sourceBill.IsDefault || targetBill.IsDefault;
        _selectedTable!.Bills.Remove(sourceBill);

        if (targetShouldBeDefault)
        {
            foreach (BillPreview bill in _selectedTable.Bills)
            {
                bill.IsDefault = bill == targetBill;
                bill.NotifyChanged();
            }
        }

        _selectedTable.NotifyChanged();
    }

    private static string Money(decimal value)
    {
        return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0}", value);
    }

    private async void CommonPriceListButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBill is null)
        {
            ShowCustomMessageBox("Vui lòng chọn hóa đơn trước khi áp dụng bảng giá kênh bán.", "Áp dụng bảng giá", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (sender is not Button button) return;

        try
        {
            IReadOnlyList<ManagedSalesChannelDto> channels = await _apiClient.GetSalesChannelsAsync();
            var activeChannels = channels.Where(c => c.IsActive).ToList();

            if (activeChannels.Count == 0)
            {
                ShowCustomMessageBox("Không có kênh bán nào đang hoạt động.", "Áp dụng bảng giá", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ContextMenu contextMenu = new()
            {
                Style = FindResource("ModernContextMenuStyle") as Style
            };

            foreach (var channel in activeChannels)
            {
                System.Windows.Controls.MenuItem menuItem = new()
                {
                    Header = channel.ChannelName,
                    Tag = channel,
                    Style = FindResource("ModernMenuItemStyle") as Style
                };
                menuItem.Click += async (s, args) =>
                {
                    try
                    {
                        var selectedChannel = (ManagedSalesChannelDto)((System.Windows.Controls.MenuItem)s).Tag;

                        var result = ShowCustomMessageBox(
                            $"Bạn có chắc chắn muốn áp dụng bảng giá của kênh '{selectedChannel.ChannelName}' cho hóa đơn này không?\nCác món ăn sẽ được cập nhật lại đơn giá.",
                            "Xác nhận áp dụng bảng giá",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            BillDto updatedBill = await _apiClient.ApplySalesChannelPricingAsync(
                                _selectedBill.BillId, selectedChannel.SalesChannelId);

                            _selectedBill.CopyFrom(MapBill(updatedBill));
                            RefreshBill();

                            ShowCustomMessageBox($"Đã áp dụng bảng giá kênh '{selectedChannel.ChannelName}' thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowCustomMessageBox(GetFriendlyError(ex), "Lỗi áp dụng bảng giá", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.PlacementTarget = button;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }
        catch (Exception ex)
        {
            ShowCustomMessageBox(GetFriendlyError(ex), "Lỗi tải kênh bán", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
