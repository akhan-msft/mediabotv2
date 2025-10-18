using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace MediaBot.Services
{
    /// <summary>
    /// Authentication provider for Microsoft Graph Communications API requests.
    /// Implements OAuth 2.0 client credentials flow using Azure AD to acquire access tokens
    /// for authenticating outbound Graph API calls from the Media Bot.
    /// </summary>
    public class GraphAuthProvider : IRequestAuthenticationProvider
    {
        private readonly IConfidentialClientApplication _clientApp;
        private readonly string[] _scopes = { "https://graph.microsoft.com/.default" };

        public GraphAuthProvider(string tenantId, string clientId, string clientSecret)
        {
            _clientApp = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority("https://login.microsoftonline.com/common") // Multi-tenant endpoint
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