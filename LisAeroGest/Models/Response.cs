namespace LisAeroGest.Models
{

    /// <summary>
    /// Representa o resultado de uma operação,
    /// indicando se foi bem-sucedida e uma mensagem opcional.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Indica se a operação foi concluída com sucesso.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Mensagem descritiva do resultado — erro ou confirmação.
        /// </summary>
        public string? Message { get; set; }
    }
}
