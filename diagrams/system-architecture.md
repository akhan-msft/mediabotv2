# Teams Media Bot - System Architecture

This diagram shows the high-level architecture and component relationships of the MediaBot system.

```mermaid
graph TB
    subgraph "User Environment"
        User["👤 User"]
        TeamsClient["🟦 MS Teams Client"]
    end
    
    subgraph "Microsoft Cloud Services"
        BotService["🤖 Azure Bot Service<br/>Bot Registration"]
        GraphAuth["🔐 Microsoft Graph Auth<br/>Azure AD"]
        GraphComms["📊 MS Graph Communications<br/>Calling API"]
        TeamsService["🔵 Microsoft Teams Service"]
    end
    
    subgraph "VM Infrastructure"
        direction TB
        WebServer["🌐 ASP.NET Core Web Server<br/>Port 443 HTTPS"]
        BotServiceApp["🤖 Bot Service Application<br/>MediaBot.Services.BotService"]
        CallHandler["📞 Call Handler<br/>MediaBot.Services.CallHandler"] 
        AuthProvider["🔑 Graph Auth Provider<br/>MSAL Authentication"]
        EventLogger["📝 Event Logger<br/>Structured Logging"]
        
        WebServer --> BotServiceApp
        BotServiceApp --> CallHandler
        BotServiceApp --> AuthProvider
        BotServiceApp --> EventLogger
    end
    
    subgraph "Configuration"
        SSL["🔒 SSL Certificates<br/>demobot.site-chain.pem"]
        Config["⚙️ Configuration<br/>appsettings.json"]
        Secrets["🔐 App Secrets<br/>ClientId, ClientSecret, TenantId"]
    end
    
    %% User Interactions
    User --> TeamsClient
    TeamsClient --> TeamsService
    
    %% Teams Integration
    TeamsService --> BotService
    BotService --> WebServer : "Webhook: /api/callback"
    
    %% Authentication Flow
    AuthProvider --> GraphAuth : "Client Credentials Flow"
    GraphAuth --> AuthProvider : "Bearer Token"
    
    %% Graph Communications
    CallHandler --> GraphComms : "CreateCall / JoinMeeting"
    GraphComms --> CallHandler : "Call Events & Media"
    GraphComms --> WebServer : "Notifications: /api/callback/notifications"
    
    %% Configuration
    Config --> BotServiceApp
    Secrets --> AuthProvider
    SSL --> WebServer
    
    %% Data Flow Annotations
    BotService -.->|"Meeting Invitations<br/>Bot Framework Protocol"| WebServer
    User -.->|"Manual Join Commands<br/>curl/PowerShell"| WebServer
    GraphComms -.->|"Real-time Call Events<br/>Audio/Video Streams"| CallHandler
    
    classDef implemented fill:#90EE90,stroke:#333,stroke-width:2px
    classDef ready fill:#FFE4B5,stroke:#333,stroke-width:2px  
    classDef future fill:#E6E6FA,stroke:#333,stroke-width:2px
    
    class WebServer,BotServiceApp,AuthProvider,EventLogger,SSL,Config implemented
    class CallHandler ready
    class GraphComms future
```

## Component Status Legend:

🟢 **Implemented & Working** - Authentication, webhooks, logging, SSL
🟡 **Ready for Implementation** - Graph Communications join logic  
🟣 **Future Enhancement** - Advanced media processing, transcription

## Key Integration Points:

### 1. **Teams → Bot Service → VM**
- Teams sends meeting invitations to Azure Bot Service
- Bot Service forwards to VM webhook `/api/callback`
- VM processes and logs the invitation

### 2. **VM → Graph Authentication → Graph Communications**  
- VM authenticates with Azure AD using client credentials
- Receives bearer token for Graph Communications API
- Makes API calls to join Teams meetings

### 3. **Graph Communications → Teams Service**
- Graph Communications API interfaces with Teams backend
- Bot appears as participant in Teams meeting
- Real-time media and events flow back to VM

## Current Implementation Gap:

The **red arrow** in the sequence diagram shows where we need to implement the actual `CreateCall` logic in the `CallHandler.JoinMeetingAsync()` method.

---

Please review these diagrams and let me know if they accurately represent your understanding of the system architecture. Once you approve, I'll proceed with implementing the actual meeting join functionality! 🚀