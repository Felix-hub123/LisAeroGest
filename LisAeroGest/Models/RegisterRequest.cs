using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// DTO para o pedido de registo da API mobile.
    /// </summary>
    public class RegisterRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O apelido é obrigatório.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A password é obrigatória.")]
        [MinLength(6, ErrorMessage = "A password deve ter pelo menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "O documento é obrigatório.")]
        [MaxLength(20)]
        public string DocumentNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? DocumentType { get; set; } = "CC";

        public DateTime? BirthDate { get; set; }
    }
}