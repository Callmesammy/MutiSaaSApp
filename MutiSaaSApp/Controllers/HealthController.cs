using Application.DTOs.Health;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MutiSaaSApp.Common;

namespace MutiSaaSApp.Controllers
{
    /// <summary>
    /// Controller for health check endpoint.
    /// Provides endpoint for monitoring application and dependency health.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly IHealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        /// <summary>
        /// Initializes a new instance of the HealthController class.
        /// </summary>
        public HealthController(
            IHealthCheckService healthCheckService,
            ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current health status of the application and its dependencies.
        /// </summary>
        /// <returns>Health status response with component details.</returns>
        /// <response code="200">Application is healthy</response>
        /// <response code="503">Application is unhealthy</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<HealthCheckResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<HealthCheckResponse>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                var healthCheck = await _healthCheckService.CheckHealthAsync();

                // Return 200 if healthy or degraded, 503 if unhealthy
                var statusCode = healthCheck.Status == "Unhealthy"
                    ? StatusCodes.Status503ServiceUnavailable
                    : StatusCodes.Status200OK;

                _logger.LogInformation("Health check requested. Status: {Status}", healthCheck.Status);

                var response = new ApiResponse<HealthCheckResponse>(
                    healthCheck,
                    $"Health status: {healthCheck.Status}");

                return StatusCode(statusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health check");

                var errorResponse = new ApiResponse<HealthCheckResponse>(
                    "Health check failed: " + ex.Message);

                return StatusCode(StatusCodes.Status503ServiceUnavailable, errorResponse);
            }
        }
    }
}
