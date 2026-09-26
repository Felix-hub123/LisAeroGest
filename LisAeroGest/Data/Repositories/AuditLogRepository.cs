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

        public async Task<IEnumerable<AuditLog>>
            GetAllWithDetailsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Flight)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t!.Passenger)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<AuditLog?>
            GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Flight)
                .Include(a => a.Ticket)
                    .ThenInclude(t => t!.Passenger)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}