using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Mobile.Models
{
    /// <summary>
    /// DTO para o pedido de registo na API.
    /// </summary>
    public class RegisterRequestDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string DocumentNumber { get; set; } = string.Empty;

        public string? DocumentType { get; set; } = "CC";
    }
}