namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Services.Elevation;
using Xunit;

public class SyncElevationUiInvokerTests
{
    private readonly SyncElevationUiInvoker _invoker = new();

    [Fact]
    public async Task PrepareForUacPromptAsync_CompletesWithoutBlocking()
    {
        var act = () => _invoker.PrepareForUacPromptAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvokeAsync_RunsActionInline()
    {
        var executed = false;

        await _invoker.InvokeAsync(() => executed = true);

        executed.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ReturnsFuncResult()
    {
        var result = await _invoker.InvokeAsync(() => 42);

        result.Should().Be(42);
    }

    [Fact]
    public async Task InvokeAsync_RespectsCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _invoker.InvokeAsync(() => { }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
