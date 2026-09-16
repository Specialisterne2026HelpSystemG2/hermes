using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hermes.Data;

/// <summary>
/// Adds the user's display name to the cookie as a claim, so the layout can greet
/// them without querying the database on every render. Identity's default factory
/// only carries the id, username and security stamp.
/// </summary>
public class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    public const string DisplayNameClaim = "display_name";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.Name))
        {
            identity.AddClaim(new Claim(DisplayNameClaim, user.Name));
        }

        return identity;
    }
}
