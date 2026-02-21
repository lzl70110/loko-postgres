using Loco1.Service.Abstractions;
using Loco1.ViewModels;                  // ViewModels live in separate project
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Service
    {
    // English: encapsulates Identity role operations used by Admin area
    public class UserRoleService : IUserRoleService
        {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoleService(UserManager<IdentityUser> userManager,
                               RoleManager<IdentityRole> roleManager)
            {
            _userManager = userManager;
            _roleManager = roleManager;
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
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return null;

            var allRoles = await _roleManager.Roles.Select(r => r.Name!).OrderBy(n => n).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            return new EditUserRolesVm
                {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? "(no email)",
                AvailableRoles = allRoles,
                SelectedRoles = userRoles.ToList()
                };
            }

        public async Task<(bool Ok, string? Error)> UpdateRolesAsync(EditUserRolesVm vm)
            {
            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user is null) return (false, "User not found");

            vm.SelectedRoles ??= new List<string>();
            var current = await _userManager.GetRolesAsync(user);

            // remove roles no longer selected
            var toRemove = current.Where(r => !vm.SelectedRoles.Contains(r)).ToList();
            if (toRemove.Any())
                {
                var removeRes = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removeRes.Succeeded)
                    {
                    var err = string.Join("; ", removeRes.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    return (false, err);
                    }
                }

            // add newly selected roles (create missing roles)
            var toAdd = vm.SelectedRoles.Where(r => !current.Contains(r)).ToList();
            if (toAdd.Any())
                {
                foreach (var role in toAdd)
                    {
                    if (!await _roleManager.RoleExistsAsync(role))
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }

                var addRes = await _userManager.AddToRolesAsync(user, toAdd);
                if (!addRes.Succeeded)
                    {
                    var err = string.Join("; ", addRes.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    return (false, err);
                    }
                }

            return (true, null);
            }
        }
    }