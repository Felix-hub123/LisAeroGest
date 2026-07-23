using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Interfaces
{
    /// <summary>
    /// Repositório de gates de embarque.
    /// Herda as operações CRUD do GenericRepository e adiciona queries específicas.
    /// </summary>
    public class GateRepository : GenericRepository<Gate>, IGateRepository
    {
        /// <summary>
        /// Inicializa o repositório com o contexto da base de dados.
        /// </summary>
        public GateRepository(DataContext context) : base(context) { }

        /// <summary>
        /// Obtém todos os gates com estado disponível para atribuição a voos.
        /// </summary>
        public async Task<IEnumerable<Gate>> GetAvailableGatesAsync()
            => await _dbSet
                .Where(g => g.Status == "Available")
                .ToListAsync();

        /// <summary>
        /// Obtém todos os gates de um terminal específico.
        /// </summary>
        public async Task<IEnumerable<Gate>> GetByTerminalAsync(string terminal)
            => await _dbSet
                .Where(g => g.Terminal == terminal)
                .ToListAsync();

        /// <summary>
        /// Obtém um gate pelo seu número identificador.
        /// </summary>
        public async Task<Gate?> GetByGateNumberAsync(string gateNumber)
            => await _dbSet
                .FirstOrDefaultAsync(g => g.GateNumber == gateNumber);

        /// <summary>
        /// Verifica se existe algum voo associado a este gate.
        /// </summary>
        public async Task<bool> IsUsedInFlightsAsync(int gateId)
            => await _context.Flights
                .AnyAsync(f => f.GateId == gateId);

        /// <summary>
        /// Devolve todos os gates como IQueryable para queries personalizadas.
        /// </summary>
        public IQueryable<Gate> GetAllQueryable()
            => _dbSet.AsQueryable();


        // Implementação da verificação de ocupação do Gate:
        public async Task<bool> IsGateOccupiedAsync(int gateId, DateTime departureTime, DateTime arrivalTime, int? currentFlightId = null)
        {
            // Margem de segurança operacional de 30 minutos entre voos
            var startMargin = departureTime.AddMinutes(-30);
            var endMargin = arrivalTime.AddMinutes(30);

            var query = _context.Flights
                .Where(f => f.GateId == gateId && f.Status != "Cancelled");

            // Se for uma edição de voo, ignora o próprio voo
            if (currentFlightId.HasValue)
            {
                query = query.Where(f => f.Id != currentFlightId.Value);
            }

            // Verifica se existe algum voo agendado nesse portão cuja janela temporal se sobreponha
            return await query.AnyAsync(f =>
                (f.DepartureTime >= startMargin && f.DepartureTime <= endMargin) ||
                (f.ArrivalTime >= startMargin && f.ArrivalTime <= endMargin) ||
                (f.DepartureTime <= startMargin && f.ArrivalTime >= endMargin));
        }
    }
}
