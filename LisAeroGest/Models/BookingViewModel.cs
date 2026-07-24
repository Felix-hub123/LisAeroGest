using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{
    public class BookingViewModel
    {
        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

        public int SelectedSeatId { get; set; }
        public string? SelectedSeatCode { get; set; }
        public decimal TotalPrice { get; set; }

        // Agrupamento dos lugares por fila para renderizar a grelha/mapa no View
        public List<List<Seat>> SeatGrid { get; set; } = new();
    }
}
