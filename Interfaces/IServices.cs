using MediaBot.Models;

namespace MediaBot.Interfaces
{
    public interface IEventLogger
    {
        void LogEvent(CallEvent callEvent);
        void LogEvent(string eventType, string callId, string description, Dictionary<string, object>? additionalData = null);
    }

    public interface ICallHandler
    {
        Task HandleIncomingCallAsync(string callId, string meetingId);
        Task HandleCallEstablishedAsync(string callId);
        Task HandleParticipantChangedAsync(string callId, string participantId, string participantName, bool joined);
        Task HandleAudioStreamChangedAsync(string callId, bool started);
        Task HandleCallEndedAsync(string callId);
        Task JoinMeetingAsync(string meetingUrl);
    }

    public interface IBotService
    {
        Task InitializeAsync();
        Task StartAsync();
        Task StopAsync();
        bool IsInitialized { get; }
    }
}