using Microsoft.AspNetCore.Mvc;
using MediaBot.Interfaces;

namespace MediaBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IBotService _botService;
        private readonly IEventLogger _eventLogger;

        public HealthController(IBotService botService, IEventLogger eventLogger)
        {
            _botService = botService;
            _eventLogger = eventLogger;
        }

        [HttpGet]
        public IActionResult GetHealth()
        {
            var health = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                botInitialized = _botService.IsInitialized,
                version = "1.0.0-phase1"
            };

            return Ok(health);
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var status = new
            {
                botService = new
                {
                    initialized = _botService.IsInitialized,
                    status = _botService.IsInitialized ? "ready" : "not initialized"
                },
                server = new
                {
                    uptime = Environment.TickCount64 / 1000,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                },
                capabilities = new
                {
                    canJoinMeetings = true,
                    canLogEvents = true,
                    mediaProcessing = false // Phase 1 doesn't include media processing
                }
            };

            _eventLogger.LogEvent(
                "HealthCheckRequested",
                "system",
                "Health status requested",
                new Dictionary<string, object>
                {
                    ["BotInitialized"] = _botService.IsInitialized,
                    ["RequestTime"] = DateTime.UtcNow
                }
            );

            return Ok(status);
        }
    }
}