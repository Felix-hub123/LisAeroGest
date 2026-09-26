using LisAeroGest.Data.Interfaces;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FlightCommunicationsController : Controller
    {
        private readonly IFlightCommunicationRepository
            _flightCommunicationRepository;

        public FlightCommunicationsController(
            IFlightCommunicationRepository flightCommunicationRepository)
        {
            _flightCommunicationRepository =
                flightCommunicationRepository;
        }


        // =========================================================
        //  HISTÓRICO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? type,
            string? flightNumber)
        {
            var communications =
                (await _flightCommunicationRepository
                    .GetAllWithDetailsAsync())
                .ToList();

            // -----------------------------------------------------
            // Filtro por pesquisa
            // -----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                communications = communications
                    .Where(c =>
                        c.Message.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        (c.Flight?.FlightNumber ?? "")
                            .Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase)
                        ||
                        ($"{c.SentByUser?.FirstName} {c.SentByUser?.LastName}")
                            .Contains(
                                search,
                                StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // -----------------------------------------------------
            // Filtro por tipo
            // -----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(type))
            {
                communications = communications
                    .Where(c =>
                        string.Equals(
                            c.Type,
                            type,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // -----------------------------------------------------
            // Filtro por voo
            // -----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(flightNumber))
            {
                flightNumber = flightNumber.Trim();

                communications = communications
                    .Where(c =>
                        (c.Flight?.FlightNumber ?? "")
                            .Contains(
                                flightNumber,
                                StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // -----------------------------------------------------
            // ViewModel
            // -----------------------------------------------------
            var model = new FlightCommunicationHistoryViewModel
            {
                Search = search,
                Type = type,
                FlightNumber = flightNumber,

                TotalCommunications = communications.Count,

                TotalRecipients = communications
                    .Sum(c => c.RecipientsCount),

                CommunicationsToday = communications
                    .Count(c =>
                        c.CreatedAt.Date == DateTime.UtcNow.Date),

                Communications = communications
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c =>
                        new FlightCommunicationItemViewModel
                        {
                            Id = c.Id,

                            FlightId = c.FlightId,

                            FlightNumber =
                                c.Flight?.FlightNumber ?? "—",

                            Type = c.Type,

                            Message = c.Message,

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
        //  DETALHES
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

                    FlightId = communication.FlightId,

                    FlightNumber =
                        communication.Flight?.FlightNumber ?? "—",

                    Type = communication.Type,

                    Message = communication.Message,

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
        //  HELPERS
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