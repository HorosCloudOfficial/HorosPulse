namespace HorosPulse.Core.Enums;

/// <summary>Mem-Reduct-kompatible Speicherbereiche für NtSetSystemInformation-Purge.</summary>
public enum MemoryPurgeArea
{
    WorkingSet = 0,
    SystemFileCache = 1,
    ModifiedPageList = 2,
    StandbyList = 3,
    LowPriorityStandby = 4,
    RegistryCache = 5,
    CombineMemoryLists = 6,
}
