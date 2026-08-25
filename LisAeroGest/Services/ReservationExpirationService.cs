using LisAeroGest.Data;
using LisAeroGest.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Services
{
    public class ReservationExpirationService : BackgroundService, IReservationExpirationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReservationExpirationService> _logger;

        public ReservationExpirationService(
            IServiceProvider serviceProvider,
            ILogger<ReservationExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReservationExpirationService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                    // Reservas com mais de 30 minutos e ainda em estado "Reserved"
                    var limite = DateTime.UtcNow.AddMinutes(-30);

                    var expiradas = await context.Tickets
                        .Where(t => t.Status == "Reserved" && t.PurchaseDate < limite)
                        .ToListAsync(stoppingToken);

                    if (expiradas.Any())
                    {
                        foreach (var ticket in expiradas)
                        {
                            ticket.Status = "Expired";

                            // Se o lugar estiver marcado como ocupado, libertar
                            if (ticket.SeatId > 0)
                            {
                                var seat = await context.Seats.FindAsync(ticket.SeatId);
                                if (seat != null && !seat.IsAvailable)
                                {
                                    seat.IsAvailable = true;
                                    context.Seats.Update(seat);
                                }
                            }
                        }

                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation(
                            "{Count} reservas expiradas foram canceladas e lugares libertados.",
                            expiradas.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar expiração de reservas.");
                }

                // Aguarda 5 minutos antes da próxima execução
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("ReservationExpirationService terminado.");
        }

        // Implementação explícita da interface (opcional, pois BackgroundService já tem)
        Task IReservationExpirationService.StartAsync(CancellationToken cancellationToken)
            => base.StartAsync(cancellationToken);

        Task IReservationExpirationService.StopAsync(CancellationToken cancellationToken)
            => base.StopAsync(cancellationToken);
    }
}
