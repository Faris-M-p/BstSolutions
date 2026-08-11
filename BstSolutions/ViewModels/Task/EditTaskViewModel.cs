using System.ComponentModel.DataAnnotations;
using BstSolutions.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BstSolutions.ViewModels.Task;

public class EditTaskViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [Display(Name = "Priority")]
    public Priority Priority { get; set; }

    [Required]
    [Display(Name = "Status")]
    public WorkTaskStatus Status { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; }

    [HiddenInput]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
