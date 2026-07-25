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
         .ThenInclude(a => a!.Seats) 
         .Include(f => f.Gate)
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
        {
            var todayUtc = DateTime.UtcNow.Date;
            var tomorrowUtc = todayUtc.AddDays(1);

            return await _dbSet
                .Include(f => f.Airline)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Gate)
                .Where(f => f.DepartureTime >= todayUtc && f.DepartureTime < tomorrowUtc)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Flight>> GetArrivalBoardAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var tomorrowUtc = todayUtc.AddDays(1);

            return await _dbSet
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport) 
                .Include(f => f.Gate)
                .Where(f => f.ArrivalTime >= todayUtc && f.ArrivalTime < tomorrowUtc)
                .OrderBy(f => f.ArrivalTime)
                .ToListAsync();
        }

        public IQueryable<Flight> GetAllQueryable()
            => _dbSet
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Gate)
                .AsQueryable();

        public async Task<IEnumerable<Flight>> GetAvailableFlightsAsync(string? origin, string? destination, DateTime? date)
        {
            var query = _dbSet
                .Include(f => f.Airline)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Aircraft)
                .Where(f => f.Status != "Cancelled" && f.DepartureTime > DateTime.UtcNow);

            if (!string.IsNullOrEmpty(origin))
                query = query.Where(f => f.OriginAirport!.IATACode == origin);

            if (!string.IsNullOrEmpty(destination))
                query = query.Where(f => f.DestinationAirport!.IATACode == destination);

            if (date.HasValue)
                query = query.Where(f => f.DepartureTime.Date == date.Value.Date);

            return await query.OrderBy(f => f.DepartureTime).ToListAsync();
        }


    }
}
