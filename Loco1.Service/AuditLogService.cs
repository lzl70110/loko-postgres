using System;
using System.Threading.Tasks;
using Loco1.Data;               // LocoDbContext
using Loco1.Data.Models;        // AuditLog (или твоето име)
using Loco1.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Service
{
    // EN: Service for writing audit entries.
    // NOTE: Minimal implementation; adjust entity names/fields to your model.
    public sealed class AuditLogService : IAuditLogService
    {
        private readonly LocoDbContext _db;

        public AuditLogService(LocoDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task LogAsync(string user, string action, string entityName, int entityId, object? navigationEntity = null)
        {
            throw new NotImplementedException();
        }

        public Task LogCreateAsync(string user, string entityName, int entityId)
        {
            throw new NotImplementedException();
        }

        public Task LogDeleteAsync(string user, string entityName, int entityId)
        {
            throw new NotImplementedException();
        }

        public Task LogUpdateAsync(string user, string entityName, int entityId)
        {
            throw new NotImplementedException();
        }

        public async Task WriteAsync(AuditLog entry)
        {
            // Guard
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            _db.AuditLogs.Add(entry);
            await _db.SaveChangesAsync();
        }
    }
}