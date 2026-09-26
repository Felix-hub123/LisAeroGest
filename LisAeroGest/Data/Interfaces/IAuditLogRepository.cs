using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IAuditLogRepository :
        IGenericRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetAllWithDetailsAsync();

        Task<AuditLog?> GetByIdWithDetailsAsync(int id);
    }
}