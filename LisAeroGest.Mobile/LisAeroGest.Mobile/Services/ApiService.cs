using LisAeroGest.Mobile.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Maui.Networking;
namespace LisAeroGest.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Obtém a lista de Voos de Partida (Departures)
        /// </summary>
        public async Task<List<FlightDto>> GetDeparturesAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                Debug.WriteLine("[ApiService] Sem ligação à Internet.");
                return new List<FlightDto>();
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                // Ajusta o endpoint "api/flights/departures" para a rota exata da tua API no Render
                var response = await _httpClient.GetAsync("api/flights/departures", cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var departures = await response.Content.ReadFromJsonAsync<List<FlightDto>>(cancellationToken: cts.Token);
                    return departures ?? new List<FlightDto>();
                }

                Debug.WriteLine($"[ApiService Error] HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                return new List<FlightDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService Error] Exceção ao obter partidas: {ex.Message}");
                return new List<FlightDto>();
            }
        }

        /// <summary>
        /// Obtém a lista de Voos de Chegada (Arrivals) - Opcional
        /// </summary>
        public async Task<List<FlightDto>> GetArrivalsAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return new List<FlightDto>();

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                // Ajusta a rota para a tua API no Render se necessário
                var response = await _httpClient.GetAsync("api/flights/arrivals", cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<FlightDto>>(cancellationToken: cts.Token)
                           ?? new List<FlightDto>();
                }

                return new List<FlightDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService Error] Exceção ao obter chegadas: {ex.Message}");
                return new List<FlightDto>();
            }
        }

        /// <summary>
        /// Obtém os bilhetes do passageiro autenticado, com o estado do check-in.
        /// </summary>
        public async Task<List<TicketDto>> GetMyTicketsAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                Debug.WriteLine("[ApiService] Sem ligação à Internet.");
                return new List<TicketDto>();
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                var response = await _httpClient.GetAsync("api/voos/my-tickets", cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var tickets = await response.Content.ReadFromJsonAsync<List<TicketDto>>(cancellationToken: cts.Token);
                    return tickets ?? new List<TicketDto>();
                }

                Debug.WriteLine($"[ApiService Error] HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                return new List<TicketDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService Error] Exceção ao obter bilhetes: {ex.Message}");
                return new List<TicketDto>();
            }
        }

        public async Task<CheckInResultDto> DoCheckInAsync(int ticketId)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                var response = await _httpClient.PostAsJsonAsync(
                    "api/checkin", new CheckInRequestDto { TicketId = ticketId }, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<CheckInResponseDto>(cancellationToken: cts.Token);
                    return new CheckInResultDto
                    {
                        Success = true,
                        BoardingPassId = data!.Id,
                        FlightNumber = data.FlightNumber,
                        Gate = data.Gate,
                        SequenceNumber = data.SequenceNumber,
                        QRData = data.QRData
                    };
                }

                var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(cancellationToken: cts.Token);
                return new CheckInResultDto { Success = false, ErrorMessage = error?.Message ?? "Não foi possível fazer check-in." };
            }
            catch (Exception ex)
            {
                return new CheckInResultDto { Success = false, ErrorMessage = $"Erro de ligação: {ex.Message}" };
            }
        }
    }
}
