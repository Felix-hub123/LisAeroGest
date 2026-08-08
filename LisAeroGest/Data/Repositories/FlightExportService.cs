using LisAeroGest.Data.Entities.DTOs;
using LisAeroGest.Data.Interfaces;
using System.Reflection.Metadata;
using System.Text;
using System.Xml.Serialization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;


namespace LisAeroGest.Data.Repositories
{
    public class FlightExportService : IFlightExportService
    {
        private readonly IFlightRepository _flightRepository;

        public FlightExportService(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<byte[]> ExportFlightsToXmlAsync()
        {
            var flights = await _flightRepository.GetAllWithDetailsAsync();

            var report = new FlightReportDto
            {
                GeneratedAt = DateTime.UtcNow,
                Flights = flights.Select(f => new FlightExportItemDto
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber ?? "N/A",
                    OriginAirport = f.OriginAirport?.Name ?? "Desconhecido",
                    DestinationAirport = f.DestinationAirport?.Name ?? "Desconhecido",
                    DepartureTime = f.DepartureTime,
                    Price = 0, 
                    TotalSeats = 100, 
                    OccupiedSeats = 0, 
                    Status = f.DepartureTime > DateTime.UtcNow ? "Agendado" : "Concluído"
                }).ToList()
            };

            var serializer = new XmlSerializer(typeof(FlightReportDto));
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8);

            serializer.Serialize(writer, report);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportFlightsToPdfAsync()
        {
            var flights = await _flightRepository.GetAllWithDetailsAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("LisAeroGest - Relatório Geral de Voos")
                               .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                            col.Item().Text($"Emitido em: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC")
                               .FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });

                    // Tabela de Dados
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);  // N.º Voo
                            columns.RelativeColumn(2);  // Origem
                            columns.RelativeColumn(2);  // Destino
                            columns.ConstantColumn(110); // Partida
                            columns.ConstantColumn(70);  // Preço
                            columns.ConstantColumn(80);  // Ocupação
                            columns.ConstantColumn(70);  // Estado
                        });

                        // Cabeçalho da Tabela
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Voo").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Origem").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Destino").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Partida").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Preço").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Ocupação").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Estado").Bold().FontColor(Colors.White);
                        });

                        foreach (var flight in flights)
                        {
                            var occupied = flight.Seats?.Count ?? 0;
                         
                            var capacity = flight.Aircraft?.TotalCapacity ?? flight.Seats?.Count ?? 0;
                            var status = flight.DepartureTime > DateTime.UtcNow ? "Agendado" : "Concluído";

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(flight.FlightNumber ?? "N/A");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(flight.OriginAirport?.Name ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(flight.DestinationAirport?.Name ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(flight.DepartureTime.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{flight.BasePrice:C2}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{occupied}/{capacity}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(status);
                        }
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
