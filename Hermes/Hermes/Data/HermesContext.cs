using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class HermesContext(DbContextOptions<HermesContext> options) : IdentityDbContext<Hermes.Data.ApplicationUser>(options)
{
}
