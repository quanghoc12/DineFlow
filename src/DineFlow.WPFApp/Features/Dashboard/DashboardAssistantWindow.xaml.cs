using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DineFlow.WPFApp.Features.Dashboard;

public partial class DashboardAssistantWindow : Window
{
    private readonly DashboardAssistantViewModel _viewModel;

    public DashboardAssistantWindow(DashboardAssistantViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        Closed += DashboardAssistantWindow_Closed;
    }

    private void DashboardAssistantWindow_Closed(object? sender, EventArgs e)
    {
        _viewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.InvokeAsync(() => MessagesScrollViewer.ScrollToEnd());
    }

    private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            return;
        }

        if (_viewModel.SendCommand.CanExecute(null))
        {
            _viewModel.SendCommand.Execute(null);
        }

        e.Handled = true;
    }

    private void SuggestedQuestionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string question })
        {
            _viewModel.UseSuggestedQuestion(question);
        }
    }
}
