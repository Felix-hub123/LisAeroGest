using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class AirlineRepository : GenericRepository<Airline>, IAirlineRepository
    {
        public AirlineRepository(DataContext context) : base(context) { }

        public async Task<Airline?> GetByIATACodeAsync(string iataCode)
            => await _dbSet.FirstOrDefaultAsync(a => a.IATACode == iataCode);

        public async Task<Airline?> GetWithFlightsAsync(int id)
            => await _dbSet.Include(a => a.Flights).FirstOrDefaultAsync(a => a.Id == id);

        public IQueryable<Airline> GetAllQueryable()
            => _dbSet.AsQueryable();
    }
}
