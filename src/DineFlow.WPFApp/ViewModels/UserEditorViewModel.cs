using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DineFlow.BusinessObjects.Auth.DTOs;
using DineFlow.BusinessObjects.Auth.Entities;
using DineFlow.Repositories.Interfaces;
using DineFlow.Services.Interfaces;
using DineFlow.WPFApp.Core;

namespace DineFlow.WPFApp.ViewModels;

public class UserEditorViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IRoleRepository _roleRepository;
    
    private bool _isEditMode;
    private int _userId;
    private string _username = string.Empty;
    private string _fullName = string.Empty;
    private string _password = string.Empty;
    private Role? _selectedRole;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public ObservableCollection<Role> Roles { get; } = new();

    public UserEditorViewModel(IUserService userService, IRoleRepository roleRepository)
    {
        _userService = userService;
        _roleRepository = roleRepository;
        SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, CanExecuteSave);
    }

    public string Title => _isEditMode ? "Edit User" : "Create User";

    public string Username
    {
        get => _username;
        set { SetProperty(ref _username, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public string FullName
    {
        get => _fullName;
        set { SetProperty(ref _fullName, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public string Password
    {
        get => _password;
        set { SetProperty(ref _password, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public Role? SelectedRole
    {
        get => _selectedRole;
        set { SetProperty(ref _selectedRole, value); CommandManager.InvalidateRequerySuggested(); }
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

    public bool IsCreateMode => !_isEditMode;

    public AsyncRelayCommand SaveCommand { get; }
    public event Action? RequestClose;

    public void InitializeCreateMode()
    {
        _isEditMode = false;
        _ = LoadRolesAsync();
    }

    public void InitializeEditMode(UserDisplayDto user)
    {
        _isEditMode = true;
        _userId = user.UserId;
        Username = user.Username;
        FullName = user.FullName;
        _ = LoadRolesAsync(user.RoleName);
    }

    private async Task LoadRolesAsync(string? selectedRoleName = null)
    {
        IsBusy = true;
        try
        {
            var roles = await _roleRepository.GetAllAsync();
            Roles.Clear();
            foreach (var role in roles)
            {
                Roles.Add(role);
                if (_isEditMode && role.RoleName == selectedRoleName)
                {
                    SelectedRole = role;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load roles: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteSave(object? parameter)
    {
        if (IsBusy) return false;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(FullName) || SelectedRole == null) return false;
        if (!_isEditMode && string.IsNullOrWhiteSpace(Password)) return false;
        return true;
    }

    private async Task ExecuteSaveAsync(object? parameter)
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            if (_isEditMode)
            {
                await _userService.UpdateUserAsync(new UpdateUserRequestDto
                {
                    UserId = _userId,
                    Username = Username,
                    FullName = FullName,
                    RoleId = SelectedRole!.RoleId
                });
            }
            else
            {
                await _userService.CreateUserAsync(new CreateUserRequestDto
                {
                    Username = Username,
                    FullName = FullName,
                    Password = Password,
                    RoleId = SelectedRole!.RoleId
                });
            }
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
