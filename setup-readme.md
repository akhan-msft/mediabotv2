# MediaBot Phase 1 Setup (Teams Meeting Event Logger)

## Overview
This document outlines the setup and implementation plan for a Microsoft Teams media bot that logs meeting events to the console. The bot will be built with .NET 8, hosted on an Azure VM (Linux or Windows), and will use Kestrel for HTTP hosting. The bot will join scheduled meetings via manual trigger and log relevant events.

---

## 1. Azure Resources Required

### a. Azure Virtual Machine
- **OS:** Windows Server 2019/2022 (REQUIRED for application-hosted media bots)
- **Size:** Standard D2s v3 (2 vCPUs, 8GB RAM) minimum for development/testing
- **Public IP:** Required for bot endpoint
- **Network Security Group:** Allow HTTPS (443) and required media ports

### b. Azure Bot Service
- Register bot for Teams channel
- Set messaging endpoint to VM's public IP (e.g., `https://[VM-IP]/api/callback`)

### c. Azure AD App Registration
- Register app in Azure AD
- Required Microsoft Graph API permissions:
  - `Calls.AccessMedia.All`
  - `Calls.JoinGroupCall.All`
  - `OnlineMeetings.Read.All`
- Generate client secret for authentication

---

## 2. VM Setup
- Deploy Windows Server VM (required for media bot capabilities)
- Install .NET 8 SDK and Runtime
- Install Windows Media Foundation components
- Generate and install a self-signed SSL certificate for HTTPS
- Open required ports in firewall/NSG

---

## 3. Application Architecture

```
MediaBot/
├── Program.cs
├── Startup.cs
├── Controllers/
│   └── CallbackController.cs
├── Services/
│   ├── BotService.cs
│   ├── CallHandler.cs
│   └── EventLogger.cs
├── Models/
│   └── CallEvent.cs
└── appsettings.json
```

---

## 4. Implementation Details

- **Framework:** .NET 8 Console Application
- **Hosting:** Kestrel (no IIS required)
- **Logging:** Console only
- **Bot Join:** Manual trigger to join meetings
- **Certificate:** Self-signed for development/testing

### Key Events to Log
- Bot invited to meeting
- Bot joined meeting
- Participants joined/left
- Audio stream available
- Meeting ended

---

## 5. Deployment Steps

1. Build and publish the .NET 8 console app
2. Copy published files to VM
3. Configure and install self-signed SSL certificate
4. Run the app on VM (as a console app)
5. Register bot endpoint in Azure Bot Service
6. Test event logging by manually triggering bot to join a scheduled Teams meeting

---

## 6. Notes
- **Windows Server VM is REQUIRED** even for Phase 1 due to Microsoft.Graph.Communications.Calls.Media library requirements
- No need for IIS, Application Insights, Service Bus, or Event Hubs for Phase 1
- All logs are written to console for simplicity
- Manual trigger for bot join is implemented for initial testing
- Even basic event handling requires the media library which has Windows dependencies

---

## Azure Setup Steps - Logical Sequence

Here are the **exact steps in logical order** that you need to complete on Azure:

## 🔐 Step 1: Link Accounts & Create App Registration

### A. Link Your Accounts (One-time setup)
1. Sign into **Azure Portal** with `adnan.x.khan@gmail.com`
2. Go to **Subscriptions** → Select your subscription
3. **Access control (IAM)** → **Add role assignment**
4. **Role**: Owner
5. **Assign to**: `akhan@y13nf.onmicrosoft.com`
6. **Save**

### B. Create App Registration
1. **Sign OUT** and sign back in with `akhan@y13nf.onmicrosoft.com`
2. Go to **Azure Active Directory** → **App registrations**
3. Click **"New registration"**
4. **Name**: `MediaBot-TeamsApp`
5. **Supported account types**: Accounts in this organizational directory only
6. Click **"Register"**
7. **Copy and save**: Application (client) ID and Directory (tenant) ID

## 🔑 Step 2: Configure App Permissions & Secrets
1. In your app registration → **API permissions**
2. Click **"Add a permission"** → **Microsoft Graph** → **Application permissions**
3. Add these permissions:
   - `Calls.AccessMedia.All`
   - `Calls.JoinGroupCall.All` 
   - `Calls.Initiate.All` ← **NEW - CRITICAL FOR REAL-TIME AUDIO**
   - `OnlineMeetings.Read.All`
4. Click **"Grant admin consent"** (requires admin rights)
5. Go to **Certificates & secrets** → **New client secret**
6. **Description**: `MediaBot-Secret`
7. **Expires**: 12 months
8. **Copy and save the secret VALUE** (you won't see it again!)

## 💻 Step 3: Create Windows Server VM
1. **Azure Portal** → **Virtual machines** → **Create**
2. **Resource group**: Create new → `MediaBot-RG`
3. **VM name**: `MediaBot-VM`
4. **Region**: Choose closest to you
5. **Image**: `Windows Server 2022 Datacenter - x64 Gen2`
6. **Size**: `Standard D2s v3` (2 vCPUs, 8 GB RAM)
7. **Administrator account**: 
   - Username: `azureuser`
   - Password: Create strong password
8. **Inbound port rules**: Allow RDP (3389) and HTTPS (443)
9. Click **"Review + create"** → **"Create"**
10. **Copy and save the Public IP address**

## 🔧 Step 4: Configure Network Security Group
1. Go to your VM → **Networking** → **Network security group**
2. **Add inbound rule**:
   - **Source**: Any
   - **Source port ranges**: *
   - **Destination**: Any  
   - **Destination port ranges**: `443`
   - **Protocol**: TCP
   - **Action**: Allow
   - **Priority**: 1000
   - **Name**: `Allow-HTTPS`

## 🤖 Step 5: Register Azure Bot Service
1. **Azure Portal** → **Create a resource** → Search **"Azure Bot"**
2. **Bot handle**: `mediabot-teams` (must be globally unique)
3. **Resource group**: Use existing `MediaBot-RG`
4. **Pricing tier**: F0 (Free)
5. **Microsoft App ID**: Select **"Use existing app registration"**
6. **App ID**: Use the Application ID from Step 1
7. **Messaging endpoint**: `https://[YOUR-VM-PUBLIC-IP]/api/callback/incoming`
8. Click **"Create"**

## 📱 Step 6: Configure Teams Channel
1. Go to your Bot Service → **Channels**
2. Click **Microsoft Teams** icon
3. **Enable** Microsoft Teams channel
4. Click **"Apply"**
5. **Copy the Teams App ID** (you'll need this later)

## 🖥️ Step 7: Setup VM Environment
**RDP into your VM and run these commands:**

```powershell
# Install .NET 8
Invoke-WebRequest -Uri "https://download.microsoft.com/download/8/4/8/848c652a-0c9a-4f3e-96dc-6b9937bb85e1/dotnet-hosting-8.0.0-win.exe" -OutFile "dotnet-hosting.exe"
.\dotnet-hosting.exe /quiet

# Install Chocolatey (package manager)
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Install Git and Certbot
choco install git -y
choco install certbot -y

# IMPORTANT: Before running certbot, make sure:
# 1. demobot.site A record points to this VM's public IP
# 2. Wait 5-10 minutes for DNS propagation
# 3. Test with: nslookup demobot.site

# Get SSL Certificate for demobot.site
certbot certonly --standalone -d demobot.site

# Certificate will be saved to: C:\Certbot\live\demobot.site\
```

## ⚙️ Step 8: Configure Your Bot Application
**Update your `appsettings.json` with the values from previous steps:**

```json
{
  "Bot": {
    "AppId": "YOUR-APPLICATION-ID-FROM-STEP-1",
    "AppSecret": "YOUR-CLIENT-SECRET-FROM-STEP-2", 
    "TenantId": "YOUR-TENANT-ID-FROM-STEP-1",
    "BaseUrl": "https://YOUR-VM-PUBLIC-IP-FROM-STEP-3"
  },
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "C:\\cert.pfx",
          "Password": "YourCertPassword123!"
        }
      }
    }
  }
}
```

## 🚀 Step 9: Deploy and Test
```powershell
# On VM - clone your MediaBot repository
git clone https://github.com/akhan-msft/mediabotv2.git
cd mediabotv2

# Restore NuGet packages
dotnet restore MediaBot.csproj

# Build the MediaBot project
dotnet build MediaBot.csproj

# Run the bot application (will start on https://0.0.0.0:443)
dotnet run --project MediaBot.csproj

# In another PowerShell window, test the health endpoint
Invoke-WebRequest -Uri "https://demobot.site/api/health" -SkipCertificateCheck

# Test the status endpoint for detailed bot information
Invoke-WebRequest -Uri "https://demobot.site/api/health/status" -SkipCertificateCheck

# Test manual join endpoint (replace with actual Teams meeting URL)
$body = @{ MeetingUrl = "https://teams.microsoft.com/l/meetup-join/..." } | ConvertTo-Json
Invoke-WebRequest -Uri "https://demobot.site/api/callback/join" -Method POST -Body $body -ContentType "application/json" -SkipCertificateCheck
```

## ✅ Step 10: Test Bot in Teams

### A. Verify Azure Bot Service Endpoint
1. **Azure Portal** → **MediaBot-RG** → **mediabot-teams** → **Settings** → **Configuration**
2. **Update messaging endpoint** to: `https://demobot.site/api/callback/incoming` (use domain, not IP)
3. **Save** configuration

### B. Test Manual Join (Recommended First Test)
```powershell
# Create a scheduled Teams meeting and copy the join URL
$meetingUrl = "https://teams.microsoft.com/l/meetup-join/19%3ameeting_YOUR_MEETING_ID"
$body = @{ MeetingUrl = $meetingUrl } | ConvertTo-Json
Invoke-WebRequest -Uri "https://demobot.site/api/callback/join" -Method POST -Body $body -ContentType "application/json" -SkipCertificateCheck
```

### C. Create Teams App Package (For Full Integration)

#### 1. Create App Icons
```powershell
# On your Windows VM, run this script to create icons
.\create-icons.ps1
```
This creates:
- `outline.png` (32x32) - Simple black outline icon
- `color.png` (192x192) - Full color icon with "MB" text

#### 2. Create manifest.json
**Update the `manifest.json` file with YOUR actual Bot App ID:**
```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/teams/v1.16/MicrosoftTeams.schema.json",
  "manifestVersion": "1.16",
  "version": "1.0.0",
  "id": "12345678-1234-1234-1234-123456789012",
  "developer": {
    "name": "MediaBot Developer",
    "websiteUrl": "https://demobot.site",
    "privacyUrl": "https://demobot.site/privacy",
    "termsOfUseUrl": "https://demobot.site/terms"
  },
  "name": {
    "short": "MediaBot",
    "full": "MediaBot Meeting Transcription Bot"
  },
  "description": {
    "short": "A bot that joins Teams meetings and logs events",
    "full": "MediaBot joins Teams meetings to transcribe and log meeting events including participant changes and audio streams."
  },
  "icons": {
    "outline": "outline.png",
    "color": "color.png"
  },
  "accentColor": "#1E88E5",
  "bots": [
    {
      "botId": "12345678-1234-1234-1234-123456789012",
      "scopes": ["team", "groupchat"],
      "supportsFiles": false,
      "isNotificationOnly": false,
      "supportsCalling": true,
      "supportsVideo": false
    }
  ],
  "permissions": ["identity", "messageTeamMembers"],
  "validDomains": ["demobot.site"],
  "webApplicationInfo": {
    "id": "12345678-1234-1234-1234-123456789012",
    "resource": "https://RscBasedStoreApp"
  }
}
```
**⚠️ CRITICAL**: Replace `12345678-1234-1234-1234-123456789012` with your **actual Bot App ID** from Step 1B!

#### 3. Build App Package
```powershell
# Automated package builder (recommended)
.\build-teams-app.ps1

# Manual package creation
Compress-Archive -Path @("manifest.json", "outline.png", "color.png") -DestinationPath "MediaBot-TeamsApp.zip" -Force
```

#### 4. Upload to Teams
1. **Microsoft Teams** → **Apps** → **Manage your apps**
2. **"Upload an app"** → **"Upload a custom app"**
3. **Select** `MediaBot-TeamsApp.zip`
4. **"Add"** to install the bot
5. **Add bot to a team** or test in direct chat

#### 5. Alternative: Cross-Platform Package Creation
**Linux/macOS:**
```bash
zip -r MediaBot-TeamsApp.zip manifest.json outline.png color.png
```

**PowerShell (any OS):**
```powershell
Compress-Archive -Path @("manifest.json", "outline.png", "color.png") -DestinationPath "MediaBot-TeamsApp.zip" -Force
```

### D. Test Meeting Integration
1. **Schedule a Teams meeting** (not instant meeting)
2. **Add bot** to meeting via app or manual join endpoint
3. **Join the meeting** yourself and observe bot behavior

### E. Monitor Console Logs
Watch for color-coded events:
- **🟢 Green**: Bot initialization and join events
- **🟡 Yellow**: Participant changes
- **🔵 Cyan**: Audio stream events
- **🔴 Red**: Errors or call ended events

---

**⚠️ IMPORTANT**: Complete steps 1-6 BEFORE creating the VM, as you need the App ID for the Bot Service registration.

**📋 What You'll Need to Save:**
- Application (Client) ID
- Client Secret  
- Tenant ID
- VM Public IP
- Certificate Password

This logical sequence ensures all dependencies are created in the right order!
