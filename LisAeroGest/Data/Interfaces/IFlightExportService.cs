namespace LisAeroGest.Data.Interfaces
{
    public interface IFlightExportService
    {
        Task<byte[]> ExportFlightsToXmlAsync();
        Task<byte[]> ExportFlightsToPdfAsync();
    }
}
