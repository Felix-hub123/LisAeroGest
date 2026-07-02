using LisAeroGest.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data
{
    /// <summary>
    /// Classe responsável por popular a base de dados com dados iniciais,
    /// incluindo roles, utilizadores, aeroportos, companhias aéreas, gates,
    /// aeronaves e voos de exemplo.
    /// </summary>
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Inicializa o SeedDb com as dependências necessárias.
        /// </summary>
        public SeedDb(DataContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Método principal que executa todos os seeds por ordem.
        /// </summary>
        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            await SeedRolesAsync();
            await SeedAdminAsync();
            await SeedAirportsAsync();
            await SeedAirlinesAsync();
            await SeedGatesAsync();
            await SeedAircraftsAsync();
            await SeedFlightsAsync();
        }

        /// <summary>
        /// Cria as roles do sistema se ainda não existirem.
        /// </summary>
        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "Employee", "Passenger" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        /// <summary>
        /// Cria o utilizador administrador padrão se ainda não existir.
        /// </summary>
        private async Task SeedAdminAsync()
        {
            var email = "admin@lisaerogest.pt";
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    FirstName = "Admin",
                    LastName = "LisAeroGest",
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    IsPasswordSet = true
                };

                var result = await _userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(user, "Admin");
            }
        }

        /// <summary>
        /// Cria os aeroportos iniciais se ainda não existirem.
        /// </summary>
        private async Task SeedAirportsAsync()
        {
            if (await _context.Airports.AnyAsync()) return;

            var airports = new List<Airport>
            {
                new Airport
                {
                    Name = "Aeroporto Humberto Delgado",
                    City = "Lisboa",
                    Country = "Portugal",
                    IATACode = "LIS",
                    DefaultFee = 25.00m
                },
                new Airport
                {
                    Name = "Aeroporto Francisco Sá Carneiro",
                    City = "Porto",
                    Country = "Portugal",
                    IATACode = "OPO",
                    DefaultFee = 20.00m
                },
                new Airport
                {
                    Name = "Aeroporto de Faro",
                    City = "Faro",
                    Country = "Portugal",
                    IATACode = "FAO",
                    DefaultFee = 18.00m
                },
                new Airport
                {
                    Name = "Adolfo Suárez Madrid-Barajas",
                    City = "Madrid",
                    Country = "Espanha",
                    IATACode = "MAD",
                    DefaultFee = 30.00m
                },
                new Airport
                {
                    Name = "Aeroporto de Paris Charles de Gaulle",
                    City = "Paris",
                    Country = "França",
                    IATACode = "CDG",
                    DefaultFee = 40.00m
                },
                new Airport
                {
                    Name = "Aeroporto de Londres Heathrow",
                    City = "Londres",
                    Country = "Reino Unido",
                    IATACode = "LHR",
                    DefaultFee = 45.00m
                },
                new Airport
                {
                    Name = "Aeroporto de Frankfurt",
                    City = "Frankfurt",
                    Country = "Alemanha",
                    IATACode = "FRA",
                    DefaultFee = 38.00m
                },
                new Airport
                {
                    Name = "Aeroporto de Amesterdão Schiphol",
                    City = "Amesterdão",
                    Country = "Países Baixos",
                    IATACode = "AMS",
                    DefaultFee = 35.00m
                }
            };

            await _context.Airports.AddRangeAsync(airports);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cria as companhias aéreas iniciais se ainda não existirem.
        /// </summary>
        private async Task SeedAirlinesAsync()
        {
            if (await _context.Airlines.AnyAsync()) return;

            var airlines = new List<Airline>
            {
                new Airline { Name = "TAP Air Portugal", IATACode = "TP", Country = "Portugal" },
                new Airline { Name = "Ryanair",          IATACode = "FR", Country = "Irlanda" },
                new Airline { Name = "EasyJet",          IATACode = "U2", Country = "Reino Unido" },
                new Airline { Name = "Iberia",           IATACode = "IB", Country = "Espanha" },
                new Airline { Name = "Lufthansa",        IATACode = "LH", Country = "Alemanha" }
            };

            await _context.Airlines.AddRangeAsync(airlines);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cria os gates dos dois terminais se ainda não existirem.
        /// </summary>
        private async Task SeedGatesAsync()
        {
            if (await _context.Gates.AnyAsync()) return;

            var gates = new List<Gate>();

            // Terminal 1 — Gates A01 a A10
            for (int i = 1; i <= 10; i++)
                gates.Add(new Gate
                {
                    GateNumber = $"A{i:D2}",
                    Terminal = "Terminal 1",
                    Status = "Available"
                });

            // Terminal 2 — Gates B01 a B10
            for (int i = 1; i <= 10; i++)
                gates.Add(new Gate
                {
                    GateNumber = $"B{i:D2}",
                    Terminal = "Terminal 2",
                    Status = "Available"
                });

            await _context.Gates.AddRangeAsync(gates);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cria as aeronaves da frota se ainda não existirem.
        /// </summary>
        private async Task SeedAircraftsAsync()
        {
            if (await _context.Aircrafts.AnyAsync()) return;

            var aircrafts = new List<Aircraft>
            {
                new Aircraft { Brand = "Airbus", Model = "A320neo",          EconomySeats = 150, BusinessSeats = 12, IsAvailable = true },
                new Aircraft { Brand = "Airbus", Model = "A321LR",           EconomySeats = 180, BusinessSeats = 16, IsAvailable = true },
                new Aircraft { Brand = "Boeing", Model = "737-800",          EconomySeats = 162, BusinessSeats = 0,  IsAvailable = true },
                new Aircraft { Brand = "Boeing", Model = "787-9 Dreamliner", EconomySeats = 210, BusinessSeats = 30, IsAvailable = true },
                new Aircraft { Brand = "Airbus", Model = "A330-900neo",      EconomySeats = 260, BusinessSeats = 32, IsAvailable = true }
            };

            await _context.Aircrafts.AddRangeAsync(aircrafts);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cria voos de exemplo para o dia atual se ainda não existirem.
        /// Os voos respeitam as regras de negócio: origem diferente do destino,
        /// associados a companhias, aeronaves e gates reais do seed.
        /// </summary>
        private async Task SeedFlightsAsync()
        {
            if (await _context.Flights.AnyAsync()) return;

            // Obtém aeroportos
            var lis = await _context.Airports.FirstOrDefaultAsync(a => a.IATACode == "LIS");
            var opo = await _context.Airports.FirstOrDefaultAsync(a => a.IATACode == "OPO");
            var mad = await _context.Airports.FirstOrDefaultAsync(a => a.IATACode == "MAD");
            var cdg = await _context.Airports.FirstOrDefaultAsync(a => a.IATACode == "CDG");
            var lhr = await _context.Airports.FirstOrDefaultAsync(a => a.IATACode == "LHR");
            var fra = await _context.Airports.FirstOrDefaultAsync(a => a.IATACode == "FRA");
            var ams = await _context.Airports.FirstOrDefaultAsync(a => a.IATACode == "AMS");

            // Obtém companhias aéreas
            var tap = await _context.Airlines.FirstOrDefaultAsync(a => a.IATACode == "TP");
            var ryanair = await _context.Airlines.FirstOrDefaultAsync(a => a.IATACode == "FR");
            var easyjet = await _context.Airlines.FirstOrDefaultAsync(a => a.IATACode == "U2");
            var iberia = await _context.Airlines.FirstOrDefaultAsync(a => a.IATACode == "IB");
            var lufthansa = await _context.Airlines.FirstOrDefaultAsync(a => a.IATACode == "LH");

            // Obtém aeronaves
            var a320 = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Model == "A320neo");
            var a321 = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Model == "A321LR");
            var b737 = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Model == "737-800");
            var b787 = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Model == "787-9 Dreamliner");
            var a330 = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Model == "A330-900neo");

            // Obtém gates
            var gateA01 = await _context.Gates.FirstOrDefaultAsync(g => g.GateNumber == "A01");
            var gateA02 = await _context.Gates.FirstOrDefaultAsync(g => g.GateNumber == "A02");
            var gateA03 = await _context.Gates.FirstOrDefaultAsync(g => g.GateNumber == "A03");
            var gateB01 = await _context.Gates.FirstOrDefaultAsync(g => g.GateNumber == "B01");
            var gateB02 = await _context.Gates.FirstOrDefaultAsync(g => g.GateNumber == "B02");
            var gateB03 = await _context.Gates.FirstOrDefaultAsync(g => g.GateNumber == "B03");

            // Garante que os dados essenciais existem
            if (lis == null || tap == null || a320 == null) return;

            var today = DateTime.Today;

            var flights = new List<Flight>
            {
                // ─── Partidas de Lisboa ───────────────────────────────────
                new Flight
                {
                    FlightNumber = "TP401",
                    AirlineId = tap.Id,
                    OriginAirportId = lis.Id,
                    DestinationAirportId = opo!.Id,
                    AircraftId = a320.Id,
                    GateId = gateA01!.Id,
                    DepartureTime = today.AddHours(7).AddMinutes(30),
                    ArrivalTime = today.AddHours(8).AddMinutes(30),
                    BasePrice = 89.99m,
                    Status = "Departed"
                },
                new Flight
                {
                    FlightNumber = "TP531",
                    AirlineId = tap.Id,
                    OriginAirportId = lis.Id,
                    DestinationAirportId = cdg!.Id,
                    AircraftId = a321!.Id,
                    GateId = gateA02!.Id,
                    DepartureTime = today.AddHours(9).AddMinutes(15),
                    ArrivalTime = today.AddHours(12).AddMinutes(45),
                    BasePrice = 149.99m,
                    Status = "Departed"
                },
                new Flight
                {
                    FlightNumber = "FR1234",
                    AirlineId = ryanair!.Id,
                    OriginAirportId = lis.Id,
                    DestinationAirportId = mad!.Id,
                    AircraftId = b737!.Id,
                    GateId = gateB01!.Id,
                    DepartureTime = today.AddHours(10).AddMinutes(0),
                    ArrivalTime = today.AddHours(11).AddMinutes(45),
                    BasePrice = 49.99m,
                    Status = "CheckIn"
                },
                new Flight
                {
                    FlightNumber = "U2441",
                    AirlineId = easyjet!.Id,
                    OriginAirportId = lis.Id,
                    DestinationAirportId = lhr!.Id,
                    AircraftId = a320.Id,
                    GateId = gateB02!.Id,
                    DepartureTime = today.AddHours(11).AddMinutes(30),
                    ArrivalTime = today.AddHours(13).AddMinutes(50),
                    BasePrice = 79.99m,
                    Status = "Boarding"
                },
                new Flight
                {
                    FlightNumber = "LH1810",
                    AirlineId = lufthansa!.Id,
                    OriginAirportId = lis.Id,
                    DestinationAirportId = fra!.Id,
                    AircraftId = b787!.Id,
                    GateId = gateA03!.Id,
                    DepartureTime = today.AddHours(13).AddMinutes(0),
                    ArrivalTime = today.AddHours(16).AddMinutes(30),
                    BasePrice = 199.99m,
                    Status = "Scheduled"
                },
                new Flight
                {
                    FlightNumber = "TP781",
                    AirlineId = tap.Id,
                    OriginAirportId = lis.Id,
                    DestinationAirportId = ams!.Id,
                    AircraftId = a330!.Id,
                    GateId = gateB03!.Id,
                    DepartureTime = today.AddHours(15).AddMinutes(45),
                    ArrivalTime = today.AddHours(19).AddMinutes(15),
                    BasePrice = 179.99m,
                    Status = "Scheduled"
                },
                new Flight
                {
                    FlightNumber = "IB3210",
                    AirlineId = iberia!.Id,
                    OriginAirportId = lis.Id,
                    DestinationAirportId = mad.Id,
                    AircraftId = a321.Id,
                    GateId = null,
                    DepartureTime = today.AddHours(18).AddMinutes(20),
                    ArrivalTime = today.AddHours(20).AddMinutes(5),
                    BasePrice = 129.99m,
                    Status = "Delayed",
                    DelayedDepartureTime = today.AddHours(19).AddMinutes(30)
                },

                // ─── Chegadas a Lisboa ────────────────────────────────────
                new Flight
                {
                    FlightNumber = "TP402",
                    AirlineId = tap.Id,
                    OriginAirportId = opo.Id,
                    DestinationAirportId = lis.Id,
                    AircraftId = a320.Id,
                    GateId = gateA01.Id,
                    DepartureTime = today.AddHours(6).AddMinutes(0),
                    ArrivalTime = today.AddHours(7).AddMinutes(0),
                    BasePrice = 89.99m,
                    Status = "Departed"
                },
                new Flight
                {
                    FlightNumber = "FR5678",
                    AirlineId = ryanair.Id,
                    OriginAirportId = mad.Id,
                    DestinationAirportId = lis.Id,
                    AircraftId = b737.Id,
                    GateId = gateB01.Id,
                    DepartureTime = today.AddHours(8).AddMinutes(30),
                    ArrivalTime = today.AddHours(10).AddMinutes(15),
                    BasePrice = 49.99m,
                    Status = "Departed"
                },
                new Flight
                {
                    FlightNumber = "LH1811",
                    AirlineId = lufthansa.Id,
                    OriginAirportId = fra.Id,
                    DestinationAirportId = lis.Id,
                    AircraftId = b787.Id,
                    GateId = gateA02.Id,
                    DepartureTime = today.AddHours(10).AddMinutes(0),
                    ArrivalTime = today.AddHours(13).AddMinutes(30),
                    BasePrice = 199.99m,
                    Status = "Scheduled"
                },
                new Flight
                {
                    FlightNumber = "U2442",
                    AirlineId = easyjet.Id,
                    OriginAirportId = lhr.Id,
                    DestinationAirportId = lis.Id,
                    AircraftId = a320.Id,
                    GateId = gateB02.Id,
                    DepartureTime = today.AddHours(12).AddMinutes(0),
                    ArrivalTime = today.AddHours(14).AddMinutes(20),
                    BasePrice = 79.99m,
                    Status = "Scheduled"
                }
            };

            await _context.Flights.AddRangeAsync(flights);
            await _context.SaveChangesAsync();
        }

    }
}
