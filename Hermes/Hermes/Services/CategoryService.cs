using Hermes.Common;
using Hermes.Data;
using Hermes.Models;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Services;

public class CategoryService(HermesContext context) : ICategoryService
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<OperationResult<Category>> CreateAsync(CategoryFormModel model, CancellationToken ct = default)
    {
        var name = model.Name.Trim();

        if (await NameTakenAsync(name, null, ct))
        {
            return OperationResult<Category>.FieldFailure(
                nameof(CategoryFormModel.Name), "A category with this name already exists.");
        }

        var category = new Category { Name = name, CreatedAt = DateTime.UtcNow };
        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);

        return OperationResult<Category>.Success(category);
    }

    public async Task<OperationResult<Category>> UpdateAsync(CategoryFormModel model, CancellationToken ct = default)
    {
        if (model.Id is null)
        {
            return OperationResult<Category>.Failure("Cannot update a category without an id.");
        }

        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == model.Id.Value, ct);
        if (category is null)
        {
            return OperationResult<Category>.Failure("Category not found.");
        }

        var name = model.Name.Trim();

        if (await NameTakenAsync(name, category.Id, ct))
        {
            return OperationResult<Category>.FieldFailure(
                nameof(CategoryFormModel.Name), "A category with this name already exists.");
        }

        category.Name = name;
        await context.SaveChangesAsync(ct);

        return OperationResult<Category>.Success(category);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return OperationResult.Failure("Category not found.");
        }

        var inUse = await context.Tickets.AnyAsync(t => t.CategoryId == id, ct);
        if (inUse)
        {
            return OperationResult.Failure(
                "This category is used by one or more tickets and cannot be deleted.");
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    // RF06.2. SQLite compares text case-sensitively, so lowering both sides is what
    // stops "Hardware" and "hardware" both being created.
    private Task<bool> NameTakenAsync(string name, int? excludeId, CancellationToken ct)
    {
        var normalized = name.ToLowerInvariant();

        return context.Categories.AnyAsync(
            c => (excludeId == null || c.Id != excludeId) && c.Name.ToLower() == normalized, ct);
    }
}
