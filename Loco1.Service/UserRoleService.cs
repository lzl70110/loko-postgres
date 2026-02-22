using Loco1.Service.Abstractions;
using Loco1.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Loco1.Localizer;


namespace Loco1.Service
    {
    public class UserRoleService : IUserRoleService
        {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private const string RoleOwner = "Owner";
        private const string RoleAdmin = "Admin";

        public UserRoleService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IStringLocalizer<SharedResource> localizer)
            {
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
            }

        public async Task<List<UserWithRolesVm>> GetAllUsersWithRolesAsync()
            {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserWithRolesVm>(users.Count);

            foreach (var u in users)
                {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserWithRolesVm
                    {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? "(no email)",
                    Roles = roles.ToList()
                    });
                }

            return result;
            }

        public async Task<EditUserRolesVm?> GetEditModelAsync(string userId)
            {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return null;

            var allRoles = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(n => n)
                .ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);

            return new EditUserRolesVm
                {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? "(no email)",
                AvailableRoles = allRoles,
                SelectedRoles = userRoles.Take(1).ToList() // single-role
                };
            }

        public async Task<(bool Ok, string? Error)> UpdateRolesAsync(EditUserRolesVm vm)
            {
            if (vm is null || string.IsNullOrWhiteSpace(vm.UserId))
                return (false, "Invalid request.");

            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user is null) return (false, "User not found.");

            var desiredList = (vm.SelectedRoles ?? Enumerable.Empty<string>())
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList();

            if (desiredList.Count != 1)
                return (false, "Please select a role.");

            var desiredRole = desiredList[0].Trim();

            var current = await _userManager.GetRolesAsync(user);

            const string RoleOwner = "Owner";
            const string RoleAdmin = "Admin";

            var isOwnerNow = current.Contains(RoleOwner);
            var willBeOwner = string.Equals(desiredRole, RoleOwner, StringComparison.OrdinalIgnoreCase);
            if (isOwnerNow && !willBeOwner)
                return (false, "Cannot remove Owner role.");

            var isAdminNow = current.Contains(RoleAdmin);
            var willBeAdmin = string.Equals(desiredRole, RoleAdmin, StringComparison.OrdinalIgnoreCase);
            if (isAdminNow && !willBeAdmin)
                {
                var adminCount = (await _userManager.GetUsersInRoleAsync(RoleAdmin)).Count;
                if (adminCount <= 1)
                    return (false, "Cannot remove the last admin.");
                }

            if (!await _roleManager.RoleExistsAsync(desiredRole))
                {
                var create = await _roleManager.CreateAsync(new IdentityRole(desiredRole));
                if (!create.Succeeded)
                    return (false, "Failed to ensure role(s).");
                }

            var toRemove = current.Where(r => !string.Equals(r, desiredRole, StringComparison.OrdinalIgnoreCase)).ToList();
            if (toRemove.Any())
                {
                var rem = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!rem.Succeeded)
                    return (false, string.Join("; ", rem.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }

            if (!current.Any(r => string.Equals(r, desiredRole, StringComparison.OrdinalIgnoreCase)))
                {
                var add = await _userManager.AddToRoleAsync(user, desiredRole);
                if (!add.Succeeded)
                    return (false, string.Join("; ", add.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }

            return (true, null);
            }
        }
    }