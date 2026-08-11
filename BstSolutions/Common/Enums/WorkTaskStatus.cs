namespace BstSolutions.Common.Enums;

/// <summary>
/// Work task status values. Stored in SQL Server as INT matching these underlying values.
/// Named WorkTaskStatus to avoid clashing with System.Threading.Tasks.TaskStatus.
/// </summary>
public enum WorkTaskStatus
{
    New = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
