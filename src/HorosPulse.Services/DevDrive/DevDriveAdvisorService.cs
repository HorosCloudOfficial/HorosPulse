namespace HorosPulse.Services.DevDrive;

using Microsoft.Extensions.Logging;
using HorosPulse.Core.Interfaces;
using HorosPulse.Core.Models;

public sealed class DevDriveAdvisorService : IDevDriveAdvisorService
{
    private readonly IDevDriveVolumeProbe _volumeProbe;
    private readonly ILogger<DevDriveAdvisorService> _logger;

    internal static readonly IReadOnlyList<(string DisplayName, string PathTemplate)> DefaultPathTemplates =
    [
        ("Quellcode (Visual Studio)", @"%USERPROFILE%\source"),
        ("npm-Cache", @"%LOCALAPPDATA%\npm-cache"),
        ("NuGet-Pakete", @"%USERPROFILE%\.nuget\packages"),
        ("pnpm Store", @"%LOCALAPPDATA%\pnpm\store"),
        ("Cargo Registry", @"%USERPROFILE%\.cargo"),
        ("Cursor-Daten", @"%APPDATA%\Cursor"),
        ("VS Code-Daten", @"%APPDATA%\Code"),
        ("Temp (Build-Artefakte)", @"%LOCALAPPDATA%\Temp"),
    ];

    public DevDriveAdvisorService(IDevDriveVolumeProbe volumeProbe, ILogger<DevDriveAdvisorService> logger)
    {
        _volumeProbe = volumeProbe;
        _logger = logger;
    }

    public Task<DevDriveAdvisorState> GetAssessmentAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var volumes = _volumeProbe.GetVolumes();
        var devVolumes = volumes.Where(v => v.IsDevDrive).ToList();
        var hasDevDrive = devVolumes.Count > 0;
        var primaryDevDrive = devVolumes.FirstOrDefault();

        var placements = DefaultPathTemplates
            .Select(template => EvaluatePath(template.DisplayName, template.PathTemplate, hasDevDrive))
            .ToList();

        var pathsOnDevDrive = placements.Count(p => p.Status == DevPathPlacementStatus.OnDevDrive);
        var pathsNeedingMigration = placements.Count(p =>
            p.Status is DevPathPlacementStatus.OnSlowVolume or DevPathPlacementStatus.OnReFsNotDev);

        var recommendations = BuildRecommendations(
            hasDevDrive,
            devVolumes,
            primaryDevDrive,
            placements);

        _logger.LogDebug(
            "Dev-Drive-Bewertung: HasDevDrive={HasDevDrive}, OnDevDrive={OnDevDrive}, NeedsMigration={NeedsMigration}",
            hasDevDrive,
            pathsOnDevDrive,
            pathsNeedingMigration);

        return Task.FromResult(new DevDriveAdvisorState
        {
            HasDevDrive = hasDevDrive,
            DevDriveVolumes = devVolumes,
            AllVolumes = volumes,
            PathPlacements = placements,
            Recommendations = recommendations,
            PathsOnDevDrive = pathsOnDevDrive,
            PathsNeedingMigration = pathsNeedingMigration,
        });
    }

    internal DevPathPlacement EvaluatePath(string displayName, string pathTemplate, bool hasDevDrive)
    {
        var expanded = ExpandPath(pathTemplate);
        var exists = Directory.Exists(expanded) || File.Exists(expanded);

        if (!hasDevDrive)
        {
            return new DevPathPlacement
            {
                DisplayName = displayName,
                Path = expanded,
                PathExists = exists,
                ResolvedDriveLetter = TryGetDriveLetter(expanded),
                IsOnDevDrive = false,
                IsOnReFs = false,
                Status = exists
                    ? DevPathPlacementStatus.NoDevDriveAvailable
                    : DevPathPlacementStatus.NotFound,
            };
        }

        if (!exists)
        {
            return new DevPathPlacement
            {
                DisplayName = displayName,
                Path = expanded,
                PathExists = false,
                ResolvedDriveLetter = TryGetDriveLetter(expanded),
                IsOnDevDrive = false,
                IsOnReFs = false,
                Status = DevPathPlacementStatus.NotFound,
            };
        }

        var volume = _volumeProbe.GetVolumeForPath(expanded);
        var driveLetter = volume?.DriveLetter ?? TryGetDriveLetter(expanded);

        if (volume?.IsDevDrive == true)
        {
            return new DevPathPlacement
            {
                DisplayName = displayName,
                Path = expanded,
                PathExists = true,
                ResolvedDriveLetter = driveLetter,
                IsOnDevDrive = true,
                IsOnReFs = true,
                Status = DevPathPlacementStatus.OnDevDrive,
            };
        }

        if (volume?.IsReFs == true)
        {
            return new DevPathPlacement
            {
                DisplayName = displayName,
                Path = expanded,
                PathExists = true,
                ResolvedDriveLetter = driveLetter,
                IsOnDevDrive = false,
                IsOnReFs = true,
                Status = DevPathPlacementStatus.OnReFsNotDev,
            };
        }

        return new DevPathPlacement
        {
            DisplayName = displayName,
            Path = expanded,
            PathExists = true,
            ResolvedDriveLetter = driveLetter,
            IsOnDevDrive = false,
            IsOnReFs = false,
            Status = DevPathPlacementStatus.OnSlowVolume,
        };
    }

    internal static string ExpandPath(string pathTemplate)
    {
        var expanded = Environment.ExpandEnvironmentVariables(pathTemplate);
        try
        {
            return Path.GetFullPath(expanded);
        }
        catch
        {
            return expanded;
        }
    }

    internal static string? TryGetDriveLetter(string path)
    {
        var root = Path.GetPathRoot(path);
        return string.IsNullOrWhiteSpace(root) ? null : root.TrimEnd('\\');
    }

    internal static IReadOnlyList<DevDriveRecommendation> BuildRecommendations(
        bool hasDevDrive,
        IReadOnlyList<DevDriveVolumeInfo> devVolumes,
        DevDriveVolumeInfo? primaryDevDrive,
        IReadOnlyList<DevPathPlacement> placements)
    {
        var recommendations = new List<DevDriveRecommendation>();

        if (!hasDevDrive)
        {
            recommendations.Add(new DevDriveRecommendation
            {
                Title = "Dev Drive einrichten",
                Description =
                    "Windows 11 (22H2+) unterstützt Dev Drive — ein ReFS-Volume, optimiert für Builds, Git und Package-Caches. " +
                    "Einrichtung: Einstellungen → System → Speicher → Erweiterte Speichereinstellungen → Dev Drive. " +
                    "Microsoft berichtet von bis zu 30 % schnelleren Builds bei verschobenen Caches.",
                Priority = DevDriveRecommendationPriority.High,
            });

            recommendations.Add(new DevDriveRecommendation
            {
                Title = "Defender-Leistungsmodus",
                Description =
                    "Auf einem echten Dev Drive aktiviert Windows automatisch den Defender-Leistungsmodus (asynchrones Scannen). " +
                    "Kombinieren Sie Dev Drive mit Build-Schutz (Prozess-Ausschlüsse) in HorosPulse für maximale Build-Performance.",
                Priority = DevDriveRecommendationPriority.Medium,
            });

            return recommendations;
        }

        var devDriveLabel = primaryDevDrive?.DriveLetter ?? devVolumes[0].DriveLetter;
        var slowPaths = placements
            .Where(p => p.Status is DevPathPlacementStatus.OnSlowVolume or DevPathPlacementStatus.OnReFsNotDev)
            .ToList();

        if (slowPaths.Count > 0)
        {
            recommendations.Add(new DevDriveRecommendation
            {
                Title = $"{slowPaths.Count} Dev-Pfad(e) auf langsamer Platte",
                Description =
                    $"Verschieben Sie Build-relevante Ordner auf {devDriveLabel}. " +
                    "HorosPulse führt keine automatische Migration durch — kopieren/verschieben Sie Daten manuell " +
                    "und passen Sie anschließend Umgebungsvariablen an.",
                Priority = DevDriveRecommendationPriority.High,
            });
        }

        foreach (var placement in slowPaths)
        {
            var hint = GetMigrationHint(placement, devDriveLabel);
            if (hint is not null)
                recommendations.Add(hint);
        }

        var refsNotDev = placements.Where(p => p.Status == DevPathPlacementStatus.OnReFsNotDev).ToList();
        if (refsNotDev.Count > 0)
        {
            recommendations.Add(new DevDriveRecommendation
            {
                Title = "ReFS ohne Dev-Drive-Flag",
                Description =
                    "Einige Pfade liegen auf ReFS, aber nicht auf einem offiziellen Dev Drive. " +
                    "Nur als Dev Drive formatierte Volumes erhalten Defender-Leistungsmodus und Copy-on-Write-Optimierungen.",
                Priority = DevDriveRecommendationPriority.Medium,
            });
        }

        if (slowPaths.Count == 0)
        {
            recommendations.Add(new DevDriveRecommendation
            {
                Title = "Dev-Pfade optimal platziert",
                Description =
                    $"Alle vorhandenen Standard-Dev-Pfade liegen auf {devDriveLabel}. " +
                    "Behalten Sie Package-Caches und Repositories auf demselben Dev-Drive-Volume für Copy-on-Write (CoW) Builds.",
                Priority = DevDriveRecommendationPriority.Info,
            });
        }

        recommendations.Add(new DevDriveRecommendation
        {
            Title = "Freier Speicher prüfen",
            Description =
                $"Dev Drive {devDriveLabel}: {primaryDevDrive?.FreeSpaceDisplay ?? "—"} frei von {primaryDevDrive?.TotalSpaceDisplay ?? "—"}. " +
                "Package-Caches (npm, NuGet, Cargo) wachsen schnell — mindestens 20 GB Puffer empfohlen.",
            Priority = DevDriveRecommendationPriority.Low,
        });

        return recommendations;
    }

    internal static DevDriveRecommendation? GetMigrationHint(DevPathPlacement placement, string devDriveLetter)
    {
        var targetRoot = devDriveLetter.TrimEnd('\\') + "\\";

        return placement.DisplayName switch
        {
            "Quellcode (Visual Studio)" => new DevDriveRecommendation
            {
                Title = $"Migration: {placement.DisplayName}",
                Description =
                    $"Verschieben Sie Repositories nach {targetRoot}Dev\\source und setzen Sie in Visual Studio " +
                    $"den Standard-Speicherort oder klonen Sie neu nach {targetRoot}Dev\\source.",
                Priority = DevDriveRecommendationPriority.Medium,
                RelatedPath = placement.Path,
            },
            "npm-Cache" => new DevDriveRecommendation
            {
                Title = $"Migration: {placement.DisplayName}",
                Description =
                    $"Erstellen Sie {targetRoot}Dev\\npm-cache, verschieben Sie den Inhalt von {placement.Path} " +
                    $"und setzen Sie die Umgebungsvariable NPM_CONFIG_CACHE={targetRoot}Dev\\npm-cache (persistent über Systemeigenschaften).",
                Priority = DevDriveRecommendationPriority.Medium,
                RelatedPath = placement.Path,
            },
            "NuGet-Pakete" => new DevDriveRecommendation
            {
                Title = $"Migration: {placement.DisplayName}",
                Description =
                    $"Verschieben Sie {placement.Path} nach {targetRoot}Dev\\.nuget\\packages und setzen Sie " +
                    $"NUGET_PACKAGES={targetRoot}Dev\\.nuget\\packages als Benutzer-Umgebungsvariable.",
                Priority = DevDriveRecommendationPriority.Medium,
                RelatedPath = placement.Path,
            },
            "pnpm Store" => new DevDriveRecommendation
            {
                Title = $"Migration: {placement.DisplayName}",
                Description =
                    $"Führen Sie pnpm config set store-dir {targetRoot}Dev\\pnpm-store aus " +
                    "und verschieben Sie den bestehenden Store-Inhalt manuell.",
                Priority = DevDriveRecommendationPriority.Medium,
                RelatedPath = placement.Path,
            },
            "Cargo Registry" => new DevDriveRecommendation
            {
                Title = $"Migration: {placement.DisplayName}",
                Description =
                    $"Setzen Sie CARGO_HOME={targetRoot}Dev\\.cargo in den Benutzer-Umgebungsvariablen " +
                    "und verschieben Sie Registry/Cache dorthin.",
                Priority = DevDriveRecommendationPriority.Medium,
                RelatedPath = placement.Path,
            },
            "Cursor-Daten" or "VS Code-Daten" => new DevDriveRecommendation
            {
                Title = $"Migration: {placement.DisplayName}",
                Description =
                    $"IDE-Daten können per Symlink auf {targetRoot}Dev\\IDE verschoben werden: " +
                    "Ordner verschieben, dann mklink /D am ursprünglichen Pfad. Nur bei geschlossener IDE.",
                Priority = DevDriveRecommendationPriority.Low,
                RelatedPath = placement.Path,
            },
            "Temp (Build-Artefakte)" => new DevDriveRecommendation
            {
                Title = $"Migration: {placement.DisplayName}",
                Description =
                    $"Setzen Sie TEMP und TMP auf {targetRoot}Dev\\Temp (Benutzer-Umgebungsvariablen) " +
                    "für schnellere MSBuild-Zwischendateien.",
                Priority = DevDriveRecommendationPriority.Low,
                RelatedPath = placement.Path,
            },
            _ => null,
        };
    }
}
