namespace LisAeroGest.Data.Interfaces
{
    public interface IReservationExpirationService
    {
        /// <summary>
        /// Inicia o serviço de expiração de reservas.
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Para o serviço de expiração de reservas.
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
