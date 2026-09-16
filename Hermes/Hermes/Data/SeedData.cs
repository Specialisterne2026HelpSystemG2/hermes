using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Data;

public static class SeedData
{
    public const string AdminRole = "Admin";

    private const string AdminEmail = "admin@hermes.local";
    private const string AdminPassword = "Admin123!";

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
        if (await userManager.FindByEmailAsync(AdminEmail) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true
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
