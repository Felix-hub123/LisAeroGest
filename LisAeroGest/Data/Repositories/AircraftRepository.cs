using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class AircraftRepository : GenericRepository<Aircraft>, IAircraftRepository
    {
        public AircraftRepository(DataContext context) : base(context) { }

        public async Task<Aircraft?> GetWithSeatsAsync(int id)
            => await _dbSet.Include(a => a.Seats).FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Aircraft>> GetAvailableAsync()
            => await _dbSet.Where(a => a.IsAvailable).ToListAsync();

        public IQueryable<Aircraft> GetAllQueryable()
            => _dbSet.AsQueryable();
    }
}
