namespace LisAeroGest.Data.Interfaces
{
    public interface IPayPalService
    {
        /// <summary>
        /// Cria uma ordem de pagamento no PayPal e devolve o Order ID.
        /// </summary>
        Task<string> CreateOrderAsync(decimal amount, string currency, string ticketReference);

        /// <summary>
        /// Captura (finaliza) o pagamento de uma ordem já aprovada pelo comprador.
        /// Devolve true se o pagamento foi capturado com sucesso ("COMPLETED").
        /// </summary>
        Task<bool> CaptureOrderAsync(string orderId);
    }
}
