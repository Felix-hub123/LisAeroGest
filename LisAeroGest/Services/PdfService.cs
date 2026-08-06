using LisAeroGest.Data.Entities;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LisAeroGest.Services
{
    public class PdfService
    {
        static PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateTicketPdf(Ticket ticket)
        {
            var qrContent = $"TICKET|{ticket.Id}|{ticket.Flight?.FlightNumber}|{ticket.Seat?.Code}";
            var qrCodeImage = GenerateQrCode(qrContent);
            var flight = ticket.Flight;
            var passenger = ticket.Passenger;
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Element(c => ComposeHeader(c, "BILHETE ELETRÓNICO"));
                    page.Content().Element(c => ComposeTicketContent(c, ticket, flight, passenger, qrCodeImage));
                    page.Footer().Element(ComposeFooter);
                });
            });
            return document.GeneratePdf();
        }
        public byte[] GenerateBoardingPassPdf(BoardingPass bp)
        {
            var qrContent = $"BOARDING|{bp.Id}|{bp.Ticket?.Flight?.FlightNumber}|{bp.Gate}|{bp.SequenceNumber}";
            var qrCodeImage = GenerateQrCode(qrContent);
            var ticket = bp.Ticket;
            var flight = ticket?.Flight;
            var passenger = ticket?.Passenger;
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Element(c => ComposeHeader(c, "CARTÃO DE EMBARQUE"));
                    page.Content().Element(c => ComposeBoardingContent(c, bp, flight, passenger, qrCodeImage));
                    page.Footer().Element(ComposeFooter);
                });
            });
            return document.GeneratePdf();
        }
        // ─── QR Code ──────────────────────────────────────────────────────────
        private byte[] GenerateQrCode(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(5);
        }
        // ─── Header ─────────────────────────────────────────────────────────────
        private void ComposeHeader(IContainer container, string title)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("LISEROGEST")
                        .Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
                    col.Item().Text("Lisboa Airport — Humberto Delgado")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text(title)
                        .Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
                    col.Item().Text($"Emitido: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }
        // ─── Conteúdo do Bilhete ────────────────────────────────────────────────
        private void ComposeTicketContent(IContainer container, Ticket ticket,
            Flight? flight, Passenger? passenger, byte[] qrCodeImage)
        {
            container.PaddingVertical(15).Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingVertical(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("PASSAGEIRO").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(passenger?.User?.FullName ?? "—").Bold().FontSize(12);
                        c.Item().Text($"Doc: {passenger?.DocumentType} {passenger?.DocumentNumber}").FontSize(9);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("VOO").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(flight?.FlightNumber ?? "—").Bold().FontSize(14);
                        c.Item().Text($"{flight?.OriginAirport?.City} → {flight?.DestinationAirport?.City}").FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("DATA / HORA").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(flight?.DepartureTime.ToString("dd/MM/yyyy") ?? "—").Bold().FontSize(11);
                        c.Item().Text(flight?.DepartureTime.ToString("HH:mm") ?? "—").Bold().FontSize(14);
                    });
                });
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingVertical(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("CLASSE").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(ticket.Seat?.SeatClass ?? "—").FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("LUGAR").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(ticket.Seat?.Code ?? "—").Bold().FontSize(12);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("PORTÃO").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text("—").FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("BAGAGEM").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(ticket.ExtraLuggage == true ? "Extra (+1)" : "Normal (1)").FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("REFEIÇÃO").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(ticket.MealIncluded == true ? "Incluída" : "—").FontSize(10);
                    });
                });
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingVertical(10).Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text($"PREÇO TOTAL: {ticket.TotalPrice:C}").Bold().FontSize(12);
                    row.RelativeItem().AlignRight().Height(70).Image(qrCodeImage);
                });
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(8).AlignCenter().Text(
                    "Este bilhete é válido apenas para o passageiro aqui identificado. " +
                    "Apresente um documento de identificação válido no momento do embarque.")
                    .FontSize(7).FontColor(Colors.Grey.Darken1);
            });
        }
        // ─── Conteúdo do Cartão de Embarque ───────────────────────────────────
        private void ComposeBoardingContent(IContainer container, BoardingPass bp,
            Flight? flight, Passenger? passenger, byte[] qrCodeImage)
        {
            container.PaddingVertical(15).Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingVertical(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("PASSAGEIRO").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(passenger?.User?.FullName ?? "—").Bold().FontSize(12);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("VOO").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(flight?.FlightNumber ?? "—").Bold().FontSize(14);
                        c.Item().Text($"{flight?.OriginAirport?.IATACode} → {flight?.DestinationAirport?.IATACode}").FontSize(10);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("DATA").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(flight?.DepartureTime.ToString("dd/MM/yyyy") ?? "—").Bold().FontSize(11);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("HORA").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(flight?.DepartureTime.ToString("HH:mm") ?? "—").Bold().FontSize(14);
                    });
                });
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingVertical(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("PORTÃO").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(bp.Gate ?? "—").Bold().FontSize(16);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("SEQUÊNCIA").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(bp.SequenceNumber.ToString()).Bold().FontSize(16);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("LUGAR").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(bp.Ticket?.Seat?.Code ?? "—").Bold().FontSize(16);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("CLASSE").Bold().FontColor(Colors.Grey.Darken2);
                        c.Item().PaddingTop(4).Text(bp.Ticket?.Seat?.SeatClass ?? "—").FontSize(10);
                    });
                });
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingVertical(10).Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text("QR CODE").Bold().FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().AlignRight().Height(80).Image(qrCodeImage);
                });
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(8).AlignCenter().Text(
                    $"Cartão de Embarque — {bp.Ticket?.Passenger?.User?.FullName} — " +
                    $"Voo {flight?.FlightNumber} — Portão {bp.Gate ?? "—"} — {flight?.DepartureTime:dd/MM/yyyy HH:mm}")
                    .FontSize(7).FontColor(Colors.Grey.Darken1);
            });
        }
        // ─── Footer ────────────────────────────────────────────────────────────
        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("LisAeroGest — Aeroporto de Lisboa | ").FontSize(7).FontColor(Colors.Grey.Medium);
                text.Span("Humberto Delgado Airport (LIS)").FontSize(7).FontColor(Colors.Grey.Medium);
            });
        }
    }
}
