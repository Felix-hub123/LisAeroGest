namespace LisAeroGest.Helpers
{
    public static class FlightStatusHelper
    {
        /// <summary>
        /// Devolve o texto em português correspondente ao estado do voo.
        /// </summary>
        public static string GetStatusText(string status) => status switch
        {
            "Scheduled" => "Previsto",
            "CheckIn" => "Check-in",
            "Boarding" => "A Embarcar",
            "Departed" => "Partiu",
            "Delayed" => "Atrasado",
            "Cancelled" => "Cancelado",
            _ => status
        };

        /// <summary>
        /// Devolve a classe CSS do Bootstrap para colorir o badge do estado.
        /// </summary>
        public static string GetBadgeClass(string status) => status switch
        {
            "Scheduled" => "bg-primary",
            "CheckIn" => "bg-info text-dark",
            "Boarding" => "bg-success",
            "Departed" => "bg-secondary",
            "Delayed" => "bg-warning text-dark",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };

        /// <summary>
        /// Devolve a classe CSS para destacar a linha da tabela
        /// consoante o estado do voo (ex: vermelho para cancelado).
        /// </summary>
        public static string GetRowClass(string status) => status switch
        {
            "Cancelled" => "table-danger",
            "Delayed" => "table-warning",
            "Boarding" => "table-success",
            _ => ""
        };
    }
}
