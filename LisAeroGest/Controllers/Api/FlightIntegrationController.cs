using LisAeroGest.Services.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LisAeroGest.Controllers.Api
{
    [ApiController]
    [Route("api/integration/flights")]
    [Authorize(Roles = "Admin,Employee")]
    public class FlightIntegrationController : ControllerBase
    {
        private readonly AirlineApiClient _airlineApiClient;
        private readonly ILogger<FlightIntegrationController> _logger;

        public FlightIntegrationController(
            AirlineApiClient airlineApiClient,
            ILogger<FlightIntegrationController> logger)
        {
            _airlineApiClient = airlineApiClient;
            _logger = logger;
        }

        [HttpGet("external")]
        public async Task<IActionResult> GetExternalFlights(
            CancellationToken cancellationToken)
        {
            try
            {
                var flights = await _airlineApiClient
                    .GetFlightsAsync(cancellationToken);

                _logger.LogInformation(
                    "Consulta à companhia concluída: {FlightCount} voos.",
                    flights.Count);

                return Ok(flights);
            }
            catch (OperationCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "Tempo de espera excedido ao consultar a companhia.");

                return Problem(
                    statusCode: StatusCodes.Status504GatewayTimeout,
                    title: "A companhia não respondeu a tempo.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Falha HTTP ao consultar a companhia.");

                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Não foi possível consultar a companhia.");
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    "A companhia devolveu JSON inválido.");

                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "A resposta da companhia tem um formato inválido.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(
                    ex,
                    "Não foi possível concluir a consulta à companhia.");

                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Não foi possível processar a resposta da companhia.");
            }
        }
    }
}