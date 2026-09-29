using LisAeroGest.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel para a listagem de voos com
    /// filtros, ordenação e paginação.
    /// </summary>
    public class FlightFilterViewModel
    {
        // =========================================================
        // VOOS
        // =========================================================

        public IEnumerable<Flight>? Flights { get; set; }


        // =========================================================
        // COMBOS DOS FILTROS
        // =========================================================

        public IEnumerable<SelectListItem>? Airlines { get; set; }

        public IEnumerable<SelectListItem>? Airports { get; set; }

        public IEnumerable<SelectListItem>? Statuses { get; set; }


        // =========================================================
        // FILTROS SELECIONADOS
        // =========================================================

        public int? FilterAirlineId { get; set; }

        public int? FilterOriginId { get; set; }

        public int? FilterDestinationId { get; set; }

        public string? FilterStatus { get; set; }


        // =========================================================
        // ORDENAÇÃO
        // =========================================================

        public string SortOrder { get; set; } = "departure_desc";


        // =========================================================
        // PAGINAÇÃO
        // =========================================================

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; }

        public int TotalPages =>
            TotalItems == 0
                ? 1
                : (int)Math.Ceiling(
                    TotalItems / (double)PageSize);


        public bool HasPreviousPage =>
            CurrentPage > 1;


        public bool HasNextPage =>
            CurrentPage < TotalPages;


        public int FirstItem =>
            TotalItems == 0
                ? 0
                : ((CurrentPage - 1) * PageSize) + 1;


        public int LastItem =>
            Math.Min(
                CurrentPage * PageSize,
                TotalItems);


        // =========================================================
        // MÉTODOS AUXILIARES
        // =========================================================

        public bool IsDepartingSoon(Flight flight) =>
            flight?.DepartureTime != null &&
            (flight.DepartureTime - DateTime.Now).TotalHours <= 2 &&
            (flight.DepartureTime - DateTime.Now).TotalMinutes > 0;


        public string GetDisplayGate(Flight flight)
        {
            if (flight?.Gate == null)
                return "---";

            return IsDepartingSoon(flight)
                ? flight.Gate.GateNumber
                : "---";
        }


        public bool CanPurchase(Flight flight) =>
            flight != null &&
            flight.Status != "Cancelled" &&
            flight.Status != "Departed" &&
            flight.DepartureTime > DateTime.Now.AddHours(1);


        public string GetTimeUntil(Flight flight)
        {
            if (flight?.DepartureTime == null)
                return "---";

            var diff =
                flight.DepartureTime - DateTime.Now;

            if (diff.TotalHours < 0)
                return "Já partiu";

            if (diff.TotalHours < 1)
                return $"Em {diff.Minutes} min";

            if (diff.TotalHours < 24)
                return $"Em {diff.Hours}h{diff.Minutes}min";

            return flight.DepartureTime
                .ToString("dd/MM HH:mm");
        }


        public string GetStatusBadgeColor(string status) =>
            status switch
            {
                "Scheduled" => "bg-primary",
                "CheckIn" => "bg-warning",
                "Boarding" => "bg-success",
                "Departed" => "bg-secondary",
                "Delayed" => "bg-danger",
                "Cancelled" => "bg-dark",
                _ => "bg-secondary"
            };


        public string GetStatusDisplay(string status) =>
            status switch
            {
                "Scheduled" => "Previsto",
                "CheckIn" => "Em check-in",
                "Boarding" => "Embarcando",
                "Departed" => "Partido",
                "Delayed" => "Atrasado",
                "Cancelled" => "Cancelado",
                _ => status
            };
    }
}