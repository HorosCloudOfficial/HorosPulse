namespace WindowsPerformance.Services.Cursor;

using WindowsPerformance.Core.Interfaces;

public sealed class CursorOptimizationModule : IOptimizationModule
{
    private readonly ICursorOptimizer _cursorOptimizer;

    public CursorOptimizationModule(ICursorOptimizer cursorOptimizer) =>
        _cursorOptimizer = cursorOptimizer;

    public string ModuleName => "Cursor";

    public bool CanApply => true;

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _cursorOptimizer.ApplyOptimizationsAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var result = await _cursorOptimizer.RevertOptimizationsAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(result.ErrorMessage);
    }
}
