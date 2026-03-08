 
using System.Security.Claims;
using System.Linq;
using GCommon;
using Loco1.Localizer;
using Loco1.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static System.StringComparison;

namespace Loco1.Web.Controllers
{
    // Requires view permissions for roles
    [Authorize(Policy = Perm.Roles_View)]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStringLocalizer<SharedResource> _L;
        private readonly IRolePermissionService _permService;

        // Keep owner name consistent with RolesBase; do not list/edit via UI
        private const string OwnerRoleName = RolesBase.Owner;

        // Claim type used to store permissions on roles
        private const string PermissionClaimType = "permission";

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            IStringLocalizer<SharedResource> L,
            IRolePermissionService permService)
        {
            _roleManager = roleManager;
            _L = L;
            _permService = permService;
        }

        // GET: /Roles
        // Lists roles in predefined order (RolesBase.All). Skips roles not in the whitelist (Owner on purpose).
        public async Task<IActionResult> Index()
        {
            var dbRoles = await _roleManager.Roles
                .AsNoTracking()
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();

            var dbByName = dbRoles
                .GroupBy(r => r.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var orderSource = RolesBase.All;
            var roles = new List<RoleListItem>(orderSource.Length);

            foreach (var name in orderSource)
            {
                if (!dbByName.TryGetValue(name, out var r)) continue;

                roles.Add(new RoleListItem
                {
                    Id = r.Id,
                    Name = LocalizeRole(name) // Uses resource key "Role_<Name>" with fallback
                });
            }

            return View(roles);
        }

        // GET: /Roles/Edit/{id}
        // Build full VM via service; service handles ordering; controller stays thin.
        [Authorize(Policy = Perm.Roles_Edit)]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Guard: Owner cannot be edited via UI
            if (string.Equals(role.Name, OwnerRoleName, Ordinal))
            {
                TempData["StatusMessage"] = _L["OwnerPermissionsCannotBeChanged"].Value;
                return RedirectToAction(nameof(Index));
            }

            // Service returns ALL groups (so you can grant new permissions)
            var vm = _permService.BuildRolePermVm(id);

            return View(vm);
        }

        // POST: /Roles/Edit
        // Diff-based update of role permission claims with validation + universal View rule.
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
            if (string.Equals(role.Name, OwnerRoleName, Ordinal))
            {
                TempData["StatusMessage"] = _L["OwnerPermissionsCannotBeChanged"].Value;
                return RedirectToAction(nameof(Index));
            }

            // Current claims
            var current = (await _roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Desired = all checked codes from posted form
            var desired = vm.Groups
                .SelectMany(g => g.Items)
                .Where(i => i.Granted)
                .Select(i => i.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Existing helper (suffix-based) — keeps backward compatibility
            EnsureViewForStrongerOps(desired);

            // UNIVERSAL: any Add/Edit/Delete => ensure matching View (supports '.' and '_')
            var autoView = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var code in desired)
            {
                var up = code.ToUpperInvariant();
                bool isStrong =
                       up.EndsWith(".EDIT") || up.EndsWith(".ADD") || up.EndsWith(".DELETE")
                    || up.EndsWith("_EDIT") || up.EndsWith("_ADD") || up.EndsWith("_DELETE");
                if (!isStrong) continue;

                char sep = up.Contains('.') ? '.' : '_';
                int idx = code.LastIndexOf(sep);
                if (idx < 0) continue;

                var prefix = code[..idx];
                var viewCode = $"{prefix}{sep}View";
                autoView.Add(viewCode);
            }
            foreach (var v in autoView) desired.Add(v);

            // SAFETY: remove strong ops if corresponding View is not present
            var toRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var code in desired)
            {
                var up = code.ToUpperInvariant();
                bool isStrong =
                       up.EndsWith(".EDIT") || up.EndsWith(".ADD") || up.EndsWith(".DELETE")
                    || up.EndsWith("_EDIT") || up.EndsWith("_ADD") || up.EndsWith("_DELETE");
                if (!isStrong) continue;

                char sep = up.Contains('.') ? '.' : '_';
                int idx = code.LastIndexOf(sep);
                if (idx < 0) continue;

                var prefix = code[..idx];
                var viewCode = $"{prefix}{sep}View";
                if (!desired.Contains(viewCode)) toRemove.Add(code);
            }
            foreach (var c in toRemove) desired.Remove(c);

            // Add missing claims
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

            // Remove extra claims
            foreach (var toRem in current.Except(desired))
            {
                var res = await _roleManager.RemoveClaimAsync(role, new Claim(PermissionClaimType, toRem));
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

        // Uses resx key "Role_<Name>" with fallback to raw role name
        private string LocalizeRole(string roleName)
        {
            var key = $"Role_{roleName}";
            var ls = _L[key];
            return ls.ResourceNotFound ? roleName : ls.Value;
        }

        // Ensures that a matching View permission exists when Edit/Add/Delete are selected.
        // Supports codes like "Perm.Users.Edit" or "Perm_Users_Edit".
        private static void EnsureViewForStrongerOps(HashSet<string> selected)
        {
            if (selected == null || selected.Count == 0) return;

            var snapshot = selected.ToList();
            foreach (var code in snapshot)
            {
                if (TryBuildViewCode(code, out var viewCode))
                {
                    selected.Add(viewCode);
                }
            }

            static bool TryBuildViewCode(string code, out string viewCode)
            {
                viewCode = string.Empty;

                var suffixes = new[] { ".Edit", ".Add", ".Delete", "_Edit", "_Add", "_Delete" };

                foreach (var sfx in suffixes)
                {
                    if (code.EndsWith(sfx, OrdinalIgnoreCase))
                    {
                        var baseCode = code[..^sfx.Length];
                        var sep = sfx[0]; // '.' or '_'
                        viewCode = $"{baseCode}{sep}View";
                        return true;
                    }
                }

                return false;
            }
        }
    }
}