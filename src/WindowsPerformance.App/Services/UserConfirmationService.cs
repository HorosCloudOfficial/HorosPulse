namespace WindowsPerformance.App.Services;

using System.Windows;
using WindowsPerformance.Core.Interfaces;

public sealed class UserConfirmationService : IUserConfirmationService
{
    public bool Confirm(string title, string message, bool isWarning = false) =>
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            isWarning ? MessageBoxImage.Warning : MessageBoxImage.Question) == MessageBoxResult.Yes;
}
