namespace LisAeroGest.Mobile.Models
{
    /// <summary>
    /// Resultado de uma chamada à API.
    /// Distingue sucesso (com dados) de falha (com mensagem de erro).
    /// </summary>
    public class ApiResult<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? ErrorMessage { get; init; }

        public static ApiResult<T> Ok(T data) =>
            new() { Success = true, Data = data };

        public static ApiResult<T> Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }
}