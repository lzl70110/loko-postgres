using GCommon;
using Loco1.Localizer;
using Loco1.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

using static GCommon.RolesBase;
 


// Disambiguate the constants type (avoid assembly type clash)


namespace Loco1.Web.Controllers
{
    [Authorize(Policy = Perm.Roles_View)]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStringLocalizer<SharedResource> _L;

        private const string OwnerRoleName = "Owner";
        private const string PermissionClaimType = "permission";

        public RolesController(RoleManager<IdentityRole> roleManager,
                               IStringLocalizer<SharedResource> L)
        {
            _roleManager = roleManager;
            _L = L;
        }

        // Index: filter/order by EV.Roles.All; localized display names
        public async Task<IActionResult> Index()
        {
            var dbRoles = await _roleManager.Roles
                .AsNoTracking()
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();

            var dbByName = dbRoles
                .GroupBy(r => r.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
     
            var orderSource =RolesBase. All; // Use the ordered whitelist

            var roles = new List<RoleListItem>(orderSource.Length);
            foreach (var name in orderSource)
            {
                if (!dbByName.TryGetValue(name, out var r)) continue; // skip if not in DB

                roles.Add(new RoleListItem
                {
                    Id = r.Id,
                    Name = LocalizeRole(name) // Role_<Name> with fallback
                });
            }

            return View(roles);
        }

        [Authorize(Policy = Perm.Roles_Edit)]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Guard: Owner cannot be edited via UI
            if (string.Equals(role.Name, OwnerRoleName, StringComparison.Ordinal))
            {
                TempData["StatusMessage"] = _L["OwnerPermissionsCannotBeChanged"].Value;
                return RedirectToAction(nameof(Index));
            }

            var current = (await _roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Localized groups/items from Perm (NameKey/ResourceKey) — label only
            var groups = Perm.Groups.Select(g => new PermGroupVm
            {
                GroupName = _L[g.NameKey].Value,
                Items = g.Items
                    .Select(i => new PermItemVm
                    {
                        Code = i.Code,
                        Display = _L[i.ResourceKey].Value,
                        Granted = current.Contains(i.Code)
                    })
                    .OrderBy(x => x.Display, StringComparer.CurrentCultureIgnoreCase)
                    .ToList()
            }).ToList();

            var vm = new RolePermVm
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty,
                Groups = groups
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Policy = Perm.Roles_Edit)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RolePermVm vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = _L["InvalidFormData"].Value;
                return View(vm);
            }

            var role = await _roleManager.FindByIdAsync(vm.RoleId);
            if (role == null) return NotFound();

            // Guard: Owner cannot be edited via UI
            if (string.Equals(role.Name, OwnerRoleName, StringComparison.Ordinal))
            {
                TempData["StatusMessage"] = _L["OwnerPermissionsCannotBeChanged"].Value;
                return RedirectToAction(nameof(Index));
            }

            var current = (await _roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var desired = vm.Groups
                .SelectMany(g => g.Items)
                .Where(i => i.Granted)
                .Select(i => i.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Rule: strong ops imply View
            EnsureViewForStrongerOps(desired);

            // Add
            foreach (var toAdd in desired.Except(current))
            {
                var res = await _roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, toAdd));
                if (!res.Succeeded)
                {
                    TempData["StatusMessage"] = string.Format(
                        _L["FailedToAddClaim"].Value,
                        string.Join(", ", res.Errors.Select(e => e.Description)));
                    return View(vm);
                }
            }

            // Remove
            foreach (var toRemove in current.Except(desired))
            {
                var res = await _roleManager.RemoveClaimAsync(role, new Claim(PermissionClaimType, toRemove));
                if (!res.Succeeded)
                {
                    TempData["StatusMessage"] = string.Format(
                        _L["FailedToRemoveClaim"].Value,
                        string.Join(", ", res.Errors.Select(e => e.Description)));
                    return View(vm);
                }
            }

            TempData["StatusMessage"] = _L["PermissionsUpdatedSuccessfully"].Value;
            return RedirectToAction(nameof(Edit), new { id = role.Id });
        }

        // Localize role display using resx key "Role_<Name>" with fallback
        private string LocalizeRole(string roleName)
        {
            var key = $"Role_{roleName}";
            var ls = _L[key];
            return ls.ResourceNotFound ? roleName : ls.Value;
        }

        // Ensures .View exists when .Add/.Edit/.Delete are present
        private static void EnsureViewForStrongerOps(HashSet<string> selected)
        {
            if (selected == null || selected.Count == 0) return;

            foreach (var code in selected.ToList())
            {
                if (code.EndsWith(".Edit", StringComparison.OrdinalIgnoreCase) ||
                    code.EndsWith(".Add", StringComparison.OrdinalIgnoreCase) ||
                    code.EndsWith(".Delete", StringComparison.OrdinalIgnoreCase))
                {
                    var viewCode =
                        code.EndsWith(".Edit", StringComparison.OrdinalIgnoreCase) ? code[..^5] + ".View" :
                        code.EndsWith(".Add", StringComparison.OrdinalIgnoreCase) ? code[..^4] + ".View" :
                        /* .Delete */                                                   code[..^7] + ".View";

                    selected.Add(viewCode);
                }
            }
        }
    }
}