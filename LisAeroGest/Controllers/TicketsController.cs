using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela listagem e consulta global
    /// de bilhetes vendidos.
    /// </summary>
    [Authorize(Roles = "Admin,Employee")]
    public class TicketsController : Controller
    {
        private const int PageSize = 20;

        private readonly ITicketRepository _ticketRepository;
        private readonly IConverterHelper _converterHelper;


        public TicketsController(
            ITicketRepository ticketRepository,
            IConverterHelper converterHelper)
        {
            _ticketRepository = ticketRepository;
            _converterHelper = converterHelper;
        }


        /// <summary>
        /// Lista os bilhetes com pesquisa,
        /// filtro, ordenação e paginação.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? status,
            string sortOrder = "date_desc",
            int page = 1)
        {
            // =====================================================
            // QUERY BASE
            // =====================================================

            var query =
                _ticketRepository.GetAllQueryable();


            // =====================================================
            // PESQUISA
            // =====================================================

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term =
                    searchTerm
                        .Trim()
                        .ToLower();

                query = query.Where(t =>

                    (t.Passenger != null &&
                     t.Passenger.FirstName != null &&
                     t.Passenger.FirstName
                         .ToLower()
                         .Contains(term))

                    ||

                    (t.Passenger != null &&
                     t.Passenger.LastName != null &&
                     t.Passenger.LastName
                         .ToLower()
                         .Contains(term))

                    ||

                    (t.Flight != null &&
                     t.Flight.FlightNumber != null &&
                     t.Flight.FlightNumber
                         .ToLower()
                         .Contains(term))
                );
            }


            // =====================================================
            // FILTRO POR ESTADO
            // =====================================================

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(
                    t => t.Status == status);
            }


            // =====================================================
            // TOTAL DE RESULTADOS
            // =====================================================

            var totalCount =
                await query.CountAsync();


            // =====================================================
            // ORDENAÇÃO
            // =====================================================

            query = sortOrder switch
            {
                // Data
                "date_asc" =>
                    query.OrderBy(
                        t => t.PurchaseDate),

                "date_desc" =>
                    query.OrderByDescending(
                        t => t.PurchaseDate),

                // Preço
                "price_asc" =>
                    query.OrderBy(
                        t => t.TotalPrice),

                "price_desc" =>
                    query.OrderByDescending(
                        t => t.TotalPrice),

                // Voo
                "flight_asc" =>
                    query.OrderBy(
                        t => t.Flight!.FlightNumber),

                "flight_desc" =>
                    query.OrderByDescending(
                        t => t.Flight!.FlightNumber),

                // Passageiro
                "passenger_asc" =>
                    query
                        .OrderBy(
                            t => t.Passenger!.FirstName)
                        .ThenBy(
                            t => t.Passenger!.LastName),

                "passenger_desc" =>
                    query
                        .OrderByDescending(
                            t => t.Passenger!.FirstName)
                        .ThenByDescending(
                            t => t.Passenger!.LastName),

                // Estado
                "status_asc" =>
                    query.OrderBy(
                        t => t.Status),

                "status_desc" =>
                    query.OrderByDescending(
                        t => t.Status),

                // Padrão
                _ =>
                    query.OrderByDescending(
                        t => t.PurchaseDate)
            };


            // =====================================================
            // PAGINAÇÃO
            // =====================================================

            var totalPages =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        totalCount / (double)PageSize));


            page =
                Math.Max(
                    1,
                    Math.Min(
                        page,
                        totalPages));


            var tickets =
                await query
                    .Skip(
                        (page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();


            // =====================================================
            // VIEWMODEL
            // =====================================================

            var viewModel =
                new TicketsIndexViewModel
                {
                    Tickets = tickets,

                    SearchTerm =
                        searchTerm ?? string.Empty,

                    Status = status,

                    StatusOptions =
                        _converterHelper
                            .ToTicketStatusSelectList(status),

                    SortOrder = sortOrder,

                    Page = page,

                    PageSize = PageSize,

                    TotalPages = totalPages,

                    TotalCount = totalCount
                };


            return View(viewModel);
        }
    }
}