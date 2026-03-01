using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Loco1.Data.Models;

namespace Loco1.Web.Infrastructure
{
    /// <summary>
    /// Copies "permission" claims from the user's roles (RoleClaims) into the user principal.
    /// Runs after successful auth and before authorization policies.
    /// </summary>
    public sealed class RolePermissionClaimsTransformer : IClaimsTransformation
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolePermissionClaimsTransformer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true)
                return principal;

            var identity = principal.Identity as ClaimsIdentity;
            if (identity == null)
                return principal;

            // Avoid duplicating — if we already attached "permission" claims, skip
            if (identity.HasClaim(c => c.Type == "permission"))
                return principal;

            var user = await _userManager.GetUserAsync(principal);
            if (user is null) return principal;

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var rc in roleClaims)
                {
                    if (rc.Type == "permission")
                    {
                        // Inject as-is (e.g., "Roles.Edit", "Locomotives.View", ...)
                        identity.AddClaim(new Claim("permission", rc.Value));
                    }
                }
            }

            return principal;
        }
    }
}