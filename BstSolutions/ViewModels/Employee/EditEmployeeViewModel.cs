using System.ComponentModel.DataAnnotations;
using BstSolutions.Common.Validation;

namespace BstSolutions.ViewModels.Employee;

public class EditEmployeeViewModel
{
    [GreaterThanZero]
    [Display(Name = "Employee")]
    public int Id { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [Display(Name = "First Name")]
    [NoScriptTags]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [Display(Name = "Last Name")]
    [NoScriptTags]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [Display(Name = "Email")]
    [NoScriptTags]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}
