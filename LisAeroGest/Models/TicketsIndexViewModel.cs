using LisAeroGest.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LisAeroGest.Models
{
    public class TicketsIndexViewModel
    {
        // =========================================================
        // DADOS
        // =========================================================

        public IEnumerable<Ticket> Tickets { get; set; }
            = new List<Ticket>();


        // =========================================================
        // FILTROS
        // =========================================================

        public string SearchTerm { get; set; }
            = string.Empty;

        public string? Status { get; set; }

        public IEnumerable<SelectListItem> StatusOptions { get; set; }
            = new List<SelectListItem>();


        // =========================================================
        // ORDENAÇÃO
        // =========================================================

        public string SortOrder { get; set; }
            = "date_desc";


        // =========================================================
        // PAGINAÇÃO
        // =========================================================

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }


        public bool HasPreviousPage =>
            Page > 1;


        public bool HasNextPage =>
            Page < TotalPages;


        public int FirstItem =>
            TotalCount == 0
                ? 0
                : ((Page - 1) * PageSize) + 1;


        public int LastItem =>
            Math.Min(
                Page * PageSize,
                TotalCount);
    }
}