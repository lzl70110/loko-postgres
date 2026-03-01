using Loco1.Data;
using Loco1.Repositories.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly LocoDbContext _db;
    public RoleRepository(LocoDbContext db) => _db = db;

    public Task<IdentityRole?> FindByNameAsync(string roleName)
        => _db.Roles.FirstOrDefaultAsync(r => r.NormalizedName == roleName.ToUpper());

    public Task<bool> ExistsAsync(string roleName)
        => _db.Roles.AnyAsync(r => r.NormalizedName == roleName.ToUpper());

    public Task<List<string>> GetAllNamesAsync()
        => _db.Roles.Select(r => r.Name!).OrderBy(n => n).ToListAsync();
}