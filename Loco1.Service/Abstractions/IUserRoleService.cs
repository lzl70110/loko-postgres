using Loco1.ViewModels;

using System.Collections.Generic;
using System.Threading.Tasks;
using Loco1.ViewModels.Roles;  

public interface IUserRoleService
{
    Task<List<UserWithRolesVm>> GetAllUsersWithRolesAsync();

    // Важно: nullable същият като в имплементацията (EditUserRolesVm?).
    Task<EditUserRolesVm?> GetEditModelAsync(string userId);

    Task<(bool Ok, string? Error)> UpdateRolesAsync(EditUserRolesVm vm);
    Task<(bool Ok, string? Error)> DeactivateUserAsync(string userId);
    Task<(bool Ok, string? Error)> DeleteUserSafeAsync(string userId, bool hardDelete);
    Task<(bool Ok, string? Error)> RestoreUserAsync(string userId);
}