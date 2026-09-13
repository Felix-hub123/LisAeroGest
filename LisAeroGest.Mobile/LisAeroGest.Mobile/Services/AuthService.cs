using LisAeroGest.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LisAeroGest.Mobile.Services
{
    /// <summary>
    /// Serviço responsável pela autenticação do utilizador na API.
    /// Gere o token JWT e o armazenamento seguro das credenciais.
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string TokenKey = "jwt_token";

        /// <summary>
        /// Inicializa o AuthService com o HttpClient injectado.
        /// </summary>
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Autentica o utilizador na API e guarda o token JWT em SecureStorage.
        /// </summary>
        /// <param name="username">Email do utilizador.</param>
        /// <param name="password">Password do utilizador.</param>
        /// <returns>True se o login foi bem sucedido, False caso contrário.</returns>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var loginDto = new LoginDto
                {
                    Email = username,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        // Guarda o token de forma segura no dispositivo
                        await SecureStorage.SetAsync(TokenKey, result.Token);

                        // Adiciona o token ao header do HttpClient para pedidos futuros
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

                        return true;
                    }
                }

                Debug.WriteLine($"[AuthService] Login falhou: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] Erro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica se existe um token JWT válido guardado no dispositivo.
        /// </summary>
        /// <returns>True se o utilizador está autenticado.</returns>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        /// Obtém o token JWT guardado no dispositivo.
        /// </summary>
        public async Task<string?> GetTokenAsync()
        {
            return await SecureStorage.GetAsync(TokenKey);
        }

        /// <summary>
        /// Regista um novo utilizador na API e guarda o token JWT.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(
            string firstName, string lastName, string email, string password, string documentNumber)
        {
            try
            {
                var registerDto = new RegisterRequestDto
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Password = password,
                    DocumentNumber = documentNumber,
                    DocumentType = "CC"
                };

                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", registerDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        await SecureStorage.SetAsync(TokenKey, result.Token);
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
                        return (true, null);
                    }
                }

                // Ler mensagem de erro da API
                var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
                return (false, error?.Message ?? "Não foi possível criar a conta.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] Erro registo: {ex.Message}");
                return (false, "Erro de ligação. Verifique a internet.");
            }
        }

        /// <summary>
        /// Termina a sessão do utilizador removendo o token do dispositivo.
        /// </summary>
        public void Logout()
        {
            SecureStorage.Remove(TokenKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
