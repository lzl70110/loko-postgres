using GCommon;
using Loco1.Localizer;
using Loco1.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Loco1.Web.Controllers
{
    [Authorize(Policy = Perm.Roles_View)]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStringLocalizer<SharedResource> _L;

        // За защита на Owner
        private const string OwnerRoleName = "Owner";
        private const string PermissionClaimType = "permission";

        public RolesController(RoleManager<IdentityRole> roleManager,
                               IStringLocalizer<SharedResource> L)
        {
            _roleManager = roleManager;
            _L = L;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles
                                    .Select(r => new RoleListItem
                                    {
                                        Id = r.Id,
                                        Name = r.Name!
                                    })
                                    .ToList();

            return View(roles);
        }

        [Authorize(Policy = Perm.Roles_Edit)]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (role.Name == OwnerRoleName)
            {
                TempData["StatusMessage"] = _L["OwnerPermissionsCannotBeChanged"].Value;
                return RedirectToAction(nameof(Index));
            }

            var currentClaims = (await _roleManager.GetClaimsAsync(role))
                                .Where(c => c.Type == PermissionClaimType)
                                .Select(c => c.Value)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var groups = Perm.Groups
                 .Select(g => new PermGroupVm
                 {
                     GroupName = g.Name,
                     Items = g.Items.Select(i => new PermItemVm
                     {
                         Code = i.Code,
                         Display = i.Display,
                         Granted = currentClaims.Contains(i.Code)
                     }).ToList()
                 }).ToList();

            var vm = new RolePermVm
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty, // avoid nullable warning
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

            if (role.Name == OwnerRoleName)
            {
                TempData["StatusMessage"] = _L["OwnerPermissionsCannotBeChanged"].Value;
                return RedirectToAction(nameof(Index));
            }

            var currentClaims = (await _roleManager.GetClaimsAsync(role))
                                .Where(c => c.Type == PermissionClaimType)
                                .Select(c => c.Value)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var desiredClaims = vm.Groups
                                  .SelectMany(g => g.Items)
                                  .Where(i => i.Granted)
                                  .Select(i => i.Code)
                                  .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Add new claims
            foreach (var claim in desiredClaims.Except(currentClaims))
            {
                var res = await _roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, claim));
                if (!res.Succeeded)
                {
                    TempData["StatusMessage"] = string.Format(
                        _L["FailedToAddClaim"], string.Join(", ", res.Errors.Select(e => e.Description)));
                    return View(vm);
                }
            }

            // Remove old claims
            foreach (var claim in currentClaims.Except(desiredClaims))
            {
                var res = await _roleManager.RemoveClaimAsync(role, new Claim(PermissionClaimType, claim));
                if (!res.Succeeded)
                {
                    TempData["StatusMessage"] = string.Format(
                        _L["FailedToRemoveClaim"], string.Join(", ", res.Errors.Select(e => e.Description)));
                    return View(vm);
                }
            }

            TempData["StatusMessage"] = _L["PermissionsUpdatedSuccessfully"].Value;
            return RedirectToAction(nameof(Edit), new { id = role.Id });
        }
    }
}