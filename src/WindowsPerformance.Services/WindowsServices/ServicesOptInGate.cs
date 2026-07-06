namespace WindowsPerformance.Services.WindowsServices;

using WindowsPerformance.Core.Interfaces;

public sealed class ServicesOptInGate : IServicesOptInGate
{
    public bool IsConfirmed { get; set; }
}
