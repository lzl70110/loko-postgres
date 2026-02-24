using Loco1.Data;
using Loco1.Data.Models;

namespace Loco1.Services;

public class AuditLogService : IAuditLogService
    {
    private readonly LocoDbContext _context;

    public AuditLogService(LocoDbContext context)
        {
        _context = context;
        }

    public async Task LogAsync(
        string user,
        string action,
        string entityName,
        int entityId,
        object? navigationEntity = null)
        {
        var log = new AuditLog
            {
            CreatedBy = user,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow
            };

        if (navigationEntity is Locomotive loco) log.Locomotive = loco;
        if (navigationEntity is Fuel fuel) log.Fuel = fuel;
        if (navigationEntity is ShiftWork shift) log.ShiftWork = shift;

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
        }

    public Task LogCreateAsync(string user, string entityName, int entityId)
        => LogAsync(user, "Create", entityName, entityId);

    public Task LogUpdateAsync(string user, string entityName, int entityId)
        => LogAsync(user, "Update", entityName, entityId);

    public Task LogDeleteAsync(string user, string entityName, int entityId)
        => LogAsync(user, "Delete", entityName, entityId);
    }