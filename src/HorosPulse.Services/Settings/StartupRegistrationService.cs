namespace HorosPulse.Services.Settings;

using StartupHelper;
using HorosPulse.Core.Interfaces;

public sealed class StartupRegistrationService : IStartupRegistrationService
{
    private readonly StartupManager _manager = new("HorosPulse", RegistrationScope.Local);

    public bool IsRegistered => _manager.IsRegistered;

    public bool Register() => _manager.Register();

    public bool Unregister() => _manager.Unregister();
}
