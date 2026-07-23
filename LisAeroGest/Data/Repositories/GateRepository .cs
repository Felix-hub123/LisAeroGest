using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    /// <summary>
    /// Interface do repositório de gates de embarque.
    /// Define as operações específicas além do CRUD genérico.
    /// </summary>
    public interface IGateRepository : IGenericRepository<Gate>
    {
        /// <summary>
        /// Obtém todos os gates com estado disponível.
        /// </summary>
        Task<IEnumerable<Gate>> GetAvailableGatesAsync();

        /// <summary>
        /// Obtém todos os gates de um terminal específico.
        /// </summary>
        /// <param name="terminal">Nome do terminal (ex: "Terminal 1").</param>
        Task<IEnumerable<Gate>> GetByTerminalAsync(string terminal);

        /// <summary>
        /// Obtém um gate pelo seu número identificador (ex: "A01", "B03").
        /// </summary>
        /// <param name="gateNumber">Número do gate a pesquisar.</param>
        Task<Gate?> GetByGateNumberAsync(string gateNumber);

        /// <summary>
        /// Verifica se o gate está associado a algum voo.
        /// Usado para impedir a eliminação de gates em uso.
        /// </summary>
        /// <param name="gateId">Identificador do gate a verificar.</param>
        Task<bool> IsUsedInFlightsAsync(int gateId);

        /// <summary>
        /// Devolve todos os gates como IQueryable para queries personalizadas.
        /// </summary>
        IQueryable<Gate> GetAllQueryable();

        Task<bool> IsGateOccupiedAsync(int gateId, DateTime departureTime, DateTime arrivalTime, int? currentFlightId = null);


    }
}
