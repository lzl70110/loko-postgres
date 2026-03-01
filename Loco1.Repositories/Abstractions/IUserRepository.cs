using Loco1.Data.Models;

namespace Loco1.Repositories.Abstractions;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByIdAsync(string id);
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<List<ApplicationUser>> GetAllAsync();
    Task<List<string>> GetUserRoleNamesAsync(string userId);
}