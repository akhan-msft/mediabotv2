using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace MediaBot.Services
{
    public class GraphAuthProvider : IRequestAuthenticationProvider
    {
        private readonly IConfidentialClientApplication _clientApp;
        private readonly string[] _scopes = { "https://graph.microsoft.com/.default" };

        public GraphAuthProvider(string tenantId, string clientId, string clientSecret)
        {
            _clientApp = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                .Build();
        }

        public async Task AuthenticateOutboundRequestAsync(HttpRequestMessage request, string requestUri)
        {
            try
            {
                var result = await _clientApp.AcquireTokenForClient(_scopes).ExecuteAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to acquire authentication token: {ex.Message}", ex);
            }
        }

        public Task<RequestValidationResult> ValidateInboundRequestAsync(HttpRequestMessage request)
        {
            // For outbound requests only, no validation needed for inbound
            return Task.FromResult(new RequestValidationResult { IsValid = true });
        }
    }
}