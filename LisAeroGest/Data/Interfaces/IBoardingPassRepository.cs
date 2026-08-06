using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IBoardingPassRepository
    {
        Task<BoardingPass?> GetByIdAsync(int id);
        Task<BoardingPass?> GetByTicketIdAsync(int ticketId);
        Task<int> GetNextSequenceNumberAsync(int flightId);
        Task AddAsync(BoardingPass boardingPass);

        /// <summary>
        /// Obtém o cartão de embarque com todos os relacionamentos necessários (Ticket, Flight, Passenger, Seat, Airports).
        /// </summary>
        Task<BoardingPass?> GetBoardingPassWithDetailsAsync(int id);

        Task UpdateAsync(BoardingPass boardingPass);
        Task SaveAsync();
    }
}