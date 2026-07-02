using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{

    /// <summary>
    /// ViewModel para o painel de partidas e chegadas da página inicial.
    /// </summary>
    public class HomeBoardViewModel
    {
        /// <summary>
        /// Lista de voos a partir do aeroporto de Lisboa hoje.
        /// </summary>
        public IEnumerable<Flight> Departures { get; set; } = new List<Flight>();

        /// <summary>
        /// Lista de voos a chegar ao aeroporto de Lisboa hoje.
        /// </summary>
        public IEnumerable<Flight> Arrivals { get; set; } = new List<Flight>();

        /// <summary>
        /// Número de voos atualmente em operação (a embarcar ou já partidos).
        /// </summary>
        public int ActiveFlightsCount { get; set; }

        /// <summary>
        /// Número de voos atrasados ou cancelados.
        /// </summary>
        public int DisruptedFlightsCount { get; set; }
    }
}
