using System.Windows;

namespace DineFlow.WPFApp.Helpers;

public static class MessageBoxHelper
{
    public static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static void ShowSuccess(string message)
    {
        MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
