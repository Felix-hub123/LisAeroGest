using LisAeroGest.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    public class ForumTopicViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é de preenchimento obrigatório.")]
        [StringLength(100, ErrorMessage = "O {0} não pode ter mais de {1} caracteres.")]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "O conteúdo é de preenchimento obrigatório.")]
        [Display(Name = "Conteúdo")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Fechado")]
        public bool IsClosed { get; set; }

        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; }

        public string? CreatedByUserId { get; set; }

        [Display(Name = "Autor")]
        public string AuthorName { get; set; } = string.Empty;

        public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
    }
}
