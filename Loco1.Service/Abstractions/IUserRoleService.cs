using Loco1.ViewModels.Roles;

namespace Loco1.Service.Abstractions
    {
    public interface IUserRoleService
        {
        Task<List<UserWithRolesVm>> GetAllUsersWithRolesAsync();
        Task<EditUserRolesVm?> GetEditModelAsync(string userId);
        Task<(bool Ok, string? Error)> UpdateRolesAsync(EditUserRolesVm vm);
        Task<(bool Ok, string? Error)> DeactivateUserAsync(string userId);
        Task<(bool Ok, string? Error)> DeleteUserSafeAsync(string userId, bool hardDelete);
        Task<(bool Ok, string? Error)> RestoreUserAsync(string userId);
        }
    }