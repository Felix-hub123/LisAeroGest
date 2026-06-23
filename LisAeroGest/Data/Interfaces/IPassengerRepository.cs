using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IPassengerRepository : IGenericRepository<Passenger>
    {
        Task<Passenger?> GetByUserIdAsync(string userId);
        Task<Passenger?> GetWithTicketsAsync(int id);
        IQueryable<Passenger> GetAllQueryable();
    }
}
