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
    // Not [Required]: when editing, leaving it blank keeps the current password.
    // UserPasswordAttribute handles that conditional requirement and the strength rules.
    [UserPassword]
    [StringLength(64, ErrorMessage = "Password must be at most {1} characters.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }

    // Nullable enums so the "-- Select --" option in InputSelect yields null and the
    // [Required] below fires.
    [Required(ErrorMessage = "User type is required.")]
    public UserType? Type { get; set; }

    [Required(ErrorMessage = "A department is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "A department is required.")]
    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Genuinely cross-field validation. Everything that concerns a single property
    /// lives in an attribute instead, so it shows up on the first submit.
    ///
    /// Note: Blazor's DataAnnotationsValidator only runs IValidatableObject when
    /// validating the whole object, meaning on submit rather than on each field
    /// change, and only once every property attribute has passed.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            // Editing without changing the password: nothing to compare.
            yield break;
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
        DepartmentId = user.DepartmentId,
        IsActive = user.IsActive
    };
}
