using LisAeroGest.Data.Interfaces;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FlightCommunicationsController : Controller
    {
        private const int PageSize = 10;

        private readonly IFlightCommunicationRepository
            _flightCommunicationRepository;

        public FlightCommunicationsController(
            IFlightCommunicationRepository flightCommunicationRepository)
        {
            _flightCommunicationRepository =
                flightCommunicationRepository;
        }


        // =========================================================
        // HISTÓRICO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? type,
            string? flightNumber,
            string sortOrder = "date_desc",
            int page = 1)
        {
            var query =
                _flightCommunicationRepository
                    .GetAllWithDetailsQueryable();


            // =====================================================
            // PESQUISA
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.Message.Contains(search) ||

                    (c.Flight != null &&
                     c.Flight.FlightNumber.Contains(search)) ||

                    (c.SentByUser != null &&
                     (
                         c.SentByUser.FirstName.Contains(search) ||
                         c.SentByUser.LastName.Contains(search)
                     )));
            }


            // =====================================================
            // FILTRO POR TIPO
            // =====================================================

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(c => c.Type == type);
            }


            // =====================================================
            // FILTRO POR VOO
            // =====================================================

            if (!string.IsNullOrWhiteSpace(flightNumber))
            {
                flightNumber = flightNumber.Trim();

                query = query.Where(c =>
                    c.Flight != null &&
                    c.Flight.FlightNumber.Contains(flightNumber));
            }


            // =====================================================
            // ESTATÍSTICAS DOS RESULTADOS FILTRADOS
            // =====================================================

            var totalCommunications =
                await query.CountAsync();

            var totalRecipients =
                await query.SumAsync(c =>
                    (int?)c.RecipientsCount) ?? 0;

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var communicationsToday =
                await query.CountAsync(c =>
                    c.CreatedAt >= today &&
                    c.CreatedAt < tomorrow);


            // =====================================================
            // ORDENAÇÃO
            // =====================================================

            query = sortOrder switch
            {
                "date_asc" =>
                    query.OrderBy(c => c.CreatedAt),

                "flight_asc" =>
                    query.OrderBy(c =>
                        c.Flight != null
                            ? c.Flight.FlightNumber
                            : ""),

                "flight_desc" =>
                    query.OrderByDescending(c =>
                        c.Flight != null
                            ? c.Flight.FlightNumber
                            : ""),

                "employee_asc" =>
                    query
                        .OrderBy(c =>
                            c.SentByUser != null
                                ? c.SentByUser.FirstName
                                : "")
                        .ThenBy(c =>
                            c.SentByUser != null
                                ? c.SentByUser.LastName
                                : ""),

                "recipients_desc" =>
                    query.OrderByDescending(c =>
                        c.RecipientsCount),

                "recipients_asc" =>
                    query.OrderBy(c =>
                        c.RecipientsCount),

                _ =>
                    query.OrderByDescending(c =>
                        c.CreatedAt)
            };


            // =====================================================
            // PAGINAÇÃO
            // =====================================================

            var totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalCommunications /
                    (double)PageSize));

            page = Math.Max(
                1,
                Math.Min(page, totalPages));


            var communications = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();


            // =====================================================
            // VIEWMODEL
            // =====================================================

            var model =
                new FlightCommunicationHistoryViewModel
                {
                    Search = search,

                    Type = type,

                    FlightNumber = flightNumber,

                    SortOrder = sortOrder,

                    Page = page,

                    PageSize = PageSize,

                    TotalPages = totalPages,

                    TotalCommunications =
                        totalCommunications,

                    TotalRecipients =
                        totalRecipients,

                    CommunicationsToday =
                        communicationsToday,

                    Communications =
                        communications
                            .Select(c =>
                                new FlightCommunicationItemViewModel
                                {
                                    Id = c.Id,

                                    FlightId =
                                        c.FlightId,

                                    FlightNumber =
                                        c.Flight?.FlightNumber
                                        ?? "—",

                                    Type =
                                        c.Type,

                                    Message =
                                        c.Message,

                                    EmployeeName =
                                        GetEmployeeName(
                                            c.SentByUser?.FirstName,
                                            c.SentByUser?.LastName),

                                    RecipientsCount =
                                        c.RecipientsCount,

                                    CreatedAt =
                                        c.CreatedAt
                                })
                            .ToList()
                };

            return View(model);
        }


        // =========================================================
        // DETALHES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var communication =
                await _flightCommunicationRepository
                    .GetByIdWithDetailsAsync(id);

            if (communication == null)
            {
                return NotFound();
            }

            var model =
                new FlightCommunicationDetailsViewModel
                {
                    Id = communication.Id,

                    FlightId =
                        communication.FlightId,

                    FlightNumber =
                        communication.Flight?.FlightNumber
                        ?? "—",

                    Type =
                        communication.Type,

                    Message =
                        communication.Message,

                    EmployeeName =
                        GetEmployeeName(
                            communication.SentByUser?.FirstName,
                            communication.SentByUser?.LastName),

                    EmployeeEmail =
                        communication.SentByUser?.Email,

                    RecipientsCount =
                        communication.RecipientsCount,

                    CreatedAt =
                        communication.CreatedAt
                };

            return View(model);
        }


        // =========================================================
        // HELPERS
        // =========================================================

        private static string GetEmployeeName(
            string? firstName,
            string? lastName)
        {
            var fullName =
                $"{firstName} {lastName}".Trim();

            return string.IsNullOrWhiteSpace(fullName)
                ? "Utilizador não identificado"
                : fullName;
        }
    }
}