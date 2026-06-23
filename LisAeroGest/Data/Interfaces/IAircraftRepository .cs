using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IAircraftRepository : IGenericRepository<Aircraft>
    {
        Task<Aircraft?> GetWithSeatsAsync(int id);
        Task<IEnumerable<Aircraft>> GetAvailableAsync();
        IQueryable<Aircraft> GetAllQueryable();

    }
}
