using System.ComponentModel.DataAnnotations;
using BstSolutions.Common.Enums;
using BstSolutions.Common.Validation;

namespace BstSolutions.ViewModels.Task;

public class CreateTaskViewModel
{
    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(150, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [Display(Name = "Title")]
    [NoScriptTags]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [Display(Name = "Description")]
    [NoScriptTags]
    public string? Description { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [GreaterThanZero(ErrorMessage = "{0} must be greater than zero.")]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [Display(Name = "Priority")]
    [EnumDataType(typeof(Priority))]
    public Priority Priority { get; set; } = Priority.Medium;

    [Required(ErrorMessage = "{0} is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    [DateNotInPast(ErrorMessage = "{0} cannot be earlier than today's date.")]
    public DateTime DueDate { get; set; }
}
