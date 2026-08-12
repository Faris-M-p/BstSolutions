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
    [StringLength(200, ErrorMessage = "Search text cannot exceed 200 characters.")]
    [NoScriptTags]
    public string? Search { get; set; }

    [StringLength(50)]
    [NoScriptTags]
    public string? SortBy { get; set; }

    [StringLength(10)]
    [NoScriptTags]
    public string? SortDirection { get; set; }
}
