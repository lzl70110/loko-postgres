using Loco1.Data;
using Loco1.Data.Models;
using Loco1.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly LocoDbContext _db;
    public UserRepository(LocoDbContext db) => _db = db;

    public Task<ApplicationUser?> FindByIdAsync(string id)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<ApplicationUser?> FindByEmailAsync(string email)
        => _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());

    public Task<List<ApplicationUser>> GetAllAsync()
        => _db.Users.AsNoTracking().ToListAsync();

    public Task<List<string>> GetUserRoleNamesAsync(string userId)
        => (from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId
            select r.Name!).ToListAsync();
}