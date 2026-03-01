using Loco1.Localizer;
using Loco1.Service.Abstractions;
using Loco1.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Loco1.Web.Controllers
    {
    // Access restricted to administrative roles
    [Authorize(Roles = "Owner,Admin")]
    public class AdminController : Controller
        {
        private readonly IUserRoleService _userRoleService;
        private readonly IStringLocalizer<SharedResource> L;

        public AdminController(
            IUserRoleService userRoleService,
            IStringLocalizer<SharedResource> localizer)
            {
            _userRoleService = userRoleService;
            L = localizer;
            }

        /// <summary>
        /// List all users with their roles.
        /// </summary>
        public async Task<IActionResult> Users()
            {
            var model = await _userRoleService.GetAllUsersWithRolesAsync();
            return View(model);
            }

        /// <summary>
        /// Load role editing page for a specific user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditRoles(string id)
            {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var vm = await _userRoleService.GetEditModelAsync(id);
            if (vm == null)
                return NotFound();

            return View(vm);
            }

        /// <summary>
        /// Update roles for a user.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesVm vm)
            {
            if (!ModelState.IsValid)
                {
                var rebuilt = await _userRoleService.GetEditModelAsync(vm.UserId);
                if (rebuilt != null)
                    {
                    rebuilt.SelectedRoles = vm.SelectedRoles ?? new List<string>();
                    return View(rebuilt);
                    }
                return RedirectToAction(nameof(Users));
                }

            var (ok, errorKey) = await _userRoleService.UpdateRolesAsync(vm);

            if (!ok)
                {
                var msg = L[errorKey ?? "Role update failed."].Value;
                ModelState.AddModelError(string.Empty, msg);

                var rebuilt = await _userRoleService.GetEditModelAsync(vm.UserId);
                if (rebuilt != null)
                    {
                    rebuilt.SelectedRoles = vm.SelectedRoles ?? new List<string>();
                    return View(rebuilt);
                    }

                return RedirectToAction(nameof(Users));
                }

            TempData["StatusMessage"] = L["Roles updated."].Value;
            return RedirectToAction(nameof(Users));
            }

        /// <summary>
        /// Soft-deactivate a user account (cannot deactivate self).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(string id)
            {
            if (string.IsNullOrWhiteSpace(id))
                {
                TempData["StatusMessage"] = L["Invalid request."].Value;
                return RedirectToAction(nameof(Users));
                }

            var currentUserId =
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == id)
                {
                TempData["StatusMessage"] = L["You cannot deactivate yourself."].Value;
                return RedirectToAction(nameof(Users));
                }

            var (ok, errorKey) = await _userRoleService.DeactivateUserAsync(id);

            if (!ok)
                {
                TempData["StatusMessage"] = L[errorKey ?? "Delete failed."].Value;
                return RedirectToAction(nameof(Users));
                }

            TempData["StatusMessage"] = L["User deactivated."].Value;
            return RedirectToAction(nameof(Users));
            }

        /// <summary>
        /// Restore a previously deactivated user account.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreUser(string id)
            {
            if (string.IsNullOrWhiteSpace(id))
                {
                TempData["StatusMessage"] = L["Invalid request."].Value;
                return RedirectToAction(nameof(Users));
                }

            var (ok, errorKey) = await _userRoleService.RestoreUserAsync(id);

            if (!ok)
                {
                TempData["StatusMessage"] = L[errorKey ?? "Restore failed."].Value;
                return RedirectToAction(nameof(Users));
                }

            TempData["StatusMessage"] = L["User restored."].Value;
            return RedirectToAction(nameof(Users));
            }
        }
    }