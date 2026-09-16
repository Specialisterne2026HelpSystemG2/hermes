using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Data;

public class HermesContext(DbContextOptions<HermesContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.Name)
                .HasMaxLength(120)
                .IsRequired();

            // Enums are stored as int, which is the EF Core default.
            entity.Property(u => u.Type).HasConversion<int>().IsRequired();
            entity.Property(u => u.Department).HasConversion<int>().IsRequired();

            entity.Property(u => u.IsActive).IsRequired();
        });
    }
}
