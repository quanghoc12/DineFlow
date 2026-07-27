using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Auth;
using DineFlow.WPFApp.Core;
using System.Windows.Input;

namespace DineFlow.WPFApp.Features.Auth;

public sealed class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public AsyncRelayCommand LoginCommand { get; }
    public event Action? LoginSucceeded;

    private bool CanLogin()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrEmpty(Password);
    }

    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            LoginResult result = await _authService.LoginAsync(new LoginRequest
            {
                Username = Username,
                Password = Password
            });

            if (result.IsSuccess)
            {
                LoginSucceeded?.Invoke();
                return;
            }

            ErrorMessage = result.ErrorMessage;
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Không thể đăng nhập: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
