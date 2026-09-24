using System.Net;
using System.Net.Http.Headers;

namespace LisAeroGest.Mobile.Services
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private const string TokenKey = "jwt_token";

        /// <summary>
        /// A AppShell subscreve este evento para forçar logout quando a API devolve 401.
        /// </summary>
        public static event Func<Task>? UnauthorizedDetected;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await SecureStorage.GetAsync(TokenKey);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                SecureStorage.Remove(TokenKey);
                SecureStorage.Remove("auth_role");
                SecureStorage.Remove("auth_email");

                var handler = UnauthorizedDetected;
                if (handler != null)
                    _ = handler.Invoke();
            }

            return response;
        }
    }
}