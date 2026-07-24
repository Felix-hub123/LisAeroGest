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
                Brand = aircraft.Brand,
                Model = aircraft.Model,
                EconomySeats = aircraft.EconomySeats,
                BusinessSeats = aircraft.BusinessSeats,
                IsAvailable = aircraft.IsAvailable,
                ImageId = aircraft.ImageId
            };
        }

        public void UpdateAircraftFromViewModel(Aircraft aircraft, AircraftViewModel model, Guid imageId)
        {
            aircraft.Brand = model.Brand;
            aircraft.Model = model.Model;
            aircraft.EconomySeats = model.EconomySeats;
            aircraft.BusinessSeats = model.BusinessSeats;
            aircraft.IsAvailable = model.IsAvailable;
            aircraft.ImageId = imageId;
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

        public void UpdateAirlineFromViewModel(Airline airline, AirlineViewModel model, Guid imageId)
        {
            airline.Name = model.Name;
            airline.IATACode = model.IATACode!.ToUpper();
            airline.Country = model.Country;
            airline.ImageId = imageId;
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

        public AirportViewModel ToAirportViewModel(Airport airport)
        {
            return new AirportViewModel
            {
                Id = airport.Id,
                Name = airport.Name,
                City = airport.City,
                Country = airport.Country,
                IATACode = airport.IATACode,
                DefaultFee = airport.DefaultFee,
                ImageId = airport.ImageId
            };
        }

        public void UpdateAirportFromViewModel(Airport airport, AirportViewModel model, Guid imageId)
        {
            airport.Name = model.Name;
            airport.City = model.City;
            airport.Country = model.Country;
            airport.IATACode = model.IATACode!.ToUpper();
            airport.DefaultFee = model.DefaultFee;
            airport.ImageId = imageId;
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
                Title = topic.Title,
                Content = topic.Content,
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

        public Ticket ToTicket(TicketTemp tempItem, Flight? flight, string userId)
        {
            var basePrice = flight?.BasePrice ?? 0;
            var luggageFee = tempItem.ExtraLuggage ? 30 : 0;
            var mealFee = tempItem.MealIncluded ? 15 : 0;

            return new Ticket
            {
                PassengerId = tempItem.PassengerId,
                FlightId = tempItem.FlightId,
                SeatId = tempItem.SeatId,
                TotalPrice = basePrice + luggageFee + mealFee,
                ExtraLuggage = tempItem.ExtraLuggage,
                MealIncluded = tempItem.MealIncluded,
                Status = "Paid",
                PurchaseDate = DateTime.UtcNow,
                CreatedByUserId = userId
            };
        }


        public TicketTemp ToTicketTemp(int flightId, int seatId, Passenger passenger, bool extraLuggage, bool mealIncluded)
        {
            return new TicketTemp
            {
                FlightId = flightId,
                SeatId = seatId,
                PassengerId = passenger.Id,
                ExtraLuggage = extraLuggage,
                MealIncluded = mealIncluded,
                Price = 0,
                CreatedByUserId = passenger.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
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



    }
}
