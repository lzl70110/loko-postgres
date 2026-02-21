using Loco1.Web.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore; // for ToListAsync

namespace Loco1.Web.Controllers
    {
    // Only Admins can access this controller
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
        {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IStringLocalizer<SharedResource> localizer)
            {
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
            }

        // GET: /Admin/Users
        // English: list all users with their roles
        public async Task<IActionResult> Users()
            {
            // English: async DB enumeration
            var users = await _userManager.Users.ToListAsync();

            var model = new List<UserWithRolesVm>();
            foreach (var u in users)
                {
                var roles = await _userManager.GetRolesAsync(u);
                model.Add(new UserWithRolesVm
                    {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? "(no email)",
                    Roles = roles.ToList()
                    });
                }

            return View(model);
            }

        // GET: /Admin/EditRoles/{id}
        // English: view with role checkboxes for the selected user
        public async Task<IActionResult> EditRoles(string id)
            {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var allRoles = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(n => n)
                .ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);

            var vm = new EditUserRolesVm
                {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? "(no email)",
                AvailableRoles = allRoles,
                SelectedRoles = userRoles.ToList()
                };

            return View(vm);
            }

        // POST: /Admin/EditRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesVm vm)
            {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user is null) return NotFound();

            // English: normalize null -> empty selection
            vm.SelectedRoles ??= new List<string>();

            var current = await _userManager.GetRolesAsync(user);

            // English: prevent removing own Admin (optional safety)
            // if (User.Identity?.Name == user.Email && current.Contains("Admin") && !vm.SelectedRoles.Contains("Admin"))
            // {
            //     ModelState.AddModelError("", _localizer["You cannot remove your own Admin role."]);
            //     return View(vm);
            // }

            // English: remove roles not selected anymore
            var toRemove = current.Where(r => !vm.SelectedRoles.Contains(r)).ToList();
            if (toRemove.Any())
                {
                var removeRes = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removeRes.Succeeded)
                    {
                    ModelState.AddModelError("", _localizer["Failed to remove some roles."]);
                    return View(vm);
                    }
                }

            // English: add newly selected roles (ensure they exist)
            var toAdd = vm.SelectedRoles.Where(r => !current.Contains(r)).ToList();
            if (toAdd.Any())
                {
                foreach (var role in toAdd)
                    if (!await _roleManager.RoleExistsAsync(role))
                        await _roleManager.CreateAsync(new IdentityRole(role));

                var addRes = await _userManager.AddToRolesAsync(user, toAdd);
                if (!addRes.Succeeded)
                    {
                    ModelState.AddModelError("", _localizer["Failed to add some roles."]);
                    return View(vm);
                    }
                }

            TempData["StatusMessage"] = _localizer["Roles updated."];
            return RedirectToAction(nameof(Users));
            }
        }

    // View models (keep simple)
    public class UserWithRolesVm
        {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
        }

    public class EditUserRolesVm
        {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public List<string> AvailableRoles { get; set; } = new();
        public List<string> SelectedRoles { get; set; } = new();
        }
    }