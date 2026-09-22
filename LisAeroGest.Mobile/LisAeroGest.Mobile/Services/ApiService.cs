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

        // ═══════════════════════════════════════════════════════════
        // MÉTODOS GENÉRICOS
        // ═══════════════════════════════════════════════════════════

        private static bool HasInternet() =>
            Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        /// <summary>
        /// GET genérico — devolve ApiResult com dados ou mensagem de erro.
        /// </summary>
        private async Task<ApiResult<T>> GetAsync<T>(string endpoint)
        {
            if (!HasInternet())
                return ApiResult<T>.Fail(
                    "Sem ligação à Internet. Verifique a sua rede.");

            try
            {
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var response = await _httpClient.GetAsync(
                    endpoint, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine(
                        $"[ApiService] HTTP {(int)response.StatusCode} em {endpoint}");
                    return ApiResult<T>.Fail(
                        $"Erro do servidor ({(int)response.StatusCode}).");
                }

                var data = await response.Content
                    .ReadFromJsonAsync<T>(
                        cancellationToken: cts.Token);

                if (data == null)
                    return ApiResult<T>.Fail("Resposta vazia do servidor.");

                return ApiResult<T>.Ok(data);
            }
            catch (TaskCanceledException)
            {
                return ApiResult<T>.Fail(
                    "O servidor demorou demasiado tempo a responder.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService] Exceção em {endpoint}: {ex.Message}");
                return ApiResult<T>.Fail(
                    "Erro de ligação ao servidor.");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PARTIDAS
        // ═══════════════════════════════════════════════════════════

        public Task<ApiResult<List<FlightDto>>> GetDeparturesAsync()
            => GetAsync<List<FlightDto>>("api/flights/departures");

        // ═══════════════════════════════════════════════════════════
        // CHEGADAS
        // ═══════════════════════════════════════════════════════════

        public Task<ApiResult<List<FlightDto>>> GetArrivalsAsync()
            => GetAsync<List<FlightDto>>("api/flights/arrivals");

        // ═══════════════════════════════════════════════════════════
        // MEUS BILHETES
        // ═══════════════════════════════════════════════════════════

        public Task<ApiResult<List<TicketDto>>> GetMyTicketsAsync()
     => GetAsync<List<TicketDto>>("api/voos/my-tickets");

        // ═══════════════════════════════════════════════════════════
        // DETALHES DE VOO
        // ═══════════════════════════════════════════════════════════

        public Task<ApiResult<FlightDetailDto>> GetDetailsAsync(int id)
            => GetAsync<FlightDetailDto>($"api/voos/{id}");

        // ═══════════════════════════════════════════════════════════
        // LUGARES
        // ═══════════════════════════════════════════════════════════

        public Task<ApiResult<List<SeatDto>>> GetSeatsAsync(int id)
            => GetAsync<List<SeatDto>>($"api/voos/{id}/seats");

        // ═══════════════════════════════════════════════════════════
        // CHECK-IN
        // ═══════════════════════════════════════════════════════════

        public async Task<CheckInResultDto> DoCheckInAsync(int ticketId)
        {
            if (!HasInternet())
            {
                return new CheckInResultDto
                {
                    Success = false,
                    ErrorMessage =
                        "Sem ligação à Internet. Verifique a sua rede."
                };
            }

            try
            {
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var response = await _httpClient.PostAsJsonAsync(
                    "api/checkin",
                    new CheckInRequestDto { TicketId = ticketId },
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
            catch (TaskCanceledException)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    ErrorMessage =
                        "O servidor demorou demasiado tempo a responder."
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

        public async Task<ValidateBoardingPassResult> ValidateBoardingPassAsync(
      string qrData)
        {
            try
            {
                if (!HasInternet())
                {
                    return new ValidateBoardingPassResult
                    {
                        Success = false,
                        ErrorMessage = "Sem ligação à internet."
                    };
                }

                var response = await _httpClient.PostAsJsonAsync(
                    "api/checkin/validate",
                    new
                    {
                        QRData = qrData
                    });

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = "Bilhete inválido.";

                    try
                    {
                        var json = await response.Content
                            .ReadFromJsonAsync<Dictionary<string, string>>();

                        if (json != null &&
                            json.TryGetValue("message", out var message))
                        {
                            errorMessage = message;
                        }
                    }
                    catch
                    {
                        // Mantém a mensagem padrão
                    }

                    return new ValidateBoardingPassResult
                    {
                        Success = false,
                        ErrorMessage = errorMessage
                    };
                }

                var result = await response.Content
                    .ReadFromJsonAsync<ValidateBoardingPassResult>();

                return result ?? new ValidateBoardingPassResult
                {
                    Success = false,
                    ErrorMessage = "Resposta inválida da API."
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService] Validate QR: {ex.Message}");

                return new ValidateBoardingPassResult
                {
                    Success = false,
                    ErrorMessage = "Erro ao validar o bilhete."
                };
            }
        }

        public Task<ApiResult<List<EmployeeCheckInDto>>> SearchEmployeeTicketsAsync(string query)
            => GetAsync<List<EmployeeCheckInDto>>(
                $"api/employee/checkin/search?query={Uri.EscapeDataString(query)}");

        public async Task<EmployeeCheckInResultDto> EmployeeCheckInAsync(int ticketId)
        {
            if (!HasInternet())
                return new EmployeeCheckInResultDto { Success = false, ErrorMessage = "Sem ligação à Internet." };

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var response = await _httpClient.PostAsJsonAsync(
                    "api/employee/checkin",
                    new EmployeeCheckInRequestDto { TicketId = ticketId },
                    cts.Token);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<EmployeeCheckInResultDto>(cancellationToken: cts.Token)
                        ?? new EmployeeCheckInResultDto { Success = false, ErrorMessage = "Resposta vazia do servidor." };

                var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(cancellationToken: cts.Token);
                return new EmployeeCheckInResultDto { Success = false, ErrorMessage = error?.Message ?? "Não foi possível concluir o check-in." };
            }
            catch (TaskCanceledException)
            {
                return new EmployeeCheckInResultDto { Success = false, ErrorMessage = "O servidor demorou demasiado tempo a responder." };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Employee check-in: {ex.Message}");
                return new EmployeeCheckInResultDto { Success = false, ErrorMessage = "Erro de ligação ao servidor." };
            }
        }

        public Task<ApiResult<List<NotificationDto>>> GetNotificationsAsync()
            => GetAsync<List<NotificationDto>>("api/notifications");

        public async Task<ApiResult<bool>> MarkNotificationReadAsync(int id)
        {
            if (!HasInternet()) return ApiResult<bool>.Fail("Sem ligação à Internet.");
            try
            {
                using var response = await _httpClient.PostAsync($"api/notifications/{id}/read", null);
                return response.IsSuccessStatusCode
                    ? ApiResult<bool>.Ok(true)
                    : ApiResult<bool>.Fail("Não foi possível atualizar a notificação.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService] Mark notification: {ex.Message}");
                return ApiResult<bool>.Fail("Erro de ligação ao servidor.");
            }
        }
    }
}
