using System;
using Microsoft.Extensions.DependencyInjection;
using DineFlow.WPFApp.Core;

namespace DineFlow.WPFApp.Services;

public class NavigationService : INavigationService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private IServiceScope? _currentScope;
    private BaseViewModel? _currentView;

    public NavigationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public BaseViewModel? CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            CurrentViewChanged?.Invoke();
        }
    }

    public event Action? CurrentViewChanged;

    public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        // Prevent unnecessary reloading and database queries if already on the requested view
        if (_currentView is TViewModel) return;

        // 1. Dispose the old scope to clean up DbContext and Transient resources
        _currentScope?.Dispose();

        // 2. Create a fresh scope for the new view
        _currentScope = _scopeFactory.CreateScope();

        // 3. Resolve the ViewModel from the new scope
        CurrentView = _currentScope.ServiceProvider.GetRequiredService<TViewModel>();
    }

    public void Clear()
    {
        _currentScope?.Dispose();
        _currentScope = null;
        CurrentView = null;
    }

    public void Dispose()
    {
        Clear();
    }
}
