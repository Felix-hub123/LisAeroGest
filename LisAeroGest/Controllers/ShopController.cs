using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controlador responsável pelo ciclo de pesquisa, seleção de lugares,
    /// carrinho, reservas e compra de bilhetes.
    /// </summary>
    public class ShopController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAirportRepository _airportRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly PdfService _pdfService;
        private readonly IPayPalService _payPalService;
        private readonly WeatherService _weatherService;
        private readonly IWhatsAppService _whatsAppService;

        // Valores dos serviços adicionais.
        private const decimal ExtraLuggageFee = 30m;
        private const decimal MealFee = 15m;

        public ShopController(
            IFlightRepository flightRepository,
            IAirportRepository airportRepository,
            ISeatRepository seatRepository,
            ITicketRepository ticketRepository,
            IPassengerRepository passengerRepository,
            IUserHelper userHelper,
            IConverterHelper converterHelper,
            PdfService pdfService,
            IPayPalService payPalService,
            WeatherService weatherService,
            IWhatsAppService whatsAppService)
        {
            _flightRepository = flightRepository;
            _airportRepository = airportRepository;
            _seatRepository = seatRepository;
            _ticketRepository = ticketRepository;
            _passengerRepository = passengerRepository;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
            _pdfService = pdfService;
            _payPalService = payPalService;
            _weatherService = weatherService;
            _whatsAppService = whatsAppService;
        }

        // ============================================================
        // UTILITÁRIOS
        // ============================================================

        /// <summary>
        /// Obtém o passageiro associado ao utilizador atualmente autenticado.
        /// </summary>
        private async Task<Passenger?> GetCurrentPassengerAsync()
        {
            if (string.IsNullOrWhiteSpace(User.Identity?.Name))
                return null;

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
                return null;

            return await _passengerRepository.GetByUserIdAsync(user.Id);
        }

        /// <summary>
        /// Calcula o preço total da reserva.
        /// O preço do lugar é sempre obtido do servidor.
        /// </summary>
        private static decimal CalculateTotalPrice(
            decimal flightPrice,
            decimal seatPrice,
            bool extraLuggage,
            bool mealIncluded)
        {
            return flightPrice
                   + seatPrice
                   + (extraLuggage ? ExtraLuggageFee : 0m)
                   + (mealIncluded ? MealFee : 0m);
        }

        // ============================================================
        // PESQUISA DE VOOS
        // ============================================================

        /// <summary>
        /// Lista os voos disponíveis.
        /// Este método é público e pode ser utilizado por visitantes
        /// não autenticados.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(
                 string? origin,
                 string? destination,
                 DateTime? date,
                 DateTime? returnDate,
                 int? passengers,
                 string? tripType,
                 string? cabinClass,
                 bool? directOnly)
        {
            var destinationCode = ExtractIataOrText(destination);

            var flights = await _flightRepository.GetAvailableFlightsAsync(
                origin,
                destinationCode,
                date);

            var airports = await _airportRepository.GetAllAsync();

            var model = new ShopIndexViewModel
            {
                Origin = origin,
                Destination = destination,
                Date = date?.ToString("yyyy-MM-dd"),
                ReturnDate = returnDate?.ToString("yyyy-MM-dd"),
                Passengers = passengers ?? 1,
                TripType = string.IsNullOrWhiteSpace(tripType) ? "oneway" : tripType,
                CabinClass = string.IsNullOrWhiteSpace(cabinClass) ? "Economy" : cabinClass,
                DirectOnly = directOnly == true,
                Airports = _converterHelper.ToAirportSelectList(airports),
                Flights = flights.Select(MapFlightSearchItem).ToList()
            };

            return View(model);
        }

        private static FlightSearchItemViewModel MapFlightSearchItem(Flight flight)
        {
            var duration = flight.ArrivalTime - flight.DepartureTime;

            return new FlightSearchItemViewModel
            {
                Id = flight.Id,
                FlightNumber = flight.FlightNumber ?? "",
                AirlineName = flight.Airline?.Name ?? "—",
                AircraftModel = flight.Aircraft?.Model,
                OriginCode = flight.OriginAirport?.IATACode ?? "—",
                OriginCity = flight.OriginAirport?.City ?? "",
                DestinationCode = flight.DestinationAirport?.IATACode ?? "—",
                DestinationCity = flight.DestinationAirport?.City ?? "",
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                BasePrice = flight.BasePrice,
                Status = flight.Status,
                DurationMinutes = (int)duration.TotalMinutes,
                DurationLabel = $"{(int)duration.TotalHours}h {duration.Minutes:00}m"
            };
        }

        private static string? ExtractIataOrText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var start = value.LastIndexOf('(');
            var end = value.LastIndexOf(')');
            if (start >= 0 && end > start)
                return value.Substring(start + 1, end - start - 1).Trim().ToUpperInvariant();

            return value.Trim();
        }

        // ============================================================
        // SELEÇÃO DE LUGAR
        // ============================================================

        /// <summary>
        /// Mostra o mapa de lugares de um voo.
        ///
        /// IMPORTANTE:
        /// Este método NÃO exige autenticação.
        /// O visitante pode pesquisar e escolher um lugar normalmente.
        /// </summary>




        // ============================================================
        // ADICIONAR AO CARRINHO
        // ============================================================

        /// <summary>
        /// Recebe a escolha do visitante.
        ///
        /// Se não estiver autenticado:
        /// - NÃO cria Ticket;
        /// - NÃO bloqueia o lugar;
        /// - guarda a seleção na Session;
        /// - encaminha para Login.
        ///
        /// Se estiver autenticado:
        /// - valida novamente o voo/lugar;
        /// - cria a reserva;
        /// - coloca-a no carrinho.
        /// </summary>
        // ============================================================
        // ADICIONAR AO CARRINHO (ATUALIZADO PARA GUEST CHECKOUT)
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
             int flightId,
             List<int> seatIds,
             bool extraLuggage,
             bool mealIncluded)
        {
            if (flightId <= 0 || seatIds == null || !seatIds.Any())
            {
                TempData["Error"] = "Selecione pelo menos um lugar.";
                return RedirectToAction(nameof(Index));
            }

            // 1. SE FOR VISITANTE (NÃO AUTENTICADO): Redireciona para o formulário de Convidado
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("GuestCheckout", new
                {
                    flightId,
                    seatIds = string.Join(",", seatIds), // Serializa a lista
                    extraLuggage,
                    mealIncluded
                });
            }

            // 2. SE ESTIVER AUTENTICADO: Processa diretamente
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null)
            {
                TempData["Error"] = "Não foi possível identificar o seu perfil de passageiro.";
                return RedirectToAction("Index", "Home");
            }

            // 3. Criar um ticket por cada lugar selecionado
            foreach (var seatId in seatIds)
            {
                var result = await AddTicketToCartAsync(
                    passenger,
                    flightId,
                    seatId,
                    extraLuggage,
                    mealIncluded);

                // Se houver erro, parar e devolver
                if (result is RedirectToActionResult redirect && redirect.ActionName != "Cart")
                    return result;
            }

            TempData["Success"] = $"{seatIds.Count} lugar(es) reservado(s) com sucesso!";
            return RedirectToAction(nameof(Cart));
        }

        // ============================================================
        // PROCESSAR CHECKOUT DE CONVIDADO
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessGuestCheckout(GuestCheckoutViewModel model)
        {
            var seatIds = (model.SeatIds ?? new List<int>())
                .Concat(model.SeatId > 0 ? new[] { model.SeatId } : Array.Empty<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (!seatIds.Any())
            {
                TempData["Error"] = "Não foi seleccionado nenhum lugar.";
                return RedirectToAction(nameof(SelectSeat), new { flightId = model.FlightId });
            }

            model.SeatIds = seatIds;
            model.SeatId = seatIds.First();

            if (!ModelState.IsValid)
            {
                var flightInvalid = await _flightRepository.GetWithDetailsAsync(model.FlightId);
                model.FlightNumber = flightInvalid?.FlightNumber ?? "";
                model.OriginCode = flightInvalid?.OriginAirport?.IATACode ?? "";
                model.DestinationCode = flightInvalid?.DestinationAirport?.IATACode ?? "";
                return View("GuestCheckout", model);
            }

            var passenger = await _passengerRepository.GetByEmailAsync(model.Email);
            if (passenger == null)
            {
                passenger = new Passenger
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    DocumentNumber = model.DocumentNumber,
                    DocumentType = "CC",
                    PhoneNumber = model.PhoneNumber,
                    UserId = null,
                    RegistrationDate = DateTime.UtcNow
                };
                await _passengerRepository.AddAsync(passenger);
                await _passengerRepository.SaveAsync();
            }
            else
            {
                passenger.FirstName = model.FirstName;
                passenger.LastName = model.LastName;
                passenger.DocumentNumber = model.DocumentNumber;
                passenger.PhoneNumber = model.PhoneNumber;
                await _passengerRepository.UpdateAsync(passenger);
                await _passengerRepository.SaveAsync();
            }

            Ticket? lastTicket = null;
            foreach (var seatId in seatIds)
            {
                lastTicket = await CreateTicketAsync(
                    passenger, model.FlightId, seatId, model.ExtraLuggage, model.MealIncluded);
                if (lastTicket == null)
                    return RedirectToAction(nameof(SelectSeat), new { flightId = model.FlightId });
            }

            if (model.WantToCreateAccount)
            {
                HttpContext.Session.SetString("PendingRegistration", model.Email);
                HttpContext.Session.SetString("PendingTicketId", lastTicket!.Id.ToString());
            }

            return RedirectToAction("GuestPayment", new { ticketId = lastTicket!.Id });
        }


        /// <summary>
        /// PDF da reserva ou do bilhete, sem login. Exige token.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GuestTicketPdf(int ticketId, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return NotFound();

            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);
            if (ticket == null || ticket.DownloadToken != token)
                return NotFound();

            // Só reserva. Bilhete pago exige conta.
            if (ticket.Status != "Reserved")
                return NotFound();

            var bytes = _pdfService.GenerateTicketPdf(ticket);
            return File(bytes, "application/pdf", $"Reserva_{ticket.Id}.pdf");
        }

        // ============================================================
        // CONCLUSÃO APÓS LOGIN
        // ============================================================

        /// <summary>
        /// Recupera a seleção feita pelo visitante antes do login.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CompleteAddToCart()
        {
            var sessionData = HttpContext.Session.GetString("PendingBooking");
            if (string.IsNullOrEmpty(sessionData))
            {
                TempData["Error"] = "A sua sessão de reserva expirou ou é inválida.";
                return RedirectToAction("Index", "Shop");
            }

            var pendingBooking = JsonSerializer.Deserialize<PendingBookingDto>(sessionData);

            // Processa e valida tudo para o utilizador autenticado
            return await ProcessBookingAsync(pendingBooking!);
        }

        // ============================================================
        // CRIAÇÃO DA RESERVA
        // ============================================================

        /// <summary>
        /// Método central responsável pela validação e criação do Ticket.
        /// </summary>
        private async Task<IActionResult> AddTicketToCartAsync(
            Passenger passenger,
            int flightId,
            int seatId,
            bool extraLuggage,
            bool mealIncluded)
        {
            // --------------------------------------------------------
            // 1. Obter o voo
            // --------------------------------------------------------

            var flight =
                await _flightRepository.GetWithDetailsAsync(flightId);

            if (flight == null || flight.WasDeleted)
            {
                TempData["Error"] =
                    "O voo selecionado já não está disponível.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // 2. Obter o lugar
            // --------------------------------------------------------

            var seat =
                await _seatRepository.GetByIdAsync(seatId);

            if (seat == null || seat.WasDeleted)
            {
                TempData["Error"] =
                    "O lugar selecionado não existe.";

                return RedirectToAction(
                    nameof(SelectSeat),
                    new { flightId });
            }

            // --------------------------------------------------------
            // 3. Confirmar que o lugar pertence ao voo
            // --------------------------------------------------------

            if (!seat.FlightId.HasValue ||
                seat.FlightId.Value != flightId)
            {
                TempData["Error"] =
                    "O lugar selecionado não pertence a este voo.";

                return RedirectToAction(
                    nameof(SelectSeat),
                    new { flightId });
            }

            // --------------------------------------------------------
            // 4. Verificar disponibilidade
            // --------------------------------------------------------

            if (!seat.IsAvailable)
            {
                TempData["Error"] =
                    $"O lugar {seat.Code} acabou de ser reservado por outro utilizador.";

                return RedirectToAction(
                    nameof(SelectSeat),
                    new { flightId });
            }

            // --------------------------------------------------------
            // 5. Calcular o preço no servidor
            // --------------------------------------------------------

            var totalPrice = CalculateTotalPrice(
                flight.BasePrice,
                seat.BasePrice,
                extraLuggage,
                mealIncluded);

            // --------------------------------------------------------
            // 6. Bloquear o lugar
            // --------------------------------------------------------

            seat.IsAvailable = false;

            await _seatRepository.UpdateAsync(seat);

            // --------------------------------------------------------
            // 7. Criar Ticket através do Converter
            // --------------------------------------------------------

            var ticket = _converterHelper.ToTicket(
               flightId,
               seatId,
               passenger,
               extraLuggage,
               mealIncluded,
               totalPrice);

            ticket.DownloadToken = Guid.NewGuid().ToString("N");
                       
            await _ticketRepository.AddAsync(ticket);

            await _ticketRepository.SaveAsync();

            TempData["Success"] =
                $"Lugar {seat.Code} reservado temporariamente. " +
                "A reserva fica disponível no carrinho durante 30 minutos.";

            return RedirectToAction(nameof(Cart));
        }



        /// <summary>
        /// Ecrã final de confirmação da compra para convidados.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckoutConfirmation(int ticketId)
        {
            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);
            if (ticket == null)
            {
                TempData["Error"] = "Bilhete não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            var flight = await _flightRepository.GetWithDetailsAsync(ticket.FlightId);
            var seat = await _seatRepository.GetByIdAsync(ticket.SeatId);
            var passenger = await _passengerRepository.GetByIdAsync(ticket.PassengerId);

            var model = new CheckoutConfirmationViewModel
            {
                Ticket = ticket,
                Flight = flight,
                Seat = seat,
                Passenger = passenger,
                TotalPrice = ticket.TotalPrice,
                ExtraLuggage = ticket.ExtraLuggage,
                MealIncluded = ticket.MealIncluded
            };

            // Verifica se o passageiro é convidado (UserId == null)
            var isGuest = string.IsNullOrEmpty(passenger?.UserId);
            ViewBag.IsGuest = isGuest;
            ViewBag.PendingRegistration = HttpContext.Session.GetString("PendingRegistration");
            ViewBag.TicketDisplayId = ticket.Id.ToString("D6");
            ViewBag.DownloadToken = ticket.DownloadToken;
            ViewBag.TicketId = ticket.Id;

            return View(model);
        }

        // ============================================================
        // CARRINHO
        // ============================================================

        /// <summary>
        /// Mostra as reservas ativas do passageiro.
        /// Também liberta lugares de reservas expiradas.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var passenger = await GetCurrentPassengerAsync();

            if (passenger == null)
                return RedirectToAction("Index", "Home");

            var tickets = (await _ticketRepository
                    .GetByPassengerAsync(passenger.Id))
                .Where(t => t.Status == "Reserved")
                .ToList();

            var expiredTickets = tickets
                .Where(t => !t.IsReservationValid)
                .ToList();

            if (expiredTickets.Any())
            {
                foreach (var expiredTicket in expiredTickets)
                {
                    var seat =
                        await _seatRepository
                            .GetByIdAsync(expiredTicket.SeatId);

                    if (seat != null)
                    {
                        seat.IsAvailable = true;

                        await _seatRepository
                            .UpdateAsync(seat);
                    }

                    expiredTicket.Status = "Expired";

                    await _ticketRepository
                        .UpdateAsync(expiredTicket);

                    tickets.Remove(expiredTicket);
                }

                await _ticketRepository.SaveAsync();

                TempData["Error"] =
                    "Algumas reservas expiraram e os respetivos lugares foram libertados.";
            }

            return View(tickets);
        }



        // 4. Método centralizado de Validação e Criação do Ticket
        private async Task<IActionResult> ProcessBookingAsync(PendingBookingDto booking)
        {
            var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
            var seat = await _seatRepository.GetByIdAsync(booking.SeatId);

            // Validação 1: O Voo e o Lugar existem?
            if (flight == null || seat == null)
            {
                TempData["Error"] = "O voo ou o lugar selecionado já não se encontra disponível.";
                return RedirectToAction("Index", "Shop");
            }

            // Validação 2: O lugar já foi ocupado por outro utilizador no meio tempo?
            if (!seat.IsAvailable)
            {
                TempData["Error"] = "Lamentamos, mas o lugar selecionado acabou de ser reservado por outro cliente.";
                return RedirectToAction(nameof(SelectSeat), new { flightId = booking.FlightId });
            }

            // Obter o Passenger associado ao User atual
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            var passenger = await _passengerRepository.GetByUserIdAsync(user.Id);

            if (passenger == null)
            {
                TempData["Error"] = "Perfil de passageiro não encontrado. Por favor complete o seu perfil.";
                return RedirectToAction("Profile", "Account");
            }

            // Marcar o lugar como ocupado
            seat.IsAvailable = true;
            await _seatRepository.UpdateAsync(seat);

            // Criar o Ticket/Reserva no carrinho (Status: PendingPayment ou InCart)
            var ticket = new Ticket
            {
                FlightId = flight.Id,
                PassengerId = passenger.Id,
                SeatId = seat.Id,
                Status = "InCart",
                TotalPrice = flight.BasePrice + seat.BasePrice, 
                PurchaseDate = DateTime.UtcNow
            };

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveAsync();

            // Limpar os dados da reserva pendente da Sessão
            HttpContext.Session.Remove("PendingBooking");

            TempData["Success"] = "Voo adicionado ao carrinho com sucesso!";
            return RedirectToAction("Index", "Cart");
        }

        // ============================================================
        // REMOVER DO CARRINHO
        // ============================================================

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int ticketId)
        {
            var passenger = await GetCurrentPassengerAsync();

            if (passenger == null)
                return RedirectToAction("Index", "Home");

            var ticket =
                await _ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null ||
                ticket.PassengerId != passenger.Id ||
                ticket.Status != "Reserved")
            {
                return RedirectToAction(nameof(Cart));
            }

            var seat =
                await _seatRepository.GetByIdAsync(ticket.SeatId);

            if (seat != null)
            {
                seat.IsAvailable = true;

                await _seatRepository.UpdateAsync(seat);
            }

            ticket.Status = "Cancelled";

            await _ticketRepository.UpdateAsync(ticket);

            await _ticketRepository.SaveAsync();

            TempData["Success"] =
                "A reserva foi removida e o lugar voltou a estar disponível.";

            return RedirectToAction(nameof(Cart));
        }

        // ============================================================
        // CHECKOUT
        // ============================================================

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
           
            return RedirectToAction(nameof(Payment));
        }

        // ============================================================
        // MEUS BILHETES
        // ============================================================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyTickets()
        {
            var passenger = await GetCurrentPassengerAsync();

            if (passenger == null)
                return RedirectToAction("Index", "Home");

            var tickets =
                (await _ticketRepository
                    .GetByPassengerAsync(passenger.Id))
                .Where(t =>
                    t.Status == "Paid" ||
                    t.Status == "CheckedIn")
                .ToList();

            return View(tickets);
        }

        // ============================================================
        // DOWNLOAD DO BILHETE
        // ============================================================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadTicketPdf(int ticketId)
        {
            var passenger = await GetCurrentPassengerAsync();

            if (passenger == null)
                return RedirectToAction("Index", "Home");

            var ticket =
                await _ticketRepository
                    .GetTicketWithDetailsAsync(ticketId);

            if (ticket == null ||
                ticket.PassengerId != passenger.Id)
            {
                return NotFound();
            }

            var pdfBytes =
                _pdfService.GenerateTicketPdf(ticket);

            return File(
                pdfBytes,
                "application/pdf",
                $"Bilhete_{ticket.Id}.pdf");
        }


        /// <summary>
        /// Autocomplete de aeroportos para o campo destino.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SearchAirports(string? term)
        {
            var airports = await _airportRepository.SearchAsync(term ?? "", 8);

            var result = airports.Select(a => new AirportSuggestionViewModel
            {
                Iata = a.IATACode ?? "",
                City = a.City ?? "",
                Country = a.Country ?? "",
                Name = a.Name ?? "",
                Label = $"{a.City} ({a.IATACode})"
            });

            return Json(result);
        }

        /// <summary>
        /// Ecrã para recolher os dados do convidado (nome, email, documento).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GuestCheckout(
              int flightId,
              string seatIds,
              bool extraLuggage,
              bool mealIncluded)
        {
            // Converter a string "1,2,3" numa lista de inteiros
            var seatIdList = string.IsNullOrWhiteSpace(seatIds)
                ? new List<int>()
                : seatIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(int.Parse)
                         .ToList();

            if (!seatIdList.Any())
            {
                TempData["Error"] = "Selecione pelo menos um lugar.";
                return RedirectToAction(nameof(SelectSeat), new { flightId });
            }

            // Se o utilizador está autenticado, processa diretamente
            if (User.Identity?.IsAuthenticated == true)
            {
                var passenger = await GetCurrentPassengerAsync();
                if (passenger != null)
                {
                    foreach (var seatId in seatIdList)
                    {
                        var result = await AddTicketToCartAsync(
                            passenger, flightId, seatId, extraLuggage, mealIncluded);

                        if (result is RedirectToActionResult redirect && redirect.ActionName != "Cart")
                            return result;
                    }
                    return RedirectToAction(nameof(Cart));
                }
            }

            // Visitante: mostrar formulário de checkout convidado
            var flight = await _flightRepository.GetWithDetailsAsync(flightId);
            if (flight == null)
            {
                TempData["Error"] = "Voo não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Obter os lugares selecionados
            var seats = new List<Seat>();
            foreach (var seatId in seatIdList)
            {
                var seat = await _seatRepository.GetByIdAsync(seatId);
                if (seat != null)
                    seats.Add(seat);
            }

            var model = new GuestCheckoutViewModel
            {
                FlightId = flightId,
                SeatIds = seatIdList,
                SeatId = seatIdList.First(),
                ExtraLuggage = extraLuggage,
                MealIncluded = mealIncluded,
                FlightNumber = flight.FlightNumber ?? "",
                OriginCode = flight.OriginAirport?.IATACode ?? "",
                DestinationCode = flight.DestinationAirport?.IATACode ?? "",
                SeatCode = string.Join(", ", seats.Select(s => s.Code)),
                FlightPrice = flight.BasePrice * seatIdList.Count,
                SeatPrice = seats.Sum(s => s.BasePrice),
                TotalPrice = (flight.BasePrice * seatIdList.Count)
                             + seats.Sum(s => s.BasePrice)
                             + (extraLuggage ? ExtraLuggageFee : 0m)
                             + (mealIncluded ? MealFee : 0m)
            };

            return View(model);
        }


        /// <summary>
        /// Cria a reserva e retorna o ticket criado.
        /// </summary>
        private async Task<Ticket?> CreateTicketAsync(
            Passenger passenger,
            int flightId,
            int seatId,
            bool extraLuggage,
            bool mealIncluded)
        {
            // 1. Obter o voo
            var flight = await _flightRepository.GetWithDetailsAsync(flightId);
            if (flight == null || flight.WasDeleted)
            {
                TempData["Error"] = "O voo selecionado já não está disponível.";
                return null;
            }

            // 2. Obter o lugar
            var seat = await _seatRepository.GetByIdAsync(seatId);
            if (seat == null || !seat.IsAvailable)
            {
                TempData["Error"] = "O lugar selecionado já não está disponível.";
                return null;
            }

            // 3. Calcular o preço
            var totalPrice = CalculateTotalPrice(
                flight.BasePrice,
                seat.BasePrice,
                extraLuggage,
                mealIncluded);

            // 4. Bloquear o lugar
            seat.IsAvailable = false;
            await _seatRepository.UpdateAsync(seat);

            // 5. Criar o Ticket (SEM TicketNumber)
            var ticket = new Ticket
            {
                FlightId = flightId,
                SeatId = seatId,
                PassengerId = passenger.Id,
                Status = "Reserved",
                TotalPrice = totalPrice,
                ExtraLuggage = extraLuggage,
                MealIncluded = mealIncluded,
                ReservationExpiresAt = DateTime.UtcNow.AddMinutes(30),
                PurchaseDate = DateTime.UtcNow,
                DownloadToken = Guid.NewGuid().ToString("N")
            };

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveAsync();

            TempData["Success"] = "Reserva criada com sucesso!";

            return ticket;
        }


        /// <summary>
        /// Cria uma ordem de pagamento no PayPal para o bilhete especificado.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePayPalOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                // 1. Validar o ticket
                var ticket = await _ticketRepository.GetTicketWithDetailsAsync(request.TicketId);
                if (ticket == null)
                    return BadRequest(new { message = "Bilhete não encontrado." });

                if (ticket.Status != "Reserved")
                    return BadRequest(new { message = "Esta reserva já foi processada ou expirou." });

                // 2. Verificar se a reserva expirou (30 min) - USAR PurchaseDate
                var timeSinceCreation = DateTime.UtcNow - ticket.PurchaseDate;
                if (timeSinceCreation.TotalMinutes > 30)
                {
                    ticket.Status = "Expired";
                    await _ticketRepository.UpdateAsync(ticket);
                    await _ticketRepository.SaveAsync();
                    return BadRequest(new { message = "A reserva expirou. Por favor, faça uma nova reserva." });
                }

                // 3. Criar ordem no PayPal
                var orderId = await _payPalService.CreateOrderAsync(
                    ticket.TotalPrice,
                    "EUR",
                    ticket.Id.ToString()
                );

                return Ok(new { orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar o pedido de pagamento. Tente novamente." });
            }
        }

        // ================================================================
        // PAYPAL — CAPTURAR PAGAMENTO
        // ================================================================

        /// <summary>
        /// Captura (finaliza) um pagamento já aprovado pelo comprador no PayPal.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CapturePayPalOrder([FromBody] CaptureOrderRequest request)
        {
            try
            {
                // 1. Validar o ticket
                var ticket = await _ticketRepository.GetTicketWithDetailsAsync(request.TicketId);
                if (ticket == null)
                    return BadRequest(new { message = "Bilhete não encontrado." });

                if (ticket.Status != "Reserved")
                    return BadRequest(new { message = "Esta reserva já foi processada ou expirou." });

                // 2. Capturar o pagamento no PayPal
                var success = await _payPalService.CaptureOrderAsync(request.OrderId);
                if (!success)
                    return BadRequest(new { message = "Pagamento não foi confirmado pelo PayPal." });

                // 3. Atualizar o ticket para Pago
                ticket.Status = "Paid";
                ticket.PurchaseDate = DateTime.UtcNow;
                ticket.ReservationExpiresAt = null; // Remove a expiração

                await _ticketRepository.UpdateAsync(ticket);
                await _ticketRepository.SaveAsync();
                var phone = ticket.Passenger?.PhoneNumber
             ?? ticket.Passenger?.User?.PhoneNumber
             ?? "";
                await _whatsAppService.SendTicketMessageAsync(
                    phone,
                    $"LisAeroGest — Pagamento confirmado. Voo {ticket.Flight?.FlightNumber}, lugar {ticket.Seat?.Code}, bilhete #{ticket.Id}.");
                // 4. Retornar URL de redirecionamento
                return Ok(new
                {
                    success = true,
                    redirectUrl = Url.Action(nameof(CheckoutConfirmation), new { ticketId = ticket.Id })
                });
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Erro ao capturar pagamento PayPal para ticket {TicketId}", request.TicketId);
                return StatusCode(500, new { message = "Erro ao processar o pagamento. Tente novamente." });
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Payment()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null)
                return RedirectToAction("Index", "Home");

            var tickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id))
                .Where(t => t.Status == "Reserved" && t.IsReservationValid)
                .ToList();

            if (!tickets.Any())
            {
                TempData["Error"] = "O seu carrinho está vazio ou as reservas expiraram.";
                return RedirectToAction(nameof(Cart));
            }

            var model = new PaymentViewModel
            {
                Tickets = tickets,
                TotalPrice = tickets.Sum(t => t.TotalPrice)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCartPayPalOrder()
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null)
                return BadRequest(new { message = "Passageiro não identificado." });

            var tickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id))
                .Where(t => t.Status == "Reserved" && t.IsReservationValid)
                .ToList();

            if (!tickets.Any())
                return BadRequest(new { message = "Não existem reservas válidas para pagar." });

            var total = tickets.Sum(t => t.TotalPrice);
            var orderId = await _payPalService.CreateOrderAsync(total, "EUR", $"cart-{passenger.Id}");

            return Ok(new { orderId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CaptureCartPayPalOrder([FromBody] CaptureOrderRequest request)
        {
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null)
                return BadRequest(new { message = "Passageiro não identificado." });

            var success = await _payPalService.CaptureOrderAsync(request.OrderId);
            if (!success)
                return BadRequest(new { message = "Pagamento não foi confirmado pelo PayPal." });

            var tickets = (await _ticketRepository.GetByPassengerAsync(passenger.Id))
                .Where(t => t.Status == "Reserved" && t.IsReservationValid)
                .ToList();

            foreach (var ticket in tickets)
            {
                ticket.Status = "Paid";
                ticket.PurchaseDate = DateTime.UtcNow;
                ticket.ReservationExpiresAt = null;
                await _ticketRepository.UpdateAsync(ticket);
            }
            await _ticketRepository.SaveAsync();

            var phone = passenger.PhoneNumber
            ?? passenger.User?.PhoneNumber
            ?? "";
            if (!string.IsNullOrWhiteSpace(phone))
            {
                await _whatsAppService.SendTicketMessageAsync(
                    phone,
                    "LisAeroGest — Pagamento do carrinho confirmado.");
            }

            return Ok(new { success = true, redirectUrl = Url.Action(nameof(MyTickets)) });
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GuestPayment(int ticketId)
        {
            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);
            if (ticket == null || ticket.Status != "Reserved")
            {
                TempData["Error"] = "Esta reserva já não está disponível para pagamento.";
                return RedirectToAction("Index", "Home");
            }

            var model = new GuestPaymentViewModel
            {
                TicketId = ticket.Id,
                FlightNumber = ticket.Flight?.FlightNumber ?? "",
                OriginCode = ticket.Flight?.OriginAirport?.IATACode ?? "",
                DestinationCode = ticket.Flight?.DestinationAirport?.IATACode ?? "",
                SeatCode = ticket.Seat?.Code ?? "",
                PassengerName = $"{ticket.Passenger?.FirstName} {ticket.Passenger?.LastName}".Trim(),
                ExtraLuggage = ticket.ExtraLuggage,
                MealIncluded = ticket.MealIncluded,
                TotalPrice = ticket.TotalPrice,
                ExpiresAt = ticket.ReservationExpiresAt,
                DownloadToken = ticket.DownloadToken
            };

            return View(model);
        }




        private static List<SeatMapRowViewModel> BuildSeatRows(IEnumerable<Seat> seats, int flightId)
        {
            var leftLetters = new[] { "A", "B", "C" };
            var rightLetters = new[] { "D", "E", "F" };

            var active = seats
                .Where(s => !s.WasDeleted && s.FlightId == flightId)
                .Select(s =>
                {
                    var code = s.Code ?? "";
                    var rowPart = new string(code.TakeWhile(char.IsDigit).ToArray());
                    var letter = new string(code.SkipWhile(char.IsDigit).ToArray()).ToUpperInvariant();
                    int.TryParse(rowPart, out var row);
                    return new { Seat = s, Row = row, Letter = letter };
                })
                .ToList();

            return active
                .GroupBy(x => x.Row)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var rowSeats = g.ToList();
                    bool isBusiness = rowSeats.Any(x =>
                        string.Equals(x.Seat.SeatClass, "Business", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(x.Seat.SeatClass, "Executiva", StringComparison.OrdinalIgnoreCase));

                    return new SeatMapRowViewModel
                    {
                        RowNumber = g.Key,
                        IsBusinessRow = isBusiness,
                        LeftSeats = leftLetters.Select(letter => MapCell(rowSeats.FirstOrDefault(x => x.Letter == letter)?.Seat, letter)).ToList(),
                        RightSeats = rightLetters.Select(letter => MapCell(rowSeats.FirstOrDefault(x => x.Letter == letter)?.Seat, letter)).ToList()
                    };
                })
                .ToList();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Calendar(int? year, int? month, DateTime? day)
        {
            var today = DateTime.Today;
            year ??= today.Year;
            month ??= today.Month;

            var start = new DateTime(year.Value, month.Value, 1);
            var end = start.AddMonths(1);

            var monthFlights = await _flightRepository.GetAllQueryable()
                .Include(f => f.Airline)
                .Include(f => f.Aircraft)
                .Include(f => f.OriginAirport)
                .Include(f => f.DestinationAirport)
                .Where(f => !f.WasDeleted
                    && f.OriginAirport!.IATACode == "LIS"
                    && f.DepartureTime >= start
                    && f.DepartureTime < end
                    && f.Status != "Cancelled"
                    && f.Status != "Departed")
                .ToListAsync();

            var counts = monthFlights
                .GroupBy(f => f.DepartureTime.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var offset = ((int)start.DayOfWeek + 6) % 7;
            var gridStart = start.AddDays(-offset);

            var days = new List<FlightCalendarDayViewModel>();
            for (var d = gridStart; d < gridStart.AddDays(42); d = d.AddDays(1))
            {
                days.Add(new FlightCalendarDayViewModel
                {
                    Date = d,
                    IsCurrentMonth = d.Month == month,
                    IsToday = d.Date == today,
                    FlightCount = counts.TryGetValue(d.Date, out var n) ? n : 0
                });
            }

            var selected = day?.Date;
            var ofDay = selected == null
                ? new List<FlightSearchItemViewModel>()
                : monthFlights
                    .Where(f => f.DepartureTime.Date == selected)
                    .OrderBy(f => f.DepartureTime)
                    .Select(MapFlightSearchItem)
                    .ToList();

            foreach (var item in ofDay)
            {
                var weather = await _weatherService.GetWeatherAsync(item.DestinationCity);
                if (weather?.Main == null)
                    continue;
                item.Temperature = weather.Main.Temp;
                item.WeatherDescription = weather.Weather?.FirstOrDefault()?.Description;
                var windKmh = (weather.Wind?.Speed ?? 0) * 3.6;
                item.IsAdverseWeather = windKmh >= 50;
            }

            var culture = new System.Globalization.CultureInfo("pt-PT");
            return View(new FlightCalendarViewModel
            {
                Year = year.Value,
                Month = month.Value,
                MonthName = culture.DateTimeFormat.GetMonthName(month.Value),
                Days = days,
                SelectedDate = selected,
                FlightsOfDay = ofDay
            });
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SelectSeat(int flightId, int passengers = 1)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(flightId);
            if (flight == null)
            {
                TempData["Error"] = "Voo não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (flight.Status == "Cancelled" || flight.Status == "Departed")
            {
                TempData["Error"] = "Este voo já não está disponível para reserva.";
                return RedirectToAction(nameof(Index));
            }

            var seats = (await _seatRepository.GetSeatsByFlightAsync(flightId)).ToList();

            var viewModel = new SelectSeatViewModel
            {
                Flight = flight,
                Seats = seats,
                ExtraLuggagePrice = ExtraLuggageFee,
                MealIncludedPrice = MealFee,
                SeatRows = BuildSeatRows(seats, flight.Id),
                PassengerCount = Math.Max(1, passengers) 
            };

            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TestWhatsApp(string phone = "937262437")
        {
            var ok = await _whatsAppService.SendTicketMessageAsync(
                phone,
                "LisAeroGest teste WhatsApp");
            return Content(ok ? "Enviado" : "Falhou — vê o Output");
        }

        private static SeatMapCellViewModel MapCell(Seat? seat, string letter)
        {
            if (seat == null)
            {
                return new SeatMapCellViewModel
                {
                    Letter = letter,
                    Exists = false,
                    CssClass = "seat-empty"
                };
            }

            bool occupied = !seat.IsAvailable;
            bool business =
                string.Equals(seat.SeatClass, "Business", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(seat.SeatClass, "Executiva", StringComparison.OrdinalIgnoreCase);

            string css = occupied ? "seat-occupied" : (business ? "seat-business" : "seat-available");

            return new SeatMapCellViewModel
            {
                SeatId = seat.Id,
                Code = seat.Code ?? "",
                Letter = letter,
                SeatClass = seat.SeatClass ?? "Economy",
                Price = seat.BasePrice,
                IsOccupied = occupied,
                Exists = true,
                CssClass = css
            };
        }
    }
}