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
        // GESTÃO DE PORTAS - FUNCIONÁRIO
        // ═══════════════════════════════════════════════════════════

        public Task<ApiResult<List<GateDto>>> GetEmployeeGatesAsync()
            => GetAsync<List<GateDto>>("api/employee/gates");


        public async Task<ApiResult<ChangeGateResultDto>> ChangeFlightGateAsync(
            int flightId,
            int gateId)
        {
            if (!HasInternet())
            {
                return ApiResult<ChangeGateResultDto>.Fail(
                    "Sem ligação à Internet.");
            }

            try
            {
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var response = await _httpClient.PutAsJsonAsync(
                    $"api/employee/flights/{flightId}/gate",
                    new
                    {
                        GateId = gateId
                    },
                    cts.Token);

                var content = await response.Content.ReadAsStringAsync(
                    cts.Token);

                Debug.WriteLine(
                    $"[ApiService] Change gate status: {(int)response.StatusCode}");

                Debug.WriteLine(
                    $"[ApiService] Change gate response: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = "Não foi possível alterar a porta.";

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        try
                        {
                            var error =
                                System.Text.Json.JsonSerializer
                                    .Deserialize<ErrorResponseDto>(
                                        content,
                                        new System.Text.Json.JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        });

                            errorMessage =
                              error?.Message ??
                              errorMessage;
                        }
                        catch
                        {
                            // Mantém mensagem padrão.
                        }
                    }

                    return ApiResult<ChangeGateResultDto>.Fail(
                        errorMessage);
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    return ApiResult<ChangeGateResultDto>.Fail(
                        "O servidor não devolveu os dados da alteração.");
                }

                var data =
                    System.Text.Json.JsonSerializer
                        .Deserialize<ChangeGateResultDto>(
                            content,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                if (data == null)
                {
                    return ApiResult<ChangeGateResultDto>.Fail(
                        "Não foi possível interpretar a resposta do servidor.");
                }

                return ApiResult<ChangeGateResultDto>.Ok(data);
            }
            catch (TaskCanceledException)
            {
                return ApiResult<ChangeGateResultDto>.Fail(
                    "O servidor demorou demasiado a responder.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService] Change gate error: {ex}");

                return ApiResult<ChangeGateResultDto>.Fail(
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
        // COMPRA / RESERVA
        // ═══════════════════════════════════════════════════════════

        public async Task<ApiResult<ReserveTicketResultDto>> ReserveTicketAsync(
      int flightId,
      int seatId,
      bool extraLuggage = false,
      bool mealIncluded = false)
        {
            if (!HasInternet())
            {
                return ApiResult<ReserveTicketResultDto>.Fail(
                    "Sem ligação à Internet.");
            }

            try
            {
                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

                var request = new ReserveTicketRequestDto
                {
                    FlightId = flightId,
                    SeatId = seatId,
                    ExtraLuggage = extraLuggage,
                    MealIncluded = mealIncluded
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "api/booking/reserve",
                    request,
                    cts.Token);

                // Read the response as text first.
                var content = await response.Content.ReadAsStringAsync(
                    cts.Token);

                Debug.WriteLine(
                    $"[ApiService] Reserve status: {(int)response.StatusCode}");

                Debug.WriteLine(
                    $"[ApiService] Reserve response: {content}");

                // Handle unsuccessful HTTP responses.
                if (!response.IsSuccessStatusCode)
                {
                    var message = string.IsNullOrWhiteSpace(content)
                        ? $"Erro ao reservar o bilhete. HTTP {(int)response.StatusCode}."
                        : content;

                    return ApiResult<ReserveTicketResultDto>.Fail(message);
                }

                // A successful response must contain data.
                if (string.IsNullOrWhiteSpace(content))
                {
                    return ApiResult<ReserveTicketResultDto>.Fail(
                        "O servidor confirmou o pedido, mas não devolveu os dados da reserva.");
                }

                try
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<ReserveTicketResultDto>(
                        content,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (data is null)
                    {
                        return ApiResult<ReserveTicketResultDto>.Fail(
                            "Não foi possível interpretar os dados da reserva.");
                    }

                    return ApiResult<ReserveTicketResultDto>.Ok(data);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    Debug.WriteLine(
                        $"[ApiService] Invalid reserve JSON: {ex.Message}");

                    Debug.WriteLine(
                        $"[ApiService] Raw reserve response: {content}");

                    return ApiResult<ReserveTicketResultDto>.Fail(
                        "O servidor devolveu uma resposta inválida ao reservar o bilhete.");
                }
            }
            catch (TaskCanceledException)
            {
                return ApiResult<ReserveTicketResultDto>.Fail(
                    "O servidor demorou demasiado a responder.");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(
                    $"[ApiService] Reserve HTTP error: {ex.Message}");

                return ApiResult<ReserveTicketResultDto>.Fail(
                    "Erro de ligação ao servidor.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService] Reserve error: {ex}");

                return ApiResult<ReserveTicketResultDto>.Fail(
                    "Ocorreu um erro ao reservar o bilhete.");
            }
        }

        public async Task<ApiResult<PayPalOrderResultDto>> CreatePayPalOrderAsync(int ticketId)
        {
            if (!HasInternet()) return ApiResult<PayPalOrderResultDto>.Fail("Sem ligação à Internet.");
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var response = await _httpClient.PostAsJsonAsync("api/booking/paypal/create", new { TicketId = ticketId }, cts.Token);
                var data = await response.Content.ReadFromJsonAsync<PayPalOrderResultDto>(cancellationToken: cts.Token);
                if (response.IsSuccessStatusCode && data != null) return ApiResult<PayPalOrderResultDto>.Ok(data);
                return ApiResult<PayPalOrderResultDto>.Fail(data?.Message ?? "Não foi possível iniciar o pagamento.");
            }
            catch (Exception ex) { Debug.WriteLine($"[ApiService] PayPal create: {ex.Message}"); return ApiResult<PayPalOrderResultDto>.Fail("Não foi possível comunicar com o PayPal."); }
        }

        public async Task<ApiResult<PaymentCaptureResultDto>> CapturePayPalOrderAsync(int ticketId, string orderId)
        {
            if (!HasInternet()) return ApiResult<PaymentCaptureResultDto>.Fail("Sem ligação à Internet.");
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var response = await _httpClient.PostAsJsonAsync("api/booking/paypal/capture", new { TicketId = ticketId, OrderId = orderId }, cts.Token);
                var data = await response.Content.ReadFromJsonAsync<PaymentCaptureResultDto>(cancellationToken: cts.Token);
                if (response.IsSuccessStatusCode && data != null) return ApiResult<PaymentCaptureResultDto>.Ok(data);
                return ApiResult<PaymentCaptureResultDto>.Fail(data?.Message ?? "O pagamento ainda não foi confirmado.");
            }
            catch (Exception ex) { Debug.WriteLine($"[ApiService] PayPal capture: {ex.Message}"); return ApiResult<PaymentCaptureResultDto>.Fail("Não foi possível confirmar o pagamento."); }
        }

        /// <summary>
        /// Obtém o cartão de embarque já existente para um bilhete.
        /// </summary>
        public Task<ApiResult<BoardingPassDto>> GetBoardingPassAsync(int ticketId)
            => GetAsync<BoardingPassDto>($"api/checkin/{ticketId}");


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

        public async Task<EmployeeCheckInResultDto> EmployeeCheckInByNameAsync(string passengerName, string? flightNumber = null, string? documentNumber = null)
        {
            if (!HasInternet())
                return new EmployeeCheckInResultDto { Success = false, ErrorMessage = "Sem ligação à Internet." };

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var response = await _httpClient.PostAsJsonAsync(
                    "api/employee/checkin/by-name",
                    new EmployeeNameCheckInRequestDto
                    {
                        PassengerName = passengerName,
                        FlightNumber = flightNumber,
                        DocumentNumber = documentNumber
                    },
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
                Debug.WriteLine($"[ApiService] Employee check-in por nome: {ex.Message}");
                return new EmployeeCheckInResultDto { Success = false, ErrorMessage = "Erro de ligação ao servidor." };
            }
        }

        public Task<ApiResult<EmployeeOperationsSummaryDto>> GetEmployeeOperationsSummaryAsync()
            => GetAsync<EmployeeOperationsSummaryDto>("api/employee/summary");


        public Task<ApiResult<EmployeeFlightOperationDto>>
             GetEmployeeFlightOperationAsync(int flightId)
        {
            return GetAsync<EmployeeFlightOperationDto>(
                $"api/employee/flights/{flightId}/operation");
        }

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

        public Task<ApiResult<UnreadCountDto>> GetUnreadNotificationCountAsync()
            => GetAsync<UnreadCountDto>("api/notifications/unread-count");

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


        public async Task<ApiResult<MbWayPaymentResultDto>>
            PayWithMbWayAsync(
                int ticketId,
                string phoneNumber)
        {
            if (!HasInternet())
            {
                return ApiResult<MbWayPaymentResultDto>.Fail(
                    "Sem ligação à Internet.");
            }

            try
            {
                using var cts =
                    new CancellationTokenSource(
                        TimeSpan.FromSeconds(15));

                var request = new MbWayPaymentRequestDto
                {
                    TicketId = ticketId,
                    PhoneNumber = phoneNumber
                };

                var response =
                    await _httpClient.PostAsJsonAsync(
                        "api/booking/mbway/pay",
                        request,
                        cts.Token);

                var content =
                    await response.Content.ReadAsStringAsync(
                        cts.Token);

                Debug.WriteLine(
                    $"[ApiService] MB WAY status: " +
                    $"{(int)response.StatusCode}");

                Debug.WriteLine(
                    $"[ApiService] MB WAY response: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage =
                      string.IsNullOrWhiteSpace(content)
                          ? $"Não foi possível efetuar o pagamento. HTTP {(int)response.StatusCode}."
                          : content;

                    return ApiResult<MbWayPaymentResultDto>
                        .Fail(errorMessage);
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    return ApiResult<MbWayPaymentResultDto>
                        .Fail(
                            "O servidor não devolveu os dados do pagamento.");
                }

                var data =
                    System.Text.Json.JsonSerializer
                        .Deserialize<MbWayPaymentResultDto>(
                            content,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                if (data == null)
                {
                    return ApiResult<MbWayPaymentResultDto>
                        .Fail(
                            "Não foi possível interpretar o pagamento.");
                }

                return ApiResult<MbWayPaymentResultDto>.Ok(data);
            }
            catch (TaskCanceledException)
            {
                return ApiResult<MbWayPaymentResultDto>.Fail(
                    "O servidor demorou demasiado a responder.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService] MB WAY error: {ex}");

                return ApiResult<MbWayPaymentResultDto>.Fail(
                    "Erro de ligação ao servidor.");
            }
        }


        // ═══════════════════════════════════════════════════════════
        // COMUNICAÇÕES DO VOO - FUNCIONÁRIO
        // ═══════════════════════════════════════════════════════════

        public async Task<ApiResult<FlightCommunicationResultDto>>
            SendFlightCommunicationAsync(
                int flightId,
                FlightCommunicationRequestDto request)
        {
            if (!HasInternet())
            {
                return ApiResult<FlightCommunicationResultDto>.Fail(
                    "Sem ligação à Internet.");
            }

            try
            {
                using var cts =
                    new CancellationTokenSource(
                        TimeSpan.FromSeconds(15));

                var response =
                    await _httpClient.PostAsJsonAsync(
                        $"api/employee/flights/{flightId}/communications",
                        request,
                        cts.Token);

                var content =
                    await response.Content.ReadAsStringAsync(
                        cts.Token);

                Debug.WriteLine(
                    $"[ApiService] Communication status: " +
                    $"{(int)response.StatusCode}");

                Debug.WriteLine(
                    $"[ApiService] Communication response: " +
                    $"{content}");

                // ERRO DO BACKEND
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage =
                        "Não foi possível enviar a comunicação.";

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        try
                        {
                            var error =
                                System.Text.Json.JsonSerializer
                                    .Deserialize<ErrorResponseDto>(
                                        content,
                                        new System.Text.Json.JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        });

                            errorMessage =
                                error?.Message ??
                                errorMessage;
                        }
                        catch
                        {
                            // Mantém a mensagem padrão.
                        }
                    }

                    return ApiResult<FlightCommunicationResultDto>
                        .Fail(errorMessage);
                }

                // RESPOSTA VAZIA
                if (string.IsNullOrWhiteSpace(content))
                {
                    return ApiResult<FlightCommunicationResultDto>
                        .Fail(
                            "O servidor não devolveu o resultado da comunicação.");
                }

                // CONVERTER JSON
                var data =
                    System.Text.Json.JsonSerializer
                        .Deserialize<FlightCommunicationResultDto>(
                            content,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                if (data == null)
                {
                    return ApiResult<FlightCommunicationResultDto>
                        .Fail(
                            "Não foi possível interpretar a resposta do servidor.");
                }

                return ApiResult<FlightCommunicationResultDto>
                    .Ok(data);
            }
            catch (TaskCanceledException)
            {
                return ApiResult<FlightCommunicationResultDto>
                    .Fail(
                        "O servidor demorou demasiado tempo a responder.");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(
                    $"[ApiService] Communication HTTP error: " +
                    $"{ex.Message}");

                return ApiResult<FlightCommunicationResultDto>
                    .Fail(
                        "Erro de ligação ao servidor.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[ApiService] Communication error: {ex}");

                return ApiResult<FlightCommunicationResultDto>
                    .Fail(
                        "Ocorreu um erro ao enviar a comunicação.");
            }
        }
    }
}
