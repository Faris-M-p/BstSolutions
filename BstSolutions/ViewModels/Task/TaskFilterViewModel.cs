using System.ComponentModel.DataAnnotations;
using BstSolutions.Common.Enums;
using BstSolutions.Common.Validation;

namespace BstSolutions.ViewModels.Task;

public class TaskFilterViewModel
{
    [Display(Name = "Employee")]
    public int? EmployeeId { get; set; }

    [Display(Name = "Status")]
    [EnumDataType(typeof(WorkTaskStatus))]
    public WorkTaskStatus? Status { get; set; }

    [Display(Name = "Priority")]
    [EnumDataType(typeof(Priority))]
    public Priority? Priority { get; set; }

    [Display(Name = "Search text")]
    [StringLength(200, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [NoScriptTags]
    public string? Search { get; set; }

    [Display(Name = "Sort by")]
    [StringLength(50, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [NoScriptTags]
    public string? SortBy { get; set; }

    [Display(Name = "Sort direction")]
    [StringLength(10, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [NoScriptTags]
    public string? SortDirection { get; set; }
}
