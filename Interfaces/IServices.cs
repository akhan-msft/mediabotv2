using MediaBot.Models;

namespace MediaBot.Interfaces
{
    /// <summary>
    /// Interface for logging call events and bot activities with structured data.
    /// </summary>
    public interface IEventLogger
    {
        void LogEvent(CallEvent callEvent);
        void LogEvent(string eventType, string callId, string description, Dictionary<string, object>? additionalData = null);
    }

    /// <summary>
    /// Interface for handling Teams call operations including joining meetings,
    /// managing call lifecycle, and processing real-time events.
    /// </summary>
    public interface ICallHandler
    {
        Task HandleIncomingCallAsync(string callId, string meetingId);
        Task HandleCallEstablishedAsync(string callId);
        Task HandleParticipantChangedAsync(string callId, string participantId, string participantName, bool joined);
        Task HandleAudioStreamChangedAsync(string callId, bool started);
        Task HandleCallEndedAsync(string callId);
        Task JoinMeetingAsync(string meetingUrl);
    }

    /// <summary>
    /// Interface for the core bot service that manages initialization, lifecycle,
    /// and access to Microsoft Graph clients for Teams media operations.
    /// </summary>
    public interface IBotService
    {
        Task InitializeAsync();
        Task StartAsync();
        Task StopAsync();
        bool IsInitialized { get; }
    }
}