using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IFlightCommunicationRepository
        : IGenericRepository<FlightCommunication>
    {
        Task<IEnumerable<FlightCommunication>> GetAllWithDetailsAsync();

        Task<FlightCommunication?> GetByIdWithDetailsAsync(int id);

        IQueryable<FlightCommunication> GetAllWithDetailsQueryable();
    }
}