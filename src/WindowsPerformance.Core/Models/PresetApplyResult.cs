namespace WindowsPerformance.Core.Models;

public sealed record PresetApplyResult(
    bool Success,
    string PresetId,
    string PresetName,
    Guid? SnapshotId,
    IReadOnlyList<PresetStepResult> Steps,
    string? ErrorMessage = null)
{
    public static PresetApplyResult Ok(
        string presetId,
        string presetName,
        Guid? snapshotId,
        IReadOnlyList<PresetStepResult> steps) =>
        new(true, presetId, presetName, snapshotId, steps);

    public static PresetApplyResult Fail(
        string presetId,
        string presetName,
        Guid? snapshotId,
        IReadOnlyList<PresetStepResult> steps,
        string errorMessage) =>
        new(false, presetId, presetName, snapshotId, steps, errorMessage);
}

public sealed record PresetStepResult(
    string StepName,
    bool Success,
    string? Message = null);
