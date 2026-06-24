using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class GateRepository : GenericRepository<Gate>, IGateRepository
    {
        public GateRepository(DataContext context) : base(context) { }

        public async Task<IEnumerable<Gate>> GetAvailableGatesAsync()
            => await _dbSet.Where(g => g.Status == "Available").ToListAsync();

        public async Task<IEnumerable<Gate>> GetByTerminalAsync(string terminal)
            => await _dbSet.Where(g => g.Terminal == terminal).ToListAsync();

        public IQueryable<Gate> GetAllQueryable()
            => _dbSet.AsQueryable();
    }
}
