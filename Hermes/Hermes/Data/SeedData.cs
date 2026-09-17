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

    private const string DefaultDepartmentName = "Information Technology";

    private static readonly string[] DefaultCategories =
    [
        "Hardware",
        "Software",
        "Network",
        "Access and permissions",
        "Other"
    ];

    private static readonly string[] DefaultDepartments =
    [
        DefaultDepartmentName,
        "Human Resources",
        "Finance",
        "Sales",
        "Operations",
        "Legal"
    ];

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<HermesContext>();
        await context.Database.MigrateAsync();

        await SeedCategoriesAsync(context);
        await SeedDepartmentsAsync(context);

        // ApplicationUser.DepartmentId is a required FK, so every user needs a real
        // department id. Departments are seeded above, so this always resolves.
        var defaultDepartmentId = await context.Departments
            .Where(d => d.Name == DefaultDepartmentName)
            .Select(d => d.Id)
            .FirstAsync();

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
            // default (empty name, Type 0, DepartmentId 0), which no longer satisfies
            // the model.
            if (existing.Type == UserType.Admin &&
                !string.IsNullOrWhiteSpace(existing.Name) &&
                existing.DepartmentId != 0)
            {
                return;
            }

            existing.Name = AdminName;
            existing.Type = UserType.Admin;
            existing.DepartmentId = defaultDepartmentId;
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
            DepartmentId = defaultDepartmentId,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, AdminPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not create the admin user: {errors}");
        }

        await userManager.AddToRoleAsync(admin, AdminRole);

        // STATIC TEST DATA — seed demo tickets for the newly created admin.
        await SeedTestTicketsAsync(context, admin.Id);
    }

    /// <summary>A ticket cannot be filed without a category, so ship a starting set.</summary>
    private static async Task SeedCategoriesAsync(HermesContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        context.Categories.AddRange(
            DefaultCategories.Select(name => new Category { Name = name, CreatedAt = DateTime.UtcNow }));

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Its own guard on purpose: sharing one with the categories meant that a database
    /// which already had categories never got departments.
    /// </summary>
    private static async Task SeedDepartmentsAsync(HermesContext context)
    {
        if (await context.Departments.AnyAsync())
        {
            return;
        }

        context.Departments.AddRange(
            DefaultDepartments.Select(name => new Department { Name = name, CreatedAt = DateTime.UtcNow }));

        await context.SaveChangesAsync();
    }


    // ──────────────────────────────────────────────────────────────────────
    //  STATIC TEST DATA — demo tickets for development and presentation.
    //  These only exist so the Tickets page is not empty out of the box.
    //  Remove this entire section (and the call in InitializeAsync) once
    //  real ticket data is being created through the application.
    // ──────────────────────────────────────────────────────────────────────
    private static async Task SeedTestTicketsAsync(HermesContext context, string adminId)
    {
        // Skip if any tickets already exist (avoids duplicates on restart).
        if (await context.Tickets.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Look up category IDs by name so the seed stays in sync with the
        // category list above without hard-coding integer keys.
        var categories = await context.Categories
            .ToDictionaryAsync(c => c.Name, c => c.Id);

        int Cat(string name) =>
            categories.TryGetValue(name, out var id) ? id : categories.Values.First();

        // --- Static test tickets ------------------------------------------------
        var testTickets = new[]
        {
            new Ticket
            {
                Title       = "VPN disconnects every hour",
                Description = "The connection drops around the top of each hour. "
                            + "Reconnecting works but it breaks the remote desktop "
                            + "session every time.",
                CategoryId  = Cat("Hardware"),
                Status      = TicketStatus.Open,
                Priority    = TicketPriority.Normal,
                CreatedById = adminId,
                CreatedAt   = now.AddHours(-2)
            },
            new Ticket
            {
                Title       = "Cannot reset my password",
                Description = "The reset email never arrives. I checked spam and "
                            + "tried twice this morning. Other emails from the "
                            + "system come through fine.",
                CategoryId  = Cat("Software"),
                Status      = TicketStatus.Open,
                Priority    = TicketPriority.High,
                CreatedById = adminId,
                CreatedAt   = now.AddHours(-5)
            },
            new Ticket
            {
                Title       = "Request access to Finance reports",
                Description = "My manager approved access yesterday but I still see "
                            + "permission denied when I try to export. The request "
                            + "was for read-only access to Q3 reports.",
                CategoryId  = Cat("Network"),
                Status      = TicketStatus.Resolved,
                Priority    = TicketPriority.Normal,
                CreatedById = adminId,
                CreatedAt   = now.AddDays(-1),
                ResolvedAt  = now.AddHours(-6)
            },
            new Ticket
            {
                Title       = "Printer on floor 3 offline",
                Description = "The queue shows ready but all jobs stay pending. "
                            + "Other floors print fine. Already tried power cycling "
                            + "the printer.",
                CategoryId  = Cat("Access and permissions"),
                Status      = TicketStatus.InProgress,
                Priority    = TicketPriority.Urgent,
                CreatedById = adminId,
                CreatedAt   = now.AddDays(-2)
            },
            new Ticket
            {
                Title       = "License key for design tool",
                Description = "Received the key from procurement but the activation "
                            + "wizard says it has already been used. Need a fresh key "
                            + "or a deactivation on the old machine.",
                CategoryId  = Cat("Other"),
                Status      = TicketStatus.Open,
                Priority    = TicketPriority.Low,
                CreatedById = adminId,
                CreatedAt   = now.AddDays(-3)
            },
            new Ticket
            {
                Title       = "Email attachment size limit",
                Description = "Trying to send a 30 MB PDF but the server rejects it. "
                            + "The error message says the limit is 10 MB. Can this be "
                            + "raised for our department?",
                CategoryId  = Cat("Hardware"),
                Status      = TicketStatus.InProgress,
                Priority    = TicketPriority.High,
                CreatedById = adminId,
                CreatedAt   = now.AddHours(-8)
            }
        };
        // --- End of static test tickets -----------------------------------------

        context.Tickets.AddRange(testTickets);
        await context.SaveChangesAsync();
    }

}
