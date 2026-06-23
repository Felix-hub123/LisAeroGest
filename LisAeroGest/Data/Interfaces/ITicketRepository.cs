using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<Ticket?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Ticket>> GetByPassengerAsync(int passengerId);
        Task<IEnumerable<Ticket>> GetByFlightAsync(int flightId);
        Task<IEnumerable<Ticket>> GetPendingCheckInAsync(int flightId);
        IQueryable<Ticket> GetAllQueryable();
    }
}
