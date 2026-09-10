using LisAeroGest.Data;
using LisAeroGest.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Services
{
    /// <summary>
    /// Serviço em segundo plano que atualiza automaticamente o estado dos voos
    /// e dos gates com base na hora atual.
    /// </summary>
    public class FlightStatusUpdaterService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FlightStatusUpdaterService> _logger;

        // Intervalo entre cada verificação (1 minuto)
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        public FlightStatusUpdaterService(
            IServiceProvider serviceProvider,
            ILogger<FlightStatusUpdaterService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FlightStatusUpdaterService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateFlightStatusesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar estados dos voos.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task UpdateFlightStatusesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var now = DateTime.Now;

            // Só processa voos que não estão cancelados nem já chegaram
            var flights = await context.Flights
                .Include(f => f.Gate)
                .Where(f => !f.WasDeleted
                         && f.Status != "Cancelled"
                         && f.Status != "Arrived")
                .ToListAsync();

            foreach (var flight in flights)
            {
                // Usa a hora de partida efetiva (com atraso se existir)
                var effectiveDeparture = flight.Status == "Delayed" && flight.DelayedDepartureTime.HasValue
                    ? flight.DelayedDepartureTime.Value
                    : flight.DepartureTime;

                var arrival = flight.ArrivalTime;
                var minutesToDeparture = (effectiveDeparture - now).TotalMinutes;

                string newStatus;

                if (now >= arrival)
                {
                    newStatus = "Arrived";
                }
                else if (now >= effectiveDeparture)
                {
                    newStatus = "Departed";
                }
                else if (minutesToDeparture <= 40)
                {
                    newStatus = "Boarding";
                }
                else if (minutesToDeparture <= 90)
                {
                    newStatus = "CheckIn";
                }
                else
                {
                    newStatus = "Scheduled";
                }

                // Não regride estados manuais de atraso
                if (flight.Status == "Delayed" && newStatus != "Arrived" && newStatus != "Departed")
                    continue;

                if (flight.Status == newStatus)
                    continue;

                var oldStatus = flight.Status;
                flight.Status = newStatus;

                // Atualiza o gate associado
                if (flight.Gate != null)
                {
                    flight.Gate.Status = newStatus switch
                    {
                        "Boarding" => "Occupied",
                        "Departed" => "Cleaning",
                        "Arrived" => "Available",
                        _ => flight.Gate.Status
                    };
                }

                _logger.LogInformation(
                    "Voo {FlightNumber}: {OldStatus} → {NewStatus}",
                    flight.FlightNumber, oldStatus, newStatus);
            }

            await context.SaveChangesAsync();
        }
    }
}