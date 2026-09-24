namespace LisAeroGest.Mobile.Models
{
    public class SeatDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public decimal BasePrice { get; set; }

        public string DisplayPrice =>
            BasePrice > 0
                ? BasePrice.ToString("C", new System.Globalization.CultureInfo("pt-PT"))
                : string.Empty;
    }
}