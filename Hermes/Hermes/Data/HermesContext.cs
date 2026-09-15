using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Data;

public class HermesContext(DbContextOptions<HermesContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
}