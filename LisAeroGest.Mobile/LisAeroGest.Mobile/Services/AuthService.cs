using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using LisAeroGest.Mobile.Models;

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

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Autentica o utilizador na API e guarda o token JWT.
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var loginDto = new LoginDto
                {
                    Email = username,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "api/Auth/login",
                    loginDto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(
                        $"[AuthService] Login falhou: {(int)response.StatusCode} - {errorBody}");
                    return false;
                }

                var result = await response.Content
                    .ReadFromJsonAsync<LoginResponseDto>();

                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                {
                    Debug.WriteLine("[AuthService] A API não devolveu um token JWT.");
                    return false;
                }

                await SecureStorage.SetAsync(TokenKey, result.Token);
                SetAuthorizationHeader(result.Token);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] Erro no login: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica se existe um token JWT válido e ainda não expirado.
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync(TokenKey);

                if (string.IsNullOrWhiteSpace(token))
                    return false;

                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(token))
                {
                    Logout();
                    return false;
                }

                var jwt = handler.ReadJwtToken(token);

                if (jwt.ValidTo <= DateTime.UtcNow)
                {
                    Logout();
                    return false;
                }

                SetAuthorizationHeader(token);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AuthService] Erro ao validar sessão: {ex.Message}");

                Logout();
                return false;
            }
        }

        /// <summary>
        /// Obtém o token JWT guardado no dispositivo.
        /// </summary>
        public async Task<string?> GetTokenAsync()
        {
            return await SecureStorage.GetAsync(TokenKey);
        }

        /// <summary>
        /// Regista um novo passageiro na API.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            string documentNumber)
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

                var response = await _httpClient.PostAsJsonAsync(
                    "api/Auth/register",
                    registerDto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(
                        $"[AuthService] Registo falhou: {(int)response.StatusCode} - {errorBody}");

                    try
                    {
                        var error = System.Text.Json.JsonSerializer
                            .Deserialize<ErrorResponseDto>(errorBody);

                        return (
                            false,
                            error?.Message ?? "Não foi possível criar a conta."
                        );
                    }
                    catch
                    {
                        return (false, "Não foi possível criar a conta.");
                    }
                }

                var result = await response.Content
                    .ReadFromJsonAsync<LoginResponseDto>();

                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                {
                    return (
                        false,
                        "A conta foi criada, mas a API não devolveu o token."
                    );
                }

                await SecureStorage.SetAsync(TokenKey, result.Token);
                SetAuthorizationHeader(result.Token);

                return (true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] Erro no registo: {ex.Message}");
                return (false, "Erro de ligação. Verifique a internet.");
            }
        }

        /// <summary>
        /// Termina a sessão e remove o token local.
        /// </summary>
        public void Logout()
        {
            SecureStorage.Remove(TokenKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}

