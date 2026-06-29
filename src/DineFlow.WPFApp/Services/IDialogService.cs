using DineFlow.BusinessObjects.Auth.DTOs;

namespace DineFlow.WPFApp.Services;

public interface IDialogService
{
    bool ShowCreateUserDialog();
    bool ShowUpdateUserDialog(UserDisplayDto user);
    bool ShowResetPasswordDialog(UserDisplayDto user);
    bool ShowConfirmationDialog(string message, string title);
}
