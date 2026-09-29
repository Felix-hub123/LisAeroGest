using LisAeroGest.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel para criação e edição de gates de embarque.
    /// </summary>
    public class GateViewModel
    {
        public int Id { get; set; }

        [Required(
            ErrorMessage = "O número do gate é obrigatório.")]
        [MaxLength(
            10,
            ErrorMessage = "O número do gate não pode exceder 10 caracteres.")]
        [Display(Name = "Número do Gate")]
        public string? GateNumber { get; set; }

        [Required(
            ErrorMessage = "O terminal é obrigatório.")]
        [MaxLength(
            50,
            ErrorMessage = "O terminal não pode exceder 50 caracteres.")]
        [Display(Name = "Terminal")]
        public string? Terminal { get; set; }

        [Required(
            ErrorMessage = "O estado é obrigatório.")]
        [Display(Name = "Estado")]
        public string Status { get; set; }
            = "Available";
    }


    // =============================================================
    // INDEX / PAGINAÇÃO
    // =============================================================

    public class GateIndexViewModel
    {
        public List<Gate> Gates { get; set; }
            = new();

        public int TotalGates { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; } = 1;

        public bool HasPreviousPage =>
            Page > 1;

        public bool HasNextPage =>
            Page < TotalPages;

        public int FirstItem =>
            TotalGates == 0
                ? 0
                : ((Page - 1) * PageSize) + 1;

        public int LastItem =>
            Math.Min(
                Page * PageSize,
                TotalGates);
    }
}