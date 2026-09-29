using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class FlightCommunicationRepository
        : GenericRepository<FlightCommunication>,
          IFlightCommunicationRepository
    {
        public FlightCommunicationRepository(DataContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<FlightCommunication>>
            GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(c => c.Flight)
                .Include(c => c.SentByUser)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<FlightCommunication?>
            GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Flight)
                .Include(c => c.SentByUser)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public IQueryable<FlightCommunication>
            GetAllWithDetailsQueryable()
        {
            return _dbSet
                .Include(c => c.Flight)
                .Include(c => c.SentByUser)
                .AsQueryable();
        }
    }
}