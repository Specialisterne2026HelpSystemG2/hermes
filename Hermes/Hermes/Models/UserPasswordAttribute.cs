using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

/// <summary>
/// Validates the password on <see cref="UserFormModel"/>: required when creating,
/// optional when editing, and strength-checked whenever one is supplied.
///
/// This is a property-level attribute rather than part of
/// <see cref="IValidatableObject.Validate"/> on purpose. Validator.TryValidateObject
/// runs the property attributes first and skips Validate entirely if any of them
/// failed, so a password rule living in Validate would stay hidden until the user
/// had fixed every other field and submitted a second time.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UserPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var members = new[] { validationContext.MemberName ?? nameof(UserFormModel.Password) };
        var password = value as string;

        if (string.IsNullOrWhiteSpace(password))
        {
            // Editing with a blank password keeps the current one.
            return validationContext.ObjectInstance is UserFormModel { IsEditMode: true }
                ? ValidationResult.Success
                : new ValidationResult("Password is required.", members);
        }

        if (password.Length < 8)
        {
            return new ValidationResult("Password must be at least 8 characters.", members);
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
        {
            return new ValidationResult(
                "Password must contain at least one letter and one digit.", members);
        }

        return ValidationResult.Success;
    }
}
