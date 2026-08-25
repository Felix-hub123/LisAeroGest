using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            IPayPalService payPalService)
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
        public async Task<IActionResult> Index(
            string? origin,
            string? destination,
            DateTime? date)
        {
            var flights = await _flightRepository.GetAvailableFlightsAsync(
                origin,
                destination,
                date);

            var airports = await _airportRepository.GetAllAsync();

            ViewBag.Airports =
                _converterHelper.ToAirportSelectList(airports);

            ViewBag.Origin = origin;
            ViewBag.Destination = destination;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");

            return View(flights);
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
       

        /// <summary>
        /// Mostra o mapa de lugares de um voo.
        /// NÃO exige autenticação — visitante pode escolher lugar.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SelectSeat(int flightId)
        {
            var flight = await _flightRepository.GetWithDetailsAsync(flightId);
            if (flight == null)
            {
                TempData["Error"] = "Voo não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Verifica se o voo está disponível
            if (flight.Status == "Cancelled" || flight.Status == "Departed")
            {
                TempData["Error"] = "Este voo já não está disponível para reserva.";
                return RedirectToAction(nameof(Index));
            }

            var seats = await _seatRepository.GetSeatsByFlightAsync(flightId);

            var viewModel = new SelectSeatViewModel
            {
                Flight = flight,
                Seats = seats.ToList(),
                ExtraLuggagePrice = 30m,
                MealIncludedPrice = 15m
            };

            return View(viewModel);
        }

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
         int seatId,
         bool extraLuggage,
         bool mealIncluded)
        {
            if (flightId <= 0 || seatId <= 0)
            {
                TempData["Error"] = "Selecione um voo e um lugar válidos.";
                return RedirectToAction(nameof(Index));
            }

            // 1. SE FOR VISITANTE (NÃO AUTENTICADO): Redireciona para o formulário de Convidado
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("GuestCheckout", new { flightId, seatId, extraLuggage, mealIncluded });
            }

            // 2. SE ESTIVER AUTENTICADO: Processa diretamente
            var passenger = await GetCurrentPassengerAsync();
            if (passenger == null)
            {
                TempData["Error"] = "Não foi possível identificar o seu perfil de passageiro.";
                return RedirectToAction("Index", "Home");
            }

            return await AddTicketToCartAsync(
                passenger,
                flightId,
                seatId,
                extraLuggage,
                mealIncluded);
        }

        // ============================================================
        // PROCESSAR CHECKOUT DE CONVIDADO
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessGuestCheckout(GuestCheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Flight = await _flightRepository.GetWithDetailsAsync(model.FlightId);
                ViewBag.Seat = await _seatRepository.GetByIdAsync(model.SeatId);
                ViewBag.TotalPrice = CalculateTotalPrice(
                    ViewBag.Flight?.BasePrice ?? 0,
                    ViewBag.Seat?.BasePrice ?? 0,
                    model.ExtraLuggage,
                    model.MealIncluded);
                return View("GuestCheckout", model);
            }

            // 1. Verificar se já existe um passageiro com este Email
            var passenger = await _passengerRepository.GetByEmailAsync(model.Email);

            // 2. Se não existir, criar novo passageiro (convidado)
            if (passenger == null)
            {
                passenger = new Passenger
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    DocumentNumber = model.DocumentNumber,
                    DocumentType = "CC", // Valor padrão
                    UserId = null, // ← NULL = convidado
                    RegistrationDate = DateTime.UtcNow
                };

                await _passengerRepository.AddAsync(passenger);
                await _passengerRepository.SaveAsync();
            }
            else
            {
                // Atualiza dados se necessário
                passenger.FirstName = model.FirstName;
                passenger.LastName = model.LastName;
                passenger.DocumentNumber = model.DocumentNumber;
                await _passengerRepository.UpdateAsync(passenger);
                await _passengerRepository.SaveAsync();
            }

            // 3. Criar a reserva e obter o ticket criado
            var ticket = await CreateTicketAsync(
                passenger,
                model.FlightId,
                model.SeatId,
                model.ExtraLuggage,
                model.MealIncluded);

            if (ticket == null)
            {
                TempData["Error"] = "Não foi possível criar a reserva. Tente novamente.";
                return RedirectToAction("SelectSeat", new { flightId = model.FlightId });
            }

            // 4. Guardar na sessão que o utilizador quer criar conta (se selecionado)
            if (model.WantToCreateAccount)
            {
                HttpContext.Session.SetString("PendingRegistration", model.Email);
                HttpContext.Session.SetString("PendingTicketId", ticket.Id.ToString());
            }

            return RedirectToAction("GuestPayment", new { ticketId = ticket.Id });
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
            return await ProcessBookingAsync(pendingBooking);
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
            ViewBag.TicketDisplayId = ticket.Id.ToString("D6"); // Formata com 6 dígitos (ex: 000123)

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
        /// Ecrã para recolher os dados do convidado (nome, email, documento).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GuestCheckout(int flightId, int seatId, bool extraLuggage, bool mealIncluded)
        {
            var model = new GuestCheckoutViewModel
            {
                FlightId = flightId,
                SeatId = seatId,
                ExtraLuggage = extraLuggage,
                MealIncluded = mealIncluded
            };

            // Se o utilizador já está autenticado, redireciona para o fluxo normal
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("AddToCart", new { flightId, seatId, extraLuggage, mealIncluded });
            }

            // Carregar dados do voo e lugar para a View
            ViewBag.Flight = _flightRepository.GetByIdAsync(flightId).Result;
            ViewBag.Seat = _seatRepository.GetByIdAsync(seatId).Result;

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
                PurchaseDate = DateTime.UtcNow
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

            ViewBag.TicketId = ticket.Id;
            ViewBag.TotalPrice = ticket.TotalPrice;
            ViewBag.FlightNumber = ticket.Flight?.FlightNumber;

            return View(ticket);
        }
    }
}