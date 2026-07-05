namespace WindowsPerformance.Services.PowerShell;

public sealed class PowerShellOptions
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public string PreferredExecutable { get; set; } = "pwsh.exe";
    public string FallbackExecutable { get; set; } = "powershell.exe";
}
