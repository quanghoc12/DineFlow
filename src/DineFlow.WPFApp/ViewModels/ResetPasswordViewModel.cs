using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DineFlow.BusinessObjects.Auth.DTOs;
using DineFlow.Services.Interfaces;
using DineFlow.WPFApp.Core;

namespace DineFlow.WPFApp.ViewModels;

public class ResetPasswordViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    
    private int _userId;
    private string _username = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public ResetPasswordViewModel(IUserService userService)
    {
        _userService = userService;
        SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, CanExecuteSave);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set { SetProperty(ref _newPassword, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set { SetProperty(ref _confirmPassword, value); CommandManager.InvalidateRequerySuggested(); }
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

    public AsyncRelayCommand SaveCommand { get; }
    public event Action? RequestClose;

    public void Initialize(UserDisplayDto user)
    {
        _userId = user.UserId;
        Username = user.Username;
    }

    private bool CanExecuteSave(object? parameter)
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(ConfirmPassword);
    }

    private async Task ExecuteSaveAsync(object? parameter)
    {
        ErrorMessage = string.Empty;
        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsBusy = true;
        try
        {
            await _userService.ResetPasswordAsync(new ResetPasswordRequestDto
            {
                UserId = _userId,
                NewPassword = NewPassword
            });
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
