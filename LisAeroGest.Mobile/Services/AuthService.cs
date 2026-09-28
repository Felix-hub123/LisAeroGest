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


        // =========================================================
        // CHAVES DA SESSÃO
        // =========================================================

        private const string TokenKey =
            "jwt_token";

        private const string RoleKey =
            "auth_role";

        private const string EmailKey =
            "auth_email";


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public AuthService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        // =========================================================
        // LOGIN
        // =========================================================

        public async Task<bool> LoginAsync(
            string username,
            string password)
        {
            try
            {
                var response =
                    await _httpClient.PostAsJsonAsync(
                        "api/Auth/login",
                        new LoginDto
                        {
                            Email = username,
                            Password = password
                        });


                // =================================================
                // LOGIN RECUSADO
                // =================================================

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine(
                        $"[AuthService] Login falhou: " +
                        $"{(int)response.StatusCode}");

                    return false;
                }


                // =================================================
                // LER RESPOSTA
                // =================================================

                var result =
                    await response.Content
                        .ReadFromJsonAsync<LoginResponseDto>();

                if (result == null ||
                    string.IsNullOrWhiteSpace(
                        result.Token))
                {
                    return false;
                }


                // =================================================
                // VALIDAR TOKEN RECEBIDO
                // =================================================

                if (!IsTokenValid(
                        result.Token))
                {
                    Debug.WriteLine(
                        "[AuthService] A API devolveu " +
                        "um token inválido ou expirado.");

                    return false;
                }


                // =================================================
                // GUARDAR SESSÃO
                // =================================================

                await SaveSessionAsync(
                    result.Token,
                    result.Email,
                    result.Roles,
                    username);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AuthService] Erro no login: {ex}");

                return false;
            }
        }


        // =========================================================
        // REGISTO
        // =========================================================

        public async Task<(
            bool Success,
            string? ErrorMessage)> RegisterAsync(
                string firstName,
                string lastName,
                string email,
                string password,
                string documentNumber)
        {
            try
            {
                var response =
                    await _httpClient.PostAsJsonAsync(
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
                {
                    return (
                        false,
                        "Não foi possível criar a conta.");
                }


                var result =
                    await response.Content
                        .ReadFromJsonAsync<LoginResponseDto>();


                if (result == null ||
                    string.IsNullOrWhiteSpace(
                        result.Token))
                {
                    return (
                        false,
                        "Conta criada, mas não foi possível iniciar sessão.");
                }


                if (!IsTokenValid(
                        result.Token))
                {
                    return (
                        false,
                        "Conta criada, mas a sessão recebida é inválida.");
                }


                await SaveSessionAsync(
                    result.Token,
                    result.Email,
                    result.Roles,
                    email);


                return (
                    true,
                    null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AuthService] Erro no registo: {ex}");

                return (
                    false,
                    "Erro de ligação. Verifique a internet.");
            }
        }


        // =========================================================
        // VERIFICAR AUTENTICAÇÃO
        // =========================================================

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var token =
                    await SecureStorage.GetAsync(
                        TokenKey);


                if (string.IsNullOrWhiteSpace(
                    token))
                {
                    return false;
                }


                // =================================================
                // VALIDAR JWT
                // =================================================

                if (!IsTokenValid(token))
                {
                    Logout();

                    return false;
                }


                // =================================================
                // GARANTIR QUE EXISTE ROLE
                // =================================================

                var role =
                    await SecureStorage.GetAsync(
                        RoleKey);


                if (string.IsNullOrWhiteSpace(role))
                {
                    role =
                        ReadRoleFromJwt(token);

                    if (string.IsNullOrWhiteSpace(role))
                    {
                        Logout();

                        return false;
                    }

                    await SecureStorage.SetAsync(
                        RoleKey,
                        role);
                }


                SetAuthorizationHeader(
                    token);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AuthService] Erro ao validar sessão: {ex}");

                Logout();

                return false;
            }
        }


        // =========================================================
        // TOKEN
        // =========================================================

        public Task<string?> GetTokenAsync()
        {
            return SecureStorage.GetAsync(
                TokenKey);
        }


        // =========================================================
        // ROLE
        // =========================================================

        public Task<string?> GetRoleAsync()
        {
            return SecureStorage.GetAsync(
                RoleKey);
        }


        // =========================================================
        // EMAIL
        // =========================================================

        public Task<string?> GetEmailAsync()
        {
            return SecureStorage.GetAsync(
                EmailKey);
        }


        // =========================================================
        // LOGOUT
        // =========================================================

        public void Logout()
        {
            try
            {
                SecureStorage.Remove(
                    TokenKey);

                SecureStorage.Remove(
                    RoleKey);

                SecureStorage.Remove(
                    EmailKey);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AuthService] Erro ao limpar SecureStorage: {ex}");
            }

            _httpClient
                .DefaultRequestHeaders
                .Authorization = null;
        }


        // =========================================================
        // GUARDAR SESSÃO
        // =========================================================

        private async Task SaveSessionAsync(
            string token,
            string? email,
            List<string>? roles,
            string fallbackEmail)
        {
            var role =
                GetSupportedRole(
                    roles,
                    token);


            if (string.IsNullOrWhiteSpace(role))
            {
                throw new InvalidOperationException(
                    "O utilizador não possui um perfil " +
                    "válido para a aplicação mobile.");
            }


            await SecureStorage.SetAsync(
                TokenKey,
                token);


            await SecureStorage.SetAsync(
                EmailKey,
                string.IsNullOrWhiteSpace(email)
                    ? fallbackEmail
                    : email);


            await SecureStorage.SetAsync(
                RoleKey,
                role);


            SetAuthorizationHeader(
                token);


            Debug.WriteLine(
                $"[AuthService] Sessão iniciada. Role={role}");
        }


        // =========================================================
        // ESCOLHER ROLE SUPORTADA
        // =========================================================

        private static string? GetSupportedRole(
            List<string>? roles,
            string token)
        {
            var availableRoles =
                roles?
                    .Where(r =>
                        !string.IsNullOrWhiteSpace(r))
                    .ToList()
                ?? new List<string>();


            // Funcionário tem prioridade se por algum motivo
            // existirem várias roles no token.
            var employee =
                availableRoles.FirstOrDefault(
                    r =>
                        string.Equals(
                            r,
                            "Employee",
                            StringComparison.OrdinalIgnoreCase));


            if (employee != null)
                return "Employee";


            var admin =
                availableRoles.FirstOrDefault(
                    r =>
                        string.Equals(
                            r,
                            "Admin",
                            StringComparison.OrdinalIgnoreCase));


            if (admin != null)
                return "Admin";


            var passenger =
                availableRoles.FirstOrDefault(
                    r =>
                        string.Equals(
                            r,
                            "Passenger",
                            StringComparison.OrdinalIgnoreCase));


            if (passenger != null)
                return "Passenger";


            // Se a API não devolver a lista de roles,
            // tentamos obter a role diretamente do JWT.
            var jwtRole =
                ReadRoleFromJwt(token);


            if (string.Equals(
                    jwtRole,
                    "Employee",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Employee";
            }


            if (string.Equals(
                    jwtRole,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Admin";
            }


            if (string.Equals(
                    jwtRole,
                    "Passenger",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Passenger";
            }


            return null;
        }


        // =========================================================
        // LER ROLE DO JWT
        // =========================================================

        private static string? ReadRoleFromJwt(
            string token)
        {
            try
            {
                var jwt =
                    new JwtSecurityTokenHandler()
                        .ReadJwtToken(token);


                return jwt.Claims
                    .FirstOrDefault(
                        c =>
                            c.Type is
                                "role"
                                or "roles"
                                or "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    ?.Value;
            }
            catch
            {
                return null;
            }
        }


        // =========================================================
        // VALIDAR JWT
        // =========================================================

        private static bool IsTokenValid(
            string token)
        {
            try
            {
                var handler =
                    new JwtSecurityTokenHandler();


                if (!handler.CanReadToken(token))
                    return false;


                var jwt =
                    handler.ReadJwtToken(token);


                // Margem de 30 segundos para evitar utilizar
                // um token que esteja praticamente expirado.
                return jwt.ValidTo >
                       DateTime.UtcNow
                           .AddSeconds(30);
            }
            catch
            {
                return false;
            }
        }


        // =========================================================
        // AUTHORIZATION HEADER
        // =========================================================

        private void SetAuthorizationHeader(
            string token)
        {
            _httpClient
                .DefaultRequestHeaders
                .Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);
        }
    }
}