// Application contract for Admin role management
using Loco1.ViewModels;
namespace Loco1.Service.Abstractions
    {
    public interface IUserRoleService
        {
        Task<List<UserWithRolesVm>> GetAllUsersWithRolesAsync();
        Task<EditUserRolesVm?> GetEditModelAsync(string userId);
        Task<(bool Ok, string? Error)> UpdateRolesAsync(EditUserRolesVm vm);
        }
    }