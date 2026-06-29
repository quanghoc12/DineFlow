using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DineFlow.BusinessObjects.Auth.Constants;
using DineFlow.Services.Interfaces;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.ViewModels;

public class MenuItemViewModel : BaseViewModel
{
    public string Title { get; set; } = string.Empty;
    public Action? NavigateAction { get; set; }
}

public class MainViewModel : BaseViewModel, IDisposable
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

    private string _currentUserName = string.Empty;
    private string _currentRole = string.Empty;

    public MainViewModel(
        ICurrentUserService currentUserService,
        IPermissionService permissionService,
        INavigationService navigationService,
        IAuthService authService)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        NavigateCommand = new RelayCommand(ExecuteNavigate);
        LogoutCommand = new RelayCommand(ExecuteLogout);

        MenuItems = new ObservableCollection<MenuItemViewModel>();

        // Subscribe to navigation changes to update the CurrentViewModel property
        _navigationService.CurrentViewChanged += NavigationService_CurrentViewChanged;

        // 1. Load user information
        LoadUserInfo();

        // 2. Determine available menus
        BuildMenu();
    }

    public string CurrentUserName
    {
        get => _currentUserName;
        set => SetProperty(ref _currentUserName, value);
    }

    public string CurrentRole
    {
        get => _currentRole;
        set => SetProperty(ref _currentRole, value);
    }

    public BaseViewModel? CurrentViewModel => _navigationService.CurrentView;

    public ObservableCollection<MenuItemViewModel> MenuItems { get; }

    public ICommand NavigateCommand { get; }
    public ICommand LogoutCommand { get; }

    // 5. Raise LogoutRequested event
    public event Action? LogoutRequested;

    private void LoadUserInfo()
    {
        CurrentUserName = _currentUserService.GetFullName();
        CurrentRole = _currentUserService.GetRole();
    }

    private void BuildMenu()
    {
        MenuItems.Clear();

        if (_permissionService.HasPermission(PermissionKeys.ManageUsers))
        {
            MenuItems.Add(new MenuItemViewModel 
            { 
                Title = "User Management", 
                NavigateAction = () => _navigationService.NavigateTo<UserManagementViewModel>() 
            });
        }
        
        // Future Modules
        if (_permissionService.HasPermission(PermissionKeys.ViewTables))
        {
            MenuItems.Add(new MenuItemViewModel 
            { 
                Title = "Tables (WIP)"
            });
        }

        if (_permissionService.HasPermission(PermissionKeys.ViewOrders))
        {
            MenuItems.Add(new MenuItemViewModel 
            { 
                Title = "Orders (WIP)"
            });
        }
    }

    private void NavigationService_CurrentViewChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }

    private void ExecuteNavigate(object? parameter)
    {
        // 3. Delegate navigation to INavigationService
        if (parameter is MenuItemViewModel menuItem && menuItem.NavigateAction != null)
        {
            menuItem.NavigateAction.Invoke();
        }
    }

    private void ExecuteLogout(object? parameter)
    {
        // 4. Logout must call IAuthService.Logout()
        _authService.Logout();

        // Ensure current scope and view are destroyed before transitioning to Login screen
        _navigationService.Clear();

        LogoutRequested?.Invoke();
    }

    public void Dispose()
    {
        _navigationService.CurrentViewChanged -= NavigationService_CurrentViewChanged;
    }
}
