using System.ComponentModel.DataAnnotations;

namespace iOne.Customer.Validation;

/// <summary>
/// Validates email format only when the value is not null or whitespace.
/// Use for optional email fields so empty values are accepted.
/// </summary>
public class OptionalEmailAddressAttribute : ValidationAttribute
{
    private static readonly EmailAddressAttribute InnerAttribute = new();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return ValidationResult.Success;

        return InnerAttribute.GetValidationResult(s, validationContext);
    }
}
