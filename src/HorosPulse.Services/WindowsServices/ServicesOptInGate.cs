namespace HorosPulse.Services.WindowsServices;

using HorosPulse.Core.Interfaces;

public sealed class ServicesOptInGate : IServicesOptInGate
{
    public bool IsConfirmed { get; set; }
}
