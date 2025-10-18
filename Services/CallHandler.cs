using Microsoft.Graph;
using Microsoft.Graph.Models;
using MediaBot.Interfaces;
using MediaBot.Models;

namespace MediaBot.Services
{
    /// <summary>
    /// Handles all call-related operations for the Teams Media Bot including joining meetings, 
    /// managing call lifecycle events, and processing participant and audio stream changes.
    /// This class acts as the primary interface for bot interaction with Teams calls and meetings.
    /// </summary>
    public class CallHandler : ICallHandler
    {
        private readonly IEventLogger _eventLogger;
        private readonly ILogger<CallHandler> _logger;
        private readonly BotService _botService;
        private readonly GraphServiceClient _graphServiceClient;

        public CallHandler(IEventLogger eventLogger, ILogger<CallHandler> logger, IBotService botService, GraphServiceClient graphServiceClient)
        {
            _eventLogger = eventLogger;
            _logger = logger;
            _botService = (BotService)botService; // Cast to access CommunicationsClient property
            _graphServiceClient = graphServiceClient;
        }

        public async Task HandleIncomingCallAsync(string callId, string meetingId)
        {
            _eventLogger.LogEvent(
                CallEventTypes.BotInvitedToMeeting,
                callId,
                $"Bot was invited to meeting: {meetingId}",
                new Dictionary<string, object> { ["MeetingId"] = meetingId }
            );

            await Task.CompletedTask; // Placeholder for future implementation
        }

        public async Task HandleCallEstablishedAsync(string callId)
        {
            _eventLogger.LogEvent(
                CallEventTypes.CallEstablished,
                callId,
                "Call has been successfully established"
            );

            _eventLogger.LogEvent(
                CallEventTypes.BotJoinedCall,
                callId,
                "Bot successfully joined the call"
            );

            await Task.CompletedTask; // Placeholder for future implementation
        }

        public async Task HandleParticipantChangedAsync(string callId, string participantId, string participantName, bool joined)
        {
            var eventType = joined ? CallEventTypes.ParticipantJoined : CallEventTypes.ParticipantLeft;
            var action = joined ? "joined" : "left";

            _eventLogger.LogEvent(
                eventType,
                callId,
                $"Participant {participantName} ({participantId}) {action} the meeting",
                new Dictionary<string, object> 
                { 
                    ["ParticipantId"] = participantId,
                    ["ParticipantName"] = participantName,
                    ["Action"] = action
                }
            );

            await Task.CompletedTask; // Placeholder for future implementation
        }

        public async Task HandleAudioStreamChangedAsync(string callId, bool started)
        {
            var eventType = started ? CallEventTypes.AudioStreamStarted : CallEventTypes.AudioStreamStopped;
            var action = started ? "started" : "stopped";

            _eventLogger.LogEvent(
                eventType,
                callId,
                $"Audio stream {action}",
                new Dictionary<string, object> { ["AudioActive"] = started }
            );

            await Task.CompletedTask; // Placeholder for future implementation
        }

        public async Task HandleCallEndedAsync(string callId)
        {
            _eventLogger.LogEvent(
                CallEventTypes.CallEnded,
                callId,
                "Call/Meeting has ended"
            );

            await Task.CompletedTask; // Placeholder for future implementation
        }

        public async Task JoinMeetingAsync(string meetingUrl)
        {
            var callId = Guid.NewGuid().ToString();
            
            try
            {
                _eventLogger.LogEvent(
                    "JoinMeetingRequested",
                    callId,
                    $"Attempting to join meeting: {meetingUrl}",
                    new Dictionary<string, object> { ["MeetingUrl"] = meetingUrl }
                );

                // Check if graph service client is available
                if (_graphServiceClient == null)
                {
                    throw new InvalidOperationException("Graph service client not initialized");
                }

                // Parse and create the call request
                var callRequest = CreateJoinMeetingCallRequest(meetingUrl, callId);

                _eventLogger.LogEvent(
                    "GraphSDKJoinAttempt",
                    callId,
                    "Creating call via Microsoft Graph SDK...",
                    new Dictionary<string, object> 
                    { 
                        ["MeetingUrl"] = meetingUrl,
                        ["CallId"] = callId,
                        ["CallbackUri"] = callRequest.CallbackUri ?? "unknown"
                    }
                );

                // Create the call using Graph SDK
                var call = await _graphServiceClient.Communications.Calls.PostAsync(callRequest);

                if (call != null)
                {
                    _eventLogger.LogEvent(
                        "CallCreatedSuccessfully",
                        callId,
                        "Successfully created call via Graph SDK",
                        new Dictionary<string, object> 
                        { 
                            ["CallId"] = callId,
                            ["GraphCallId"] = call.Id ?? "unknown",
                            ["CallState"] = call.State?.ToString() ?? "unknown",
                            ["MeetingUrl"] = meetingUrl
                        }
                    );

                    _eventLogger.LogEvent(
                        "BotJoinedMeeting",
                        callId,
                        "Bot successfully joined Teams meeting",
                        new Dictionary<string, object> 
                        { 
                            ["CallId"] = callId,
                            ["GraphCallId"] = call.Id ?? "unknown",
                            ["MeetingUrl"] = meetingUrl,
                            ["Status"] = "Successfully joined meeting - ready to receive audio streams"
                        }
                    );
                }
                else
                {
                    throw new InvalidOperationException("Graph SDK returned null call object");
                }
            }
            catch (Exception ex)
            {
                _eventLogger.LogEvent(
                    CallEventTypes.CallFailed,
                    callId,
                    $"Failed to join meeting via Graph SDK: {ex.Message}",
                    new Dictionary<string, object> 
                    { 
                        ["Error"] = ex.Message, 
                        ["MeetingUrl"] = meetingUrl,
                        ["StackTrace"] = ex.StackTrace ?? "No stack trace available",
                        ["ErrorType"] = ex.GetType().Name
                    }
                );
                throw;
            }
        }

        private Call CreateJoinMeetingCallRequest(string meetingUrl, string callId)
        {
            try
            {
                _eventLogger.LogEvent(
                    "CreatingCallRequest",
                    callId,
                    "Creating Call request for Graph SDK",
                    new Dictionary<string, object> { ["MeetingUrl"] = meetingUrl }
                );

                // Parse the Teams meeting URL to extract meeting information
                var meetingInfo = ParseMeetingUrl(meetingUrl);

                // Create the call request body according to Graph API documentation
                var callRequest = new Call
                {
                    OdataType = "#microsoft.graph.call",
                    CallbackUri = "https://demobot.site/api/callback",
                    RequestedModalities = new List<Modality?> { Modality.Audio },
                    MediaConfig = new ServiceHostedMediaConfig
                    {
                        OdataType = "#microsoft.graph.serviceHostedMediaConfig"
                    },
                    MeetingInfo = meetingInfo
                };

                _eventLogger.LogEvent(
                    "CallRequestCreated",
                    callId,
                    "Successfully created Call request",
                    new Dictionary<string, object> 
                    { 
                        ["MeetingUrl"] = meetingUrl,
                        ["CallbackUri"] = callRequest.CallbackUri,
                        ["MediaConfigured"] = true
                    }
                );

                return callRequest;
            }
            catch (Exception ex)
            {
                _eventLogger.LogEvent(
                    "CallRequestCreationFailed",
                    callId,
                    $"Failed to create Call request: {ex.Message}",
                    new Dictionary<string, object> 
                    { 
                        ["Error"] = ex.Message,
                        ["MeetingUrl"] = meetingUrl
                    }
                );
                throw;
            }
        }

        private JoinMeetingIdMeetingInfo ParseMeetingUrl(string meetingUrl)
        {
            // For Teams meeting URLs like: https://teams.microsoft.com/meet/2908149825997?p=F2PgBXX0Gidzyxb0H2
            // We need to extract the meeting ID and create appropriate MeetingInfo

            var uri = new Uri(meetingUrl);
            
            // Extract meeting ID from the path (e.g., "2908149825997" from "/meet/2908149825997")
            var pathSegments = uri.LocalPath.Trim('/').Split('/');
            var meetingId = pathSegments.Length > 1 ? pathSegments[1] : pathSegments[0];

            // Create JoinMeetingIdMeetingInfo for Teams meetings
            var meetingInfo = new JoinMeetingIdMeetingInfo
            {
                OdataType = "#microsoft.graph.joinMeetingIdMeetingInfo",
                JoinMeetingId = meetingId,
                Passcode = null // Optional, can be extracted from URL if needed
            };

            return meetingInfo;
        }

        // Note: Graph SDK v5 handles call events through webhook notifications
        // Real-time event handling will be implemented via the webhook callback endpoint
    }
}