using LisAeroGest.Data.Entities;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LisAeroGest.Helpers
{
    public interface IConverterHelper
    {
        /// <summary>
        /// Converte um <see cref="RegisterViewModel"/> para a entidade <see cref="User"/>.
        /// </summary>
        /// <param name="model">ViewModel de registo.</param>
        /// <returns>Entidade User preenchida.</returns>
        User ToUser(RegisterViewModel model);

        /// <summary>
        /// Converte um <see cref="RegisterViewModel"/> para a entidade <see cref="Passenger"/>.
        /// </summary>
        /// <param name="model">ViewModel de registo.</param>
        /// <param name="userId">ID do utilizador Identity criado.</param>
        /// <returns>Entidade Passenger preenchida.</returns>
        Passenger ToPassenger(RegisterViewModel model, string userId);

        /// <summary>
        /// Converte um <see cref="AircraftViewModel"/> para a entidade <see cref="Aircraft"/>.
        /// </summary>
        Aircraft ToAircraft(AircraftViewModel model, Guid imageId, bool isEdit = false);

        /// <summary>
        /// Converte uma entidade <see cref="Aircraft"/> para um <see cref="AircraftViewModel"/>.
        /// </summary>
        AircraftViewModel ToAircraftViewModel(Aircraft aircraft);

        /// <summary>
        /// Atualiza as propriedades de uma entidade <see cref="Aircraft"/> com os dados do <see cref="AircraftViewModel"/>.
        /// </summary>
        /// <param name="aircraft">Entidade da aeronave a ser atualizada.</param>
        /// <param name="model">ViewModel com os novos dados.</param>
        /// <param name="imageId">Novo ou atual ID da imagem associada.</param>
        void UpdateAircraftFromViewModel(Aircraft aircraft, AircraftViewModel model, Guid imageId);

        /// <summary>
        /// Converte um <see cref="AirlineViewModel"/> para a entidade <see cref="Airline"/>.
        /// </summary>
        /// <param name="model">ViewModel com os dados da companhia aérea.</param>
        /// <param name="imageId">ID do ficheiro de imagem carregado.</param>
        /// <param name="isEdit">Indica se é uma operação de edição.</param>
        /// <returns>Entidade Airline preenchida.</returns>
        Airline ToAirline(AirlineViewModel model, Guid imageId, bool isEdit = false);

        /// <summary>
        /// Converte uma entidade <see cref="Airline"/> para um <see cref="AirlineViewModel"/>.
        /// </summary>
        /// <param name="airline">Entidade da companhia aérea.</param>
        /// <returns>ViewModel preenchida para exibição em formulários.</returns>
        AirlineViewModel ToAirlineViewModel(Airline airline);

        /// <summary>
        /// Atualiza as propriedades de uma entidade <see cref="Airline"/> a partir do <see cref="AirlineViewModel"/>.
        /// </summary>
        /// <param name="airline">Entidade a ser atualizada.</param>
        /// <param name="model">ViewModel com os novos dados.</param>
        /// <param name="imageId">ID da imagem (nova ou existente).</param>
        void UpdateAirlineFromViewModel(Airline airline, AirlineViewModel model, Guid imageId);

        /// <summary>
        /// Converte um <see cref="AirportViewModel"/> para a entidade <see cref="Airport"/>.
        /// </summary>
        /// <param name="model">ViewModel com os dados do aeroporto.</param>
        /// <param name="imageId">ID da imagem associada.</param>
        /// <param name="isEdit">Indica se a operação é de edição.</param>
        /// <returns>Entidade Airport preenchida.</returns>
        Airport ToAirport(AirportViewModel model, Guid imageId, bool isEdit = false);

        /// <summary>
        /// Converte uma entidade <see cref="Airport"/> para um <see cref="AirportViewModel"/>.
        /// </summary>
        /// <param name="airport">Entidade do aeroporto.</param>
        /// <returns>ViewModel preenchido para edição na View.</returns>
        AirportViewModel ToAirportViewModel(Airport airport);

        /// <summary>
        /// Atualiza os campos da entidade <see cref="Airport"/> com os dados do <see cref="AirportViewModel"/>.
        /// </summary>
        /// <param name="airport">Entidade a atualizar.</param>
        /// <param name="model">ViewModel com os novos dados.</param>
        /// <param name="imageId">ID da imagem atualizada ou existente.</param>
        void UpdateAirportFromViewModel(Airport airport, AirportViewModel model, Guid imageId);

        /// <summary>
        /// Cria e preenche uma entidade <see cref="BoardingPass"/> com base no bilhete, sequência e porta de embarque.
        /// </summary>
        /// <param name="ticket">Bilhete associado ao cartão de embarque.</param>
        /// <param name="sequenceNumber">Número de sequência no voo.</param>
        /// <param name="gate">Porta de embarque (opcional, padrão 'TBA').</param>
        /// <param name="prefix">Prefixo para geração do QRCode (ex: 'BOARDING' ou 'DESK').</param>
        /// <returns>Entidade BoardingPass pronta a ser persistida.</returns>
        BoardingPass ToBoardingPass(Ticket ticket, int sequenceNumber, string? gate = null, string prefix = "BOARDING");

        /// <summary>
        /// Converte um modelo de vista de voo (FlightViewModel) para a entidade do domínio (Flight).
        /// </summary>
        /// <param name="model">ViewModel com os dados do formulário de voo.</param>
        /// <param name="isEdit">Indica se a conversão é para atualização de um registo existente (true) ou criação (false).</param>
        /// <returns>A entidade <see cref="Flight"/> preenchida.</returns>
        Flight ToFlight(FlightViewModel model, bool isEdit);

        /// <summary>
        /// Converte uma entidade de voo (Flight) para o seu respetivo modelo de vista (FlightViewModel).
        /// </summary>
        /// <param name="flight">Entidade de voo vinda da base de dados.</param>
        /// <returns>O <see cref="FlightViewModel"/> preenchido para apresentação/edição nas views.</returns>
        FlightViewModel ToFlightViewModel(Flight flight);

        /// <summary>
        /// Converte um modelo de vista de voo (FlightViewModel) para a entidade do domínio (Flight).
        /// </summary>
      

        /// <summary>
        /// Gera a lista de SelectListItem para Companhias Aéreas.
        /// </summary>
        IEnumerable<SelectListItem> ToComboAirlines(IEnumerable<Airline> airlines, int? selectedId = null);

        /// <summary>
        /// Gera a lista de SelectListItem para Aeroportos.
        /// </summary>
        IEnumerable<SelectListItem> ToComboAirports(IEnumerable<Airport> airports, int? selectedId = null);

        /// <summary>
        /// Gera a lista de SelectListItem para Aeronaves.
        /// </summary>
        IEnumerable<SelectListItem> ToComboAircrafts(IEnumerable<Aircraft> aircrafts, int? selectedId = null);

        /// <summary>
        /// Gera a lista de SelectListItem para Gates.
        /// </summary>
        IEnumerable<SelectListItem> ToComboGates(IEnumerable<Gate> gates, int? selectedId = null);

        /// <summary>
        /// Gera a lista de SelectListItem para os Estados Operacionais do voo.
        /// </summary>
        IEnumerable<SelectListItem> ToComboStatuses(string? selectedStatus = null);

        /// <summary>
        /// Gera dinamicamente o mapa de lugares para um voo com base na capacidade da aeronave.
        /// </summary>
        List<Seat> GenerateSeatsFromAircraftCapacity(Aircraft? aircraft, decimal basePrice);


        /// <summary>
        /// Converte uma entidade ForumTopic para a sua ViewModel correspondente.
        /// </summary>
        ForumTopicViewModel ToForumTopicViewModel(ForumTopic topic);

        /// <summary>
        /// Converte uma ViewModel de tópico para a entidade ForumTopic.
        /// </summary>
        ForumTopic ToForumTopic(ForumTopicViewModel model, string userId, bool isEdit);

        /// <summary>
        /// Converte a entrada de um formulário de comentário na entidade ForumComment.
        /// </summary>
        ForumComment ToForumComment(int topicId, string content, string userId);

        /// <summary>
        /// Converte uma entidade Gate para GateViewModel.
        /// </summary>
        GateViewModel ToGateViewModel(Gate gate);

        /// <summary>
        /// Converte uma GateViewModel para a entidade Gate (para criar ou atualizar).
        /// </summary>
        Gate ToGate(GateViewModel model, bool isEdit);

        /// <summary>
        /// Converte uma entidade Notification para a ViewModel.
        /// </summary>
        NotificationViewModel ToNotificationViewModel(Notification notification);

        /// <summary>
        /// Converte uma coleção de entidades Notification para uma lista de ViewModels.
        /// </summary>
        IEnumerable<NotificationViewModel> ToNotificationViewModelList(IEnumerable<Notification> notifications);

        /// <summary>
        /// Converte uma entidade Passenger para PassengerViewModel.
        /// </summary>
        PassengerViewModel ToPassengerViewModel(Passenger passenger);

        /// <summary>
        /// Converte a PassengerViewModel para a entidade Passenger.
        /// </summary>
        Passenger ToPassenger(PassengerViewModel model, Guid imageId, bool isEdit);

        /// <summary>
        /// Devolve a lista de opções para o dropdown de tipos de documento.
        /// </summary>
        IEnumerable<SelectListItem> GetDocumentTypes();


        /// <summary>
        /// Gera a SelectList de aeroportos para os filtros da loja.
        /// </summary>
        SelectList ToAirportSelectList(IEnumerable<Airport> airports, string? selectedValue = null);

      

        /// <summary>
        /// Converte uma entidade User e o seu nome de role no modelo de visualização UserWithRole.
        /// </summary>
        UserWithRole ToUserWithRole(User user, string role);

        /// <summary>
        /// Cria a lista de opções de Roles (Funcionário / Administrador) para a View.
        /// </summary>
        List<SelectListItem> ToRoleSelectList();

        /// <summary>
        /// Instancia um novo User com os dados fornecidos.
        /// </summary>
        User ToUser(string email, string firstName, string lastName);


        /// <summary>
        /// Gera a lista de marcas/fabricantes de aeronaves válidos.
        /// </summary>
        IEnumerable<SelectListItem> GetAircraftBrands(string? selectedBrand = null);

        /// <summary>
        /// Gera a lista de países válidos.
        /// </summary>
        IEnumerable<SelectListItem> GetCountries(string? selectedCountry = null);

        /// <summary>
        /// Gera a lista de cidades válidas com base no país (opcional).
        /// </summary>
        IEnumerable<SelectListItem> GetCities(string? selectedCountry = null, string? selectedCity = null);

        IEnumerable<SelectListItem> GetAircraftModels(string? selectedBrand = null, string? selectedModel = null);

        IEnumerable<object> GetCitiesWithIata(string? selectedCountry = null);

        public Ticket ToTicket(int flightId, int seatId, Passenger passenger, bool extraLuggage, bool mealIncluded, decimal price);




        /// <summary>
        /// Retorna a lista de países disponíveis no catálogo imutável.
        /// </summary>
        IEnumerable<SelectListItem> GetCountries();

        /// <summary>
        /// Retorna as companhias aéreas associadas a um país.
        /// </summary>
        IEnumerable<object> GetAirlinesByCountry(string? country);


        /// <summary>
        /// Devolve o texto em português correspondente ao estado do voo.
        /// </summary>
        /// <param name="status">Estado do voo em inglês.</param>
        /// <returns>Texto traduzido para português.</returns>
        string GetFlightStatusText(string status);

        /// <summary>
        /// Devolve a classe CSS do Bootstrap para colorir o badge do estado do voo.
        /// </summary>
        /// <param name="status">Estado do voo em inglês.</param>
        /// <returns>Classe CSS do Bootstrap.</returns>
        string GetFlightBadgeClass(string status);

        /// <summary>
        /// Devolve a classe CSS para destacar a linha da tabela consoante o estado do voo.
        /// </summary>
        /// <param name="status">Estado do voo em inglês.</param>
        /// <returns>Classe CSS do Bootstrap para a linha da tabela.</returns>
        string GetFlightRowClass(string status);



    }

}

