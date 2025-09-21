namespace MediaBot.Models
{
    public class CallEvent
    {
        public string EventType { get; set; } = string.Empty;
        public string CallId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? MeetingId { get; set; }
        public string? ParticipantId { get; set; }
        public string? ParticipantName { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
        public string Description { get; set; } = string.Empty;
    }

    public static class CallEventTypes
    {
        public const string BotInvitedToMeeting = "BotInvitedToMeeting";
        public const string BotJoinedCall = "BotJoinedCall";
        public const string CallEstablished = "CallEstablished";
        public const string ParticipantJoined = "ParticipantJoined";
        public const string ParticipantLeft = "ParticipantLeft";
        public const string AudioStreamStarted = "AudioStreamStarted";
        public const string AudioStreamStopped = "AudioStreamStopped";
        public const string CallEnded = "CallEnded";
        public const string CallFailed = "CallFailed";
        public const string MeetingStarted = "MeetingStarted";
        public const string MeetingEnded = "MeetingEnded";
    }
}