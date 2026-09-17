namespace LisAeroGest.Mobile.Helpers
{
    public static class PendingBooking
    {

        public static int TicketId { get; set; }
        public static int FlightId { get; set; }
        public static int SeatId { get; set; }
        public static string SeatCode { get; set; } = string.Empty;
        public static string Price { get; set; } = string.Empty;
    }
}