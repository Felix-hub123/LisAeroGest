using LisAeroGest.Data.Entities;

namespace LisAeroGest.Data.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateTicketPdf(Ticket ticket);
        byte[] GenerateBoardingPassPdf(BoardingPass boardingPass);
    }
}
