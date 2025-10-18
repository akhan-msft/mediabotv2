# GitHub Copilot Instructions for MediaBot v2

## Project Overview
This is a Microsoft Teams Media Bot built with .NET 8 that joins Teams meetings and logs meeting events to the console. The bot uses Microsoft Graph Communications APIs to interact with Teams meetings and handle real-time media streams.

## Technology Stack
- **.NET 8.0** - Target framework
- **ASP.NET Core** - Web framework with Kestrel hosting
- **Microsoft.Graph.Communications** - For Teams meeting integration
- **Microsoft Graph API** - For authentication and permissions
- **Azure Identity** - For authentication
- **Swagger/OpenAPI** - API documentation

## Code Style and Standards

### General C# Guidelines
- Use C# 12 features and modern syntax
- Enable nullable reference types (already configured in project)
- Use implicit usings (already configured in project)
- Follow Microsoft's C# coding conventions
- Use `var` for type declarations when the type is obvious
- Prefer async/await over synchronous code
- Use meaningful variable and method names

### Naming Conventions
- **Classes**: PascalCase (e.g., `BotService`, `CallHandler`)
- **Interfaces**: Prefix with `I` (e.g., `IBotService`, `IEventLogger`)
- **Methods**: PascalCase (e.g., `InitializeAsync`, `HandleIncomingCallAsync`)
- **Private fields**: `_camelCase` with underscore prefix (e.g., `_eventLogger`, `_configuration`)
- **Properties**: PascalCase (e.g., `IsInitialized`, `CommunicationsClient`)
- **Local variables**: camelCase (e.g., `appId`, `callId`)

### Project Structure
```
MediaBot/
├── Program.cs              # Application entry point
├── Startup.cs              # Service configuration and middleware
├── Controllers/            # API controllers for HTTP endpoints
│   ├── CallbackController.cs
│   └── HealthController.cs
├── Services/               # Business logic and bot services
│   ├── BotService.cs
│   ├── CallHandler.cs
│   ├── EventLogger.cs
│   └── GraphAuthProvider.cs
├── Interfaces/             # Service interfaces for dependency injection
├── Models/                 # Data models
└── appsettings.json        # Configuration (non-sensitive values only)
```

## Dependencies and Libraries

### When Adding NuGet Packages
- Always use the latest stable versions unless compatibility issues exist
- Prefer official Microsoft packages for Azure and Graph services
- Check for security vulnerabilities before adding packages
- Update package references in `MediaBot.csproj`

### Key Dependencies
- `Microsoft.Graph.Communications.Calls` - Core bot functionality
- `Microsoft.Graph.Communications.Calls.Media` - Media stream handling
- `Azure.Identity` - Azure authentication
- `Microsoft.Graph` - Graph API client
- `Swashbuckle.AspNetCore` - API documentation

## Dependency Injection

### Service Registration
- Register services in `Startup.ConfigureServices()`
- Use appropriate lifetimes:
  - `AddSingleton` - For stateful services like BotService
  - `AddScoped` - For per-request services
  - `AddTransient` - For stateless, lightweight services
- Always register both interface and implementation when needed

### Example Pattern
```csharp
services.AddSingleton<IEventLogger, EventLogger>();
services.AddSingleton<IBotService, BotService>();
```

## Configuration

### appsettings.json Structure
- **Never commit sensitive values** (App IDs, secrets, certificates)
- Use hierarchical configuration sections:
  ```json
  {
    "Bot": {
      "AppId": "...",
      "AppSecret": "...",
      "TenantId": "...",
      "BaseUrl": "..."
    }
  }
  ```
- Keep development settings in `appsettings.Development.json`
- Production secrets should use Azure Key Vault or environment variables

### Environment-Specific Files
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings (gitignored)
- `appsettings.Staging.json` - Staging settings (gitignored)

## Logging

### Console Logging Standards
- Use structured logging with `ILogger<T>`
- Log levels:
  - **Information**: Bot initialization, meeting joins, participant changes
  - **Warning**: Recoverable issues, missing configurations
  - **Error**: Exceptions, failures, critical issues
  - **Debug**: Detailed diagnostic information

### EventLogger Service
- Use `IEventLogger` for bot-specific events
- Events are color-coded for console output
- Include contextual information in event metadata

### Example
```csharp
_eventLogger.LogEvent(
    "BotJoined",
    callId,
    "Bot successfully joined the meeting",
    new Dictionary<string, object> { ["MeetingId"] = meetingId }
);
```

## API Endpoints

### Controller Guidelines
- Inherit from `ControllerBase`
- Use `[ApiController]` attribute
- Use `[Route("api/[controller]")]` for routing
- Return `IActionResult` or `ActionResult<T>`
- Use appropriate HTTP status codes

### Endpoint Conventions
- `POST /api/callback/incoming` - Teams callback for incoming calls
- `POST /api/callback/join` - Manual join endpoint
- `GET /api/health` - Health check
- `GET /api/health/status` - Detailed status

## Error Handling

### Best Practices
- Always use try-catch blocks in controllers and services
- Log exceptions with full context
- Return appropriate HTTP status codes
- Include user-friendly error messages
- Never expose sensitive information in error responses

### Example Pattern
```csharp
try
{
    // Operation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    return StatusCode(500, new { message = "Operation failed", error = ex.Message });
}
```

## Testing

### Current State
- No automated tests currently exist in the project
- Manual testing is done via API endpoints
- Test bot functionality using Teams meetings

### When Adding Tests
- Use xUnit as the testing framework
- Create a separate test project (MediaBot.Tests)
- Use Moq for mocking dependencies
- Test services in isolation using interfaces

## Security

### Important Security Rules
- **NEVER commit secrets** (API keys, passwords, certificates)
- Use `.gitignore` to exclude sensitive files:
  - `*.pfx`, `*.pem`, `*.key` (certificates)
  - `appsettings.Production.json`, `appsettings.Staging.json`
  - `.env` files
- Use Azure Key Vault for production secrets
- Validate and sanitize all user inputs
- Use HTTPS for all communications

### Authentication
- Bot uses Azure AD App Registration
- Requires Graph API permissions:
  - `Calls.AccessMedia.All`
  - `Calls.JoinGroupCall.All`
  - `Calls.Initiate.All`
  - `OnlineMeetings.Read.All`

## Deployment

### Target Environment
- Windows Server 2022 VM (required for media bot capabilities)
- .NET 8 Runtime required
- HTTPS with valid SSL certificate
- Public IP address for bot endpoint

### Build and Run
```bash
# Restore packages
dotnet restore MediaBot.csproj

# Build
dotnet build MediaBot.csproj

# Run
dotnet run --project MediaBot.csproj
```

## Comments and Documentation

### When to Add Comments
- Complex business logic that isn't obvious
- Workarounds for known issues
- TODO items for future improvements
- XML documentation comments for public APIs

### When NOT to Add Comments
- Don't state the obvious
- Don't duplicate what the code clearly shows
- Don't add comments for every method if the name is self-explanatory

### Example of Good Documentation
```csharp
/// <summary>
/// Joins a Teams meeting using the provided meeting URL.
/// </summary>
/// <param name="meetingUrl">The Teams meeting join URL</param>
/// <returns>The call ID if successful</returns>
/// <exception cref="InvalidOperationException">Thrown if bot is not initialized</exception>
public async Task<string> JoinMeetingAsync(string meetingUrl)
```

## Common Patterns in This Project

### Async/Await
- All I/O operations use async/await
- Methods return `Task` or `Task<T>`
- Use `ConfigureAwait(false)` for library code (not needed in ASP.NET Core)

### Dependency Injection
- Constructor injection is used throughout
- Services are registered in `Startup.cs`
- Interfaces abstract implementation details

### Configuration
- `IConfiguration` is injected where needed
- Access configuration using sections: `_configuration["Bot:AppId"]`

## Troubleshooting

### Common Issues
1. **Bot not initializing**: Check configuration values in appsettings.json
2. **Cannot join meetings**: Verify Azure AD permissions and consent
3. **SSL/Certificate errors**: Ensure valid certificate is configured
4. **Media errors**: Must run on Windows Server with Media Foundation

## Additional Resources
- [Microsoft Graph Communications SDK](https://github.com/microsoftgraph/microsoft-graph-comms-samples)
- [Teams Bot Documentation](https://learn.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots)
- [Azure Bot Service](https://learn.microsoft.com/en-us/azure/bot-service/)

## Special Notes for Copilot
- This is a Phase 1 implementation focused on event logging
- Future phases will add transcription and advanced media processing
- Windows Server environment is mandatory for media bot functionality
- The bot requires proper Azure setup (VM, Bot Service, App Registration)
