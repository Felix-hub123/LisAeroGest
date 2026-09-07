using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data.Repositories
{
    /// <summary>

    /// Implementação do repositório de lugares.

    /// </summary>

    public class SeatRepository : GenericRepository<Seat>, ISeatRepository

    {

        /// <summary>

        /// Inicializa o repositório com o contexto da base de dados.

        /// </summary>

        public SeatRepository(DataContext context) : base(context) { }


        /// <summary>

        /// Obtém todos os lugares de um voo específico, ordenados por código.

        /// </summary>

        public async Task<IEnumerable<Seat>> GetSeatsByFlightAsync(int flightId)
        {
            return await _context.Seats
                .Where(s => s.FlightId == flightId)
                .OrderBy(s => s.Code)
                .ToListAsync();
        }


        /// <summary>

        /// Cria os lugares para um voo a partir do template guardado na aeronave.

        /// </summary>

        public async Task GenerateSeatsForFlightAsync(int flightId, int aircraftId)

        {

            // 1) ir buscar os lugares-template que pertencem à aeronave

            var templateSeats = await _dbSet

                .Where(s => s.AircraftId == aircraftId && s.FlightId == null)

                .ToListAsync();


            // 2) clonar cada lugar para o novo voo

            foreach (var template in templateSeats)

            {

                var newSeat = new Seat

                {

                    Code = template.Code,

                    SeatClass = template.SeatClass,

                    BasePrice = template.BasePrice,

                    IsAvailable = true,

                    AircraftId = aircraftId,

                    FlightId = flightId

                };

                await _dbSet.AddAsync(newSeat);

            }


            // 3) persistir tudo de uma vez

            await _context.SaveChangesAsync();

        }


        public async Task GenerateTemplateSeatsForAircraftAsync(int aircraftId, int rows = 20)
        {
            var exists = await _dbSet.AnyAsync(s => s.AircraftId == aircraftId && s.FlightId == null);
            if (exists)
                return;

            var letters = new[] { "A", "B", "C", "D", "E", "F" };

            foreach (var row in Enumerable.Range(1, rows))
            {
                foreach (var letter in letters)
                {
                    bool business = row <= 3;
                    await _dbSet.AddAsync(new Seat
                    {
                        Code = $"{row}{letter}",
                        SeatClass = business ? "Business" : "Economy",
                        BasePrice = business ? 80m : 40m,
                        IsAvailable = true,
                        AircraftId = aircraftId,
                        FlightId = null
                    });
                }
            }

            await _context.SaveChangesAsync();
        }


        /// <summary>

        /// Devolve todos os lugares como IQueryable.

        /// </summary>

        public IQueryable<Seat> GetAllQueryable()

            => _dbSet.AsQueryable();




        /// <summary>
        /// Obtém os lugares disponíveis de um voo específico.
        /// </summary>
        public async Task<IEnumerable<Seat>> GetAvailableByFlightAsync(int flightId)
            => await _dbSet
                .Where(s => s.FlightId == flightId && s.IsAvailable)
                .OrderBy(s => s.Code)
                .ToListAsync();

        public async Task<List<int>> GetReservedSeatIdsByFlightAsync(int flightId)
        {
            return await _context.Seats
                .Where(s => s.FlightId == flightId && !s.IsAvailable)
                .Select(s => s.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Seat>> GetSeatsByFlightIdAsync(int flightId)
        {
            return await _context.Seats
                .Where(s => s.FlightId == flightId && !s.WasDeleted)
                .OrderBy(s => s.Code)
                .ToListAsync();
        }



    }
}

