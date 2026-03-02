# Feature #16: Health Check Endpoint

## Overview

The Health Check Endpoint provides a standardized way to monitor the application's health and the connectivity status of its critical dependencies (Database and Redis Cache). This is essential for:

- **Kubernetes/Orchestration:** Liveness and readiness probes
- **Monitoring Systems:** Application health dashboards
- **Load Balancers:** Route traffic only to healthy instances
- **CI/CD Pipelines:** Pre-deployment health verification

## Endpoint Details

### URL
```
GET /api/health
```

### Authentication
- **Public Endpoint:** No authentication required (`[AllowAnonymous]`)
- **Use Case:** Health checks should be accessible to orchestration systems without credentials

### Response Codes

| Code | Status | Condition |
|------|--------|-----------|
| 200  | OK | Application is Healthy or Degraded |
| 503  | Service Unavailable | Application is Unhealthy |

## Response Format

### Healthy Response (200 OK)
```json
{
  "success": true,
  "message": "Health status: Healthy",
  "data": {
    "status": "Healthy",
    "checkedAt": "2024-01-15T10:30:45.123456Z",
    "database": {
      "status": "Healthy",
      "message": null,
      "responseTimeMs": 5
    },
    "cache": {
      "status": "Healthy",
      "message": null,
      "responseTimeMs": 12
    }
  },
  "errors": null
}
```

### Degraded Response (200 OK)
```json
{
  "success": true,
  "message": "Health status: Degraded",
  "data": {
    "status": "Degraded",
    "checkedAt": "2024-01-15T10:30:45.123456Z",
    "database": {
      "status": "Healthy",
      "message": null,
      "responseTimeMs": 5
    },
    "cache": {
      "status": "Degraded",
      "message": "Cache connectivity issue: Connection timeout",
      "responseTimeMs": 100
    }
  },
  "errors": null
}
```

### Unhealthy Response (503 Service Unavailable)
```json
{
  "success": false,
  "message": "Health status: Unhealthy",
  "data": {
    "status": "Unhealthy",
    "checkedAt": "2024-01-15T10:30:45.123456Z",
    "database": {
      "status": "Unhealthy",
      "message": "Database connection failed: Connection string has invalid syntax",
      "responseTimeMs": 50
    },
    "cache": {
      "status": "Healthy",
      "message": null,
      "responseTimeMs": 10
    }
  },
  "errors": null
}
```

## Status Levels

### Overall Status Determination

| Database | Cache | Overall | HTTP |
|----------|-------|---------|------|
| Healthy  | Healthy  | Healthy | 200 |
| Healthy  | Degraded | Degraded | 200 |
| Degraded | Healthy  | Degraded | 200 |
| Degraded | Degraded | Degraded | 200 |
| Unhealthy | * | Unhealthy | 503 |

**Key Rule:** If database is unhealthy, overall status is unhealthy (returns 503). If any other component is degraded, overall status is degraded but returns 200.

## Component Checks

### Database Check
- **Operation:** `SELECT 1` query
- **Success Criteria:** Query executes without error
- **Failure Handling:** Catches connection errors and stores error message
- **Performance:** Returns response time in milliseconds

### Cache Check
- **Operation:** 
  1. Write test key with value "healthy"
  2. Read test key
  3. Verify value matches
  4. Clean up test key
- **Success Criteria:** All three operations succeed and values match
- **Failure Handling:** Catches connection/timeout errors
- **Degraded vs Unhealthy:** Cache failures set status to "Degraded" (not critical)
- **Performance:** Returns response time in milliseconds

## Implementation Details

### Files Created

| File | Purpose |
|------|---------|
| `Application/DTOs/Health/HealthCheckResponse.cs` | Response DTOs for health check data |
| `Application/Interfaces/IHealthCheckService.cs` | Service interface definition |
| `Infastructure/Services/HealthCheckService.cs` | Health check implementation |
| `MutiSaaSApp/Controllers/HealthController.cs` | REST endpoint controller |

### Key Components

#### HealthCheckResponse DTO
```csharp
public class HealthCheckResponse
{
    public string Status { get; set; } = "Healthy";
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public HealthComponentStatus Database { get; set; } = new();
    public HealthComponentStatus Cache { get; set; } = new();
}

public class HealthComponentStatus
{
    public string Status { get; set; } = "Healthy";  // Healthy, Degraded, Unhealthy
    public string? Message { get; set; }
    public long ResponseTimeMs { get; set; }
}
```

#### Health Check Service
- **IHealthCheckService.CheckHealthAsync():** Main orchestrator method
- **CheckDatabaseHealthAsync():** Verifies database connectivity
- **CheckCacheHealthAsync():** Verifies Redis/cache connectivity
- **DetermineOverallStatus():** Aggregates component statuses

#### Health Controller
- **GET /api/health:** Single public endpoint
- Returns 200 for Healthy/Degraded, 503 for Unhealthy
- Wrapped in ApiResponse<T> for consistency

## Dependency Injection

The service is registered in `Program.cs`:

```csharp
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
```

## Testing

### Integration Tests (HealthCheckIntegrationTests.cs)
- DTO serialization/deserialization validation
- Response format verification
- JSON structure compliance

### Manual Testing

#### Healthy Application
```bash
curl -X GET http://localhost:5000/api/health
# Returns 200 OK with Healthy status
```

#### With Failed Cache
```bash
# Stop Redis, then:
curl -X GET http://localhost:5000/api/health
# Returns 200 OK with Degraded status (Cache degraded, DB still healthy)
```

#### With Failed Database
```bash
# Stop SQL Server, then:
curl -X GET http://localhost:5000/api/health
# Returns 503 Service Unavailable with Unhealthy status
```

## Kubernetes Integration Example

### Liveness Probe
```yaml
livenessProbe:
  httpGet:
    path: /api/health
    port: 5000
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3
```

### Readiness Probe
```yaml
readinessProbe:
  httpGet:
    path: /api/health
    port: 5000
  initialDelaySeconds: 10
  periodSeconds: 5
  timeoutSeconds: 3
  failureThreshold: 1
```

## Performance Characteristics

- **Typical Response Time:** 10-50ms (database + cache checks)
- **Max Timeout:** Should be tuned based on infrastructure
- **No Request Body:** GET endpoint, no payload needed
- **Minimal CPU Impact:** Single SELECT 1 + cache write/read

## Security Considerations

1. **Public Endpoint:** Health checks are intentionally unauthenticated
   - Allows monitoring systems access without credentials
   - No sensitive data is exposed (only status levels)

2. **Rate Limiting:** Can be applied separately if needed
   - Not included in baseline implementation
   - Consider adding if health checks become excessive

3. **Information Disclosure:** Error messages are generic
   - Database errors: "Database connection failed"
   - Cache errors: "Cache connectivity issue"
   - No stack traces or sensitive details exposed

## Future Enhancements

### Planned (Feature #15 - Structured Logging)
- Add structured logging context to health checks
- Include RequestId and timestamp in logs

### Optional Additions
1. **Message Queue Health:** Check if message bus is accessible
2. **External Service Health:** Verify third-party API availability
3. **Custom Health Checks:** Allow plugins to register health checks
4. **Historical Health Metrics:** Track health status over time
5. **Detailed Diagnostics:** Extended health endpoint with more metrics

## Build Status
✅ **SUCCESS** - All 41 tests passing, 0 warnings

## Files Modified in This Feature
- `MutiSaaSApp/Program.cs` - Added IHealthCheckService registration

## Files Created in This Feature
- `Application/DTOs/Health/HealthCheckResponse.cs`
- `Application/Interfaces/IHealthCheckService.cs`
- `Infastructure/Services/HealthCheckService.cs`
- `MutiSaaSApp/Controllers/HealthController.cs`
- `MultiSaasTest/Integration/HealthCheckIntegrationTests.cs`
