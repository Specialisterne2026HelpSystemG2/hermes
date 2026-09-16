using Hermes.Common;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services;

/// <summary>
/// User access contract. Pages depend only on this interface, so the storage
/// mechanism can change without touching any .razor file.
///
/// Identity keys are strings, hence the string ids rather than Guid.
/// </summary>
public interface IUserService
{
    /// <summary>Lists users, optionally filtering by name or email.</summary>
    Task<IReadOnlyList<ApplicationUser>> GetAllAsync(string? search = null, CancellationToken ct = default);

    Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Registration — RF01.1.</summary>
    Task<OperationResult<ApplicationUser>> CreateAsync(UserFormModel model, CancellationToken ct = default);

    Task<OperationResult<ApplicationUser>> UpdateAsync(UserFormModel model, CancellationToken ct = default);

    Task<OperationResult> DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Checks whether the email is already taken. <paramref name="excludeId"/> allows
    /// ignoring the record being edited.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, string? excludeId = null, CancellationToken ct = default);
}
