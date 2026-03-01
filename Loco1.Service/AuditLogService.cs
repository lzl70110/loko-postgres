using System;
using System.Threading.Tasks;
using Loco1.Data;
using Loco1.Data.Models;
using Loco1.Service.Abstractions;

namespace Loco1.Service;

public sealed class AuditLogService : IAuditLogService
{
    private readonly LocoDbContext _context;

    public AuditLogService(LocoDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    // Generic log
    public async Task LogAsync(
        string user,
        string action,
        string entityName,
        int entityId,
        object? navigationEntity = null)
    {
        var log = new AuditLog
        {
            User = user,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow
        };

        // optional navigation linking
        if (navigationEntity is Locomotive loco) log.Locomotive = loco;
        else if (navigationEntity is Fuel fuel) log.Fuel = fuel;
        else if (navigationEntity is ShiftWork shift) log.ShiftWork = shift;

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    // Shortcuts
    public Task LogCreateAsync(string user, string entityName, int entityId)
        => LogAsync(user, "Create", entityName, entityId);

    public Task LogUpdateAsync(string user, string entityName, int entityId)
        => LogAsync(user, "Update", entityName, entityId);

    public Task LogDeleteAsync(string user, string entityName, int entityId)
        => LogAsync(user, "Delete", entityName, entityId);
}