using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DineFlow.BusinessObjects.Auth.DTOs;
using DineFlow.Services.Interfaces;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.ViewModels;

public class UserManagementViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IDialogService _dialogService;
    
    private ObservableCollection<UserDisplayDto> _users = new();
    private UserDisplayDto? _selectedUser;
    private string _searchText = string.Empty;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public UserManagementViewModel(IUserService userService, IDialogService dialogService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        LoadUsersCommand = new AsyncRelayCommand(ExecuteLoadUsersAsync);
        CreateUserCommand = new AsyncRelayCommand(ExecuteCreateUserAsync);
        UpdateUserCommand = new AsyncRelayCommand(ExecuteUpdateUserAsync, CanExecuteModifications);
        DisableUserCommand = new AsyncRelayCommand(ExecuteDisableUserAsync, CanExecuteDisable);
        EnableUserCommand = new AsyncRelayCommand(ExecuteEnableUserAsync, CanExecuteEnable);
        ResetPasswordCommand = new AsyncRelayCommand(ExecuteResetPasswordAsync, CanExecuteModifications);
        
        // Initial load
        _ = ExecuteLoadUsersAsync(null);
    }

    public ObservableCollection<UserDisplayDto> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }

    public UserDisplayDto? SelectedUser
    {
        get => _selectedUser;
        set 
        {
            if (SetProperty(ref _selectedUser, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set 
        {
            if (SetProperty(ref _searchText, value))
            {
                _ = ExecuteLoadUsersAsync(null); // Local filter applied within method
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public AsyncRelayCommand LoadUsersCommand { get; }
    public AsyncRelayCommand CreateUserCommand { get; }
    public AsyncRelayCommand UpdateUserCommand { get; }
    public AsyncRelayCommand DisableUserCommand { get; }
    public AsyncRelayCommand EnableUserCommand { get; }
    public AsyncRelayCommand ResetPasswordCommand { get; }

    private bool CanExecuteModifications(object? parameter) => SelectedUser != null && !IsLoading;
    private bool CanExecuteDisable(object? parameter) => SelectedUser != null && SelectedUser.IsActive && !IsLoading;
    private bool CanExecuteEnable(object? parameter) => SelectedUser != null && !SelectedUser.IsActive && !IsLoading;

    private async Task ExecuteLoadUsersAsync(object? parameter)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var rawUsers = await _userService.GetUsersAsync();
            
            // Client-side text filter implementation
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                rawUsers = rawUsers.Where(u => 
                    u.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                    u.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            Users = new ObservableCollection<UserDisplayDto>(rawUsers);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load users: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteCreateUserAsync(object? parameter)
    {
        if (_dialogService.ShowCreateUserDialog())
        {
            await ExecuteLoadUsersAsync(null);
        }
    }

    private async Task ExecuteUpdateUserAsync(object? parameter)
    {
        if (SelectedUser == null) return;
        if (_dialogService.ShowUpdateUserDialog(SelectedUser))
        {
            await ExecuteLoadUsersAsync(null);
        }
    }

    private async Task ExecuteDisableUserAsync(object? parameter)
    {
        if (SelectedUser == null) return;
        
        if (!_dialogService.ShowConfirmationDialog($"Are you sure you want to disable {SelectedUser.FullName}?", "Confirm Disable"))
            return;

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            await _userService.DisableUserAsync(SelectedUser.UserId);
            await ExecuteLoadUsersAsync(null); // Refresh grid
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to disable user: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteEnableUserAsync(object? parameter)
    {
        if (SelectedUser == null) return;
        
        if (!_dialogService.ShowConfirmationDialog($"Are you sure you want to enable {SelectedUser.FullName}?", "Confirm Enable"))
            return;

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            await _userService.EnableUserAsync(SelectedUser.UserId);
            await ExecuteLoadUsersAsync(null); // Refresh grid
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to enable user: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExecuteResetPasswordAsync(object? parameter)
    {
        if (SelectedUser == null) return;
        if (_dialogService.ShowResetPasswordDialog(SelectedUser))
        {
            // Password updated successfully
        }
        await Task.CompletedTask;
    }
}
