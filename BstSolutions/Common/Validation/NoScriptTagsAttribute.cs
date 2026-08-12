using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BstSolutions.Common.Validation;

public class NoScriptTagsAttribute : ValidationAttribute
{
    private static readonly Regex OnEventAttributeRegex = new(
        @"\son\w+\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public NoScriptTagsAttribute()
    {
        ErrorMessage = "{0} contains invalid content.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        var displayName = validationContext.DisplayName ?? validationContext.MemberName;
        var lower = text.ToLowerInvariant();

        if (lower.Contains("<script") ||
            lower.Contains("javascript:") ||
            OnEventAttributeRegex.IsMatch(text))
        {
            var message = string.Format(ErrorMessageString!, displayName);
            return new ValidationResult(message);
        }

        return ValidationResult.Success;
    }
}
