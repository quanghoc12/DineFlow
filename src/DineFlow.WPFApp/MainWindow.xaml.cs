using DineFlow.BusinessObjects.Auth;
using System.Windows;

namespace DineFlow.WPFApp;

public partial class MainWindow : Window
{
    private readonly CurrentUserDto _currentUser;

    public MainWindow(CurrentUserDto currentUser)
    {
        InitializeComponent();
        _currentUser = currentUser;
        txtWelcome.Text = $"Xin chào {_currentUser.FullName} - {_currentUser.Role}";
    }
}
