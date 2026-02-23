using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loco1.Service.Abstractions;
using Loco1.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Loco1.Data.Models;

namespace Loco1.Service
    {
    // Includes: reload roles after removal + Owner/Admin guards
    public class UserRoleService : IUserRoleService
        {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private const string RoleOwner = "Owner";
        private const string RoleAdmin = "Admin";

        public UserRoleService(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager)
            {
            _userManager = userManager;
            _roleManager = roleManager;
            }

        // ===== QUERY =====
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
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return null;

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
                SelectedRoles = userRoles.Take(1).ToList() // radio → single role
                };
            }

        // ===== UPDATE ROLES =====
        public async Task<(bool Ok, string? Error)> UpdateRolesAsync(EditUserRolesVm vm)
            {
            if (vm == null || string.IsNullOrWhiteSpace(vm.UserId))
                return (false, "Invalid request.");

            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null)
                return (false, "User not found.");

            // Radio button → single role only
            var desiredRole = vm.SelectedRoles?
                .FirstOrDefault(r => !string.IsNullOrWhiteSpace(r))?
                .Trim();

            if (string.IsNullOrWhiteSpace(desiredRole))
                return (false, "Please select a role.");

            var currentRoles = await _userManager.GetRolesAsync(user);

            // --- GUARDS ---
            var isOwnerNow = currentRoles.Any(r => r.Equals(RoleOwner, StringComparison.OrdinalIgnoreCase));
            var willBeOwner = desiredRole.Equals(RoleOwner, StringComparison.OrdinalIgnoreCase);

            // 1) Owner cannot be removed
            if (isOwnerNow && !willBeOwner)
                return (false, "Cannot remove Owner role.");

            // 2) Nobody can become new Owner via UI/service
            if (!isOwnerNow && willBeOwner)
                return (false, "Owner cannot be removed here"); // reuse UI text / or add separate key "OwnerCannotBeAssigned"

            // 3) Do not remove last Admin
            var isAdminNow = currentRoles.Any(r => r.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase));
            var willBeAdmin = desiredRole.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase);
            if (isAdminNow && !willBeAdmin)
                {
                var adminCount = (await _userManager.GetUsersInRoleAsync(RoleAdmin)).Count;
                if (adminCount <= 1)
                    return (false, "Cannot remove the last admin.");
                }

            // Ensure desired role exists (idempotent)
            if (!await _roleManager.RoleExistsAsync(desiredRole))
                {
                var create = await _roleManager.CreateAsync(new IdentityRole(desiredRole));
                if (!create.Succeeded)
                    return (false, "Failed to ensure default role.");
                }

            // No change → OK
            if (currentRoles.Count == 1 &&
                currentRoles.Any(r => r.Equals(desiredRole, StringComparison.OrdinalIgnoreCase)))
                {
                return (true, null);
                }

            // Remove all roles except the desired one
            var toRemove = currentRoles
                .Where(r => !r.Equals(desiredRole, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (toRemove.Any())
                {
                var rem = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!rem.Succeeded)
                    return (false, string.Join("; ", rem.Errors.Select(e => e.Description)));
                }

            // CRITICAL: reload roles after removal
            currentRoles = await _userManager.GetRolesAsync(user);

            // Add desired if still missing
            if (!currentRoles.Any(r => r.Equals(desiredRole, StringComparison.OrdinalIgnoreCase)))
                {
                var add = await _userManager.AddToRoleAsync(user, desiredRole);
                if (!add.Succeeded)
                    return (false, string.Join("; ", add.Errors.Select(e => e.Description)));
                }

            return (true, null);
            }

        // ===== DELETE / DEACTIVATE / RESTORE =====
        public async Task<(bool Ok, string? Error)> DeleteUserSafeAsync(string userId, bool hardDelete)
            {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "Invalid request.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return (false, "User not found.");

            // Guards
            if (await _userManager.IsInRoleAsync(user, RoleOwner))
                return (false, "Owner account cannot be deleted.");

            if (await _userManager.IsInRoleAsync(user, RoleAdmin))
                {
                var adminCount = (await _userManager.GetUsersInRoleAsync(RoleAdmin)).Count;
                if (adminCount <= 1)
                    return (false, "Cannot remove the last admin.");
                }

            if (hardDelete)
                {
                var del = await _userManager.DeleteAsync(user);
                if (!del.Succeeded)
                    return (false, string.Join("; ", del.Errors.Select(e => e.Description)));
                return (true, null);
                }
            else
                {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                await _userManager.UpdateSecurityStampAsync(user);

                var update = await _userManager.UpdateAsync(user);
                if (!update.Succeeded)
                    return (false, string.Join("; ", update.Errors.Select(e => e.Description)));

                return (true, null);
                }
            }

        public async Task<(bool Ok, string? Error)> DeactivateUserAsync(string userId)
            => await DeleteUserSafeAsync(userId, hardDelete: false);

        public async Task<(bool Ok, string? Error)> RestoreUserAsync(string userId)
            {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "Invalid request.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return (false, "User not found.");

            await _userManager.SetLockoutEnabledAsync(user, false);
            await _userManager.SetLockoutEndDateAsync(user, null);

            var baseEmail = $"restored+{user.Id}@local";

            async Task<string> MakeUnique(string email)
                {
                var candidate = email;
                int i = 1;
                while (await _userManager.FindByEmailAsync(candidate) != null)
                    {
                    var at = email.IndexOf('@');
                    candidate = at > 0
                        ? $"{email[..at]}+{i}{email[at..]}"
                        : $"{email}+{i}";
                    i++;
                    }
                return candidate;
                }

            var uniqueEmail = await MakeUnique(baseEmail);
            user.Email = uniqueEmail;
            user.NormalizedEmail = uniqueEmail.ToUpperInvariant();
            user.UserName = uniqueEmail;
            user.NormalizedUserName = uniqueEmail.ToUpperInvariant();

            await _userManager.UpdateSecurityStampAsync(user);
            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
                return (false, string.Join("; ", update.Errors.Select(e => e.Description)));

            return (true, null);
            }
        }
    }