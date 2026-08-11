using BstSolutions.Common.Enums;

namespace BstSolutions.ViewModels.Task;

public class TaskDetailsViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public Priority Priority { get; set; }

    public WorkTaskStatus Status { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? CompletedDate { get; set; }
}
