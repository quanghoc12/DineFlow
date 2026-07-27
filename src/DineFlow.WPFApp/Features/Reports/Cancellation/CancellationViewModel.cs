using System.Collections.ObjectModel;
using DineFlow.BusinessObjects.Reports;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services.Api;

namespace DineFlow.WPFApp.Features.Reports.Cancellation;

public sealed class CancellationViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;

    private DateTime _selectedDate;
    private int _cancelledBillCount;
    private int _cancelledItemCount;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public CancellationViewModel()
    {
        _apiClient = new StaffOrderApiClient();
        _selectedDate = DateTime.Today;
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                _ = LoadAsync();
            }
        }
    }

    public int CancelledBillCount
    {
        get => _cancelledBillCount;
        private set => SetProperty(ref _cancelledBillCount, value);
    }

    public int CancelledItemCount
    {
        get => _cancelledItemCount;
        private set => SetProperty(ref _cancelledItemCount, value);
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

    public ObservableCollection<CancelledBillDto> CancelledBills { get; } = [];
    public ObservableCollection<CancelledItemDto> CancelledItems { get; } = [];

    public async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            var summary = await _apiClient.GetCancellationSummaryAsync(SelectedDate);
            CancelledBillCount = summary.CancelledBillCount;
            CancelledItemCount = summary.CancelledItemCount;

            CancelledBills.Clear();
            foreach (var bill in summary.CancelledBills)
            {
                CancelledBills.Add(bill);
            }

            CancelledItems.Clear();
            foreach (var item in summary.CancelledItems)
            {
                CancelledItems.Add(item);
            }
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
}
