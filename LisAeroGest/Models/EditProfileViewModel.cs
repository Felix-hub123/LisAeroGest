using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    public class EditProfileViewModel
    {
        [Display(Name = "Nome")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Apelido")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        [Display(Name = "Foto de perfil")]
        public IFormFile? ImageFile { get; set; }

        public Guid ImageId { get; set; }

        public string? ImageUrl { get; set; }
    }
}
