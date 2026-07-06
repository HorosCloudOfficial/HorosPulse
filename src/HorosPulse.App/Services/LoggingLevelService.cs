namespace HorosPulse.App.Services;

using Serilog.Core;
using Serilog.Events;
using HorosPulse.Core.Interfaces;

public sealed class LoggingLevelService : ILoggingLevelService
{
    private readonly LoggingLevelSwitch _levelSwitch;

    public LoggingLevelService(LoggingLevelSwitch levelSwitch) =>
        _levelSwitch = levelSwitch;

    public string CurrentLevel => _levelSwitch.MinimumLevel.ToString();

    public void SetMinimumLevel(string level)
    {
        if (Enum.TryParse<LogEventLevel>(level, ignoreCase: true, out var parsed))
            _levelSwitch.MinimumLevel = parsed;
    }
}
