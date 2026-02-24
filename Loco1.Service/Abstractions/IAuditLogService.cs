namespace Loco1.Services;

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