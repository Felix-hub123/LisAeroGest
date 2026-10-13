using LisAeroGest.AirlineMock.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace LisAeroGest.AirlineMock.Controllers
{
    [ApiController]
    [Route("api/flights")]
    public class FlightsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<ExternalFlightDto>> GetFlights()
        {
            var now = DateTimeOffset.UtcNow;

            var tomorrow = new DateTimeOffset(
                now.Year,
                now.Month,
                now.Day,
                0,
                0,
                0,
                TimeSpan.Zero
            ).AddDays(1);

            var flights = new List<ExternalFlightDto>
            {
                new ExternalFlightDto
                {
                    AirlineCode = "TP",
                    FlightNumber = "TP1234",
                    ScheduledDepartureUtc = tomorrow.AddHours(10),
                    Status = "Scheduled",
                    EstimatedDepartureUtc = null,
                    UpdatedAtUtc = now
                },
                new ExternalFlightDto
                {
                    AirlineCode = "TP",
                    FlightNumber = "TP5678",
                    ScheduledDepartureUtc = tomorrow.AddHours(14),
                    Status = "Delayed",
                    EstimatedDepartureUtc = tomorrow
                        .AddHours(14)
                        .AddMinutes(45),
                    UpdatedAtUtc = now
                }
            };

            return Ok(flights);
        }
    }
}