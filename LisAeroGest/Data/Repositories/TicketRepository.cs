using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(DataContext context) : base(context) { }

        public async Task<Ticket?> GetWithDetailsAsync(int id)
            => await _dbSet
                .Include(t => t.Passenger).ThenInclude(p => p!.User)
                .Include(t => t.Flight).ThenInclude(f => f!.Airline)
                .Include(t => t.Flight).ThenInclude(f => f!.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.DestinationAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.Gate)
                .Include(t => t.Seat)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Ticket>> GetByPassengerAsync(int passengerId)
            => await _dbSet
                .Include(t => t.Flight).ThenInclude(f => f!.Airline)
                .Include(t => t.Flight).ThenInclude(f => f!.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.DestinationAirport)
                .Include(t => t.Seat)
                .Where(t => t.PassengerId == passengerId)
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

        public async Task<IEnumerable<Ticket>> GetByFlightAsync(int flightId)
            => await _dbSet
                .Include(t => t.Passenger)
                .Include(t => t.Seat)
                .Where(t => t.FlightId == flightId)
                .ToListAsync();

        public async Task<IEnumerable<Ticket>> GetPendingCheckInAsync(int flightId)
            => await _dbSet
                .Include(t => t.Passenger)
                .Include(t => t.Seat)
                .Where(t => t.FlightId == flightId && t.Status == "Paid")
                .ToListAsync();

        public IQueryable<Ticket> GetAllQueryable()
            => _dbSet
                .Include(t => t.Passenger)
                .Include(t => t.Flight)
                .Include(t => t.Seat)
                .AsQueryable();
    }
}
