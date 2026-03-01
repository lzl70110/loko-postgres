
public interface IAuditLogService
    {
    Task LogCreateAsync(string user, string entityName, int entityId);
    Task LogUpdateAsync(string user, string entityName, int entityId);
    Task LogDeleteAsync(string user, string entityName, int entityId);
    }