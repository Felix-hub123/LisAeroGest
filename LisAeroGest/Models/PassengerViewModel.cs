using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{

    /// <summary>

    /// ViewModel para criação e edição de passageiros.

    /// </summary>

    public class PassengerViewModel

    {

        /// <summary>

        /// Identificador do passageiro.

        /// </summary>

        public int Id { get; set; }


        /// <summary>

        /// Primeiro nome.

        /// </summary>

        [Required(ErrorMessage = "O nome é obrigatório.")]

        [MaxLength(100)]

        [Display(Name = "Nome")]

        public string? FirstName { get; set; }


        /// <summary>

        /// Apelido.

        /// </summary>

        [Required(ErrorMessage = "O apelido é obrigatório.")]

        [MaxLength(100)]

        [Display(Name = "Apelido")]

        public string? LastName { get; set; }


        /// <summary>

        /// Tipo de documento de identificação.

        /// </summary>

        [Required(ErrorMessage = "O tipo de documento é obrigatório.")]

        [Display(Name = "Tipo de Documento")]

        public string? DocumentType { get; set; }


        /// <summary>

        /// Número do documento de identificação.

        /// </summary>

        [Required(ErrorMessage = "O número do documento é obrigatório.")]

        [Display(Name = "Número do Documento")]

        public string? DocumentNumber { get; set; }


        /// <summary>

        /// Data de nascimento.

        /// </summary>

        [DataType(DataType.Date)]

        [Display(Name = "Data de Nascimento")]

        public DateTime? BirthDate { get; set; }


        /// <summary>

        /// Identificador do utilizador associado.

        /// </summary>

        public string? UserId { get; set; }


        /// <summary>

        /// Email do utilizador (só leitura, vem do User).

        /// </summary>

        public string? UserEmail { get; set; }


        /// <summary>

        /// URL da foto de perfil atual.

        /// </summary>

        public string? ImageFullPath { get; set; }


        /// <summary>

        /// Ficheiro de imagem de perfil — opcional.

        /// </summary>

        public IFormFile? ImageFile { get; set; }


        /// <summary>

        /// Dropdown de tipos de documento.

        /// </summary>

        public IEnumerable<SelectListItem>? DocumentTypes { get; set; }
    }


}
