namespace HorosPulse.ViewModels;

using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

internal static class RollbackUiHelper
{
    public static bool ConfirmRollback(IUserConfirmationService confirmationService, SnapshotEntry snapshot) =>
        confirmationService.Confirm(
            "Rollback bestätigen",
            $"Snapshot \"{snapshot.Label}\" ({snapshot.CreatedAt.LocalDateTime:g}) für Modul \"{snapshot.Module}\" zurücksetzen?",
            isWarning: true);
}
