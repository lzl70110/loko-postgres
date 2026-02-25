using GCommon;                         // AppRoles (canonical names)
using Loco1.Service.Abstractions;
using Loco1.ViewModels;
using Loco1.Data.Models;
using Loco1.Localizer;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Loco1.Service
    {
    public class UserRoleService : IUserRoleService
        {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStringLocalizer<SharedResource> L;

        // Use canonical names from AppRoles
        private const string RoleOwner = AppRoles.Owner;
        private const string RoleAdmin = AppRoles.Admin;

        // Explicit hierarchy (highest -> lowest) for UI ordering
        // NOTE: Keep this in sync with your intended business order.
        private static readonly string[] RoleHierarchy =
        {
            AppRoles.Owner,
            AppRoles.Admin,
            AppRoles.RailTransportManager,
            AppRoles.LocomotiveTransportManager,
            AppRoles.DieselLocomotiveRepairManager,
            AppRoles.DieselLocomotiveRepairSupervisor,
            AppRoles.LocomotivesDriversManager,
            AppRoles.User
        };
        // Note: Added explicit hierarchy for stable non-alphabetical ordering.

        public UserRoleService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IStringLocalizer<SharedResource> localizer)
            {
            _userManager = userManager;
            _roleManager = roleManager;
            L = localizer;
            }

        // Utility: order roles by hierarchy (fallback by name for safety)
        private static IEnumerable<string> OrderByHierarchy(IEnumerable<string> roles)
            => roles
                .OrderBy(r => Array.IndexOf(RoleHierarchy, r))
                .ThenBy(r => r, StringComparer.Ordinal);
        // Note: Central method that enforces hierarchy in one place.

        // ===== QUERY =====
        public async Task<List<UserWithRolesVm>> GetAllUsersWithRolesAsync()
            {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var list = new List<UserWithRolesVm>(users.Count);

            foreach (var u in users)
                {
                var roles = await _userManager.GetRolesAsync(u);

                list.Add(new UserWithRolesVm
                    {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? "(no email)",
                    Roles = roles.ToList(),      // keep canonical names; labeling happens in the View
                    IsDeactivated = u.IsDeactivated
                    });
                }

            return list;
            }

        public async Task<EditUserRolesVm?> GetEditModelAsync(string userId)
            {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return null;

            // Get existing DB roles
            var existingDbRoleNames = await _roleManager.Roles
                .Select(r => r.Name!)
                .ToListAsync();

            // Only canonical roles that exist in DB
            var available = AppRoles.All
                .Where(r => existingDbRoleNames.Contains(r))
                .ToList();

            // Hide Owner from list unless the user is already Owner
            var userIsOwner = await _userManager.IsInRoleAsync(user, RoleOwner);
            if (!userIsOwner)
                available = available.Where(r => !r.Equals(RoleOwner, StringComparison.Ordinal)).ToList();

            // Order by business hierarchy (not alphabetical)
            available = OrderByHierarchy(available).ToList();

            var userRoles = await _userManager.GetRolesAsync(user);

            return new EditUserRolesVm
                {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? "(no email)",
                AvailableRoles = available,             // already hierarchical
                SelectedRoles = userRoles.Take(1).ToList() // single-role model -> keep first
                };
            // Note: AvailableRoles now respects hierarchy and hides 'Owner' for non-owners.
            }

        // ===== UPDATE ROLES =====
        public async Task<(bool Ok, string? Error)> UpdateRolesAsync(EditUserRolesVm vm)
            {
            if (vm == null || string.IsNullOrWhiteSpace(vm.UserId))
                return (false, L["Invalid request."]); // localized

            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null)
                return (false, L["User not found."]); // localized

            var desiredRole = vm.SelectedRoles?.FirstOrDefault()?.Trim();
            if (string.IsNullOrWhiteSpace(desiredRole))
                return (false, L["Please select a role."]); // localized

            // Only allow canonical roles
            if (!AppRoles.All.Contains(desiredRole))
                return (false, L["Invalid request."]); // safety

            var roles = await _userManager.GetRolesAsync(user);

            var isOwnerNow = roles.Any(r => r.Equals(RoleOwner, StringComparison.OrdinalIgnoreCase));
            var willBeOwner = desiredRole.Equals(RoleOwner, StringComparison.OrdinalIgnoreCase);

            if (isOwnerNow && !willBeOwner)
                return (false, L["Owner cannot be changed here"]); // localized

            if (!isOwnerNow && willBeOwner)
                return (false, L["Owner cannot be changed here"]); // localized

            var isAdminNow = roles.Any(r => r.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase));
            var willBeAdmin = desiredRole.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase);

            if (isAdminNow && !willBeAdmin)
                {
                var adminCount = (await _userManager.GetUsersInRoleAsync(RoleAdmin)).Count;
                if (adminCount <= 1)
                    return (false, L["Cannot remove the last administrator."]); // localized
                }

            // Role must already exist (seed responsibility). Avoid auto-creation here to keep DB clean.
            if (!await _roleManager.RoleExistsAsync(desiredRole))
                return (false, L["No roles available"]); // localized generic message

            // If user already has exactly that single role -> no-op
            if (roles.Count == 1 && roles.Any(r => r.Equals(desiredRole, StringComparison.OrdinalIgnoreCase)))
                return (true, null);

            // Remove all other roles
            var toRemove = roles.Where(r => !r.Equals(desiredRole, StringComparison.OrdinalIgnoreCase)).ToList();
            if (toRemove.Any())
                {
                var rem = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!rem.Succeeded)
                    return (false, string.Join("; ", rem.Errors.Select(e => e.Description)));
                }

            // Add the desired role if missing
            roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any(r => r.Equals(desiredRole, StringComparison.OrdinalIgnoreCase)))
                {
                var add = await _userManager.AddToRoleAsync(user, desiredRole);
                if (!add.Succeeded)
                    return (false, string.Join("; ", add.Errors.Select(e => e.Description)));
                }

            return (true, null);
            // Note: Localized messages; only canonical roles allowed; Owner cannot be modified/assigned.
            }

        // ===== DEACTIVATE (SOFT DELETE) =====
        public async Task<(bool Ok, string? Error)> DeactivateUserAsync(string userId)
            {
            var (ok, err) = await DeleteUserSafeAsync(userId, hardDelete: false);
            return (ok, err);
            }

        // ===== FULL RESTORE =====
        public async Task<(bool Ok, string? Error)> RestoreUserAsync(string userId)
            {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, L["Invalid request."]); // localized

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return (false, L["User not found."]); // localized

            // restore email
            if (!string.IsNullOrWhiteSpace(user.OriginalEmail))
                {
                var restoredEmail = await MakeUniqueEmail(user.OriginalEmail);
                user.Email = restoredEmail;
                user.NormalizedEmail = restoredEmail.ToUpperInvariant();
                }

            // restore username
            if (!string.IsNullOrWhiteSpace(user.OriginalUserName))
                {
                user.UserName = user.OriginalUserName;
                user.NormalizedUserName = user.OriginalUserName.ToUpperInvariant();
                }

            // cleanup
            user.OriginalEmail = null;
            user.OriginalUserName = null;
            user.IsDeactivated = false;

            await _userManager.SetLockoutEnabledAsync(user, false);
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.UpdateSecurityStampAsync(user);

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
                return (false, string.Join("; ", update.Errors.Select(e => e.Description)));

            return (true, null);
            }

        // ===== INTERNAL REMOVE / DEACTIVATE =====
        public async Task<(bool Ok, string? Error)> DeleteUserSafeAsync(string userId, bool hardDelete)
            {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, L["Invalid request."]); // localized

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return (false, L["User not found."]); // localized

            if (await _userManager.IsInRoleAsync(user, RoleOwner))
                return (false, L["Owner cannot be changed here"]); // no delete for Owner

            if (await _userManager.IsInRoleAsync(user, RoleAdmin))
                {
                var adminCount = (await _userManager.GetUsersInRoleAsync(RoleAdmin)).Count;
                if (adminCount <= 1)
                    return (false, L["Cannot remove the last administrator."]); // localized
                }

            if (hardDelete)
                {
                var del = await _userManager.DeleteAsync(user);
                if (!del.Succeeded)
                    return (false, string.Join("; ", del.Errors.Select(e => e.Description)));
                return (true, null);
                }

            // record original email/username once
            if (string.IsNullOrWhiteSpace(user.OriginalEmail))
                {
                user.OriginalEmail = user.Email;
                user.OriginalUserName = user.UserName;
                }

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            var anon = $"restored+{user.Id}@local";
            user.Email = anon;
            user.NormalizedEmail = anon.ToUpperInvariant();
            user.UserName = anon;
            user.NormalizedUserName = anon.ToUpperInvariant();

            user.IsDeactivated = true;

            await _userManager.UpdateSecurityStampAsync(user);

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
                return (false, string.Join("; ", update.Errors.Select(e => e.Description)));

            return (true, null);
            }

        // ===== produce unique email on restore =====
        private async Task<string> MakeUniqueEmail(string email)
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
        }
    }