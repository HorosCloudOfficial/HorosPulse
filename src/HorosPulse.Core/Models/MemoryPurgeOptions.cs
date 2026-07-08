namespace HorosPulse.Core.Models;

using HorosPulse.Core.Enums;

/// <summary>Auswahl der zu leerenden Speicherbereiche (Mem-Reduct-Parität).</summary>
public sealed class MemoryPurgeOptions
{
    public bool PurgeWorkingSet { get; set; } = true;

    public bool PurgeSystemFileCache { get; set; } = true;

    public bool PurgeModifiedPageList { get; set; } = true;

    public bool PurgeStandbyList { get; set; } = true;

    public bool PurgeLowPriorityStandby { get; set; } = true;

    public bool PurgeRegistryCache { get; set; }

    public bool PurgeCombineMemoryLists { get; set; } = true;

    public IReadOnlyList<MemoryPurgeArea> GetEnabledAreas()
    {
        var areas = new List<MemoryPurgeArea>(7);
        if (PurgeWorkingSet) areas.Add(MemoryPurgeArea.WorkingSet);
        if (PurgeSystemFileCache) areas.Add(MemoryPurgeArea.SystemFileCache);
        if (PurgeModifiedPageList) areas.Add(MemoryPurgeArea.ModifiedPageList);
        if (PurgeStandbyList) areas.Add(MemoryPurgeArea.StandbyList);
        if (PurgeLowPriorityStandby) areas.Add(MemoryPurgeArea.LowPriorityStandby);
        if (PurgeRegistryCache) areas.Add(MemoryPurgeArea.RegistryCache);
        if (PurgeCombineMemoryLists) areas.Add(MemoryPurgeArea.CombineMemoryLists);
        return areas;
    }

    public static MemoryPurgeOptions FromSettings(CompactWindowSettings settings) => new()
    {
        PurgeWorkingSet = settings.PurgeWorkingSet,
        PurgeSystemFileCache = settings.PurgeSystemFileCache,
        PurgeModifiedPageList = settings.PurgeModifiedPageList,
        PurgeStandbyList = settings.PurgeStandbyList,
        PurgeLowPriorityStandby = settings.PurgeLowPriorityStandby,
        PurgeRegistryCache = settings.PurgeRegistryCache,
        PurgeCombineMemoryLists = settings.PurgeCombineMemoryLists,
    };
}
