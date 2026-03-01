namespace Loco1.Service.Abstractions;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserRoleService
{
    // explicit types to avoid ambiguous reference
    Task<System.Collections.Generic.List<Loco1.ViewModels.Roles.UserWithRolesVm>> GetAllUsersWithRolesAsync();

    Task<Loco1.ViewModels.Roles.EditUserRolesVm?> GetEditModelAsync(string userId);

    Task<(bool Ok, string? Error)> UpdateRolesAsync(Loco1.ViewModels.Roles.EditUserRolesVm vm);

    Task<(bool Ok, string? Error)> DeactivateUserAsync(string userId);

    Task<(bool Ok, string? Error)> DeleteUserSafeAsync(string userId, bool hardDelete);

    Task<(bool Ok, string? Error)> RestoreUserAsync(string userId);
}