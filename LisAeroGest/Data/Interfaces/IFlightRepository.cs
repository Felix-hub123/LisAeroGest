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
        Task<IEnumerable<Flight>> GetAvailableFlightsAsync(string? origin, string? destination, DateTime? date);
        IQueryable<Flight> GetAllQueryable();

        Task<IEnumerable<Flight>> GetAllWithDetailsAsync();


        /// <summary>
        /// Busca um voo com todos os relacionamentos (Airport, Airline, Gate, Aircraft)
        /// </summary>
        Task<Flight?> GetFlightWithDetailsAsync(int id);

        /// <summary>
        /// Pesquisa voos por termo (número, destino, companhia)
        /// </summary>
        Task<IEnumerable<Flight>> SearchFlightsAsync(string term);

        /// <summary>
        /// Busca voos disponíveis (para a API pública)
        /// </summary>
        Task<IEnumerable<Flight>> GetAvailableFlightsAsync();

    }
}
