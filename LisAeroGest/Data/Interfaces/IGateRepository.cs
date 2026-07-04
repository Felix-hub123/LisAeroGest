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
    }
}
