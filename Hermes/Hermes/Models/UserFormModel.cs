using System.ComponentModel.DataAnnotations;
using Hermes.Data;

namespace Hermes.Models;

/// <summary>
/// Form model for the create/edit user screen.
///
/// Kept separate from <see cref="ApplicationUser"/> on purpose: the form handles a
/// plaintext password and its confirmation, neither of which should exist on the
/// persisted entity.
///
/// RF01.2 — required fields: name, email, password, user type and department.
/// RF01.3 — email format validation.
/// </summary>
public class UserFormModel : IValidatableObject
{
    /// <summary>Null when creating; populated when editing.</summary>
    public string? Id { get; set; }

    public bool IsEditMode => !string.IsNullOrEmpty(Id);

    // ---------- RF01.2: name ----------
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(120, MinimumLength = 3,
        ErrorMessage = "Name must be between {2} and {1} characters.")]
    public string Name { get; set; } = string.Empty;

    // ---------- RF01.2 + RF01.3: email ----------
    // EmailAddressAttribute alone is permissive (it accepts "ana@local", with no dot
    // in the domain). The RegularExpression below requires user@domain.tld.
    [Required(ErrorMessage = "Email is required.")]
    [StringLength(180, ErrorMessage = "Email must be at most {1} characters.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [RegularExpression(@"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$",
        ErrorMessage = "Enter a valid email address (for example: name@company.com).")]
    public string Email { get; set; } = string.Empty;

    // ---------- RF01.2: password ----------
    // No [Required] here: when editing, the password is optional and leaving it blank
    // keeps the current one. The rules live in Validate() below.
    [StringLength(64, ErrorMessage = "Password must be at most {1} characters.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }

    // Nullable enums so the "-- Select --" option in InputSelect yields null and the
    // [Required] below fires.
    [Required(ErrorMessage = "User type is required.")]
    public UserType? Type { get; set; }

    [Required(ErrorMessage = "Department is required.")]
    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Cross-field validation (password against confirmation, and the conditional
    /// requirement of the password itself).
    ///
    /// Note: Blazor's DataAnnotationsValidator only runs IValidatableObject when
    /// validating the whole object, meaning on submit, not on each field change. So
    /// these messages appear when Save is clicked, while the attribute messages
    /// appear as soon as a field loses focus.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var passwordInformed = !string.IsNullOrWhiteSpace(Password);

        if (!IsEditMode && !passwordInformed)
        {
            yield return new ValidationResult(
                "Password is required.", new[] { nameof(Password) });
            yield break;
        }

        if (!passwordInformed)
        {
            // Editing without changing the password: nothing to validate.
            yield break;
        }

        if (Password!.Length < 8)
        {
            yield return new ValidationResult(
                "Password must be at least 8 characters.", new[] { nameof(Password) });
        }

        if (!Password.Any(char.IsLetter) || !Password.Any(char.IsDigit))
        {
            yield return new ValidationResult(
                "Password must contain at least one letter and one digit.",
                new[] { nameof(Password) });
        }

        if (Password != ConfirmPassword)
        {
            yield return new ValidationResult(
                "Password confirmation does not match.", new[] { nameof(ConfirmPassword) });
        }
    }

    /// <summary>Creates an empty form for the create screen.</summary>
    public static UserFormModel ForCreate() => new();

    /// <summary>Creates a form populated from an existing user.</summary>
    public static UserFormModel FromUser(ApplicationUser user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email ?? string.Empty,
        Type = user.Type,
        Department = user.Department,
        IsActive = user.IsActive
    };
}
