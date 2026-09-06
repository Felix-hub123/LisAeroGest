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


        /// <summary>
        /// Pesquisa aeroportos por cidade, nome ou código IATA.
        /// </summary>
        public async Task<IEnumerable<Airport>> SearchAsync(string term, int take = 8)
        {
            term = (term ?? string.Empty).Trim();

            var query = _dbSet.AsNoTracking().Where(a => !a.WasDeleted);

            if (!string.IsNullOrWhiteSpace(term))
            {
                var upper = term.ToUpper();
                query = query.Where(a =>
                    (a.IATACode != null && a.IATACode.ToUpper().Contains(upper)) ||
                    (a.City != null && a.City.ToUpper().Contains(upper)) ||
                    (a.Name != null && a.Name.ToUpper().Contains(upper)) ||
                    (a.Country != null && a.Country.ToUpper().Contains(upper)));
            }

            return await query
                .OrderBy(a => a.City)
                .Take(take)
                .ToListAsync();
        }
    }
}
