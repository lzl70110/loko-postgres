using Loco1.Data;
using Loco1.Data.Models;
using Loco1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Repositories
    {
    public class LocomotiveRepository : ILocomotiveRepository
        {
        private readonly LocoDbContext _db;

        public LocomotiveRepository(LocoDbContext db)
            {
            _db = db;
            }

        public async Task<List<Locomotive>> GetAllAsync(CancellationToken ct = default)
            {
            // Active only (IsDeleted == false), sorted
            return await _db.Locomotives
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Number)
                .ToListAsync(ct);
            }

        public async Task<Locomotive?> GetByIdAsync(int id, CancellationToken ct = default)
            {
            // Returns entity regardless of IsDeleted (service decides what to do)
            return await _db.Locomotives
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            }

        public async Task<bool> ExistsByNumberAsync(string number, int? excludeId = null, CancellationToken ct = default)
            {
            // Unique across ALL records (including soft-deleted)
            var q = _db.Locomotives
                .AsNoTracking()
                .Where(x => x.Number == number);

            if (excludeId.HasValue)
                q = q.Where(x => x.Id != excludeId.Value);

            return await q.AnyAsync(ct);
            }


        public async Task AddAsync(Locomotive entity, CancellationToken ct = default)
            {
            await _db.Locomotives.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            }

        public async Task UpdateAsync(Locomotive entity, CancellationToken ct = default)
            {
            _db.Locomotives.Update(entity);
            await _db.SaveChangesAsync(ct);
            }

        public async Task<bool> DeleteAsync(int id, string actor, string? note = null, CancellationToken ct = default)
            {
            // Soft delete with audit
            var entity = await _db.Locomotives.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) return false;

            if (entity.IsDeleted) return true; // idempotent

            entity.IsDeleted = true;
            entity.Note = note;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = string.IsNullOrWhiteSpace(actor) ? "system" : actor;

            await _db.SaveChangesAsync(ct);
            return true;
            }

        public async Task<bool> UndeleteAsync(int id, string actor, CancellationToken ct = default)
            {
            var entity = await _db.Locomotives.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) return false;

            if (!entity.IsDeleted) return true; // idempotent

            entity.IsDeleted = false;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = string.IsNullOrWhiteSpace(actor) ? "system" : actor;

            await _db.SaveChangesAsync(ct);
            return true;
            }
        }
    }