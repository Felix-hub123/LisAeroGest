using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IAirlineRepository : IGenericRepository<Airline>
    {
        Task<Airline?> GetByIATACodeAsync(string iataCode);
        Task<Airline?> GetWithFlightsAsync(int id);
        IQueryable<Airline> GetAllQueryable();
    }
}
