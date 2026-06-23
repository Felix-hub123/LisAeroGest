using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IFlightRepository : IGenericRepository<Flight>
    {
        Task<Flight?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Flight>> GetByAirlineAsync(int airlineId);
        Task<IEnumerable<Flight>> SearchAsync(int originId, int destinationId, DateTime date);
        Task<IEnumerable<Flight>> GetDepartureBoardAsync();
        Task<IEnumerable<Flight>> GetArrivalBoardAsync();
        IQueryable<Flight> GetAllQueryable();
    }
}
