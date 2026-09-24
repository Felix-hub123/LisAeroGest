namespace LisAeroGest.Mobile.Models
{
    public class GateDto
    {
        public int Id { get; set; }

        public string GateNumber { get; set; } = string.Empty;

        public string Terminal { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(Terminal)
                ? GateNumber
                : $"{GateNumber} · {Terminal}";
    }
}