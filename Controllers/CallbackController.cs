using Microsoft.AspNetCore.Mvc;
using MediaBot.Interfaces;
using MediaBot.Models;

namespace MediaBot.Controllers
{
    /// <summary>
    /// REST API controller that handles callback notifications from Microsoft Teams and Graph API.
    /// This controller processes incoming call invitations, Graph API notifications for call state changes,
    /// and provides endpoints for manually joining Teams meetings.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CallbackController : ControllerBase
    {
        private readonly ICallHandler _callHandler;
        private readonly IEventLogger _eventLogger;
        private readonly ILogger<CallbackController> _logger;

        public CallbackController(
            ICallHandler callHandler,
            IEventLogger eventLogger, 
            ILogger<CallbackController> logger)
        {
            _callHandler = callHandler;
            _eventLogger = eventLogger;
            _logger = logger;
        }

        [HttpPost("incoming")]
        public async Task<IActionResult> HandleIncomingCall([FromBody] object payload)
        {
            try
            {
                _eventLogger.LogEvent(
                    "CallbackReceived",
                    "incoming",
                    "Received incoming call callback from Teams",
                    new Dictionary<string, object> { ["Payload"] = payload.ToString() ?? "null" }
                );

                // TODO: Parse the actual Teams callback payload
                // For now, simulate handling an incoming call
                var callId = Guid.NewGuid().ToString();
                var meetingId = "simulated-meeting-" + DateTime.UtcNow.Ticks;

                await _callHandler.HandleIncomingCallAsync(callId, meetingId);

                return Ok(new { message = "Callback processed successfully", callId });
            }
            catch (Exception ex)
            {
                _eventLogger.LogEvent(
                    CallEventTypes.CallFailed,
                    "incoming",
                    $"Failed to process incoming call callback: {ex.Message}",
                    new Dictionary<string, object> { ["Error"] = ex.ToString() }
                );

                return StatusCode(500, new { error = "Failed to process callback", message = ex.Message });
            }
        }

        [HttpPost("notifications")]
        public async Task<IActionResult> HandleNotifications([FromBody] object payload)
        {
            try
            {
                _eventLogger.LogEvent(
                    "NotificationReceived",
                    "notification",
                    "Received notification from Teams Graph API",
                    new Dictionary<string, object> { ["Payload"] = payload.ToString() ?? "null" }
                );

                // TODO: Parse and handle Graph API notifications
                // This is where we'll receive events like participant changes, call state changes, etc.

                return Ok(new { message = "Notification processed successfully" });
            }
            catch (Exception ex)
            {
                _eventLogger.LogEvent(
                    "NotificationFailed",
                    "notification",
                    $"Failed to process notification: {ex.Message}",
                    new Dictionary<string, object> { ["Error"] = ex.ToString() }
                );

                return StatusCode(500, new { error = "Failed to process notification", message = ex.Message });
            }
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinMeeting([FromBody] JoinMeetingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.MeetingUrl))
                {
                    return BadRequest(new { error = "MeetingUrl is required" });
                }

                _eventLogger.LogEvent(
                    "ManualJoinRequested",
                    "manual",
                    $"Manual join meeting requested: {request.MeetingUrl}"
                );

                await _callHandler.JoinMeetingAsync(request.MeetingUrl);

                return Ok(new { message = "Join meeting request processed", meetingUrl = request.MeetingUrl });
            }
            catch (Exception ex)
            {
                _eventLogger.LogEvent(
                    CallEventTypes.CallFailed,
                    "manual",
                    $"Failed to join meeting manually: {ex.Message}",
                    new Dictionary<string, object> { ["Error"] = ex.ToString(), ["MeetingUrl"] = request.MeetingUrl ?? "null" }
                );

                return StatusCode(500, new { error = "Failed to join meeting", message = ex.Message });
            }
        }
    }

    public class JoinMeetingRequest
    {
        public string MeetingUrl { get; set; } = string.Empty;
    }
}