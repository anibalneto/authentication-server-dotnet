using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task<AuditLog> CreateAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int page, int pageSize);
}
