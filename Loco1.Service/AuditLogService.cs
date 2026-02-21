namespace Loco1.Services;
using Loco1.Data;
using Loco1.Data.Models;

public interface IAuditLogService
    {
    Task LogAsync(
        string user,
        string action,
        string entityName,
        int entityId,
        object? navigationEntity = null);

    Task LogCreateAsync(string user, string entityName, int entityId);
    Task LogUpdateAsync(string user, string entityName, int entityId);
    Task LogDeleteAsync(string user, string entityName, int entityId);
    }

public class AuditLogService : IAuditLogService
    {
    private readonly LocoDbContext _context;

    public AuditLogService(LocoDbContext context)
        {
        _context = context;
        }

    // Generic Log
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

        //   Navigation property linking based on type
        if (navigationEntity is Locomotive loco)
            log.Locomotive = loco;

        if (navigationEntity is Fuel fuel)
            log.Fuel = fuel;

        if (navigationEntity is ShiftWork shift)
            log.ShiftWork = shift;

        _context.AuditLogs.Add(log);

        await _context.SaveChangesAsync();
        }

    // Shortcuts (Create/Update/Delete)

    public Task LogCreateAsync(string user, string entityName, int entityId)
        => LogAsync(user, "Create", entityName, entityId);

    public Task LogUpdateAsync(string user, string entityName, int entityId)
        => LogAsync(user, "Update", entityName, entityId);

    public Task LogDeleteAsync(string user, string entityName, int entityId)
        => LogAsync(user, "Delete", entityName, entityId);
    }