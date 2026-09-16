using Hermes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Data;

public static class SeedData
{
    public const string AdminRole = "Admin";

    private const string AdminEmail = "admin@hermes.local";
    private const string AdminPassword = "Admin123!";
    private const string AdminName = "Hermes Administrator";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<HermesContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var existing = await userManager.FindByEmailAsync(AdminEmail);
        if (existing is not null)
        {
            // An admin created before the profile columns existed has them at their
            // default (empty name, Type 0), which no longer satisfies the model.
            if (existing.Type == UserType.Admin && !string.IsNullOrWhiteSpace(existing.Name))
            {
                return;
            }

            existing.Name = AdminName;
            existing.Type = UserType.Admin;
            existing.Department = Department.InformationTechnology;
            existing.IsActive = true;
            await userManager.UpdateAsync(existing);
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true,
            Name = AdminName,
            Type = UserType.Admin,
            Department = Department.InformationTechnology,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, AdminPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not create the admin user: {errors}");
        }

        await userManager.AddToRoleAsync(admin, AdminRole);
    }
}
