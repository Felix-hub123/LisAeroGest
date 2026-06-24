using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class PassengerRepository : GenericRepository<Passenger>, IPassengerRepository
    {
        public PassengerRepository(DataContext context) : base(context) { }

        public async Task<Passenger?> GetByUserIdAsync(string userId)
            => await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);

        public async Task<Passenger?> GetWithTicketsAsync(int id)
            => await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

        public IQueryable<Passenger> GetAllQueryable()
            => _dbSet.Include(p => p.User).AsQueryable();
    }
}
