using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class FlightRepository : GenericRepository<Flight>, IFlightRepository
    {
        public FlightRepository(DataContext context) : base(context) { }

        public async Task<Flight?> GetWithDetailsAsync(int id)
            => await _dbSet
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Aircraft)
                .Include(f => f.Gate)
                .Include(f => f.Seats)
                .FirstOrDefaultAsync(f => f.Id == id);

        public async Task<IEnumerable<Flight>> GetByAirlineAsync(int airlineId)
            => await _dbSet
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Where(f => f.AirlineId == airlineId)
                .ToListAsync();

        public async Task<IEnumerable<Flight>> SearchAsync(int originId, int destinationId, DateTime date)
            => await _dbSet
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Aircraft)
                .Where(f =>
                    f.OriginAirportId == originId &&
                    f.DestinationAirportId == destinationId &&
                    f.DepartureTime.Date == date.Date &&
                    f.Status != "Cancelled")
                .ToListAsync();

        public async Task<IEnumerable<Flight>> GetDepartureBoardAsync()
            => await _dbSet
                .Include(f => f.Airline)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Gate)
                .Where(f => f.DepartureTime.Date == DateTime.Today)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();

        public async Task<IEnumerable<Flight>> GetArrivalBoardAsync()
            => await _dbSet
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.Gate)
                .Where(f => f.ArrivalTime.Date == DateTime.Today)
                .OrderBy(f => f.ArrivalTime)
                .ToListAsync();

        public IQueryable<Flight> GetAllQueryable()
            => _dbSet
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Gate)
                .AsQueryable();
    }
}
