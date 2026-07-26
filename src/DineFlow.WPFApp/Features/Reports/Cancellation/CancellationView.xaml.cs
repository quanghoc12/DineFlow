using System.Windows;
using System.Windows.Controls;
using DineFlow.WPFApp.Features.Reports.ViewModels;

namespace DineFlow.WPFApp.Features.Reports.Cancellation;

public partial class CancellationView : UserControl
{
    private readonly CancellationViewModel _viewModel;

    public CancellationView()
        : this(new CancellationViewModel())
    {
    }

    public CancellationView(CancellationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    public async Task LoadTodayCancellationsAsync()
    {
        _viewModel.SelectedDate = DateTime.Today;
        await _viewModel.LoadAsync();
    }

    private async void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }
}
