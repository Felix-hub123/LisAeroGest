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

        public async Task<IEnumerable<Ticket>> GetCartByPassengerAsync(int passengerId)
            => await _dbSet
                .Include(t => t.Flight).ThenInclude(f => f!.Airline)
                .Include(t => t.Flight).ThenInclude(f => f!.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.DestinationAirport)
                .Include(t => t.Seat)
                .Where(t => t.PassengerId == passengerId && t.Status == "Reserved")
                .ToListAsync();

        public async Task<IEnumerable<Ticket>> GetActiveByPassengerAsync(int passengerId)
            => await _dbSet
                .Include(t => t.Flight).ThenInclude(f => f!.Airline)
                .Include(t => t.Flight).ThenInclude(f => f!.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.DestinationAirport)
                .Include(t => t.Seat)
                .Where(t => t.PassengerId == passengerId &&
                            (t.Status == "Paid" || t.Status == "CheckedIn"))
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

        /// <summary>
        /// Pesquisa bilhetes para check-in presencial por vários campos:
        /// ID, documento, email, telefone, nome ou número do voo.
        /// </summary>
        public async Task<List<Ticket>> SearchForCheckInAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Ticket>();

            var term = searchTerm.Trim();
            int.TryParse(term, out int numericTerm);

            var query = _context.Tickets
                .Include(t => t.Passenger)
                .Include(t => t.Flight).ThenInclude(f => f!.OriginAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.DestinationAirport)
                .Include(t => t.Flight).ThenInclude(f => f!.Gate)
                .Include(t => t.Flight).ThenInclude(f => f!.Airline)
                .Include(t => t.Seat)
                .Where(t => !t.WasDeleted);

            query = query.Where(t =>
                (numericTerm > 0 && t.Id == numericTerm)
                || (t.Passenger != null && t.Passenger.DocumentNumber == term)
                || (t.Passenger != null && t.Passenger.Email == term)
                || (t.Passenger != null && t.Passenger.PhoneNumber == term)
                || (t.Flight != null && t.Flight.FlightNumber == term)
                || (t.Passenger != null && (
                    EF.Functions.Like(t.Passenger.FirstName, $"%{term}%") ||
                    EF.Functions.Like(t.Passenger.LastName, $"%{term}%")
                ))
            );

            return await query
                .OrderByDescending(t => t.Flight!.DepartureTime)
                .Take(20)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém um bilhete por ID incluindo todas as navegações necessárias para a emissão do PDF.
        /// </summary>
        public async Task<Ticket?> GetTicketWithDetailsAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Passenger)
                    .ThenInclude(p => p.User)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.OriginAirport)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.DestinationAirport)
                .Include(t => t.Flight)
                    .ThenInclude(f => f.Gate)
                .FirstOrDefaultAsync(t => t.Id == id);
        }


        public async Task<IEnumerable<Ticket>> GetByFlightIdAsync(int flightId)
        {
            return await _context.Tickets
                .Include(t => t.Passenger)
                    .ThenInclude(p => p.User)
                .Where(t => t.FlightId == flightId)
                .ToListAsync();
        }



    }
}
