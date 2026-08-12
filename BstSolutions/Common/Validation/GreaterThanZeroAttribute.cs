using System.ComponentModel.DataAnnotations;

namespace BstSolutions.Common.Validation;

public class GreaterThanZeroAttribute : ValidationAttribute
{
    public GreaterThanZeroAttribute()
    {
        ErrorMessage = "{0} must be greater than zero.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var displayName = validationContext.DisplayName ?? validationContext.MemberName;

        if (value is null)
        {
            return ValidationResult.Success;
        }

        try
        {
            var numeric = Convert.ToDecimal(value);
            if (numeric <= 0)
            {
                var message = string.Format(ErrorMessageString!, displayName);
                return new ValidationResult(message);
            }
        }
        catch (Exception)
        {
            var message = string.Format(ErrorMessageString!, displayName);
            return new ValidationResult(message);
        }

        return ValidationResult.Success;
    }
}
