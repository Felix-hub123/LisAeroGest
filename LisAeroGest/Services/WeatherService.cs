using LisAeroGest.Data.Entities;
using System.Text.Json;

namespace LisAeroGest.Services
{
    public class WeatherService
    {
        /// <summary>
        /// Serviço responsável por obter dados meteorológicos da API OpenWeatherMap.
        /// </summary>

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa o WeatherService com as dependências necessárias.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP para chamadas à API.</param>
        /// <param name="configuration">Configuração para ler a API key.</param>
        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        /// <summary>
        /// Obtém os dados meteorológicos atuais para uma cidade.
        /// </summary>
        /// <param name="city">Nome da cidade.</param>
        /// <returns>Objeto com os dados do tempo ou null em caso de erro.</returns>
        public async Task<WeatherData?> GetWeatherAsync(string city)
        {
            var apiKey = _configuration["OpenWeatherMap:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return null;

            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&units=metric&lang=pt&appid={apiKey}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<WeatherData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }
    }
}
