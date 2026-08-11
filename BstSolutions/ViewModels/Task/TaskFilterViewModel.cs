using BstSolutions.Common.Enums;

namespace BstSolutions.ViewModels.Task;

public class TaskFilterViewModel
{
    public int? EmployeeId { get; set; }

    public WorkTaskStatus? Status { get; set; }

    public Priority? Priority { get; set; }

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }
}
