namespace LisAeroGest.Mobile.Models
{
    /// <summary>
    /// DTO para o pedido de registo na API.
    /// </summary>
    public class RegisterRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string? DocumentType { get; set; } = "CC";
    }
}