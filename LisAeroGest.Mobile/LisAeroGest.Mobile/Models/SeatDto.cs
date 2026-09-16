namespace LisAeroGest.Mobile.Models
{
    public class SeatDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool IsAvailable { get; set; }

        public string DisplayPrice => BasePrice.ToString("C2");
    }
}
