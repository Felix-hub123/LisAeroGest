using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LisAeroGest.Mobile.Models;

namespace LisAeroGest.Mobile.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string TokenKey = "jwt_token";
        private const string RoleKey = "auth_role";
        private const string EmailKey = "auth_email";

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/Auth/login",
                    new LoginDto { Email = username, Password = password });

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[AuthService] Login falhou: {(int)response.StatusCode}");
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                    return false;

                await SaveSessionAsync(result.Token, result.Email, result.Roles, username);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] Erro no login: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(
            string firstName, string lastName, string email,
            string password, string documentNumber)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/Auth/register",
                    new RegisterRequestDto
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        Password = password,
                        DocumentNumber = documentNumber,
                        DocumentType = "CC"
                    });

                if (!response.IsSuccessStatusCode)
                    return (false, "Não foi possível criar a conta.");

                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                    return (false, "Conta criada, mas sem token.");

                await SaveSessionAsync(result.Token, result.Email, result.Roles, email);
                return (true, null);
            }
            catch
            {
                return (false, "Erro de ligação. Verifique a internet.");
            }
        }

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
            catch
            {
                Logout();
                return false;
            }
        }

        public Task<string?> GetTokenAsync() => SecureStorage.GetAsync(TokenKey);
        public Task<string?> GetRoleAsync() => SecureStorage.GetAsync(RoleKey);
        public Task<string?> GetEmailAsync() => SecureStorage.GetAsync(EmailKey);

        public void Logout()
        {
            SecureStorage.Remove(TokenKey);
            SecureStorage.Remove(RoleKey);
            SecureStorage.Remove(EmailKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private async Task SaveSessionAsync(
            string token, string? email, List<string>? roles, string fallbackEmail)
        {
            await SecureStorage.SetAsync(TokenKey, token);
            await SecureStorage.SetAsync(EmailKey,
                string.IsNullOrWhiteSpace(email) ? fallbackEmail : email);

            var role = roles?.FirstOrDefault()
                       ?? ReadRoleFromJwt(token)
                       ?? "Passenger";

            await SecureStorage.SetAsync(RoleKey, role);
            SetAuthorizationHeader(token);
            Debug.WriteLine("ROLE=" + role);
        }

        private static string? ReadRoleFromJwt(string token)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return jwt.Claims.FirstOrDefault(c =>
                    c.Type is "role" or "roles"
                    or "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    ?.Value;
            }
            catch
            {
                return null;
            }
        }

        private void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}

