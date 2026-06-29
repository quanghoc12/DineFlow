using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DineFlow.BusinessObjects.Auth.DTOs;
using DineFlow.WPFApp.Views;
using DineFlow.WPFApp.ViewModels;

namespace DineFlow.WPFApp.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool ShowCreateUserDialog()
    {
        var window = _serviceProvider.GetRequiredService<UserEditorWindow>();
        if (window.DataContext is UserEditorViewModel vm)
        {
            vm.InitializeCreateMode();
        }
        return window.ShowDialog() == true;
    }

    public bool ShowUpdateUserDialog(UserDisplayDto user)
    {
        var window = _serviceProvider.GetRequiredService<UserEditorWindow>();
        if (window.DataContext is UserEditorViewModel vm)
        {
            vm.InitializeEditMode(user);
        }
        return window.ShowDialog() == true;
    }

    public bool ShowResetPasswordDialog(UserDisplayDto user)
    {
        var window = _serviceProvider.GetRequiredService<ResetPasswordWindow>();
        if (window.DataContext is ResetPasswordViewModel vm)
        {
            vm.Initialize(user);
        }
        return window.ShowDialog() == true;
    }

    public bool ShowConfirmationDialog(string message, string title)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return result == MessageBoxResult.Yes;
    }
}
