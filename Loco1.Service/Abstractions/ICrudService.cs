namespace Loco1.Service.Abstractions
{
    public interface ICrudService<TEntity, TKey> where TEntity : class
    {
        Task<List<TEntity>> AllAsync();
        Task<TEntity?> FindAsync(TKey id);
        Task<TKey> CreateAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TKey id);
    }
}