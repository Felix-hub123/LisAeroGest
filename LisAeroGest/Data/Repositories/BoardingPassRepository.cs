using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    public class BoardingPassRepository : IBoardingPassRepository
    {
        private readonly DataContext _context;

        public BoardingPassRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<BoardingPass?> GetByIdAsync(int id)
        {
            return await _context.BoardingPasses
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t!.Flight)
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t!.Passenger)
                        .ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(bp => bp.Id == id);
        }

        public async Task<BoardingPass?> GetByTicketIdAsync(int ticketId)
        {
            return await _context.BoardingPasses
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t!.Flight)
                        .ThenInclude(f => f!.OriginAirport)
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t!.Flight)
                        .ThenInclude(f => f!.DestinationAirport)
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t!.Passenger)
                        .ThenInclude(p => p!.User)
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t!.Seat)
                .FirstOrDefaultAsync(bp => bp.TicketId == ticketId);
        }

        public async Task<int> GetNextSequenceNumberAsync(int flightId)
        {
            var count = await _context.BoardingPasses
                .CountAsync(bp => bp.Ticket != null && bp.Ticket.FlightId == flightId);

            return count + 1;
        }

        public async Task AddAsync(BoardingPass boardingPass)
        {
            await _context.BoardingPasses.AddAsync(boardingPass);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BoardingPass boardingPass)
        {
            _context.BoardingPasses.Update(boardingPass);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtém um cartão de embarque incluindo os relacionamentos necessários para o PDF.
        /// </summary>
        public async Task<BoardingPass?> GetBoardingPassWithDetailsAsync(int id)
        {
            return await _context.BoardingPasses
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t.Seat)
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t.Passenger)
                        .ThenInclude(p => p.User)
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t.Flight)
                        .ThenInclude(f => f.OriginAirport)
                .Include(bp => bp.Ticket)
                    .ThenInclude(t => t.Flight)
                        .ThenInclude(f => f.DestinationAirport)
                .FirstOrDefaultAsync(bp => bp.Id == id);
        }
    }
}