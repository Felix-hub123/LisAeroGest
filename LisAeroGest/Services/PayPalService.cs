using LisAeroGest.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LisAeroGest.Services
{
    /// <summary>
    /// Serviço de integração com a API REST do PayPal.
    /// </summary>
    public class PayPalService : IPayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PayPalService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var clientId = _configuration["PayPal:ClientId"];
            var secret = _configuration["PayPal:Secret"];
            var baseUrl = _configuration["PayPal:BaseUrl"];

            var authBytes = Encoding.UTF8.GetBytes($"{clientId}:{secret}");
            var authHeader = Convert.ToBase64String(authBytes);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            });

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString()!;
        }

        /// <summary>
        /// Cria uma ordem de pagamento no PayPal e devolve o Order ID.
        /// </summary>
        public async Task<string> CreateOrderAsync(decimal amount, string currency, string ticketReference)
        {
            var token = await GetAccessTokenAsync();
            var baseUrl = _configuration["PayPal:BaseUrl"];

            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = ticketReference,
                        amount = new
                        {
                            currency_code = currency,
                            value = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("id").GetString()!;
        }

        /// <summary>
        /// Captura (finaliza) o pagamento de uma ordem já aprovada pelo comprador.
        /// Devolve true se o pagamento foi capturado com sucesso ("COMPLETED").
        /// </summary>
        public async Task<bool> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessTokenAsync();
            var baseUrl = _configuration["PayPal:BaseUrl"];

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders/{orderId}/capture");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var status = doc.RootElement.GetProperty("status").GetString();

            return status == "COMPLETED";
        }
    }
}

