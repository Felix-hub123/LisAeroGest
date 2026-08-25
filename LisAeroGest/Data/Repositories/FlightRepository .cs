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
           .Include(f => f.Seats)
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

           
            if (!string.IsNullOrWhiteSpace(origin))
            {
                var originTerm = origin.Trim().ToLower();
                query = query.Where(f =>
                    f.OriginAirport!.IATACode!.ToLower().Contains(originTerm) ||
                    (f.OriginAirport.City != null && f.OriginAirport.City.ToLower().Contains(originTerm)) ||
                    (f.OriginAirport.Country != null && f.OriginAirport.Country.ToLower().Contains(originTerm)));
            }

            if (!string.IsNullOrWhiteSpace(destination))
            {
                var destinationTerm = destination.Trim().ToLower();
                query = query.Where(f =>
                    f.DestinationAirport!.IATACode!.ToLower().Contains(destinationTerm) ||
                    (f.DestinationAirport.City != null && f.DestinationAirport.City.ToLower().Contains(destinationTerm)) ||
                    (f.DestinationAirport.Country != null && f.DestinationAirport.Country.ToLower().Contains(destinationTerm)));
            }

            if (date.HasValue)
                query = query.Where(f => f.DepartureTime.Date == date.Value.Date);

            return await query.OrderBy(f => f.DepartureTime).ToListAsync();
        }

        public async Task<IEnumerable<Flight>> GetAllWithDetailsAsync()
        {
            return await _context.Flights
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Aircraft)
                .Include(f => f.Seats)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }


        public async Task<Flight?> GetFlightWithDetailsAsync(int id)
        {
            return await _context.Flights
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Airline)
                .Include(f => f.Gate)
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string term)
        {
            var lowerTerm = term.ToLower();
            return await _context.Flights
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Airline)
                .Include(f => f.Gate)
                .Include(f => f.Aircraft)
                .Where(f =>
                    f.FlightNumber.ToLower().Contains(lowerTerm) ||
                    (f.DestinationAirport != null && f.DestinationAirport.City.ToLower().Contains(lowerTerm)) ||
                    (f.Airline != null && f.Airline.Name.ToLower().Contains(lowerTerm)) ||
                    (f.DestinationAirport != null && f.DestinationAirport.IATACode.ToLower().Contains(lowerTerm))
                )
                .Take(20)
                .ToListAsync();
        }

        public async Task<IEnumerable<Flight>> GetAvailableFlightsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await _context.Flights
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Include(f => f.Airline)
                .Include(f => f.Gate)
                .Include(f => f.Aircraft)
                .Where(f => f.DepartureTime.Date == today || f.DepartureTime.Date == tomorrow)
                .OrderBy(f => f.DepartureTime)
                .Take(50)
                .ToListAsync();
        }


    }
}
