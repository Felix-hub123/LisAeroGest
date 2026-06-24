using LisAeroGest.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDb(DataContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            await SeedRolesAsync();
            await SeedAdminAsync();
            await SeedAirportsAsync();
            await SeedAirlinesAsync();
            await SeedGatesAsync();
            await SeedAircraftsAsync();
        }

        // ─── Roles ───────────────────────────────────────────────────────────
        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "Employee", "Passenger" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ─── Admin ───────────────────────────────────────────────────────────
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

        // ─── Aeroportos ──────────────────────────────────────────────────────
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

        // ─── Companhias Aéreas ───────────────────────────────────────────────
        private async Task SeedAirlinesAsync()
        {
            if (await _context.Airlines.AnyAsync()) return;

            var airlines = new List<Airline>
            {
                new Airline
                {
                    Name = "TAP Air Portugal",
                    IATACode = "TP",
                    Country = "Portugal"
                },
                new Airline
                {
                    Name = "Ryanair",
                    IATACode = "FR",
                    Country = "Irlanda"
                },
                new Airline
                {
                    Name = "EasyJet",
                    IATACode = "U2",
                    Country = "Reino Unido"
                },
                new Airline
                {
                    Name = "Iberia",
                    IATACode = "IB",
                    Country = "Espanha"
                },
                new Airline
                {
                    Name = "Lufthansa",
                    IATACode = "LH",
                    Country = "Alemanha"
                }
            };

            await _context.Airlines.AddRangeAsync(airlines);
            await _context.SaveChangesAsync();
        }

        // ─── Gates ───────────────────────────────────────────────────────────
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

        // ─── Aeronaves ───────────────────────────────────────────────────────
        private async Task SeedAircraftsAsync()
        {
            if (await _context.Aircrafts.AnyAsync()) return;

            var aircrafts = new List<Aircraft>
            {
                new Aircraft
                {
                    Brand = "Airbus",
                    Model = "A320neo",
                    EconomySeats = 150,
                    BusinessSeats = 12,
                    IsAvailable = true
                },
                new Aircraft
                {
                    Brand = "Airbus",
                    Model = "A321LR",
                    EconomySeats = 180,
                    BusinessSeats = 16,
                    IsAvailable = true
                },
                new Aircraft
                {
                    Brand = "Boeing",
                    Model = "737-800",
                    EconomySeats = 162,
                    BusinessSeats = 0,
                    IsAvailable = true
                },
                new Aircraft
                {
                    Brand = "Boeing",
                    Model = "787-9 Dreamliner",
                    EconomySeats = 210,
                    BusinessSeats = 30,
                    IsAvailable = true
                },
                new Aircraft
                {
                    Brand = "Airbus",
                    Model = "A330-900neo",
                    EconomySeats = 260,
                    BusinessSeats = 32,
                    IsAvailable = true
                }
            };

            await _context.Aircrafts.AddRangeAsync(aircrafts);
            await _context.SaveChangesAsync();
        }

    }
}
