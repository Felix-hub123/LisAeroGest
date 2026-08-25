using LisAeroGest.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LisAeroGest.Models
{
    public class TicketsIndexViewModel
    {
        public IEnumerable<Ticket> Tickets { get; set; } = new List<Ticket>();

        public string SearchTerm { get; set; } = string.Empty;

        public string? Status { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new();

        public int Page { get; set; } = 1;

        public int TotalPages { get; set; } = 1;

        public int TotalCount { get; set; }
    }
}
