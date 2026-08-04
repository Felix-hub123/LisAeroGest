using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(DataContext context) : base(context) { }

        public async Task<Ticket?> GetWithDetailsAsync(int id)
              => await _dbSet
                  .Include(t => t.Passenger).ThenInclude(p => p!.User)
                  .Include(t => t.Flight).ThenInclude(f => f!.Airline)
                  .Include(t => t.Flight).ThenInclude(f => f!.OriginAirport)
                  .Include(t => t.Flight).ThenInclude(f => f!.DestinationAirport)
                  .Include(t => t.Flight).ThenInclude(f => f!.Gate)
                  .Include(t => t.Seat)
                  .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Ticket>> GetByPassengerAsync(int passengerId)
            => await _dbSet
                .Include(t => t.Flight).ThenInclude(f => f!.Airline)
                .Include(t => t.Flight).ThenInclude(f => f!.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.DestinationAirport)
                .Include(t => t.Seat)
                .Where(t => t.PassengerId == passengerId)
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

        public async Task<IEnumerable<Ticket>> GetByFlightAsync(int flightId)
            => await _dbSet
                .Include(t => t.Passenger)
                .Include(t => t.Seat)
                .Where(t => t.FlightId == flightId)
                .ToListAsync();

        public async Task<IEnumerable<Ticket>> GetPendingCheckInAsync(int flightId)
            => await _dbSet
                .Include(t => t.Passenger)
                .Include(t => t.Seat)
                .Where(t => t.FlightId == flightId && t.Status == "Paid")
                .ToListAsync();

        public IQueryable<Ticket> GetAllQueryable()
            => _dbSet
                .Include(t => t.Passenger)
                .Include(t => t.Flight)
                .Include(t => t.Seat)
                .AsQueryable();

        /// <summary>
        /// Obtém as reservas temporárias válidas (não expiradas) de um determinado passageiro.
        /// Substitui o antigo GetTempByUserAsync.
        /// </summary>
        public async Task<IEnumerable<Ticket>> GetReservedByPassengerAsync(int passengerId)
            => await _dbSet
                .Include(t => t.Flight).ThenInclude(f => f!.Airline)
                .Include(t => t.Flight).ThenInclude(f => f!.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.DestinationAirport)
                .Include(t => t.Seat)
                .Where(t => t.PassengerId == passengerId &&
                            t.Status == "Reserved" &&
                            t.ReservationExpiresAt > DateTime.UtcNow)
                .ToListAsync();

        public async Task<IEnumerable<Ticket>> SearchForCheckInAsync(string searchCriteria)
        {
            if (string.IsNullOrWhiteSpace(searchCriteria))
            {
                return Enumerable.Empty<Ticket>();
            }

            var term = searchCriteria.Trim().ToLower();

            return await _context.Tickets
                .Include(t => t.Flight)
                    .ThenInclude(f => f!.OriginAirport)
                .Include(t => t.Flight)
                    .ThenInclude(f => f!.DestinationAirport)
                .Include(t => t.Passenger)
                    .ThenInclude(p => p!.User)
                .Include(t => t.Seat)
                .Where(t =>
                    // Pesquisa por ID do bilhete (se for número)
                    t.Id.ToString() == term ||
                    // Pesquisa por Nome Completo do passageiro
                    (t.Passenger != null && t.Passenger.User != null && t.Passenger.User.FullName.ToLower().Contains(term)) ||
                    // Pesquisa por E-mail do passageiro
                    (t.Passenger != null && t.Passenger.User != null && t.Passenger.User.Email!.ToLower().Contains(term)) ||
                    // Pesquisa por Número do Voo
                    (t.Flight != null && t.Flight.FlightNumber!.ToLower().Contains(term))
                )
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();
        }
    }
}
