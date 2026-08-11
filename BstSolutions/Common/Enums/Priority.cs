namespace BstSolutions.Common.Enums;

/// <summary>
/// Task priority levels. Stored in SQL Server as INT matching these underlying values.
/// </summary>
public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
