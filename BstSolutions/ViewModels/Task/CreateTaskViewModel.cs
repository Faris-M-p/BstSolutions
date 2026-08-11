using System.ComponentModel.DataAnnotations;
using BstSolutions.Common.Enums;

namespace BstSolutions.ViewModels.Task;

public class CreateTaskViewModel
{
    [Required]
    [StringLength(150)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an employee.")]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [Display(Name = "Priority")]
    public Priority Priority { get; set; } = Priority.Medium;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; }
}
