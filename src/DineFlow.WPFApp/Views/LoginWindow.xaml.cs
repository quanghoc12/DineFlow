using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Auth;
using System.Windows;

namespace DineFlow.WPFApp.Views;

public partial class LoginWindow : Window
{
    private readonly IAuthService _authService = new AuthService();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var request = new LoginRequestDto
            {
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Password
            };

            var currentUser = _authService.Login(request);
            MessageBox.Show($"Xin chào {currentUser.FullName}");

            var mainWindow = new MainWindow(currentUser);
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Login failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
