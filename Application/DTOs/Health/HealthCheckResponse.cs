namespace Application.DTOs.Health
{
    /// <summary>
    /// Response model for health check endpoint.
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Gets or sets the overall health status.
        /// </summary>
        public string Status { get; set; } = "Healthy";

        /// <summary>
        /// Gets or sets the timestamp when the check was performed.
        /// </summary>
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the database connectivity status.
        /// </summary>
        public HealthComponentStatus Database { get; set; } = new();

        /// <summary>
        /// Gets or sets the Redis/Cache connectivity status.
        /// </summary>
        public HealthComponentStatus Cache { get; set; } = new();
    }

    /// <summary>
    /// Health status for individual components.
    /// </summary>
    public class HealthComponentStatus
    {
        /// <summary>
        /// Gets or sets the component status (Healthy, Degraded, Unhealthy).
        /// </summary>
        public string Status { get; set; } = "Healthy";

        /// <summary>
        /// Gets or sets optional error message if status is not Healthy.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the response time in milliseconds.
        /// </summary>
        public long ResponseTimeMs { get; set; }
    }
}
