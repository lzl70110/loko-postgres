using Microsoft.EntityFrameworkCore;
using Loco1.Repositories;

namespace Loco1.Service
{
    public class CrudService<TEntity, TKey> : Loco1.Service.Abstractions.ICrudService<TEntity, TKey>
        where TEntity : class
    {
        private readonly IRepository<TEntity> _repo;
        private readonly Func<TEntity, TKey> _getKey;

        public CrudService(IRepository<TEntity> repo, Func<TEntity, TKey> getKey)
        {
            _repo = repo;
            _getKey = getKey;
        }

        public Task<List<TEntity>> AllAsync() => _repo.AllAsync();

        public async Task<TEntity?> FindAsync(TKey id)
            => await _repo.Query().FirstOrDefaultAsync(x => _getKey(x)!.Equals(id));

        public async Task<TKey> CreateAsync(TEntity entity)
        {
            await _repo.AddAsync(entity);
            return _getKey(entity);
        }

        public Task UpdateAsync(TEntity entity) => _repo.UpdateAsync(entity);

        public async Task DeleteAsync(TKey id)
        {
            var entity = await FindAsync(id);
            if (entity != null) await _repo.DeleteAsync(entity);
        }
    }
}