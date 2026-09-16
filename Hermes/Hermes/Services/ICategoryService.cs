using Hermes.Common;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services;

/// <summary>RF06 — category management.</summary>
public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);

    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<OperationResult<Category>> CreateAsync(CategoryFormModel model, CancellationToken ct = default);

    Task<OperationResult<Category>> UpdateAsync(CategoryFormModel model, CancellationToken ct = default);

    Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default);
}
