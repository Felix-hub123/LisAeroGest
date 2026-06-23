using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IGateRepository : IGenericRepository<Gate>
    {
        Task<IEnumerable<Gate>> GetAvailableGatesAsync();
        Task<IEnumerable<Gate>> GetByTerminalAsync(string terminal);
        IQueryable<Gate> GetAllQueryable();
    }
}
