using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    /// <summary>

    /// Repositório responsável pela gestão dos lugares (seats) dos voos.

    /// </summary>

    public interface ISeatRepository : IGenericRepository<Seat>

    {

        /// <summary>

        /// Obtém todos os lugares de um voo específico.

        /// </summary>

        /// <param name="flightId">Identificador do voo.</param>

        Task<IEnumerable<Seat>> GetSeatsByFlightAsync(int flightId);


        /// <summary>

        /// Gera os lugares para um voo com base no template da aeronave escolhida.

        /// Cada lugar é copiado (código, classe, preço base) e marcado como disponível.

        /// </summary>

        /// <param name="flightId">Identificador do voo recém-criado.</param>

        /// <param name="aircraftId">Identificador da aeronave cujos lugares servem de template.</param>

        Task GenerateSeatsForFlightAsync(int flightId, int aircraftId);


        /// <summary>

        /// Devolve todos os lugares como IQueryable para queries personalizadas.

        /// </summary>

        IQueryable<Seat> GetAllQueryable();

        Task<List<int>> GetReservedSeatIdsByFlightAsync(int flightId);


        Task<IEnumerable<Seat>> GetAvailableByFlightAsync(int flightId);

        /// <summary>
        /// Obtém todos os lugares ativos associados a um determinado voo.
        /// </summary>
        Task<IEnumerable<Seat>> GetSeatsByFlightIdAsync(int flightId);


    }
}
