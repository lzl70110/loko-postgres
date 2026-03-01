using Loco1.Data.Models;

namespace Loco1.Repositories.Interfaces
    {
    // Repository contract (data access only)
    public interface ILocomotiveRepository
        {
        // Read
        Task<List<Locomotive>> GetAllAsync(CancellationToken ct = default);            // active only (IsDeleted == false)
        Task<Locomotive?> GetByIdAsync(int id, CancellationToken ct = default);        // returns entity by Id (can be deleted)

        Task<bool> ExistsByNumberAsync(string number, int? excludeId = null, CancellationToken ct = default);

        // Write
        Task AddAsync(Locomotive entity, CancellationToken ct = default);
        Task UpdateAsync(Locomotive entity, CancellationToken ct = default);

        // Soft delete / undelete with audit fields
        Task<bool> DeleteAsync(int id, string actor, string? note = null, CancellationToken ct = default);
        Task<bool> UndeleteAsync(int id, string actor, CancellationToken ct = default);
        }
    }