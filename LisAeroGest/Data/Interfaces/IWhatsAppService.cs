namespace LisAeroGest.Data.Interfaces
{
    public interface IWhatsAppService
    {
        Task<bool> SendTicketMessageAsync(string phoneNumber, string message);
    }
}
