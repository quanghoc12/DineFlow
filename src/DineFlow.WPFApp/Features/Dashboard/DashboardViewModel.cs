using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using DineFlow.BusinessObjects.Reports;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services.Api;

namespace DineFlow.WPFApp.Features.Dashboard;

public sealed class DashboardViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;

    private string _titleDate = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private decimal _revenueToday;
    private int _paidBillCountToday;
    private decimal _averageBillValue;
    private int _orderCountToday;
    private int _servingTableCount;
    private int _waitingPaymentTableCount;
    private int _printFailedOrderCount;
    private string _metricsPeriodText = "Hôm nay";

    public DashboardViewModel()
    {
        _apiClient = new StaffOrderApiClient();
        SelectChartItemCommand = new RelayCommand<ChartDayViewModel>(async (item) => await OnSelectChartItemAsync(item));
    }

    public string TitleDate
    {
        get => _titleDate;
        private set => SetProperty(ref _titleDate, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public decimal RevenueToday
    {
        get => _revenueToday;
        private set
        {
            if (SetProperty(ref _revenueToday, value))
            {
                OnComputedChanged();
            }
        }
    }

    public int PaidBillCountToday
    {
        get => _paidBillCountToday;
        private set => SetProperty(ref _paidBillCountToday, value);
    }

    public decimal AverageBillValue
    {
        get => _averageBillValue;
        private set
        {
            if (SetProperty(ref _averageBillValue, value))
            {
                OnComputedChanged();
            }
        }
    }

    public int OrderCountToday
    {
        get => _orderCountToday;
        private set => SetProperty(ref _orderCountToday, value);
    }

    public int ServingTableCount
    {
        get => _servingTableCount;
        private set => SetProperty(ref _servingTableCount, value);
    }

    public int WaitingPaymentTableCount
    {
        get => _waitingPaymentTableCount;
        private set => SetProperty(ref _waitingPaymentTableCount, value);
    }

    public int PrintFailedOrderCount
    {
        get => _printFailedOrderCount;
        private set => SetProperty(ref _printFailedOrderCount, value);
    }

    public string MetricsPeriodText
    {
        get => _metricsPeriodText;
        private set => SetProperty(ref _metricsPeriodText, value);
    }

    public ICommand SelectChartItemCommand { get; }

    private ChartMode _selectedChartMode = ChartMode.Last7Days;

    public ChartMode SelectedChartMode
    {
        get => _selectedChartMode;
        set
        {
            if (SetProperty(ref _selectedChartMode, value))
            {
                OnPropertyChanged(nameof(Is7DaysSelected));
                OnPropertyChanged(nameof(Is30DaysSelected));
                OnPropertyChanged(nameof(Is1YearSelected));
                OnPropertyChanged(nameof(ChartTitle));
                _ = UpdateChartDataAsync();
            }
        }
    }

    public bool Is7DaysSelected
    {
        get => SelectedChartMode == ChartMode.Last7Days;
        set { if (value) SelectedChartMode = ChartMode.Last7Days; }
    }

    public bool Is30DaysSelected
    {
        get => SelectedChartMode == ChartMode.Last30Days;
        set { if (value) SelectedChartMode = ChartMode.Last30Days; }
    }

    public bool Is1YearSelected
    {
        get => SelectedChartMode == ChartMode.Last12Months;
        set { if (value) SelectedChartMode = ChartMode.Last12Months; }
    }

    public string ChartTitle
    {
        get
        {
            return SelectedChartMode switch
            {
                ChartMode.Last7Days => "Biểu đồ doanh thu 7 ngày gần nhất",
                ChartMode.Last30Days => "Biểu đồ doanh thu 30 ngày gần nhất",
                ChartMode.Last12Months => "Biểu đồ doanh thu 12 tháng gần nhất (tính theo tháng)",
                _ => "Biểu đồ doanh thu"
            };
        }
    }

    // Sub-tab selection: TopSelling or PaymentMethods
    private string _selectedSubTab = "TopSelling";

    public string SelectedSubTab
    {
        get => _selectedSubTab;
        set
        {
            if (SetProperty(ref _selectedSubTab, value))
            {
                OnPropertyChanged(nameof(IsTopSellingTabSelected));
                OnPropertyChanged(nameof(IsPaymentMethodsTabSelected));
            }
        }
    }

    public bool IsTopSellingTabSelected
    {
        get => SelectedSubTab == "TopSelling";
        set { if (value) SelectedSubTab = "TopSelling"; }
    }

    public bool IsPaymentMethodsTabSelected
    {
        get => SelectedSubTab == "PaymentMethods";
        set { if (value) SelectedSubTab = "PaymentMethods"; }
    }

    // Top Selling Items Filters
    private string _topSellingPeriod = "Today";
    private int _selectedTopCount = 10;
    private DateTime? _topCustomDate = null;

    public List<int> TopCountOptions { get; } = new() { 5, 10, 20, 50 };

    public int SelectedTopCount
    {
        get => _selectedTopCount;
        set
        {
            if (SetProperty(ref _selectedTopCount, value))
            {
                _ = UpdateTopSellingItemsAsync();
            }
        }
    }

    public string TopSellingPeriod
    {
        get => _topSellingPeriod;
        set
        {
            if (SetProperty(ref _topSellingPeriod, value))
            {
                OnPropertyChanged(nameof(IsTopTodaySelected));
                OnPropertyChanged(nameof(IsTop7DaysSelected));
                OnPropertyChanged(nameof(IsTop30DaysSelected));
                OnPropertyChanged(nameof(TopSellingPeriodText));
                _ = UpdateTopSellingItemsAsync();
            }
        }
    }

    public bool IsTopTodaySelected
    {
        get => TopSellingPeriod == "Today";
        set 
        { 
            if (value) 
            {
                _topSellingPeriod = "Today";
                _topCustomDate = null;
                OnPropertyChanged(nameof(IsTopTodaySelected));
                OnPropertyChanged(nameof(IsTop7DaysSelected));
                OnPropertyChanged(nameof(IsTop30DaysSelected));
                OnPropertyChanged(nameof(TopCustomDate));
                OnPropertyChanged(nameof(TopSellingPeriodText));
                _ = UpdateTopSellingItemsAsync();
            } 
        }
    }

    public bool IsTop7DaysSelected
    {
        get => TopSellingPeriod == "Last7Days";
        set 
        { 
            if (value) 
            {
                _topSellingPeriod = "Last7Days";
                _topCustomDate = null;
                OnPropertyChanged(nameof(IsTopTodaySelected));
                OnPropertyChanged(nameof(IsTop7DaysSelected));
                OnPropertyChanged(nameof(IsTop30DaysSelected));
                OnPropertyChanged(nameof(TopCustomDate));
                OnPropertyChanged(nameof(TopSellingPeriodText));
                _ = UpdateTopSellingItemsAsync();
            } 
        }
    }

    public bool IsTop30DaysSelected
    {
        get => TopSellingPeriod == "Last30Days";
        set 
        { 
            if (value) 
            {
                _topSellingPeriod = "Last30Days";
                _topCustomDate = null;
                OnPropertyChanged(nameof(IsTopTodaySelected));
                OnPropertyChanged(nameof(IsTop7DaysSelected));
                OnPropertyChanged(nameof(IsTop30DaysSelected));
                OnPropertyChanged(nameof(TopCustomDate));
                OnPropertyChanged(nameof(TopSellingPeriodText));
                _ = UpdateTopSellingItemsAsync();
            } 
        }
    }

    public bool IsTopCustomSelected => TopSellingPeriod == "CustomDate";

    public DateTime? TopCustomDate
    {
        get => _topCustomDate;
        set
        {
            if (SetProperty(ref _topCustomDate, value))
            {
                if (value.HasValue)
                {
                    _topSellingPeriod = "CustomDate";
                    OnPropertyChanged(nameof(IsTopTodaySelected));
                    OnPropertyChanged(nameof(IsTop7DaysSelected));
                    OnPropertyChanged(nameof(IsTop30DaysSelected));
                    OnPropertyChanged(nameof(IsTopCustomSelected));
                    OnPropertyChanged(nameof(TopSellingPeriodText));
                }
                _ = UpdateTopSellingItemsAsync();
            }
        }
    }

    public string TopSellingPeriodText
    {
        get
        {
            return TopSellingPeriod switch
            {
                "Today" => "Khoảng thời gian: Hôm nay",
                "Last7Days" => "Khoảng thời gian: 7 ngày gần đây",
                "Last30Days" => "Khoảng thời gian: 30 ngày gần đây",
                "CustomDate" => $"Khoảng thời gian: Ngày {TopCustomDate?.ToString("dd/MM/yyyy")}",
                _ => "Khoảng thời gian"
            };
        }
    }

    // Payment Method Revenue Filters
    private string _paymentPeriod = "Today";
    private DateTime? _payCustomDate = null;

    public string PaymentPeriod
    {
        get => _paymentPeriod;
        set
        {
            if (SetProperty(ref _paymentPeriod, value))
            {
                OnPropertyChanged(nameof(IsPayTodaySelected));
                OnPropertyChanged(nameof(IsPay7DaysSelected));
                OnPropertyChanged(nameof(IsPay30DaysSelected));
                OnPropertyChanged(nameof(PaymentPeriodText));
                _ = UpdatePaymentMethodRevenueAsync();
            }
        }
    }

    public bool IsPayTodaySelected
    {
        get => PaymentPeriod == "Today";
        set 
        { 
            if (value) 
            {
                _paymentPeriod = "Today";
                _payCustomDate = null;
                OnPropertyChanged(nameof(IsPayTodaySelected));
                OnPropertyChanged(nameof(IsPay7DaysSelected));
                OnPropertyChanged(nameof(IsPay30DaysSelected));
                OnPropertyChanged(nameof(PayCustomDate));
                OnPropertyChanged(nameof(PaymentPeriodText));
                _ = UpdatePaymentMethodRevenueAsync();
            } 
        }
    }

    public bool IsPay7DaysSelected
    {
        get => PaymentPeriod == "Last7Days";
        set 
        { 
            if (value) 
            {
                _paymentPeriod = "Last7Days";
                _payCustomDate = null;
                OnPropertyChanged(nameof(IsPayTodaySelected));
                OnPropertyChanged(nameof(IsPay7DaysSelected));
                OnPropertyChanged(nameof(IsPay30DaysSelected));
                OnPropertyChanged(nameof(PayCustomDate));
                OnPropertyChanged(nameof(PaymentPeriodText));
                _ = UpdatePaymentMethodRevenueAsync();
            } 
        }
    }

    public bool IsPay30DaysSelected
    {
        get => PaymentPeriod == "Last30Days";
        set 
        { 
            if (value) 
            {
                _paymentPeriod = "Last30Days";
                _payCustomDate = null;
                OnPropertyChanged(nameof(IsPayTodaySelected));
                OnPropertyChanged(nameof(IsPay7DaysSelected));
                OnPropertyChanged(nameof(IsPay30DaysSelected));
                OnPropertyChanged(nameof(PayCustomDate));
                OnPropertyChanged(nameof(PaymentPeriodText));
                _ = UpdatePaymentMethodRevenueAsync();
            } 
        }
    }

    public bool IsPayCustomSelected => PaymentPeriod == "CustomDate";

    public DateTime? PayCustomDate
    {
        get => _payCustomDate;
        set
        {
            if (SetProperty(ref _payCustomDate, value))
            {
                if (value.HasValue)
                {
                    _paymentPeriod = "CustomDate";
                    OnPropertyChanged(nameof(IsPayTodaySelected));
                    OnPropertyChanged(nameof(IsPay7DaysSelected));
                    OnPropertyChanged(nameof(IsPay30DaysSelected));
                    OnPropertyChanged(nameof(IsPayCustomSelected));
                    OnPropertyChanged(nameof(PaymentPeriodText));
                }
                _ = UpdatePaymentMethodRevenueAsync();
            }
        }
    }

    public string PaymentPeriodText
    {
        get
        {
            return PaymentPeriod switch
            {
                "Today" => "Khoảng thời gian: Hôm nay",
                "Last7Days" => "Khoảng thời gian: 7 ngày gần đây",
                "Last30Days" => "Khoảng thời gian: 30 ngày gần đây",
                "CustomDate" => $"Khoảng thời gian: Ngày {PayCustomDate?.ToString("dd/MM/yyyy")}",
                _ => "Khoảng thời gian"
            };
        }
    }

    public string RevenueTodayText => FormatMoney(RevenueToday);
    public string AverageBillValueText => FormatMoney(AverageBillValue);

    public ObservableCollection<TopSellingItemDto> TopSellingItems { get; } = [];
    public ObservableCollection<PaymentMethodRevenueDto> RevenueByPaymentMethods { get; } = [];
    public ObservableCollection<ChartDayViewModel> ChartRevenueByDays { get; } = [];

    public async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        MetricsPeriodText = "Hôm nay";

        try
        {
            DashboardDto dashboard = await _apiClient.GetTodayDashboardAsync();
            TitleDate = dashboard.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            RevenueToday = dashboard.RevenueToday;
            PaidBillCountToday = dashboard.PaidBillCountToday;
            AverageBillValue = dashboard.AverageBillValue;
            OrderCountToday = dashboard.OrderCountToday;
            ServingTableCount = dashboard.ServingTableCount;
            WaitingPaymentTableCount = dashboard.WaitingPaymentTableCount;
            PrintFailedOrderCount = dashboard.PrintFailedOrderCount;

            await UpdateTopSellingItemsAsync();
            await UpdatePaymentMethodRevenueAsync();
            await UpdateChartDataAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateTopSellingItemsAsync()
    {
        try
        {
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            if (TopSellingPeriod == "Today")
            {
                fromDate = DateTime.Today;
                toDate = DateTime.Today;
            }
            else if (TopSellingPeriod == "Last7Days")
            {
                fromDate = DateTime.Today.AddDays(-6);
                toDate = DateTime.Today;
            }
            else if (TopSellingPeriod == "Last30Days")
            {
                fromDate = DateTime.Today.AddDays(-29);
                toDate = DateTime.Today;
            }
            else if (TopSellingPeriod == "CustomDate")
            {
                fromDate = TopCustomDate ?? DateTime.Today;
                toDate = TopCustomDate ?? DateTime.Today;
            }

            var items = await _apiClient.GetTopSellingItemsAsync(fromDate, toDate, SelectedTopCount);
            TopSellingItems.Clear();
            foreach (var item in items)
            {
                TopSellingItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi khi tải món bán chạy: " + ex.Message;
        }
    }

    private async Task UpdatePaymentMethodRevenueAsync()
    {
        try
        {
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            if (PaymentPeriod == "Today")
            {
                fromDate = DateTime.Today;
                toDate = DateTime.Today;
            }
            else if (PaymentPeriod == "Last7Days")
            {
                fromDate = DateTime.Today.AddDays(-6);
                toDate = DateTime.Today;
            }
            else if (PaymentPeriod == "Last30Days")
            {
                fromDate = DateTime.Today.AddDays(-29);
                toDate = DateTime.Today;
            }
            else if (PaymentPeriod == "CustomDate")
            {
                fromDate = PayCustomDate ?? DateTime.Today;
                toDate = PayCustomDate ?? DateTime.Today;
            }

            var methods = await _apiClient.GetRevenueByPaymentMethodAsync(fromDate, toDate);
            RevenueByPaymentMethods.Clear();
            foreach (var method in methods)
            {
                RevenueByPaymentMethods.Add(method);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi khi tải doanh thu phương thức: " + ex.Message;
        }
    }

    private async Task UpdateChartDataAsync()
    {
        try
        {
            var today = DateTime.Today;
            ChartRevenueByDays.Clear();

            if (SelectedChartMode == ChartMode.Last7Days)
            {
                var chartSummary = await _apiClient.GetRevenueSummaryAsync(today.AddDays(-6), today);
                var dailyDict = chartSummary.RevenueByDays
                    .GroupBy(x => x.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Revenue));

                var allDays = new List<(DateTime Date, decimal Revenue)>();
                for (int i = 0; i < 7; i++)
                {
                    var targetDate = today.AddDays(-6 + i).Date;
                    decimal revenue = 0m;
                    dailyDict.TryGetValue(targetDate, out revenue);
                    allDays.Add((targetDate, revenue));
                }

                decimal maxRevenue = allDays.Count > 0 ? allDays.Max(x => x.Revenue) : 0m;
                foreach (var day in allDays)
                {
                    ChartRevenueByDays.Add(new ChartDayViewModel
                    {
                        Date = day.Date,
                        DateText = day.Date.ToString("dd/MM"),
                        RevenueText = FormatMoney(day.Revenue),
                        Height = maxRevenue == 0m ? 0 : (double)(day.Revenue / maxRevenue) * 120, // 120px max
                        Width = 45,
                        ShowRevenueLabel = day.Revenue > 0
                    });
                }
            }
            else if (SelectedChartMode == ChartMode.Last30Days)
            {
                var chartSummary = await _apiClient.GetRevenueSummaryAsync(today.AddDays(-29), today);
                var dailyDict = chartSummary.RevenueByDays
                    .GroupBy(x => x.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Revenue));

                var allDays = new List<(DateTime Date, decimal Revenue)>();
                for (int i = 0; i < 30; i++)
                {
                    var targetDate = today.AddDays(-29 + i).Date;
                    decimal revenue = 0m;
                    dailyDict.TryGetValue(targetDate, out revenue);
                    allDays.Add((targetDate, revenue));
                }

                decimal maxRevenue = allDays.Count > 0 ? allDays.Max(x => x.Revenue) : 0m;
                foreach (var day in allDays)
                {
                    ChartRevenueByDays.Add(new ChartDayViewModel
                    {
                        Date = day.Date,
                        DateText = day.Date.ToString("dd/MM"),
                        RevenueText = FormatMoney(day.Revenue),
                        Height = maxRevenue == 0m ? 0 : (double)(day.Revenue / maxRevenue) * 120,
                        Width = 14,
                        ShowRevenueLabel = false // Hide label for dense charts (hover for ToolTip)
                    });
                }
            }
            else if (SelectedChartMode == ChartMode.Last12Months)
            {
                var startMonth = today.AddMonths(-11);
                var startDate = new DateTime(startMonth.Year, startMonth.Month, 1);
                var chartSummary = await _apiClient.GetRevenueSummaryAsync(startDate, today);

                var monthlyGroups = chartSummary.RevenueByDays
                    .GroupBy(x => new { x.Date.Year, x.Date.Month })
                    .ToDictionary(
                        g => (g.Key.Year, g.Key.Month),
                        g => g.Sum(x => x.Revenue)
                    );

                var allMonths = new List<(DateTime Date, decimal Revenue)>();
                for (int i = 0; i < 12; i++)
                {
                    var targetMonth = startDate.AddMonths(i);
                    decimal revenue = 0m;
                    monthlyGroups.TryGetValue((targetMonth.Year, targetMonth.Month), out revenue);
                    allMonths.Add((targetMonth, revenue));
                }

                decimal maxRevenue = allMonths.Count > 0 ? allMonths.Max(x => x.Revenue) : 0m;
                foreach (var item in allMonths)
                {
                    ChartRevenueByDays.Add(new ChartDayViewModel
                    {
                        Date = item.Date,
                        DateText = item.Date.ToString("MM/yy"),
                        RevenueText = FormatMoney(item.Revenue),
                        Height = maxRevenue == 0m ? 0 : (double)(item.Revenue / maxRevenue) * 120,
                        Width = 32,
                        ShowRevenueLabel = item.Revenue > 0
                    });
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi khi tải biểu đồ: " + ex.Message;
        }
    }

    private void OnComputedChanged()
    {
        OnPropertyChanged(nameof(RevenueTodayText));
        OnPropertyChanged(nameof(AverageBillValueText));
    }

    private static string FormatMoney(decimal amount) =>
        string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", amount);

    private async Task OnSelectChartItemAsync(ChartDayViewModel item)
    {
        if (item == null) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            DashboardDto dashboard;
            if (SelectedChartMode == ChartMode.Last12Months)
            {
                var fromDate = new DateTime(item.Date.Year, item.Date.Month, 1);
                var toDate = fromDate.AddMonths(1).AddDays(-1);
                dashboard = await _apiClient.GetDashboardByRangeAsync(fromDate, toDate);
                MetricsPeriodText = $"Tháng {item.Date:MM/yyyy}";
            }
            else
            {
                dashboard = await _apiClient.GetDashboardByDateAsync(item.Date);
                MetricsPeriodText = $"Ngày {item.Date:dd/MM/yyyy}";
            }

            RevenueToday = dashboard.RevenueToday;
            PaidBillCountToday = dashboard.PaidBillCountToday;
            AverageBillValue = dashboard.AverageBillValue;
            OrderCountToday = dashboard.OrderCountToday;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi khi cập nhật thống kê: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public enum ChartMode
{
    Last7Days,
    Last30Days,
    Last12Months
}

public sealed class ChartDayViewModel
{
    public DateTime Date { get; set; }
    public string DateText { get; set; } = string.Empty;
    public string RevenueText { get; set; } = string.Empty;
    public double Height { get; set; }
    public double Width { get; set; }
    public bool ShowRevenueLabel { get; set; }
}

public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        if (parameter is null && typeof(T).IsValueType)
        {
            return _canExecute?.Invoke(default!) ?? true;
        }
        return _canExecute?.Invoke((T)parameter!) ?? true;
    }

    public void Execute(object? parameter)
    {
        _execute((T)parameter!);
    }
}
