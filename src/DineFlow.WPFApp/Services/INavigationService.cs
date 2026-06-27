using System;
using DineFlow.WPFApp.Core;

namespace DineFlow.WPFApp.Services;

public interface INavigationService
{
    BaseViewModel? CurrentView { get; }
    event Action CurrentViewChanged;
    void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
    void Clear();
}
