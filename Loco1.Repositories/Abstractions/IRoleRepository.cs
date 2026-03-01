using Microsoft.AspNetCore.Identity;

namespace Loco1.Repositories.Abstractions;

// Read-only repository interface for roles.
public interface IRoleRepository
{
    Task<IdentityRole?> FindByNameAsync(string roleName);
    Task<bool> ExistsAsync(string roleName);
    Task<List<string>> GetAllNamesAsync();                     // ordered
}