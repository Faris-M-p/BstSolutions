using System.ComponentModel.DataAnnotations;
using BstSolutions.Common.Validation;

namespace BstSolutions.ViewModels.Authentication;

public class LoginViewModel
{
    [Required(ErrorMessage = "{0} is required.")]
    [EmailAddress(ErrorMessage = "{0} must be a valid email address.")]
    [StringLength(256, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [Display(Name = "Email")]
    [NoScriptTags]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required.")]
    [MinLength(6, ErrorMessage = "{0} must be at least {1} characters.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
