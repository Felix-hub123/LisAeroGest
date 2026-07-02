namespace LisAeroGest.Models
{
    public class ErrorViewModel
    {
        /// <summary>
        /// Identificador único do pedido HTTP que gerou o erro.
        /// Usado para correlacionar o erro nos logs da aplicação.
        /// </summary>
        /// <param name="RequestId">ID gerado automaticamente pelo ASP.NET para cada pedido.</param>
        /// <returns>
        /// <see cref="string"/> com o ID do pedido, ou <c>null</c>
        /// se o erro ocorreu fora do contexto de um pedido HTTP.
        /// </returns>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indica se o RequestId deve ser mostrado na página de erro.
        /// Devolve true apenas quando o RequestId não é nulo nem vazio.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
