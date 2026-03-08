using System.Linq.Expressions;
using Loco1.Data;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id);
        Task<List<T>> AllAsync();
        IQueryable<T> Query();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }

    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly LocoDbContext _db;
        public EfRepository(LocoDbContext db) => _db = db;

        public Task<T?> GetByIdAsync(object id) => _db.Set<T>().FindAsync(id).AsTask();
        public Task<List<T>> AllAsync() => _db.Set<T>().ToListAsync();
        public IQueryable<T> Query() => _db.Set<T>().AsQueryable();

        public Task AddAsync(T entity) { _db.Add(entity); return _db.SaveChangesAsync(); }
        public Task UpdateAsync(T entity) { _db.Update(entity); return _db.SaveChangesAsync(); }
        public async Task DeleteAsync(T entity) { _db.Remove(entity); await _db.SaveChangesAsync(); }
    }
}