namespace WindowsPerformance.Core.Interfaces;

public interface IUserConfirmationService
{
    bool Confirm(string title, string message, bool isWarning = false);
}
