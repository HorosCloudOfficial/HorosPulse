namespace WindowsPerformance.Services.Settings;

using StartupHelper;
using WindowsPerformance.Core.Interfaces;

public sealed class StartupRegistrationService : IStartupRegistrationService
{
    private readonly StartupManager _manager = new("WindowsPerformance", RegistrationScope.Local);

    public bool IsRegistered => _manager.IsRegistered;

    public bool Register() => _manager.Register();

    public bool Unregister() => _manager.Unregister();
}
