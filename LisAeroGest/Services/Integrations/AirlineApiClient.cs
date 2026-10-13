using LisAeroGest.Models.Integrations;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LisAeroGest.Services.Integrations
{
    public class AirlineApiClient
    {
        private readonly HttpClient _httpClient;

        public AirlineApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ExternalFlightDto>> GetFlightsAsync(
            CancellationToken cancellationToken = default)
        {
            var flights = await _httpClient
                .GetFromJsonAsync<List<ExternalFlightDto>>(
                    "api/flights",
                    cancellationToken);

            if (flights is null)
            {
                throw new InvalidOperationException(
                    "A companhia devolveu uma resposta JSON nula.");
            }

            return flights;
        }
    }
}