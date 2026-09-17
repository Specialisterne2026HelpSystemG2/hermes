using Hermes.Common;
using Hermes.Data;
using Hermes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Services;

/// <summary>
/// <see cref="IUserService"/> over ASP.NET Core Identity.
///
/// Writes go through <see cref="UserManager{TUser}"/> rather than the DbContext,
/// because that is what normalises the email, stamps the security stamp and hashes
/// the password. Reads use the context directly since they only feed the grid.
///
/// Keeping the Admin role in step with <see cref="UserType"/> is done here too, so
/// [Authorize(Roles = "Admin")] and the Type column can never disagree.
/// </summary>
public class IdentityUserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly HermesContext _context;

    public IdentityUserService(UserManager<ApplicationUser> userManager, HermesContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        // AsNoTracking: read-only for display, so EF need not track for changes.
        var query = _context.Users.AsNoTracking().Include(t => t.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();

            // The ToLower() calls are required: unlike SQL Server, SQLite compares text
            // case-sensitively by default, so without them "ana" would not find "Ana".
            query = query.Where(u =>
                u.Name.ToLower().Contains(term) ||
                (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(u => u.Name)
            .ToListAsync(ct);
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(t => t.Department)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<OperationResult<ApplicationUser>> CreateAsync(UserFormModel model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            return OperationResult<ApplicationUser>.FieldFailure(
                nameof(UserFormModel.Password), "Password is required.");
        }

        var departmentExists = await _context.Departments.AnyAsync(c => c.Id == model.DepartmentId, ct);
        if (!departmentExists)
        {
            return OperationResult<ApplicationUser>.FieldFailure(
                nameof(UserFormModel.DepartmentId), "Choose a valid department.");
        }

        var email = NormalizeEmail(model.Email);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Name = model.Name.Trim(),
            // The form's [Required] already guaranteed Type and Department are not null.
            Type = model.Type!.Value,
            DepartmentId = model.DepartmentId!.Value,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return ToFailure<ApplicationUser>(result);
        }

        await SyncAdminRoleAsync(user);

        return OperationResult<ApplicationUser>.Success(user);
    }

    public async Task<OperationResult<ApplicationUser>> UpdateAsync(UserFormModel model, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(model.Id))
        {
            return OperationResult<ApplicationUser>.Failure(
                "Cannot update a user without an identifier.");
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null)
        {
            return OperationResult<ApplicationUser>.Failure("User not found.");
        }

        var email = NormalizeEmail(model.Email);

        user.Name = model.Name.Trim();
        user.UserName = email;
        user.Email = email;
        user.Type = model.Type!.Value;
        user.DepartmentId = model.DepartmentId!.Value;
        user.IsActive = model.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ToFailure<ApplicationUser>(result);
        }

        // A blank password on edit means keep the current one.
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (!passwordResult.Succeeded)
            {
                return ToFailure<ApplicationUser>(passwordResult);
            }
        }

        await SyncAdminRoleAsync(user);

        return OperationResult<ApplicationUser>.Success(user);
    }

    public async Task<OperationResult> DeleteAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            // Already gone: the desired outcome happened regardless.
            return OperationResult.Success();
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded ? OperationResult.Success() : ToFailure(result);
    }

    public async Task<bool> EmailExistsAsync(string email, string? excludeId = null, CancellationToken ct = default)
    {
        var normalized = _userManager.KeyNormalizer.NormalizeEmail(NormalizeEmail(email));

        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u =>
                (excludeId == null || u.Id != excludeId) &&
                u.NormalizedEmail == normalized, ct);
    }

    /// <summary>
    /// Mirrors <see cref="UserType.Admin"/> into the Identity Admin role, so role-based
    /// authorization always matches the value shown in the users grid.
    /// </summary>
    private async Task SyncAdminRoleAsync(ApplicationUser user)
    {
        var shouldBeAdmin = user.Type == UserType.Admin;
        var isAdmin = await _userManager.IsInRoleAsync(user, SeedData.AdminRole);

        if (shouldBeAdmin && !isAdmin)
        {
            await _userManager.AddToRoleAsync(user, SeedData.AdminRole);
        }
        else if (!shouldBeAdmin && isAdmin)
        {
            await _userManager.RemoveFromRoleAsync(user, SeedData.AdminRole);
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>
    /// Turns Identity's error codes into a field-level failure where we can, so the
    /// message lands on the offending input rather than in a generic banner.
    /// </summary>
    private static OperationResult<T> ToFailure<T>(IdentityResult result)
    {
        var duplicate = result.Errors.FirstOrDefault(e =>
            e.Code is "DuplicateEmail" or "DuplicateUserName");

        if (duplicate is not null)
        {
            return OperationResult<T>.FieldFailure(
                nameof(UserFormModel.Email), "A user with this email already exists.");
        }

        var password = result.Errors.Where(e => e.Code.StartsWith("Password")).ToList();
        if (password.Count > 0)
        {
            return OperationResult<T>.FieldFailure(
                nameof(UserFormModel.Password),
                string.Join(" ", password.Select(e => e.Description)));
        }

        return OperationResult<T>.Failure(
            string.Join(" ", result.Errors.Select(e => e.Description)));
    }

    private static OperationResult ToFailure(IdentityResult result) =>
        OperationResult.Failure(string.Join(" ", result.Errors.Select(e => e.Description)));
}
