using Application.DTOs.Health;
using Application.Interfaces;
using Infastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Infastructure.Services
{
    /// <summary>
    /// Implementation of the health check service.
    /// Performs connectivity and performance checks on database and cache.
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<HealthCheckService> _logger;

        /// <summary>
        /// Initializes a new instance of the HealthCheckService class.
        /// </summary>
        public HealthCheckService(
            ApplicationDbContext dbContext,
            IDistributedCache cache,
            ILogger<HealthCheckService> logger)
        {
            _dbContext = dbContext;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Performs a comprehensive health check of all critical dependencies.
        /// </summary>
        public async Task<HealthCheckResponse> CheckHealthAsync()
        {
            var response = new HealthCheckResponse
            {
                CheckedAt = DateTime.UtcNow
            };

            // Check Database
            response.Database = await CheckDatabaseHealthAsync();

            // Check Cache
            response.Cache = await CheckCacheHealthAsync();

            // Determine overall status
            response.Status = DetermineOverallStatus(response);

            _logger.LogInformation(
                "Health check completed. Overall status: {Status}, Database: {DbStatus}, Cache: {CacheStatus}",
                response.Status,
                response.Database.Status,
                response.Cache.Status);

            return response;
        }

        /// <summary>
        /// Checks database connectivity and performance.
        /// </summary>
        private async Task<HealthComponentStatus> CheckDatabaseHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var status = new HealthComponentStatus();

            try
            {
                // Execute a simple query to verify database connectivity
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
                stopwatch.Stop();

                status.Status = "Healthy";
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                _logger.LogDebug("Database health check passed. Response time: {ResponseTime}ms", status.ResponseTimeMs);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                status.Status = "Unhealthy";
                status.Message = $"Database connection failed: {ex.Message}";
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                _logger.LogError(ex, "Database health check failed. Response time: {ResponseTime}ms", status.ResponseTimeMs);
            }

            return status;
        }

        /// <summary>
        /// Checks Redis/Cache connectivity and performance.
        /// </summary>
        private async Task<HealthComponentStatus> CheckCacheHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var status = new HealthComponentStatus();
            const string testKey = "health-check-test";
            const string testValue = "healthy";

            try
            {
                // Try to set a test value
                await _cache.SetStringAsync(testKey, testValue, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                });

                // Try to retrieve the test value
                var retrievedValue = await _cache.GetStringAsync(testKey);

                if (retrievedValue != testValue)
                {
                    throw new Exception("Cache value verification failed");
                }

                // Clean up
                await _cache.RemoveAsync(testKey);

                stopwatch.Stop();
                status.Status = "Healthy";
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                _logger.LogDebug("Cache health check passed. Response time: {ResponseTime}ms", status.ResponseTimeMs);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                status.Status = "Degraded";
                status.Message = $"Cache connectivity issue: {ex.Message}";
                status.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                _logger.LogWarning(ex, "Cache health check degraded. Response time: {ResponseTime}ms", status.ResponseTimeMs);
            }

            return status;
        }

        /// <summary>
        /// Determines the overall application health status based on component statuses.
        /// </summary>
        private static string DetermineOverallStatus(HealthCheckResponse response)
        {
            // If database is unhealthy, overall status is unhealthy
            if (response.Database.Status == "Unhealthy")
            {
                return "Unhealthy";
            }

            // If any component is degraded, overall status is degraded
            if (response.Database.Status == "Degraded" || response.Cache.Status == "Degraded")
            {
                return "Degraded";
            }

            // If all components are healthy, overall status is healthy
            return "Healthy";
        }
    }
}
