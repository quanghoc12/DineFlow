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

internal sealed class SplitTargetOption(string label, BillPreview? bill)
    {
        public string Label { get; } = label;
        public BillPreview? Bill { get; } = bill;
    }

internal sealed class SplitLineSelection(BillLinePreview line)
    {
        public BillLinePreview Line { get; } = line;
        public int SelectedQuantity { get; set; }
    }

internal sealed record CancelLineDialogResult(int Quantity, string Reason);

internal sealed record CancelBillDialogResult(string Reason);

internal sealed record PaymentPart(string Method, decimal Amount);

internal sealed record PaymentDialogResult(
        IReadOnlyList<PaymentPart> Parts,
        decimal CashReceived,
        decimal CashAmount);

internal sealed record AddMenuItemDialogResult(
        int Quantity,
        string? Note,
        string ChoiceSummary,
        decimal UnitPrice,
        IReadOnlyList<SelectedChoiceGroupRequest> SelectedChoices);

public sealed class FilterOption : INotifyPropertyChanged
{
    private bool _isActive;

    public FilterOption(string value, string label)
    {
        Value = value;
        Label = label;
    }

    public string Value { get; }
    public string Label { get; }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value) return;
            _isActive = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveTag)));
        }
    }

    public string ActiveTag => IsActive ? "Active" : string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class PendingOrderCard
    {
        public PendingOrderCard(OrderDetailDto order, string tableName)
        {
            OrderId = order.OrderId;
            OrderCode = order.OrderCode;
            TableName = tableName;
            TableSessionId = order.TableSessionId;
            HeaderText = $"{tableName} - {order.OrderCode}";
            DetailText = $"{order.SalesChannelName} | {order.CreatedAt.ToLocalTime():HH:mm dd/MM} | {order.ItemCount} món";
            ItemSummary = string.Join(", ", order.Items.Select(x => $"{x.MenuItemNameSnapshot} x{x.Quantity}"));
            DetailMessage = string.Join(Environment.NewLine + Environment.NewLine, order.Items.Select(BuildLineDetail));
        }

        public int OrderId { get; }
        public string OrderCode { get; }
        public string TableName { get; }
        public int TableSessionId { get; }
        public string HeaderText { get; }
        public string DetailText { get; }
        public string ItemSummary { get; }
        public string DetailMessage { get; }

        public static string BuildLineDetail(OrderItemDetailDto item)
        {
            string choices = item.SelectedChoices.Count == 0
                ? string.Empty
                : Environment.NewLine + "Option: " + string.Join(", ", item.SelectedChoices.Select(x => $"{x.GroupNameSnapshot}: {x.ChoiceNameSnapshot}"));
            string note = string.IsNullOrWhiteSpace(item.Note)
                ? string.Empty
                : Environment.NewLine + "Ghi chú: " + item.Note;

            return $"**{item.MenuItemNameSnapshot}** x{item.Quantity} - {item.LineTotal:N0}{choices}{note}";
        }
    }

public sealed class ServiceRequestCard
    {
        public ServiceRequestCard(ServiceRequestDto request, string tableName)
        {
            RequestId = request.RequestId;
            RequestType = request.RequestType;
            TableName = tableName;
            string requestName = request.RequestType switch
            {
                "CallStaff" => "Gọi nhân viên",
                "PaymentRequest" => "Yêu cầu thanh toán",
                _ => request.RequestType
            };
            RequestName = requestName;
            HeaderText = $"{tableName} - {requestName}";

            List<string> parts = new();
            if (request.RequestType != "PaymentRequest")
            {
                if (!string.IsNullOrWhiteSpace(request.Reason))
                    parts.Add(request.Reason);
            }
            if (!string.IsNullOrWhiteSpace(request.Message))
            {
                parts.Add(request.Message);
            }
            if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                string pm = request.PaymentMethod.Trim();
                if (string.Equals(pm, "cash", StringComparison.OrdinalIgnoreCase)) pm = "Cash";
                else if (string.Equals(pm, "momo", StringComparison.OrdinalIgnoreCase)) pm = "MoMo";
                else if (string.Equals(pm, "transfer", StringComparison.OrdinalIgnoreCase)) pm = "Chuyển khoản";
                parts.Add(pm);
            }

            DetailText = string.Join(" - ", parts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct());
            if (string.IsNullOrWhiteSpace(DetailText))
            {
                DetailText = "Không có nội dung thêm";
            }
            CreatedText = request.CreatedAt.ToLocalTime().ToString("HH:mm dd/MM/yyyy", CultureInfo.CurrentCulture);
        }

        public int RequestId { get; }
        public string RequestType { get; }
        public string TableName { get; }
        public string RequestName { get; }
        public string HeaderText { get; }
        public string DetailText { get; }
        public string CreatedText { get; }
    }

public sealed class TableCard : INotifyPropertyChanged
    {
        public TableCard(string tableName, string area, string status)
            : this(0, null, tableName, area, status, int.MaxValue, 0)
        {
        }

        public TableCard(
            int tableId,
            int? tableSessionId,
            string tableName,
            string area,
            string status,
            int areaDisplayOrder = int.MaxValue,
            int tableDisplayOrder = 0)
        {
            TableId = tableId;
            TableSessionId = tableSessionId;
            TableName = tableName;
            Area = area;
            Status = status;
            AreaDisplayOrder = areaDisplayOrder;
            TableDisplayOrder = tableDisplayOrder;
        }

        public int TableId { get; }
        public int? TableSessionId { get; set; }
        public string TableName { get; }
        public string Area { get; }
        public int AreaDisplayOrder { get; }
        public int TableDisplayOrder { get; }
        public string Status { get; set; }
        public bool IsSelected { get; set; }
        public ObservableCollection<BillPreview> Bills { get; } = [];
        public bool HasSession => TableSessionId.HasValue || Bills.Count > 0;
        public string FilterStatus => HasSession ? "Serving" : "Available";
        public string IsSelectedTag => IsSelected ? "Active" : string.Empty;

        public string DisplayStatus => Status switch
        {
            "Available" => "Trống",
            "Occupied" => "Đang phục vụ",
            "WaitingPayment" => "Chờ thanh toán",
            _ => Status
        };

        public string TotalText => Bills.Count == 0
            ? string.Empty
            : OrderManagementFormatting.Money(Bills.Sum(x => x.Total));

        public event PropertyChangedEventHandler? PropertyChanged;

        public void MarkServing()
        {
            if (Bills.Count > 0 && Status == "Available")
            {
                Status = "Occupied";
            }

            NotifyChanged();
        }

        public void NotifyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayStatus)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSession)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterStatus)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedTag)));
        }
    }

public sealed class MenuItemCard
    {
        public MenuItemCard(
            int menuItemId,
            string name,
            string category,
            decimal price,
            string color,
            IReadOnlyList<ChoiceGroupCard>? choiceGroups = null,
            string? imageUrl = null,
            bool isOutOfStock = false)
        {
            MenuItemId = menuItemId;
            Name = name;
            Category = category;
            Price = price;
            Color = color;
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl;
            ChoiceGroups = choiceGroups ?? [];
            IsOutOfStock = isOutOfStock;
        }

        public int MenuItemId { get; }
        public string Name { get; }
        public string Category { get; }
        public decimal Price { get; }
        public string Color { get; }
        public string? ImageUrl { get; }
        public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
        public IReadOnlyList<ChoiceGroupCard> ChoiceGroups { get; }
        public bool IsOutOfStock { get; }
        public bool IsOrderable => !IsOutOfStock;
        public string PriceText => OrderManagementFormatting.Money(Price);
        public string Initials => string.Join("", Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x => x[0])).ToUpperInvariant();
    }

public sealed class ChoiceGroupCard : INotifyPropertyChanged
    {
        public ChoiceGroupCard(
            string groupName,
            bool isRequired,
            int effectiveMaxSelect,
            int displayOrder,
            IReadOnlyList<ChoiceOptionCard> options)
            : this(0, groupName, isRequired, effectiveMaxSelect, displayOrder, options)
        {
        }

        public ChoiceGroupCard(
            int choiceGroupId,
            string groupName,
            bool isRequired,
            int effectiveMaxSelect,
            int displayOrder,
            IReadOnlyList<ChoiceOptionCard> options)
        {
            ChoiceGroupId = choiceGroupId;
            GroupName = groupName;
            IsRequired = isRequired;
            EffectiveMaxSelect = effectiveMaxSelect;
            DisplayOrder = displayOrder;
            Options = options.ToList();
        }

        public int ChoiceGroupId { get; }
        public string GroupName { get; }
        public bool IsRequired { get; }
        public int EffectiveMaxSelect { get; }
        public int DisplayOrder { get; }
        public List<ChoiceOptionCard> Options { get; }
        public bool IsActive { get; set; }
        public string IsActiveTag => IsActive ? "Active" : string.Empty;
        public string HeaderText => IsRequired ? $"{GroupName} *" : $"{GroupName} ({EffectiveMaxSelect})";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ChoiceGroupCard Clone()
        {
            return new ChoiceGroupCard(
                ChoiceGroupId,
                GroupName,
                IsRequired,
                EffectiveMaxSelect,
                DisplayOrder,
                Options.Select(x => x.Clone()).ToList());
        }

        public void NotifyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActiveTag)));
        }
    }

public sealed class ChoiceOptionCard : INotifyPropertyChanged
    {
        public ChoiceOptionCard(string name, decimal extraPrice)
            : this(0, name, extraPrice)
        {
        }

        public ChoiceOptionCard(int choiceItemId, string name, decimal extraPrice)
        {
            ChoiceItemId = choiceItemId;
            Name = name;
            ExtraPrice = extraPrice;
        }

        public int ChoiceItemId { get; }
        public string Name { get; }
        public decimal ExtraPrice { get; }
        public bool IsSelected { get; set; }
        public string IsSelectedTag => IsSelected ? "Active" : string.Empty;
        public string ExtraPriceText => ExtraPrice == 0m
            ? "+0"
            : $"+{OrderManagementFormatting.Money(ExtraPrice)}";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ChoiceOptionCard Clone()
        {
            return new ChoiceOptionCard(ChoiceItemId, Name, ExtraPrice);
        }

        public void NotifyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedTag)));
        }
    }

public sealed class BillPreview : INotifyPropertyChanged
    {
        public BillPreview(int billNo, string billName, bool isDefault)
            : this(0, billNo, billName, isDefault)
        {
        }

        public BillPreview(int billId, int billNo, string billName, bool isDefault)
        {
            BillId = billId;
            BillNo = billNo;
            BillName = billName;
            IsDefault = isDefault;
        }

        private string _selectedChannelName = "Bảng giá chung";
        private string _selectedChannelCode = "DINE_IN";
        private int _selectedChannelId;

        public int SelectedChannelId
        {
            get => _selectedChannelId;
            set
            {
                if (_selectedChannelId == value) return;
                _selectedChannelId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChannelId)));
            }
        }

        public string SelectedChannelCode
        {
            get => _selectedChannelCode;
            set
            {
                string normalized = string.IsNullOrWhiteSpace(value) ? "DINE_IN" : value.Trim();
                if (_selectedChannelCode == normalized) return;
                _selectedChannelCode = normalized;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChannelCode)));
            }
        }

        public string SelectedChannelName
        {
            get => _selectedChannelName;
            set
            {
                if (_selectedChannelName == value) return;
                _selectedChannelName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChannelName)));
            }
        }

        public int BillId { get; }
        public int BillNo { get; }
        public string BillName { get; private set; }
        public bool IsDefault { get; set; }
        public bool IsSelected { get; set; }
        public ObservableCollection<BillLinePreview> Lines { get; } = [];
        public decimal Total => Lines.Sum(x => x.Total);
        public string DisplayName => IsDefault ? $"{BillName} (Mặc định)" : BillName;
        public string IsSelectedTag => IsSelected ? "Active" : string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString()
        {
            return DisplayName;
        }

        public void NotifyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedTag)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Total)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChannelId)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChannelCode)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChannelName)));
        }

        public void CopyFrom(BillPreview source)
        {
            BillName = source.BillName;
            IsDefault = source.IsDefault;
            IsSelected = source.IsSelected;
            SelectedChannelId = source.SelectedChannelId;
            SelectedChannelCode = source.SelectedChannelCode;
            SelectedChannelName = source.SelectedChannelName;
            Lines.Clear();

            foreach (BillLinePreview line in source.Lines)
            {
                Lines.Add(line);
            }

            NotifyChanged();
        }

        public void Rename(string billName)
        {
            BillName = billName;
            NotifyChanged();
        }
    }

public sealed class BillLinePreview : INotifyPropertyChanged
    {
        public BillLinePreview(int menuItemId, string itemName, string choiceSummary, int quantity, decimal unitPrice)
            : this(0, menuItemId, itemName, choiceSummary, quantity, quantity, unitPrice)
        {
        }

        public BillLinePreview(int billDetailId, int menuItemId, string itemName, string choiceSummary, int quantity, decimal unitPrice)
            : this(billDetailId, menuItemId, itemName, choiceSummary, quantity, quantity, unitPrice)
        {
        }

        public BillLinePreview(
            int billDetailId,
            int menuItemId,
            string itemName,
            string choiceSummary,
            int quantity,
            int notifiedQuantity,
            decimal unitPrice)
        {
            BillDetailId = billDetailId;
            MenuItemId = menuItemId;
            ItemName = itemName;
            ChoiceSummary = choiceSummary;
            Quantity = quantity;
            NotifiedQuantity = notifiedQuantity;
            UnitPrice = unitPrice;
        }

        public int BillDetailId { get; }
        public int MenuItemId { get; }
        public string ItemName { get; }
        public string ChoiceSummary { get; }
        public int Quantity { get; set; }
        public int NotifiedQuantity { get; set; }
        public decimal UnitPrice { get; }
        public decimal Total => Quantity * UnitPrice;
        public string UnitPriceText => OrderManagementFormatting.Money(UnitPrice);
        public string TotalText => OrderManagementFormatting.Money(Total);

        public event PropertyChangedEventHandler? PropertyChanged;

        public BillLinePreview CloneWithQuantity(int quantity)
        {
            return new BillLinePreview(
                BillDetailId,
                MenuItemId,
                ItemName,
                ChoiceSummary,
                quantity,
                Math.Min(NotifiedQuantity, quantity),
                UnitPrice);
        }

        public bool CanMergeWith(BillLinePreview other)
        {
            return MenuItemId == other.MenuItemId &&
                UnitPrice == other.UnitPrice &&
                string.Equals(ChoiceSummary, other.ChoiceSummary, StringComparison.Ordinal);
        }

        public void NotifyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotifiedQuantity)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Total)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalText)));
        }
    }
