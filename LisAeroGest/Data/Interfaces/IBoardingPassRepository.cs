using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IBoardingPassRepository
    {
        Task<BoardingPass?> GetByIdAsync(int id);
        Task<BoardingPass?> GetByTicketIdAsync(int ticketId);
        Task<int> GetNextSequenceNumberAsync(int flightId);
        Task AddAsync(BoardingPass boardingPass);

        Task UpdateAsync(BoardingPass boardingPass);
        Task SaveAsync();
    }
}