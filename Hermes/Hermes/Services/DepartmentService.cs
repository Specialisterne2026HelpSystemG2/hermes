using Hermes.Common;
using Hermes.Data;
using Hermes.Models;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Services;

public class DepartmentService(HermesContext context) : IDepartmentService
{
    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct = default) =>
        await context.Departments
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<Department?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await context.Departments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<OperationResult<Department>> CreateAsync(DepartmentFormModel model, CancellationToken ct = default)
    {
        var name = model.Name.Trim();

        if (await NameTakenAsync(name, null, ct))
        {
            return OperationResult<Department>.FieldFailure(
                nameof(DepartmentFormModel.Name), "A department with this name already exists.");
        }

        var department = new Department { Name = name, CreatedAt = DateTime.UtcNow };
        context.Departments.Add(department);
        await context.SaveChangesAsync(ct);

        return OperationResult<Department>.Success(department);
    }

    public async Task<OperationResult<Department>> UpdateAsync(DepartmentFormModel model, CancellationToken ct = default)
    {
        if (model.Id is null)
        {
            return OperationResult<Department>.Failure("Cannot update a department without an id.");
        }

        var department = await context.Departments.FirstOrDefaultAsync(c => c.Id == model.Id.Value, ct);
        if (department is null)
        {
            return OperationResult<Department>.Failure("Department not found.");
        }

        var name = model.Name.Trim();

        if (await NameTakenAsync(name, department.Id, ct))
        {
            return OperationResult<Department>.FieldFailure(
                nameof(DepartmentFormModel.Name), "A department with this name already exists.");
        }

        department.Name = name;
        await context.SaveChangesAsync(ct);

        return OperationResult<Department>.Success(department);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var department = await context.Departments.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (department is null)
        {
            return OperationResult.Failure("Department not found.");
        }

        var inUse = await context.Users.AnyAsync(t => t.DepartmentId == id, ct);
        if (inUse)
        {
            return OperationResult.Failure(
                "This department is used by one or more users and cannot be deleted.");
        }

        context.Departments.Remove(department);
        await context.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    // RF06.2. SQLite compares text case-sensitively, so lowering both sides is what
    // stops "Hardware" and "hardware" both being created.
    private Task<bool> NameTakenAsync(string name, int? excludeId, CancellationToken ct)
    {
        var normalized = name.ToLowerInvariant();

        return context.Departments.AnyAsync(
            c => (excludeId == null || c.Id != excludeId) && c.Name.ToLower() == normalized, ct);
    }
}
