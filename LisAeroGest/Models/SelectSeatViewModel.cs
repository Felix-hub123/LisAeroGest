using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{
    public class SelectSeatViewModel
    {
        // Entidade Principal do Voo
        public Flight Flight { get; set; } = null!;

        // Lista de Lugares Associados
        public List<Seat> Seats { get; set; } = new();

        // Lista de IDs de Lugares já Reservados/Ocupados
        public List<int> ReservedSeatIds { get; set; } = new();

        // Preços de Serviços Adicionais
        public decimal ExtraLuggagePrice { get; set; }

        public decimal MealIncludedPrice { get; set; }

        // --- PROPRIEDADES CALCULADAS E MÉTODOS AUXILIARES PARA A VIEW ---

        /// <summary>
        /// Obtém apenas os lugares ativos, limpos de registos apagados e ordenados por código.
        /// </summary>
        public IEnumerable<Seat> ActiveSeats =>
            Seats.Where(s => !s.WasDeleted && s.FlightId == Flight?.Id)
                 .OrderBy(s => s.Code);

        /// <summary>
        /// Verifica se um determinado lugar está ocupado ou indisponível.
        /// </summary>
        public bool IsSeatOccupied(Seat seat)
        {
            if (seat == null) return true;
            return !seat.IsAvailable || ReservedSeatIds.Contains(seat.Id);
        }

        /// <summary>
        /// Verifica se o lugar pertence à classe Business/Executiva.
        /// </summary>
        public bool IsBusinessClass(Seat seat)
        {
            if (string.IsNullOrWhiteSpace(seat?.SeatClass)) return false;

            return string.Equals(seat.SeatClass, "Business", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(seat.SeatClass, "Executiva", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Retorna a classe CSS apropriada para a apresentação do lugar no mapa da cabine.
        /// </summary>
        public string GetSeatCssClass(Seat seat)
        {
            if (IsSeatOccupied(seat))
                return "seat-occupied";

            if (IsBusinessClass(seat))
                return "seat-business";

            return "seat-available";
        }
    }
}
