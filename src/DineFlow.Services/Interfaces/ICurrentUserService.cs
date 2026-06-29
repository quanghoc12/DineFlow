using DineFlow.BusinessObjects.Auth.DTOs;

namespace DineFlow.Services.Interfaces;

public interface ICurrentUserService
{
    void Login(CurrentUserDto user);
    void Logout();
    int GetCurrentUserId();
    string GetUsername();
    string GetFullName();
    string GetRole();
    bool IsAuthenticated();
    CurrentUserDto? GetCurrentUser();
}
