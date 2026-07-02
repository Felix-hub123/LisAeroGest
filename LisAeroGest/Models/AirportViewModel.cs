using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    public class AirportViewModel
    {
        /// <summary>
        /// Identificador único do aeroporto — zero para criação, maior que zero para edição.
        /// </summary>
        /// no método POST do <see cref="Controllers.AirportController"/>.
        /// </returns>
        public int Id { get; set; }

        /// <summary>
        /// Nome oficial do aeroporto.
        /// </summary>
        [Required(ErrorMessage = "O nome do aeroporto é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        [Display(Name = "Nome")]
        public string? Name { get; set; }

        /// <summary>
        /// Cidade onde o aeroporto está localizado.
        /// </summary>
        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A cidade não pode exceder 100 caracteres.")]
        [Display(Name = "Cidade")]
        public string? City { get; set; }

        /// <summary>
        /// País onde o aeroporto está localizado.
        /// </summary>
        [Required(ErrorMessage = "O país é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O país não pode exceder 100 caracteres.")]
        [Display(Name = "País")]
        public string? Country { get; set; }

        /// <summary>
        /// Código IATA de três letras que identifica o aeroporto.
        /// </summary>
        [Required(ErrorMessage = "O código IATA é obrigatório.")]
        [MinLength(3, ErrorMessage = "O código IATA deve ter exatamente 3 caracteres.")]
        [MaxLength(3, ErrorMessage = "O código IATA deve ter exatamente 3 caracteres.")]
        [Display(Name = "Código IATA")]
        public string? IATACode { get; set; }

        /// <summary>
        /// Taxa aeroportuária padrão aplicada aos voos neste aeroporto.
        /// </summary>
        [Range(0, 99999.99, ErrorMessage = "A taxa deve ser um valor positivo.")]
        [Display(Name = "Taxa Aeroportuária (€)")]
        public decimal DefaultFee { get; set; }

        /// <summary>
        /// Ficheiro de imagem do aeroporto enviado pelo utilizador — opcional.
        /// </summary>
        [Display(Name = "Imagem")]
        public IFormFile? ImageFile { get; set; }

        /// <summary>
        /// GUID atual da imagem — usado para manter a imagem existente na edição.
        /// </summary>
        public Guid ImageId { get; set; }

        /// <summary>
        /// URL completo da imagem atual — usado para pré-visualização no formulário.
        /// </summary>
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/airports/noimage.png"
            : $"https://lisaerogest.blob.core.windows.net/airports/{ImageId}";
    }

}

