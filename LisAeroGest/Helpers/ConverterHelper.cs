using LisAeroGest.Data.Entities;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LisAeroGest.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        public User ToUser(RegisterViewModel model)
        {
            return new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Username,
                UserName = model.Username,
                Address = model.Address,
                IsPasswordSet = true
            };
        }

        public Passenger ToPassenger(RegisterViewModel model, string userId)
        {
            return new Passenger
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                DocumentType = model.DocumentType,
                DocumentNumber = model.DocumentNumber,
                BirthDate = model.BirthDate,
                UserId = userId,
                RegistrationDate = DateTime.UtcNow
            };
        }


        public Aircraft ToAircraft(AircraftViewModel model, Guid imageId, bool isEdit = false)
        {
            return new Aircraft
            {
                Id = isEdit ? model.Id : 0,
                Brand = model.Brand,
                Model = model.Model,
                EconomySeats = model.EconomySeats,
                BusinessSeats = model.BusinessSeats,
                IsAvailable = model.IsAvailable,
                ImageId = imageId
            };
        }

        public AircraftViewModel ToAircraftViewModel(Aircraft aircraft)
        {
            return new AircraftViewModel
            {
                Id = aircraft.Id,
                Brand = aircraft.Brand!,
                Model = aircraft.Model!,
                EconomySeats = aircraft.EconomySeats,
                BusinessSeats = aircraft.BusinessSeats,
                IsAvailable = aircraft.IsAvailable,
                ImageId = aircraft.ImageId
            };
        }

        public void UpdateAircraftFromViewModel(Aircraft aircraft, AircraftViewModel model, Guid imageId)
        {
            // Atualiza apenas as propriedades operacionais
            aircraft.EconomySeats = model.EconomySeats;
            aircraft.BusinessSeats = model.BusinessSeats;
            aircraft.IsAvailable = model.IsAvailable;

            if (imageId != Guid.Empty)
            {
                aircraft.ImageId = imageId;
            }
        }


        public Airline ToAirline(AirlineViewModel model, Guid imageId, bool isEdit = false)
        {
            return new Airline
            {
                Id = isEdit ? model.Id : 0,
                Name = model.Name,
                IATACode = model.IATACode!.ToUpper(),
                Country = model.Country,
                ImageId = imageId
            };
        }

        public AirlineViewModel ToAirlineViewModel(Airline airline)
        {
            return new AirlineViewModel
            {
                Id = airline.Id,
                Name = airline.Name,
                IATACode = airline.IATACode,
                Country = airline.Country,
                ImageId = airline.ImageId
            };
        }

       

        public Airport ToAirport(AirportViewModel model, Guid imageId, bool isEdit = false)
        {
            return new Airport
            {
                Id = isEdit ? model.Id : 0,
                Name = model.Name,
                City = model.City,
                Country = model.Country,
                IATACode = model.IATACode!.ToUpper(),
                DefaultFee = model.DefaultFee,
                ImageId = imageId
            };
        }

      

    

        public BoardingPass ToBoardingPass(Ticket ticket, int sequenceNumber, string? gate = null, string prefix = "BOARDING")
        {
            var flightNumber = ticket.Flight?.FlightNumber ?? "FL";

            return new BoardingPass
            {
                TicketId = ticket.Id,
                Gate = string.IsNullOrWhiteSpace(gate) ? "TBA" : gate,
                SequenceNumber = sequenceNumber,
                IssuedAt = DateTime.UtcNow,
                QRCode = $"{prefix}-{flightNumber}-{ticket.Id}-{Guid.NewGuid().ToString()[..8]}"
            };
        }


        public Flight ToFlight(FlightViewModel model, bool isEdit)
        {
            return new Flight
            {
                Id = isEdit ? model.Id : 0,
                FlightNumber = model.FlightNumber?.ToUpper() ?? string.Empty,
                AirlineId = model.AirlineId,
                OriginAirportId = model.OriginAirportId,
                DestinationAirportId = model.DestinationAirportId,
                AircraftId = model.AircraftId,
                GateId = model.GateId,
                DepartureTime = model.DepartureTime,
                ArrivalTime = model.ArrivalTime,
                BasePrice = model.BasePrice,
                Status = model.Status
            };
        }

        public FlightViewModel ToFlightViewModel(Flight flight)
        {
            return new FlightViewModel
            {
                Id = flight.Id,
                FlightNumber = flight.FlightNumber,
                AirlineId = flight.AirlineId,
                OriginAirportId = flight.OriginAirportId,
                DestinationAirportId = flight.DestinationAirportId,
                AircraftId = flight.AircraftId,
                GateId = flight.GateId,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                BasePrice = flight.BasePrice,
                Status = flight.Status
            };
        }


      

        public IEnumerable<SelectListItem> ToComboAirlines(IEnumerable<Airline> airlines, int? selectedId = null)
        {
            return airlines.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.IATACode} — {a.Name}",
                Selected = selectedId.HasValue && a.Id == selectedId.Value
            });
        }

        public IEnumerable<SelectListItem> ToComboAirports(IEnumerable<Airport> airports, int? selectedId = null)
        {
            return airports.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.IATACode} — {a.Name}, {a.City}",
                Selected = selectedId.HasValue && a.Id == selectedId.Value
            });
        }

        public IEnumerable<SelectListItem> ToComboAircrafts(IEnumerable<Aircraft> aircrafts, int? selectedId = null)
        {
            return aircrafts.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Brand} {a.Model} ({a.EconomySeats}Eco + {a.BusinessSeats}Exec)",
                Selected = selectedId.HasValue && a.Id == selectedId.Value
            });
        }

        public IEnumerable<SelectListItem> ToComboGates(IEnumerable<Gate> gates, int? selectedId = null)
        {
            return gates.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = $"Gate {g.GateNumber} — {g.Terminal}",
                Selected = selectedId.HasValue && g.Id == selectedId.Value
            });
        }

        public IEnumerable<SelectListItem> ToComboStatuses(string? selectedStatus = null)
        {
            var statuses = new[]
            {
                ("Scheduled", "Previsto"),
                ("CheckIn", "Check-in"),
                ("Boarding", "A Embarcar"),
                ("Departed", "Partiu"),
                ("Delayed", "Atrasado"),
                ("Cancelled", "Cancelado")
            };

            return statuses.Select(s => new SelectListItem
            {
                Value = s.Item1,
                Text = s.Item2,
                Selected = s.Item1 == selectedStatus
            });
        }

        public List<Seat> GenerateSeatsFromAircraftCapacity(Aircraft? aircraft, decimal basePrice)
        {
            var seats = new List<Seat>();
            if (aircraft == null) return seats;

            int businessSeats = aircraft.BusinessSeats > 0 ? aircraft.BusinessSeats : 12;
            int economySeats = aircraft.EconomySeats > 0 ? aircraft.EconomySeats : 150;

            string[] columns = { "A", "B", "C", "D", "E", "F" };
            int row = 1;

            // Executiva
            int businessRows = (int)Math.Ceiling(businessSeats / 6.0);
            for (int r = 0; r < businessRows; r++)
            {
                foreach (var col in columns)
                {
                    if (seats.Count >= businessSeats) break;
                    seats.Add(new Seat
                    {
                        Code = $"{row}{col}",
                        SeatClass = "Business",
                        BasePrice = basePrice * 1.5m,
                        IsAvailable = true
                    });
                }
                row++;
            }

            // Económica
            int totalGoal = businessSeats + economySeats;
            while (seats.Count < totalGoal)
            {
                foreach (var col in columns)
                {
                    if (seats.Count >= totalGoal) break;
                    seats.Add(new Seat
                    {
                        Code = $"{row}{col}",
                        SeatClass = "Economy",
                        BasePrice = basePrice,
                        IsAvailable = true
                    });
                }
                row++;
            }

            return seats;
        }


        public ForumTopicViewModel ToForumTopicViewModel(ForumTopic topic)
        {
            return new ForumTopicViewModel
            {
                Id = topic.Id,
                Title = topic.Title!,
                Content = topic.Content!,
                IsClosed = topic.IsClosed,
                CreatedAt = topic.CreatedAt,
                CreatedByUserId = topic.CreatedByUserId,
                AuthorName = topic.CreatedBy?.FullName ?? "Utilizador Desconhecido",
                Comments = topic.Comments ?? new List<ForumComment>()
            };
        }

        public ForumTopic ToForumTopic(ForumTopicViewModel model, string userId, bool isEdit)
        {
            return new ForumTopic
            {
                Id = isEdit ? model.Id : 0,
                Title = model.Title,
                Content = model.Content,
                IsClosed = model.IsClosed,
                CreatedByUserId = userId,
                CreatedAt = isEdit ? model.CreatedAt : DateTime.UtcNow
            };
        }

        public ForumComment ToForumComment(int topicId, string content, string userId)
        {
            return new ForumComment
            {
                ForumTopicId = topicId,
                Content = content.Trim(),
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public GateViewModel ToGateViewModel(Gate gate)
        {
            return new GateViewModel
            {
                Id = gate.Id,
                GateNumber = gate.GateNumber,
                Terminal = gate.Terminal,
                Status = gate.Status
            };
        }

        public Gate ToGate(GateViewModel model, bool isEdit)
        {
            return new Gate
            {
                Id = isEdit ? model.Id : 0,
                GateNumber = model.GateNumber?.ToUpper() ?? string.Empty,
                Terminal = model.Terminal,
                Status = model.Status
            };
        }

        public NotificationViewModel ToNotificationViewModel(Notification notification)
        {
            return new NotificationViewModel
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title ?? string.Empty,
                Message = notification.Message ?? string.Empty,
                Link = notification.Link,
                Icon = string.IsNullOrWhiteSpace(notification.Icon) ? "bi-bell" : notification.Icon,
                ColorClass = string.IsNullOrWhiteSpace(notification.ColorClass) ? "text-primary" : notification.ColorClass,
                Type = notification.Type ?? "System",
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public IEnumerable<NotificationViewModel> ToNotificationViewModelList(IEnumerable<Notification> notifications)
        {
            return notifications.Select(ToNotificationViewModel).ToList();
        }

        public PassengerViewModel ToPassengerViewModel(Passenger passenger)
        {
            return new PassengerViewModel
            {
                Id = passenger.Id,
                FirstName = passenger.FirstName,
                LastName = passenger.LastName,
                DocumentType = passenger.DocumentType,
                DocumentNumber = passenger.DocumentNumber,
                BirthDate = passenger.BirthDate,
                UserId = passenger.UserId,
                UserEmail = passenger.User?.Email,
                ImageFullPath = passenger.ImageFullPath,
                DocumentTypes = GetDocumentTypes()
            };
        }

        public Passenger ToPassenger(PassengerViewModel model, Guid imageId, bool isEdit)
        {
            return new Passenger
            {
                Id = isEdit ? model.Id : 0,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DocumentType = model.DocumentType,
                DocumentNumber = model.DocumentNumber,
                BirthDate = model.BirthDate,
                UserId = model.UserId,
                ImageId = imageId
            };
        }

        public IEnumerable<SelectListItem> GetDocumentTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Cartão de Cidadão", Value = "CC" },
                new SelectListItem { Text = "Passaporte", Value = "Passaporte" },
                new SelectListItem { Text = "BI (Bilhete de Identidade)", Value = "BI" }
            };
        }

        public SelectList ToAirportSelectList(IEnumerable<Airport> airports, string? selectedValue = null)
        {
            return new SelectList(airports, "IATACode", "Name", selectedValue);
        }

        public void ConfirmTicketPayment(Ticket ticket, Flight? flight, string userId)
        {
            var basePrice = flight?.BasePrice ?? 0;
            var luggageFee = ticket.ExtraLuggage ? 30 : 0;
            var mealFee = ticket.MealIncluded ? 15 : 0;

            // Atualiza o bilhete existente
            ticket.TotalPrice = basePrice + luggageFee + mealFee;
            ticket.Status = "Paid";
            ticket.PurchaseDate = DateTime.UtcNow;
            ticket.ReservationExpiresAt = null; // Remove a expiração da reserva
            ticket.CreatedByUserId = userId;
        }


        public IEnumerable<SelectListItem> GetCountries(string? selectedCountry = null)
        {
            var countryList = new List<string>
            {
                "Alemanha", "Angola", "Arábia Saudita", "Argentina", "Austrália",
                "Áustria", "Bélgica", "Brasil", "Cabo Verde", "Canadá", "Catar",
                "China", "Colômbia", "Coreia do Sul", "Dinamarca", "Emirados Árabes Unidos",
                "Espanha", "Estados Unidos", "Finlândia", "França", "Grécia",
                "Holanda", "Irlanda", "Itália", "Japão", "Marrocos", "México",
                "Moçambique", "Noruega", "Nova Zelândia", "Polónia", "Portugal",
                "Reino Unido", "Singapura", "Suíça", "Suécia", "Turquia"
            };

            var list = countryList.Select(c => new SelectListItem
            {
                Text = c,
                Value = c,
                Selected = string.Equals(c, selectedCountry, StringComparison.OrdinalIgnoreCase)
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[ Selecione um País ]",
                Value = string.Empty,
                Selected = string.IsNullOrEmpty(selectedCountry)
            });

            return list;
        }

        public Ticket ToTicket(int flightId, int seatId, Passenger passenger, bool extraLuggage, bool mealIncluded, decimal price)
        {
            return new Ticket
            {
                FlightId = flightId,
                SeatId = seatId,
                PassengerId = passenger.Id,
                ExtraLuggage = extraLuggage,
                MealIncluded = mealIncluded,
                TotalPrice = price,
                Status = "Reserved", 
                CreatedByUserId = passenger.UserId,
                PurchaseDate = DateTime.UtcNow,
                ReservationExpiresAt = DateTime.UtcNow.AddMinutes(30) 
            };
        }


        public UserWithRole ToUserWithRole(User user, string role)
        {
            return new UserWithRole
            {
                User = user,
                Role = string.IsNullOrEmpty(role) ? "Sem role" : role
            };
        }

        public List<SelectListItem> ToRoleSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Funcionário", Value = "Employee" },
                new SelectListItem { Text = "Administrador", Value = "Admin" }
            };
        }

        public User ToUser(string email, string firstName, string lastName)
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                IsPasswordSet = false
            };
        }

      

        #region Dropdowns & Listas Estruturadas

        /// <summary>
        /// Devolve as principais marcas e fabricantes de aeronaves do mercado mundial.
        /// </summary>
        public IEnumerable<SelectListItem> GetAircraftBrands(string? selectedBrand = null)
        {
            var brands = new List<string>
            {
                // Aviação Comercial / Mainline
                "Airbus",
                "Boeing",
                
                // Aviação Regional / Turboprop & Regional Jets
                "Embraer",
                "ATR",
                "Bombardier",
                "De Havilland Canada",
                "Dornier",
                "COMAC",
                "Sukhoi",
                
                // Jactos Executivos e Aviação Geral
                "Gulfstream",
                "Dassault Falcon",
                "Cessna",
                "Beechcraft",
                "Piper Aircraft",
                "Pilatus",
                "Cirrus Aircraft",
                "HondaJet"
            };

            return brands
                .OrderBy(b => b)
                .Select(b => new SelectListItem
                {
                    Value = b,
                    Text = b,
                    Selected = b == selectedBrand
                });
        }

        #region Dicionário Completo de Aeroportos e IATA

        private static readonly Dictionary<string, Dictionary<string, (string AirportName, string IataCode)>> AirportsMap = new()
        {
            { "Alemanha", new() {
                { "Berlim", ("Aeroporto de Berlim-Brandremburgo", "BER") },
                { "Frankfurt", ("Aeroporto de Frankfurt", "FRA") },
                { "Munique", ("Aeroporto de Munique", "MUC") }
            }},
            { "Angola", new() {
                { "Luanda", ("Aeroporto Internacional 4 de Fevereiro", "LAD") },
                { "Benguela", ("Aeroporto de Benguela", "BUG") },
                { "Lubango", ("Aeroporto Internacional da Mukanka", "SDD") },
                { "Cabinda", ("Aeroporto de Cabinda", "CAB") }
            }},
            { "Arábia Saudita", new() {
                { "Riad", ("Aeroporto Internacional Rei Khalid", "RUH") },
                { "Jidá", ("Aeroporto Internacional Rei Abdulaziz", "JED") }
            }},
            { "Argentina", new() {
                { "Buenos Aires", ("Aeroporto Internacional Ministro Pistarini", "EZE") }
            }},
            { "Austrália", new() {
                { "Sydney", ("Aeroporto Internacional de Sydney Kingsford Smith", "SYD") },
                { "Melbourne", ("Aeroporto de Melbourne", "MEL") }
            }},
            { "Áustria", new() {
                { "Viena", ("Aeroporto Internacional de Viena", "VIE") }
            }},
            { "Bélgica", new() {
                { "Bruxelas", ("Aeroporto de Bruxelas", "BRU") }
            }},
            { "Brasil", new() {
                { "São Paulo", ("Aeroporto Internacional de Guarulhos", "GRU") },
                { "Rio de Janeiro", ("Aeroporto Internacional Tom Jobim", "GIG") },
                { "Brasília", ("Aeroporto Internacional de Brasília", "BSB") }
            }},
            { "Cabo Verde", new() {
                { "Praia", ("Aeroporto Internacional Nelson Mandela", "RAI") },
                { "Sal", ("Aeroporto Internacional Amílcar Cabral", "SID") }
            }},
            { "Canadá", new() {
                { "Toronto", ("Aeroporto Internacional Toronto Pearson", "YYZ") },
                { "Vancouver", ("Aeroporto Internacional de Vancouver", "YVR") },
                { "Montreal", ("Aeroporto Internacional Pierre Elliott Trudeau", "YUL") }
            }},
            { "Catar", new() {
                { "Doha", ("Aeroporto Internacional de Hamad", "DOH") }
            }},
            { "China", new() {
                { "Pequim", ("Aeroporto Internacional de Pequim Capital", "PEK") },
                { "Xangai", ("Aeroporto Internacional de Xangai Pudong", "PVG") }
            }},
            { "Colômbia", new() {
                { "Bogotá", ("Aeroporto Internacional El Dorado", "BOG") }
            }},
            { "Coreia do Sul", new() {
                { "Seul", ("Aeroporto Internacional de Incheon", "ICN") }
            }},
            { "Dinamarca", new() {
                { "Copenhaga", ("Aeroporto de Copenhaga", "CPH") }
            }},
            { "Emirados Árabes Unidos", new() {
                { "Dubai", ("Aeroporto Internacional do Dubai", "DXB") },
                { "Abu Dhabi", ("Aeroporto Internacional de Abu Dhabi", "AUH") }
            }},
            { "Espanha", new() {
                { "Madrid", ("Aeropuerto Adolfo Suárez Madrid-Barajas", "MAD") },
                { "Barcelona", ("Aeropuerto Josep Tarradellas Barcelona-El Prat", "BCN") }
            }},
            { "Estados Unidos", new() {
                { "Nova Iorque", ("John F. Kennedy International Airport", "JFK") },
                { "Los Angeles", ("Los Angeles International Airport", "LAX") },
                { "Miami", ("Aeroporto Internacional de Miami", "MIA") }
            }},
            { "Finlândia", new() {
                { "Helsínquia", ("Aeroporto de Helsínquia-Vantaa", "HEL") }
            }},
            { "França", new() {
                { "Paris", ("Aéroport de Paris-Charles de Gaulle", "CDG") },
                { "Nice", ("Aéroport Nice Côte d'Azur", "NCE") },
                { "Lyon", ("Aeroporto de Lyon-Saint-Exupéry", "LYS") }
            }},
            { "Grécia", new() {
                { "Atenas", ("Aeroporto Internacional de Atenas Eleftherios Venizelos", "ATH") }
            }},
            { "Holanda", new() {
                { "Amesterdão", ("Aeroporto de Amesterdão Schiphol", "AMS") }
            }},
            { "Irlanda", new() {
                { "Dublim", ("Aeroporto de Dublim", "DUB") }
            }},
            { "Itália", new() {
                { "Roma", ("Aeroporto Internacional de Roma-Fiumicino", "FCO") },
                { "Milão", ("Aeroporto de Milão-Malpensa", "MXP") }
            }},
            { "Japão", new() {
                { "Tóquio", ("Aeroporto Internacional de Narita", "NRT") },
                { "Osaka", ("Aeroporto Internacional de Kansai", "KIX") }
            }},
            { "Marrocos", new() {
                { "Casablanca", ("Aeroporto Internacional Mohammed V", "CMN") },
                { "Marraquexe", ("Aeroporto de Marraquexe-Menara", "RAK") }
            }},
            { "México", new() {
                { "Cidade do México", ("Aeroporto Internacional da Cidade do México", "MEX") }
            }},
            { "Moçambique", new() {
                { "Maputo", ("Aeroporto Internacional de Maputo", "MPM") },
                { "Beira", ("Aeroporto Internacional da Beira", "BEW") }
            }},
            { "Noruega", new() {
                { "Oslo", ("Aeroporto de Oslo Gardermoen", "OSL") }
            }},
            { "Nova Zelândia", new() {
                { "Auckland", ("Aeroporto de Auckland", "AKL") }
            }},
            { "Polónia", new() {
                { "Varsóvia", ("Aeroporto de Varsóvia-Chopin", "WAW") }
            }},
            { "Portugal", new() {
                { "Lisboa", ("Aeroporto Humberto Delgado", "LIS") },
                { "Porto", ("Aeroporto Francisco Sá Carneiro", "OPO") },
                { "Faro", ("Aeroporto Gago Coutinho", "FAO") },
                { "Funchal", ("Aeroporto Internacional da Madeira Cristiano Ronaldo", "FNC") },
                { "Ponta Delgada", ("Aeroporto João Paulo II", "PDL") }
            }},
            { "Reino Unido", new() {
                { "Londres", ("Aeroporto de Londres Heathrow", "LHR") },
                { "Manchester", ("Aeroporto de Manchester", "MAN") }
            }},
            { "Singapura", new() {
                { "Singapura", ("Aeroporto de Singapura Changi", "SIN") }
            }},
            { "Suíça", new() {
                { "Zurique", ("Aeroporto de Zurique", "ZRH") },
                { "Genebra", ("Aeroporto de Genebra", "GVA") }
            }},
            { "Suécia", new() {
                { "Estocolmo", ("Aeroporto de Estocolmo-Arlanda", "ARN") }
            }},
            { "Turquia", new() {
                { "Istambul", ("Aeroporto de Istambul", "IST") }
            }}
        };

        #endregion

        #region Dicionário Completo de Companhias Aéreas (Immutability Pattern)

        private static readonly Dictionary<string, List<(string Name, string IataCode)>> AirlinesMap = new()
        {
            { "Portugal", new() {
                ("TAP Air Portugal", "TP"),
                ("SATA Air Açores", "SP"),
                ("Azores Airlines", "S4"),
                ("euroAtlantic Airways", "YU"),
                ("Hi Fly", "5K")
            }},
            { "Espanha", new() {
                ("Iberia", "IB"),
                ("Vueling Airlines", "VY"),
                ("Air Europa", "UX"),
                ("Volotea", "V7")
            }},
            { "França", new() {
                ("Air France", "AF"),
                ("Transavia France", "TO"),
                ("Corsair", "SS")
            }},
            { "Alemanha", new() {
                ("Lufthansa", "LH"),
                ("Eurowings", "EW"),
                ("Condor", "DE")
            }},
            { "Reino Unido", new() {
                ("British Airways", "BA"),
                ("easyJet", "U2"),
                ("Virgin Atlantic", "VS"),
                ("Jet2", "LS")
            }},
            { "Estados Unidos", new() {
                ("American Airlines", "AA"),
                ("Delta Air Lines", "DL"),
                ("United Airlines", "UA")
            }},
            { "Brasil", new() {
                ("LATAM Brasil", "LA"),
                ("GOL Linhas Aéreas", "G3"),
                ("Azul Linhas Aéreas", "AD")
            }},
            { "Angola", new() {
                ("TAAG Linhas Aéreas de Angola", "DT"),
                ("Fly Angola", "EQ")
            }},
                        { "Itália", new() {
                ("ITA Airways", "AZ"),
                ("Air Dolomiti", "EN"),
                ("Neos", "NO")
            }},
            { "Turquia", new() {
                ("Turkish Airlines", "TK"),
                ("Pegasus Airlines", "PC"),
                ("SunExpress", "XQ")
            }},
            { "México", new() {
                ("Aeroméxico", "AM"),
                ("Volaris", "Y4"),
                ("VivaAerobus", "VB")
            }},
            { "China", new() {
                ("Air China", "CA"),
                ("China Southern Airlines", "CZ"),
                ("China Eastern Airlines", "MU"),
                ("Hainan Airlines", "HU")
            }},
            { "Grécia", new() {
                ("Aegean Airlines", "A3"),
                ("Sky Express", "GQ")
            }},
            { "Áustria", new() {
                ("Austrian Airlines", "OS")
            }},
            { "Japão", new() {
                ("All Nippon Airways (ANA)", "NH"),
                ("Japan Airlines (JAL)", "JL"),
                ("Peach Aviation", "MM")
            }},
            { "Emirados Árabes Unidos", new() {
                ("Emirates", "EK"),
                ("Etihad Airways", "EY"),
                ("flydubai", "FZ"),
                ("Air Arabia", "G9")
            }},
            { "Catar", new() {
                ("Qatar Airways", "QR")
            }},
            { "Tailândia", new() {
                ("Thai Airways", "TG"),
                ("Bangkok Airways", "PG"),
                ("Thai AirAsia", "FD")
            }}
        };

        /// <summary>
        /// Converte as chaves do dicionário de países numa lista para dropdown (SelectListItem).
        /// </summary>
        public IEnumerable<SelectListItem> GetCountries()
        {
            var countries = AirlinesMap.Keys.OrderBy(c => c).ToList();

            var list = new List<SelectListItem>
            {
                       new SelectListItem { Text = "-- Selecione o País --", Value = "" }
            };

            list.AddRange(countries.Select(c => new SelectListItem
            {
                Text = c,
                Value = c
            }));

            return list;
        }

        /// <summary>
        /// Retorna a lista de companhias filtradas pelo país em formato JSON para o AJAX.
        /// </summary>
        public IEnumerable<object> GetAirlinesByCountry(string? country)
        {
            if (!string.IsNullOrEmpty(country) && AirlinesMap.TryGetValue(country, out var airlines))
            {
                return airlines.Select(a => new
                {
                    name = a.Name,
                    iata = a.IataCode
                }).OrderBy(a => a.name);
            }

            return Enumerable.Empty<object>();
        }

        #endregion



        public IEnumerable<object> GetCitiesWithIata(string? selectedCountry = null)
        {
            if (!string.IsNullOrEmpty(selectedCountry) && AirportsMap.TryGetValue(selectedCountry, out var countryCities))
            {
                return countryCities.Select(c => new
                {
                    value = c.Key,
                    text = c.Key,
                    airport = c.Value.AirportName,
                    iata = c.Value.IataCode
                }).OrderBy(c => c.text);
            }

            return AirportsMap.Values
                .SelectMany(c => c)
                .DistinctBy(c => c.Key)
                .Select(c => new
                {
                    value = c.Key,
                    text = c.Key,
                    airport = c.Value.AirportName,
                    iata = c.Value.IataCode
                })
                .OrderBy(c => c.text);
        }


        public IEnumerable<SelectListItem> GetAircraftModels(string? selectedBrand = null, string? selectedModel = null)
        {
            var modelsMap = new Dictionary<string, List<string>>
            {
                { "Airbus", new List<string> { "A220-100", "A220-300", "A319neo", "A320neo", "A321neo", "A321XLR", "A330-900neo", "A350-900", "A350-1000", "A380-800" } },
                { "Boeing", new List<string> { "737-800", "737 MAX 8", "737 MAX 9", "767-300ER", "777-300ER", "777X", "787-8 Dreamliner", "787-9 Dreamliner", "787-10 Dreamliner" } },
                { "Embraer", new List<string> { "E175", "E190-E2", "E195-E2", "Phenom 100EV", "Phenom 300E", "Praetor 500", "Praetor 600" } },
                { "ATR", new List<string> { "ATR 42-600", "ATR 72-600" } },
                { "Bombardier", new List<string> { "CRJ-900", "CRJ-1000", "Challenger 350", "Challenger 650", "Global 7500", "Global 8000" } },
                { "De Havilland Canada", new List<string> { "Dash 8-Q400" } },
                { "Dornier", new List<string> { "Do 228", "Do 328" } },
                { "COMAC", new List<string> { "ARJ21", "C919", "C929" } },
                { "Sukhoi", new List<string> { "Superjet 100" } },
                { "Gulfstream", new List<string> { "G280", "G500", "G600", "G650ER", "G700", "G800" } },
                { "Dassault Falcon", new List<string> { "Falcon 2000LXS", "Falcon 6X", "Falcon 8X", "Falcon 10X" } },
                { "Cessna", new List<string> { "172 Skyhawk", "208 Grand Caravan", "Citation CJ4", "Citation Latitude", "Citation Longitude" } },
                { "Beechcraft", new List<string> { "King Air 260", "King Air 360" } },
                { "Piper Aircraft", new List<string> { "PA-28 Archer", "M600SLS" } },
                { "Pilatus", new List<string> { "PC-12 NGX", "PC-24" } },
                { "Cirrus Aircraft", new List<string> { "SR22T", "Vision Jet SF50" } },
                { "HondaJet", new List<string> { "HA-420 HondaJet Elite II" } }
            };

            List<string> models;

            if (!string.IsNullOrEmpty(selectedBrand) && modelsMap.TryGetValue(selectedBrand, out var brandModels))
            {
                models = brandModels;
            }
            else
            {
                models = modelsMap.Values.SelectMany(m => m).Distinct().ToList();
            }

            return models
                .OrderBy(m => m)
                .Select(m => new SelectListItem
                {
                    Value = m,
                    Text = m,
                    Selected = m == selectedModel
                });
        }

        /// <summary>
        /// Devolve cidades agrupadas pelos respetivos países para SelectListItem.
        /// </summary>
        public IEnumerable<SelectListItem> GetCities(string? selectedCountry = null, string? selectedCity = null)
        {
            IEnumerable<string> cities;

            // Procura no AirportsMap em vez de CitiesIataMap
            if (!string.IsNullOrEmpty(selectedCountry) && AirportsMap.TryGetValue(selectedCountry, out var countryCities))
            {
                cities = countryCities.Keys;
            }
            else
            {
                // Se não houver país selecionado, junta todas as cidades únicas de todos os países
                cities = AirportsMap.Values.SelectMany(c => c.Keys).Distinct();
            }

            var list = cities
                .OrderBy(c => c)
                .Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c,
                    Selected = string.Equals(c, selectedCity, StringComparison.OrdinalIgnoreCase)
                }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[ Selecione uma Cidade ]",
                Value = string.Empty,
                Selected = string.IsNullOrEmpty(selectedCity)
            });

            return list;
        }



       

        public AirportViewModel ToAirportViewModel(Airport airport)
        {
            return new AirportViewModel
            {
                Id = airport.Id,
                Name = airport.Name ?? string.Empty,
                IATACode = airport.IATACode ?? string.Empty,
                City = airport.City ?? string.Empty,
                Country = airport.Country ?? string.Empty,
                DefaultFee = airport.DefaultFee,
                ImageId = airport.ImageId
            };
        }

        /// <summary>
        /// Atualiza apenas os dados operacionais de um Aeroporto existente (Edit).
        /// Protege Name, IATACode, City e Country contra sobrescrita indevida.
        /// </summary>
        public void UpdateAirlineFromViewModel(Airline airline, AirlineViewModel model, Guid imageId)
        {
            airline.Name = model.Name;
            airline.IATACode = model.IATACode!.ToUpper();
            airline.Country = model.Country;
            airline.ImageId = imageId;
        }

      

        public void UpdateAirportFromViewModel(Airport airport, AirportViewModel model, Guid imageId)
        {
            // Apenas atualizamos a taxa operacional e a imagem na edição
            airport.DefaultFee = model.DefaultFee;
            airport.ImageId = imageId;
        }


        #endregion

        /// <summary>
        /// Devolve o texto em português correspondente ao estado do voo.
        /// </summary>
        /// <param name="status">Estado do voo em inglês.</param>
        /// <returns>Texto traduzido para português.</returns>
        public string GetFlightStatusText(string status) => status switch
        {
            "Scheduled" => "Previsto",
            "CheckIn" => "Check-in",
            "Boarding" => "A Embarcar",
            "Departed" => "Partiu",
            "Delayed" => "Atrasado",
            "Cancelled" => "Cancelado",
            _ => status
        };

        public string GetFlightBadgeClass(string status) => status switch
        {
            "Scheduled" => "bg-primary",
            "CheckIn" => "bg-info text-dark",
            "Boarding" => "bg-success",
            "Departed" => "bg-secondary",
            "Delayed" => "bg-warning text-dark",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };

        public string GetFlightRowClass(string status) => status switch
        {
            "Cancelled" => "table-danger",
            "Delayed" => "table-warning",
            "Boarding" => "table-success",
            _ => ""
        };







    }

}
