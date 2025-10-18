using MediaBot.Interfaces;
using MediaBot.Models;

namespace MediaBot.Services
{
    /// <summary>
    /// Provides event logging functionality with color-coded console output for different event types.
    /// Logs bot lifecycle events, call events, participant changes, and media stream activities
    /// with structured formatting and timestamps for debugging and monitoring.
    /// </summary>
    public class EventLogger : IEventLogger
    {
        private readonly ILogger<EventLogger> _logger;

        public EventLogger(ILogger<EventLogger> logger)
        {
            _logger = logger;
        }

        public void LogEvent(CallEvent callEvent)
        {
            var logMessage = FormatLogMessage(callEvent);
            
            // Log to console with color coding based on event type
            switch (callEvent.EventType)
            {
                case CallEventTypes.BotInvitedToMeeting:
                case CallEventTypes.BotJoinedCall:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case CallEventTypes.CallFailed:
                case CallEventTypes.CallEnded:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case CallEventTypes.ParticipantJoined:
                case CallEventTypes.ParticipantLeft:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case CallEventTypes.AudioStreamStarted:
                case CallEventTypes.AudioStreamStopped:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            Console.WriteLine(logMessage);
            Console.ResetColor();

            // Also log using ILogger
            _logger.LogInformation(logMessage);
        }

        public void LogEvent(string eventType, string callId, string description, Dictionary<string, object>? additionalData = null)
        {
            var callEvent = new CallEvent
            {
                EventType = eventType,
                CallId = callId,
                Description = description,
                AdditionalData = additionalData ?? new Dictionary<string, object>()
            };

            LogEvent(callEvent);
        }

        private string FormatLogMessage(CallEvent callEvent)
        {
            var timestamp = callEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var additionalInfo = string.Empty;

            if (callEvent.AdditionalData.Any())
            {
                var additionalDataStr = string.Join(", ", 
                    callEvent.AdditionalData.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                additionalInfo = $" | Additional Data: {additionalDataStr}";
            }

            return $"[{timestamp}] {callEvent.EventType} | Call ID: {callEvent.CallId} | {callEvent.Description}{additionalInfo}";
        }
    }
}