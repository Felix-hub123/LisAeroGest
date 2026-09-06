using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Dados da página de pesquisa de voos (Shop/Index).
    /// Critérios da pesquisa + resultados.
    /// </summary>
    public class ShopIndexViewModel
    {
        /// <summary>
        /// Código IATA ou texto da origem.
        /// </summary>
        [Display(Name = "Origem")]
        public string? Origin { get; set; }

        /// <summary>
        /// Destino (cidade, IATA ou "Cidade (IATA)").
        /// </summary>
        [Display(Name = "Destino")]
        public string? Destination { get; set; }

        /// <summary>
        /// Data de ida (yyyy-MM-dd).
        /// </summary>
        [Display(Name = "Ida")]
        public string? Date { get; set; }

        /// <summary>
        /// Data de volta (yyyy-MM-dd).
        /// </summary>
        [Display(Name = "Volta")]
        public string? ReturnDate { get; set; }

        /// <summary>
        /// Número de passageiros.
        /// </summary>
        [Display(Name = "Passageiros")]
        public int Passengers { get; set; } = 1;

        /// <summary>
        /// Tipo de viagem: round ou oneway.
        /// </summary>
        [Display(Name = "Tipo de Viagem")]
        public string TripType { get; set; } = "oneway";

        /// <summary>
        /// Classe: Economy, Business ou First.
        /// </summary>
        [Display(Name = "Classe")]
        public string CabinClass { get; set; } = "Economy";

        /// <summary>
        /// Filtrar apenas voos diretos.
        /// </summary>
        [Display(Name = "Apenas voos diretos")]
        public bool DirectOnly { get; set; }

        /// <summary>
        /// Lista de aeroportos para o select da origem.
        /// </summary>
        public IEnumerable<SelectListItem> Airports { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Resultados da pesquisa.
        /// </summary>
        public List<FlightSearchItemViewModel> Flights { get; set; } = new();

        /// <summary>
        /// Número de voos encontrados.
        /// </summary>
        [Display(Name = "Resultados")]
        public int ResultCount => Flights.Count;

        /// <summary>
        /// Indica se o utilizador aplicou algum filtro.
        /// </summary>
        public bool HasFilters =>
            !string.IsNullOrWhiteSpace(Origin) ||
            !string.IsNullOrWhiteSpace(Destination) ||
            !string.IsNullOrWhiteSpace(Date);

        /// <summary>
        /// Rótulo da classe em português.
        /// </summary>
        [Display(Name = "Classe")]
        public string CabinClassLabel => CabinClass switch
        {
            "Business" => "Executiva",
            "First" => "Primeira",
            _ => "Económica"
        };

        /// <summary>
        /// Texto do tipo de viagem.
        /// </summary>
        [Display(Name = "Tipo de Viagem")]
        public string TripTypeLabel => TripType == "round" ? "Ida e volta" : "Só ida";

        /// <summary>
        /// Texto de passageiros (1 passageiro / N passageiros).
        /// </summary>
        [Display(Name = "Passageiros")]
        public string PassengersLabel =>
            Passengers == 1 ? "1 passageiro" : $"{Passengers} passageiros";
    }
}