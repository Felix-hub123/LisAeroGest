using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<Ticket?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Ticket>> GetByPassengerAsync(int passengerId);
        Task<IEnumerable<Ticket>> GetByFlightAsync(int flightId);

        /// <summary>
        /// Obtém bilhetes pagos que ainda não efetuaram check-in para um voo.
        /// </summary>
        Task<IEnumerable<Ticket>> GetPendingCheckInAsync(int flightId);

        IQueryable<Ticket> GetAllQueryable();

        Task<IEnumerable<Ticket>> SearchForCheckInAsync(string searchCriteria);

        /// <summary>
        /// Obtém reservas temporárias pendentes de pagamento para um determinado passageiro.
        /// </summary>
        Task<IEnumerable<Ticket>> GetReservedByPassengerAsync(int passengerId);

        /// <summary>
        /// Obtém TODAS as reservas em carrinho de um passageiro (válidas e já expiradas),
        /// para a página do Carrinho poder mostrar as válidas e limpar as expiradas.
        /// </summary>
        Task<IEnumerable<Ticket>> GetCartByPassengerAsync(int passengerId);

        /// <summary>
        /// Obtém os bilhetes ativos (pagos ou já com check-in) de um passageiro.
        /// </summary>
        Task<IEnumerable<Ticket>> GetActiveByPassengerAsync(int passengerId);


        /// <summary>
        /// Obtém um bilhete com todos os relacionamentos necessários carregados (Flight, Seat, Passenger, User, Airports).
        /// </summary>
        Task<Ticket?> GetTicketWithDetailsAsync(int id);

        Task<IEnumerable<Ticket>> GetByFlightIdAsync(int flightId);



    }
}
