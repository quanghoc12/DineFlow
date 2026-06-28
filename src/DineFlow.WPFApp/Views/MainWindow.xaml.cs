using System.Windows;
using DineFlow.WPFApp.ViewModels;

namespace DineFlow.WPFApp.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        
        // Constructor injection only
        DataContext = viewModel;

        // Subscribe LogoutRequested event
        viewModel.LogoutRequested += ViewModel_LogoutRequested;
    }

    private void ViewModel_LogoutRequested()
    {
        // No memory leaks from event subscriptions
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.LogoutRequested -= ViewModel_LogoutRequested;
        }

        // Close MainWindow. Control returns to the orchestration loop in App.xaml.cs
        this.Close();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
        else
            WindowState = WindowState.Maximized;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
