using Application.DTOs.Health;

namespace Application.Interfaces
{
    /// <summary>
    /// Service for performing health checks on application dependencies.
    /// Checks database and cache connectivity and performance.
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// Performs a comprehensive health check of all critical dependencies.
        /// </summary>
        /// <returns>A HealthCheckResponse containing the status of all components.</returns>
        Task<HealthCheckResponse> CheckHealthAsync();
    }
}
