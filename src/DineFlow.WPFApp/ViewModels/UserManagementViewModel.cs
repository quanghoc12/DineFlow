using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Auth;
using DineFlow.WPFApp.Core;
using System.Collections.ObjectModel;

namespace DineFlow.WPFApp.ViewModels;

public sealed class UserManagementViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private List<UserSummary> _allUsers = [];
    private UserSummary? _selectedUser;
    private string _searchText = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public UserManagementViewModel(IUserService userService)
    {
        _userService = userService;
    }

    public ObservableCollection<UserSummary> Users { get; } = [];

    public UserSummary? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
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
        private set => SetProperty(ref _isBusy, value);
    }

    public Task LoadAsync() => ExecuteAsync(async () =>
    {
        _allUsers = (await _userService.GetUsersAsync()).ToList();
        ApplyFilter();
    });

    public Task CreateAsync(CreateUserRequest request) =>
        ExecuteAndReloadAsync(() => _userService.CreateAsync(request));

    public Task UpdateAsync(UpdateUserRequest request) =>
        ExecuteAndReloadAsync(() => _userService.UpdateAsync(request));

    public Task SetActiveAsync(UserSummary user, bool active) =>
        ExecuteAndReloadAsync(() => _userService.SetActiveAsync(user.UserId, active));

    public Task ResetPasswordAsync(UserSummary user, string password) =>
        ExecuteAsync(() => _userService.ResetPasswordAsync(user.UserId, password));

    private async Task ExecuteAndReloadAsync(Func<Task> action)
    {
        await ExecuteAsync(action);
        if (string.IsNullOrEmpty(ErrorMessage))
        {
            await LoadAsync();
        }
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<UserSummary> filtered = _allUsers;
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(user =>
                user.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                user.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        Users.Clear();
        foreach (UserSummary user in filtered)
        {
            Users.Add(user);
        }
    }
}
