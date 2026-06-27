using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DineFlow.BusinessObjects.Auth.DTOs;
using DineFlow.Services.Interfaces;
using DineFlow.WPFApp.Core;

namespace DineFlow.WPFApp.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync, CanExecuteLogin);
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
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public AsyncRelayCommand LoginCommand { get; }

    // Event to signal the UI (LoginWindow) that login succeeded so it can transition
    public event Action? LoginSuccess;

    private bool CanExecuteLogin(object? parameter)
    {
        // 1. Validate empty username/password
        return !string.IsNullOrWhiteSpace(Username) && 
               !string.IsNullOrWhiteSpace(Password) && 
               !IsBusy;
    }

    private async Task ExecuteLoginAsync(object? parameter)
    {
        ErrorMessage = string.Empty;
        
        // 2. Set IsBusy during login
        IsBusy = true;

        try
        {
            var request = new LoginRequestDto
            {
                Username = this.Username,
                Password = this.Password
            };

            // 3. Call IAuthService.LoginAsync()
            var result = await _authService.LoginAsync(request);

            if (result.IsSuccess)
            {
                // 5. Trigger success event on successful login
                LoginSuccess?.Invoke();
            }
            else
            {
                // 4. Show ErrorMessage on failure
                ErrorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi kết nối: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
