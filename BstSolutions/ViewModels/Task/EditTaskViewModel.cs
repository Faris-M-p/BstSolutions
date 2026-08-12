using System.ComponentModel.DataAnnotations;
using BstSolutions.Common.Enums;
using BstSolutions.Common.Validation;
using Microsoft.AspNetCore.Mvc;

namespace BstSolutions.ViewModels.Task;

public class EditTaskViewModel
{
    [GreaterThanZero]
    [Display(Name = "Task")]
    public int Id { get; set; }

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
    [GreaterThanZero(ErrorMessage = "Please select an employee.")]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [Display(Name = "Priority")]
    [EnumDataType(typeof(Priority))]
    public Priority Priority { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [Display(Name = "Status")]
    [EnumDataType(typeof(WorkTaskStatus))]
    public WorkTaskStatus Status { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; }

    [HiddenInput]
    [Required]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
