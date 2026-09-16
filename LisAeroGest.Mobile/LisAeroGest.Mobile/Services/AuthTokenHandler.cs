using System.Net;
using System.Net.Http.Headers;
using Microsoft.Maui.Storage;

namespace LisAeroGest.Mobile.Services
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private const string TokenKey = "jwt_token";

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

            var response = await base.SendAsync(
                request,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                SecureStorage.Remove(TokenKey);
            }

            return response;
        }
    }
}
