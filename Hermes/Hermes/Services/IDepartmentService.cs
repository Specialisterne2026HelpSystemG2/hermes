using Hermes.Common;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services;

/// <summary>RF06 — category management.</summary>
public interface IDepartmentService
{
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct = default);

    Task<Department?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<OperationResult<Department>> CreateAsync(DepartmentFormModel model, CancellationToken ct = default);

    Task<OperationResult<Department>> UpdateAsync(DepartmentFormModel model, CancellationToken ct = default);

    Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default);
}
