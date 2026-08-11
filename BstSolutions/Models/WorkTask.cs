using BstSolutions.Common.Enums;

namespace BstSolutions.Models;

/// <summary>
/// EF Core entity mapped to the WorkTasks table.
/// SQL columns: ID_WorkTask, FK_Employee (mapped via Fluent API).
/// </summary>
public class WorkTask
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int EmployeeId { get; set; }

    public Priority Priority { get; set; }

    public WorkTaskStatus Status { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public Employee Employee { get; set; } = null!;
}
