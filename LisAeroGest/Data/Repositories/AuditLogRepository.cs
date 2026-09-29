using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class AuditLogRepository :
        GenericRepository<AuditLog>,
        IAuditLogRepository
    {
        public AuditLogRepository(DataContext context)
            : base(context)
        {
        }


        // =========================================================
        // QUERY PARA FILTROS / ORDENAÇÃO / PAGINAÇÃO
        // =========================================================

        public IQueryable<AuditLog>
            GetAllWithDetailsQueryable()
        {
            return _dbSet
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Flight)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t!.Passenger);
        }


        // =========================================================
        // TODOS OS REGISTOS
        // =========================================================

        public async Task<IEnumerable<AuditLog>>
            GetAllWithDetailsAsync()
        {
            return await GetAllWithDetailsQueryable()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }


        // =========================================================
        // REGISTO POR ID
        // =========================================================

        public async Task<AuditLog?>
            GetByIdWithDetailsAsync(int id)
        {
            return await GetAllWithDetailsQueryable()
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}