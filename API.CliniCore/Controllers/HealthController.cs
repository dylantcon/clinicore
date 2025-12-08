using Microsoft.AspNetCore.Mvc;

namespace API.CliniCore.Controllers
{
    /// <summary>
    /// Health check endpoint for service availability monitoring.
    /// Used by clients to verify the API is running before making requests.
    /// </summary>
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Simple health check - returns 200 OK if the service is running.
        /// </summary>
        [HttpGet("/health")]
        public IActionResult Health() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

        /// <summary>
        /// Readiness check - returns 200 OK when service is ready to accept requests.
        /// Can be extended to check database connectivity, external dependencies, etc.
        /// </summary>
        [HttpGet("/ready")]
        public IActionResult Ready() => Ok(new { status = "ready", timestamp = DateTime.UtcNow });
    }
}
