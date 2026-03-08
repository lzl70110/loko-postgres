using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Loco1.Data.Models;   

namespace Loco1.Web.Infrastructure
{
    public sealed class RoleClaimsTransformation : IClaimsTransformation
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleClaimsTransformation(
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

            var identity = (ClaimsIdentity)principal.Identity!;

            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return principal;

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(role);

                foreach (var claim in roleClaims.Where(c => c.Type == "permission"))
                {
                    if (!identity.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                    {
                        identity.AddClaim(new Claim(claim.Type, claim.Value));
                    }
                }
            }

            return principal;
        }
    }
}