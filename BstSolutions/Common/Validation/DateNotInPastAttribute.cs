using System.ComponentModel.DataAnnotations;

namespace BstSolutions.Common.Validation;

public class DateNotInPastAttribute : ValidationAttribute
{
    public DateNotInPastAttribute()
    {
        ErrorMessage = "{0} cannot be earlier than today's date.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var displayName = validationContext.DisplayName ?? validationContext.MemberName;

        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not DateTime date)
        {
            var invalidMessage = string.Format(ErrorMessageString!, displayName);
            return new ValidationResult(invalidMessage);
        }

        if (date.Date < DateTime.Today)
        {
            var message = string.Format(ErrorMessageString!, displayName);
            return new ValidationResult(message);
        }

        return ValidationResult.Success;
    }
}
