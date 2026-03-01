using System;
using System.Linq;
using System.Threading.Tasks;
using Loco1.Data.Models;
using Loco1.Repositories.Abstractions;
using Loco1.Service.Abstractions;
using Loco1.ViewModels.Roles;
using Microsoft.AspNetCore.Identity;

namespace Loco1.Service;

public sealed class UserRoleService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IUserRepository users,
    IRoleRepository roles) : IUserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    private readonly RoleManager<IdentityRole> _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
    private readonly IRoleRepository _roles = roles ?? throw new ArgumentNullException(nameof(roles));

    public async Task<System.Collections.Generic.List<UserWithRolesVm>> GetAllUsersWithRolesAsync()
    {
        var list = await _users.GetAllAsync();
        var result = new System.Collections.Generic.List<UserWithRolesVm>(list.Count);

        foreach (var u in list)
        {
            var roleNames = await _users.GetUserRoleNamesAsync(u.Id);
            result.Add(new UserWithRolesVm
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                Roles = roleNames
            });
        }

        return result;
    }

    public async Task<EditUserRolesVm?> GetEditModelAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return null;

        var allRoleNames = await _roles.GetAllNamesAsync();
        var currentRoleNames = await _users.GetUserRoleNamesAsync(user.Id);

        return new EditUserRolesVm
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Owner = false,
            OwnerRoleName = "Owner",
            AvailableRoles = allRoleNames,
            SelectedRoles = [.. currentRoleNames]
        };
    }

    public async Task<(bool Ok, string? Error)> UpdateRolesAsync(EditUserRolesVm vm)
    {
        if (vm is null) return (false, "Model is null");

        var user = await _userManager.FindByIdAsync(vm.UserId);
        if (user == null) return (false, "User not found");

        var current = await _userManager.GetRolesAsync(user);
        if (current.Count > 0)
        {
            var rem = await _userManager.RemoveFromRolesAsync(user, current);
            if (!rem.Succeeded)
                return (false, string.Join(", ", rem.Errors.Select(e => e.Description)));
        }

        var toAdd = vm.SelectedRoles?.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                   ?? [];

        foreach (var r in toAdd)
        {
            if (!await _roleManager.RoleExistsAsync(r))
                return (false, $"Role '{r}' does not exist");

            var add = await _userManager.AddToRoleAsync(user, r);
            if (!add.Succeeded)
                return (false, string.Join(", ", add.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> DeactivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        var res = await _userManager.UpdateAsync(user);
        return res.Succeeded
            ? (true, null)
            : (false, string.Join(", ", res.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Ok, string? Error)> DeleteUserSafeAsync(string userId, bool hardDelete)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        if (hardDelete)
        {
            var del = await _userManager.DeleteAsync(user);
            return del.Succeeded
                ? (true, null)
                : (false, string.Join(", ", del.Errors.Select(e => e.Description)));
        }

        const string mark = "restored+";
        user.Email = user.Email is null ? null : $"{mark}{user.Email}";
        user.UserName = user.UserName is null ? null : $"{mark}{user.UserName}";
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        var upd = await _userManager.UpdateAsync(user);
        return upd.Succeeded
            ? (true, null)
            : (false, string.Join(", ", upd.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Ok, string? Error)> RestoreUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        const string mark = "restored+";
        if (user.Email?.StartsWith(mark, StringComparison.OrdinalIgnoreCase) == true)
            user.Email = user.Email[mark.Length..];
        if (user.UserName?.StartsWith(mark, StringComparison.OrdinalIgnoreCase) == true)
            user.UserName = user.UserName[mark.Length..];

        user.LockoutEnd = null;

        var upd = await _userManager.UpdateAsync(user);
        return upd.Succeeded
            ? (true, null)
            : (false, string.Join(", ", upd.Errors.Select(e => e.Description)));
    }
}
