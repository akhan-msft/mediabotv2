using Microsoft.Graph.Communications.Client;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Authentication;
using MediaBot.Interfaces;
using System.Diagnostics;
using Azure.Identity;
using Microsoft.Graph;

namespace MediaBot.Services
{
    public class BotService : IBotService
    {
        private readonly IEventLogger _eventLogger;
        private readonly ILogger<BotService> _logger;
        private readonly IConfiguration _configuration;
        private ICommunicationsClient? _communicationsClient;
        private GraphServiceClient? _graphServiceClient;
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;
        public ICommunicationsClient? CommunicationsClient => _communicationsClient;
        public GraphServiceClient? GraphServiceClient => _graphServiceClient;

        public BotService(
            IEventLogger eventLogger, 
            ILogger<BotService> logger, 
            IConfiguration configuration)
        {
            _eventLogger = eventLogger;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _eventLogger.LogEvent(
                    "BotInitializing",
                    "system",
                    "Initializing Teams Media Bot..."
                );

                // Validate configuration
                var appId = _configuration["Bot:AppId"];
                var appSecret = _configuration["Bot:AppSecret"];
                var tenantId = _configuration["Bot:TenantId"];
                var baseUrl = _configuration["Bot:BaseUrl"];

                if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret) || 
                    string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(baseUrl))
                {
                    throw new InvalidOperationException("Bot configuration is incomplete. Check appsettings.json");
                }

                _eventLogger.LogEvent(
                    "BotConfigurationValidated",
                    "system",
                    "Bot configuration validated successfully",
                    new Dictionary<string, object>
                    {
                        ["AppId"] = appId,
                        ["TenantId"] = tenantId,
                        ["BaseUrl"] = baseUrl
                    }
                );

                // Initialize Graph Communications Client
                await InitializeCommunicationsClientAsync(appId, appSecret, tenantId, baseUrl);
                
                // Initialize Graph Service Client for v5 SDK
                await InitializeGraphServiceClientAsync(appId, appSecret, tenantId);
                
                _eventLogger.LogEvent(
                    "BotInitialized",
                    "system", 
                    "Teams Media Bot initialized successfully"
                );

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _eventLogger.LogEvent(
                    "BotInitializationFailed",
                    "system",
                    $"Failed to initialize bot: {ex.Message}",
                    new Dictionary<string, object> { ["Error"] = ex.ToString() }
                );
                throw;
            }
        }

        private async Task InitializeCommunicationsClientAsync(string appId, string appSecret, string tenantId, string baseUrl)
        {
            try
            {
                _eventLogger.LogEvent(
                    "InitializingGraphClient",
                    "system",
                    "Initializing Microsoft Graph Communications Client..."
                );

                // Create authentication provider
                var authProvider = new GraphAuthProvider(tenantId, appId, appSecret);

                // Create the communications client with authentication
                var builder = new CommunicationsClientBuilder("MediaBot", appId)
                    .SetServiceBaseUrl(new Uri("https://graph.microsoft.com/beta"))
                    .SetNotificationUrl(new Uri($"{baseUrl}/api/callback/notifications"))
                    .SetAuthenticationProvider(authProvider);

                _communicationsClient = builder.Build();

                _eventLogger.LogEvent(
                    "GraphClientInitialized",
                    "system",
                    "Microsoft Graph Communications Client initialized successfully"
                );

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _eventLogger.LogEvent(
                    "GraphClientInitializationFailed",
                    "system",
                    $"Failed to initialize Graph Communications client: {ex.Message}",
                    new Dictionary<string, object> { ["Error"] = ex.ToString() }
                );
                throw;
            }
        }

        private async Task InitializeGraphServiceClientAsync(string appId, string appSecret, string tenantId)
        {
            try
            {
                _eventLogger.LogEvent(
                    "InitializingGraphServiceClient",
                    "system",
                    "Initializing Microsoft Graph Service Client v5..."
                );

                // Create ClientSecretCredential for multi-tenant authentication
                var options = new ClientSecretCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };
                var credential = new ClientSecretCredential("common", appId, appSecret, options);

                // Create GraphServiceClient with the credential
                _graphServiceClient = new GraphServiceClient(credential);

                _eventLogger.LogEvent(
                    "GraphServiceClientInitialized",
                    "system",
                    "Microsoft Graph Service Client v5 initialized successfully"
                );

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _eventLogger.LogEvent(
                    "GraphServiceClientInitializationFailed",
                    "system",
                    $"Failed to initialize Graph Service client: {ex.Message}",
                    new Dictionary<string, object> { ["Error"] = ex.ToString() }
                );
                throw;
            }
        }

        public async Task StartAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            _eventLogger.LogEvent(
                "BotStarted",
                "system",
                "Teams Media Bot service started and ready to receive calls"
            );

            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            _eventLogger.LogEvent(
                "BotStopping",
                "system",
                "Teams Media Bot service is stopping..."
            );

            // Cleanup communications client and active calls
            _communicationsClient?.Dispose();

            _eventLogger.LogEvent(
                "BotStopped",
                "system",
                "Teams Media Bot service stopped"
            );

            await Task.CompletedTask;
        }
    }
}