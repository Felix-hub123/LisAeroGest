using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela listagem e consulta global de bilhetes vendidos,
    /// para gestão por Administradores e Funcionários.
    /// </summary>
    [Authorize(Roles = "Admin,Employee")]
    public class TicketsController : Controller
    {
        private const int PageSize = 20;

        private readonly ITicketRepository _ticketRepository;
        private readonly IConverterHelper _converterHelper;

        public TicketsController(ITicketRepository ticketRepository,
            IConverterHelper converterHelper)
        {
            _ticketRepository = ticketRepository;
            _converterHelper = converterHelper;

        }

        /// <summary>
        /// Lista todos os bilhetes vendidos no sistema, com pesquisa por nome de
        /// passageiro ou número de voo, filtro por estado, e paginação.
        /// </summary>
        public async Task<IActionResult> Index(string? searchTerm, string? status, int page = 1)
        {
            var query = _ticketRepository.GetAllQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(t =>
                    (t.Passenger != null && t.Passenger.FirstName != null && t.Passenger.FirstName.ToLower().Contains(term)) ||
                    (t.Passenger != null && t.Passenger.LastName != null && t.Passenger.LastName.ToLower().Contains(term)) ||
                    (t.Flight != null && t.Flight.FlightNumber != null && t.Flight.FlightNumber.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            query = query.OrderByDescending(t => t.PurchaseDate);

            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Max(1, Math.Min(page, totalPages));

            var tickets = await query
                 .Skip((page - 1) * PageSize)
                 .Take(PageSize)
                 .ToListAsync();

            var viewModel = new TicketsIndexViewModel
            {
                Tickets = tickets,
                SearchTerm = searchTerm ?? string.Empty,
                Status = status,
                StatusOptions = _converterHelper.ToTicketStatusSelectList(status),
                Page = page,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            return View(viewModel);
        }
    }
}
