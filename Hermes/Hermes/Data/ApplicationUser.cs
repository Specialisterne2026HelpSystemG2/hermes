using Hermes.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Data;

/// <summary>
/// Identity supplies Id, UserName, Email, PasswordHash and the unique normalised
/// email index. Only the Hermes-specific profile fields live here.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string Name { get; set; } = string.Empty;

    public UserType Type { get; set; }

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
