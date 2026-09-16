using LisAeroGest.Mobile.Models;
using Microsoft.Maui.Networking;
using System.Diagnostics;
using System.Net.Http.Json;

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
        /// Obtém a lista de voos de partida.
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
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var response = await _httpClient.GetAsync(
                    "api/flights/departures",
                    cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var departures = await response.Content
                        .ReadFromJsonAsync<List<FlightDto>>(
                            cancellationToken: cts.Token);

                    return departures ?? new List<FlightDto>();
                }

                Debug.WriteLine(
                    $"[ApiService Error] HTTP {(int)response.StatusCode}: " +
                    response.ReasonPhrase);

                return new List<FlightDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService Error] Exceção ao obter partidas: {ex.Message}");

                return new List<FlightDto>();
            }
        }

        /// <summary>
        /// Obtém a lista de voos de chegada.
        /// </summary>
        public async Task<List<FlightDto>> GetArrivalsAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                Debug.WriteLine("[ApiService] Sem ligação à Internet.");
                return new List<FlightDto>();
            }

            try
            {
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var response = await _httpClient.GetAsync(
                    "api/flights/arrivals",
                    cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content
                        .ReadFromJsonAsync<List<FlightDto>>(
                            cancellationToken: cts.Token)
                        ?? new List<FlightDto>();
                }

                Debug.WriteLine(
                    $"[ApiService Error] HTTP {(int)response.StatusCode}: " +
                    response.ReasonPhrase);

                return new List<FlightDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService Error] Exceção ao obter chegadas: {ex.Message}");

                return new List<FlightDto>();
            }
        }

        /// <summary>
        /// Obtém os bilhetes do passageiro autenticado.
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
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var response = await _httpClient.GetAsync("api/flights/my-tickets", cts.Token);


                if (response.IsSuccessStatusCode)
                {
                    var tickets = await response.Content
                        .ReadFromJsonAsync<List<TicketDto>>(
                            cancellationToken: cts.Token);

                    return tickets ?? new List<TicketDto>();
                }

                Debug.WriteLine(
                    $"[ApiService Error] HTTP {(int)response.StatusCode}: " +
                    response.ReasonPhrase);

                return new List<TicketDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService Error] Exceção ao obter bilhetes: {ex.Message}");

                return new List<TicketDto>();
            }
        }

        /// <summary>
        /// Obtém os detalhes de um voo específico.
        /// </summary>
        public async Task<FlightDetailDto?> GetDetailsAsync(int id)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                Debug.WriteLine("[ApiService] Sem ligação à Internet.");
                return null;
            }

            try
            {
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var response = await _httpClient.GetAsync(
                    $"api/voos/{id}",
                    cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content
                        .ReadFromJsonAsync<FlightDetailDto>(
                            cancellationToken: cts.Token);
                }

                var errorBody = await response.Content
                    .ReadAsStringAsync(cts.Token);

                Debug.WriteLine(
                    $"[ApiService Error] HTTP {(int)response.StatusCode}: {errorBody}");

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService Error] Exceção ao obter detalhes: {ex.Message}");

                return null;
            }
        }


        /// <summary>
        /// Obtém os lugares disponíveis de um voo.
        /// </summary>
        public async Task<List<SeatDto>> GetSeatsAsync(int id)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                Debug.WriteLine("[ApiService] Sem ligação à Internet.");
                return new List<SeatDto>();
            }

            try
            {
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var response = await _httpClient.GetAsync(
                    $"api/voos/{id}/seats",
                    cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content
                        .ReadAsStringAsync(cts.Token);

                    Debug.WriteLine(
                        $"[ApiService Error] HTTP {(int)response.StatusCode} " +
                        $"ao obter lugares: {errorBody}");

                    return new List<SeatDto>();
                }

                return await response.Content
                    .ReadFromJsonAsync<List<SeatDto>>(
                        cancellationToken: cts.Token)
                    ?? new List<SeatDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService Error] Exceção ao obter lugares: {ex.Message}");

                return new List<SeatDto>();
            }
        }

        /// <summary>
        /// Realiza o check-in de um bilhete.
        /// </summary>
        public async Task<CheckInResultDto> DoCheckInAsync(int ticketId)
        {
            try
            {
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var response = await _httpClient.PostAsJsonAsync(
                    "api/checkin",
                    new CheckInRequestDto
                    {
                        TicketId = ticketId
                    },
                    cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content
                        .ReadFromJsonAsync<CheckInResponseDto>(
                            cancellationToken: cts.Token);

                    if (data == null)
                    {
                        return new CheckInResultDto
                        {
                            Success = false,
                            ErrorMessage =
                                "A API devolveu uma resposta vazia."
                        };
                    }

                    return new CheckInResultDto
                    {
                        Success = true,
                        BoardingPassId = data.Id,
                        FlightNumber = data.FlightNumber,
                        Gate = data.Gate,
                        SequenceNumber = data.SequenceNumber,
                        QRData = data.QRData
                    };
                }

                var error = await response.Content
                    .ReadFromJsonAsync<ErrorResponseDto>(
                        cancellationToken: cts.Token);

                return new CheckInResultDto
                {
                    Success = false,
                    ErrorMessage = error?.Message
                        ?? "Não foi possível fazer check-in."
                };
            }
            catch (Exception ex)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    ErrorMessage = $"Erro de ligação: {ex.Message}"
                };
            }
        }
    }
}
