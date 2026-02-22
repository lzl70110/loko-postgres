using Loco1.Localizer;
using Loco1.Service.Abstractions;                // <-- use your service contract
using Loco1.ViewModels;                           // <-- VMs are in separate project
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Loco1.Web.Controllers
    {
    // EN: Allow both Owner and Admin to access admin area
    [Authorize(Roles = "Owner,Admin")]
    public class AdminController : Controller
        {
        private readonly IUserRoleService _userRoleService;                 // EN: thin controller -> delegate to service
        private readonly IStringLocalizer<SharedResource> L;

        public AdminController(
            IUserRoleService userRoleService,
            IStringLocalizer<SharedResource> localizer)
            {
            _userRoleService = userRoleService;
            L = localizer;
            }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
            {
            // EN: pull ready view-models from the service
            var model = await _userRoleService.GetAllUsersWithRolesAsync();
            return View(model); // Views/Admin/Users.cshtml
            }

        // GET: /Admin/EditRoles/{id}
        [HttpGet]
        public async Task<IActionResult> EditRoles(string id)
            {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var vm = await _userRoleService.GetEditModelAsync(id);
            if (vm is null) return NotFound();

            return View(vm); // Views/Admin/EditRoles.cshtml
            }

        // POST: /Admin/EditRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesVm vm)
            {
            if (!ModelState.IsValid) return View(vm);

            // EN: service enforces guard rails (no Owner removal, keep at least one Admin)
            var (ok, error) = await _userRoleService.UpdateRolesAsync(vm);
            if (!ok)
                {
                // EN: show localized error + rebuild the VM lists
                ModelState.AddModelError(string.Empty, L[error ?? "Role update failed."]);

                var rebuilt = await _userRoleService.GetEditModelAsync(vm.UserId);
                if (rebuilt is not null)
                    {
                    // EN: keep submitted selection so the user sees their choices
                    rebuilt.SelectedRoles = vm.SelectedRoles ?? new List<string>();
                    return View(rebuilt);
                    }
                return View(vm);
                }
            

            TempData["StatusMessage"] = L["Roles updated."];
            
            return RedirectToAction(nameof(Users));
            }
        }
    }