using System.Linq;
using System.Security.Claims;
using GCommon;
using Loco1.ViewModels.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Loco1.Service
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStringLocalizer _L;

        private const string PermissionClaimType = "permission";

        public RolePermissionService(
            RoleManager<IdentityRole> roleManager,
            IStringLocalizer<Loco1.Localizer.SharedResource> localizer)
        {
            _roleManager = roleManager;
            _L = localizer;
        }

        // Returns FULL VM with ALL groups; marks Granted by role claims; applies stable ordering.
        public RolePermVm BuildRolePermVm(string roleId)
        {
            var role = _roleManager.FindByIdAsync(roleId).Result;
            if (role == null)
                return new RolePermVm { RoleId = roleId, RoleName = string.Empty, Groups = new List<PermGroupVm>() };

            var claims = _roleManager.GetClaimsAsync(role).Result
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var groups = Perm.Groups
                .Select(g => new PermGroupVm
                {
                    GroupName = g.NameKey,
                    Items = g.Items
                        .Select(i => new PermItemVm
                        {
                            Code = i.Code,            // claim value to persist
                            Display = i.ResourceKey,  // resource key; View uses @L[Display]
                            Granted = claims.Contains(i.Code)
                        })
                        .OrderBy(i => GetActionWeight(i.Code))
                        .ThenBy(i => _L[i.Display].Value, StringComparer.CurrentCultureIgnoreCase)
                        .ThenBy(i => i.Code, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                })
                .OrderBy(g => GroupIndex(g.GroupName))
                .ThenBy(g => _L[g.GroupName].Value, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            return new RolePermVm
            {
                RoleId = role.Id,
                RoleName = role.Name ?? "",
                Groups = groups
            };
        }

        // Builds single group VM for the given role; same ordering as above
        public PermGroupVm BuildGroupVm(string roleId, string groupName)
        {
            var role = _roleManager.FindByIdAsync(roleId).Result;
            if (role == null)
                return new PermGroupVm { GroupName = groupName, Items = new List<PermItemVm>() };

            var claims = _roleManager.GetClaimsAsync(role).Result
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var src = Perm.Groups.First(g => g.NameKey == groupName);

            return new PermGroupVm
            {
                GroupName = groupName,
                Items = src.Items
                    .Select(i => new PermItemVm
                    {
                        Code = i.Code,
                        Display = i.ResourceKey,
                        Granted = claims.Contains(i.Code)
                    })
                    .OrderBy(i => GetActionWeight(i.Code))
                    .ThenBy(i => _L[i.Display].Value, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(i => i.Code, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
        }

        // Updates role claims for a single group
        public void UpdateGroup(string roleId, string groupName, List<PermItemVm> items)
        {
            var role = _roleManager.FindByIdAsync(roleId).Result;
            if (role == null) return;

            var current = _roleManager.GetClaimsAsync(role).Result
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var desired = items
                .Where(x => x.Granted)
                .Select(x => x.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var add in desired.Except(current))
                _roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, add)).Wait();

            foreach (var rem in current.Except(desired))
                _roleManager.RemoveClaimAsync(role, new Claim(PermissionClaimType, rem)).Wait();
        }

        // ---- helpers for consistent ordering ---------------------------------

        private static int GetActionWeight(string code)
        {
            var u = code?.ToUpperInvariant() ?? string.Empty;

            if (u.EndsWith(".VIEW") || u.EndsWith("_VIEW")) return 0;
            if (u.EndsWith(".ADD") || u.EndsWith("_ADD") ||
                u.EndsWith(".CREATE") || u.EndsWith("_CREATE")) return 1;
            if (u.EndsWith(".EDIT") || u.EndsWith("_EDIT")) return 2;
            if (u.EndsWith(".DELETE") || u.EndsWith("_DELETE")) return 3;

            return 9; // other/custom
        }

        private static readonly string[] GroupOrder =
        {
            "Group_Users",
            "Group_Roles",
            "Group_Locomotives",
            "Group_Exploitation",
            "Group_Repairs"
        };

        private static int GroupIndex(string? nameKey)
        {
            var idx = Array.IndexOf(GroupOrder, nameKey ?? string.Empty);
            return idx >= 0 ? idx : int.MaxValue;
        }
    }
}