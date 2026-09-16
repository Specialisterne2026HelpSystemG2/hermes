using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

/// <summary>
/// User profile (RF01.2). Admin maps to the Identity role of the same name.
/// </summary>
public enum UserType
{
    [Display(Name = "Admin")]
    Admin = 1,

    [Display(Name = "Standard user")]
    Standard = 4
}
