using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IAirportRepository : IGenericRepository<Airport>
    {
        Task<Airport?> GetByIATACodeAsync(string iataCode);
        Task<bool> IsUsedInFlightsAsync(int airportId);
        IQueryable<Airport> GetAllQueryable();

        Task<IEnumerable<Airport>> SearchAsync(string term, int take = 8);

    }
}
