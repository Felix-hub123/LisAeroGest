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


        public async Task<Passenger?> GetWithTicketsAndFlightsAsync(int id)

            => await _dbSet

                .Include(p => p.User)

                .Include(p => p.Tickets)

                    .ThenInclude(t => t.Flight)

                .FirstOrDefaultAsync(p => p.Id == id);


        public IQueryable<Passenger> GetAllQueryable()

            => _dbSet.Include(p => p.User).AsQueryable();


        public async Task<Passenger?> GetByEmailAsync(string email)
        {
            var emailLower = email.ToLower();

            return await _context.Passengers
                .Include(p => p.User)
                .FirstOrDefaultAsync(p =>
                    (p.Email != null && p.Email.ToLower() == emailLower) ||
                    (p.User != null && p.User.Email != null && p.User.Email.ToLower() == emailLower));
        }
    }
}
