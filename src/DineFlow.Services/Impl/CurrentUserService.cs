using DineFlow.BusinessObjects.Auth.DTOs;
using DineFlow.Services.Interfaces;

namespace DineFlow.Services.Impl;

public class CurrentUserService : ICurrentUserService
{
    private CurrentUserDto? _currentUser;
    private readonly object _lock = new object();

    public void Login(CurrentUserDto user)
    {
        lock (_lock)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
        }
    }

    public void Logout()
    {
        lock (_lock)
        {
            _currentUser = null;
        }
    }

    public int GetCurrentUserId()
    {
        lock (_lock)
        {
            if (_currentUser == null) throw new InvalidOperationException("No user is logged in.");
            return _currentUser.UserId;
        }
    }

    public string GetUsername()
    {
        lock (_lock)
        {
            return _currentUser?.Username ?? string.Empty;
        }
    }

    public string GetFullName()
    {
        lock (_lock)
        {
            return _currentUser?.FullName ?? string.Empty;
        }
    }

    public string GetRole()
    {
        lock (_lock)
        {
            return _currentUser?.Role ?? string.Empty;
        }
    }

    public bool IsAuthenticated()
    {
        lock (_lock)
        {
            return _currentUser != null;
        }
    }

    public CurrentUserDto? GetCurrentUser()
    {
        lock (_lock)
        {
            return _currentUser;
        }
    }
}
