using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class AirportRepository : GenericRepository<Airport>, IAirportRepository
    {
        public AirportRepository(DataContext context) : base(context) { }

        public async Task<Airport?> GetByIATACodeAsync(string iataCode)
            => await _dbSet.FirstOrDefaultAsync(a => a.IATACode == iataCode);

        public async Task<bool> IsUsedInFlightsAsync(int airportId)
            => await _context.Flights.AnyAsync(f =>
                f.OriginAirportId == airportId ||
                f.DestinationAirportId == airportId);

        public IQueryable<Airport> GetAllQueryable()
            => _dbSet.AsQueryable();
    }
}
