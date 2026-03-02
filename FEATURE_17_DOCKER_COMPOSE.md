# Feature #17: Docker Compose — Full Stack

## Overview

MutiSaaSApp is now fully containerized with Docker Compose, providing a complete development and production environment with all dependencies:

- **SQL Server 2022:** Database service
- **Redis 7:** Distributed cache
- **MutiSaaSApp API:** .NET 10 ASP.NET Core application
- **Networking:** Custom bridge network for service communication
- **Persistence:** Volumes for data retention
- **Health Checks:** Automated health monitoring for all services

## Quick Start

### Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose (included with Docker Desktop)
- Minimum 4GB RAM, 2 CPU cores

### Start the Full Stack

```bash
# Navigate to project root
cd C:\Users\USER\source\repos\MutiSaaSApp

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Verify Services

```bash
# Check if all containers are running
docker ps

# Check service health
docker-compose ps

# Test API health
curl http://localhost:5000/api/health

# View logs for specific service
docker-compose logs mutisaas-api
docker-compose logs sqlserver
docker-compose logs redis
```

## Service Details

### SQL Server

**Image:** mcr.microsoft.com/mssql/server:2022-latest
**Port:** 1433
**Container Name:** mutisaas-sqlserver
**Volume:** sqlserver-data (persistent)

**Environment Variables:**
- `SA_PASSWORD`: Database admin password
- `ACCEPT_EULA`: Accept license agreement
- `MSSQL_PID`: Edition (Developer, Express, Standard, Enterprise)

**Connection String (from other containers):**
```
Server=sqlserver;Database=MutiSaaSAppDb;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;
```

**Connection String (from host):**
```
Server=localhost,1433;Database=MutiSaaSAppDb;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;
```

### Redis Cache

**Image:** redis:7-alpine
**Port:** 6379
**Container Name:** mutisaas-redis
**Volume:** redis-data (persistent)

**Connection String (from other containers):**
```
redis:6379
```

**Connection String (from host):**
```
localhost:6379
```

**Health Check:**
```bash
redis-cli ping
# Should return: PONG
```

### MutiSaaSApp API

**Build:** Multi-stage Docker build from Dockerfile
**Port:** 5000
**Container Name:** mutisaas-api
**Volumes:**
- `./logs:/app/logs` - Log files persistent storage
- Configuration files (read-only)

**Environment Variables:**
- `ASPNETCORE_ENVIRONMENT`: Development or Production
- `ConnectionStrings__DefaultConnection`: Database connection
- `Redis__ConnectionString`: Redis connection
- `Jwt__Secret`: JWT signing key
- `Jwt__ExpiryMinutes`: Token expiry duration

**Health Check:** `GET /api/health`

## Dockerfile Details

### Multi-Stage Build

**Stage 1: Build**
- Uses `mcr.microsoft.com/dotnet/sdk:10.0`
- Restores NuGet packages
- Builds application
- Publishes release build

**Stage 2: Runtime**
- Uses `mcr.microsoft.com/dotnet/aspnet:10.0`
- Copies published artifacts
- Creates logs directory
- Sets environment variables
- Exposes port 5000
- Configured health checks

### Build Optimization

- Multi-stage reduces final image size
- Layer caching optimizes build speed
- Only runtime required (SDK not included)
- Minimal attack surface

## Docker Compose Configuration

### Services

```yaml
services:
  sqlserver:    # SQL Server 2022
  redis:        # Redis Cache
  mutisaas-api: # Application
```

### Networking

**Network:** mutisaas-network (bridge)
- Services communicate via service name
- Isolated from host network
- DNS resolution built-in

### Volumes

**sqlserver-data:**
- Persistent database files
- Survives container restart
- Data preserved across versions

**redis-data:**
- Persistent cache data
- AOF (Append-Only File) enabled
- Survives container restart

### Dependencies

```
mutisaas-api depends on:
  ├─ sqlserver (healthy)
  └─ redis (healthy)

Services start in dependency order
```

### Health Checks

Each service has health monitoring:

**SQL Server:**
```bash
sqlcmd -S localhost -U sa -P YourPassword123! -Q "SELECT 1"
```

**Redis:**
```bash
redis-cli ping
```

**API:**
```bash
wget --spider http://localhost:5000/api/health
```

## Development Workflow

### Build the Image

```bash
# Build without starting
docker-compose build

# Rebuild from scratch (no cache)
docker-compose build --no-cache
```

### Run Services

```bash
# Start in background
docker-compose up -d

# Start with attached logs
docker-compose up

# Start specific service
docker-compose up mutisaas-api
```

### View Logs

```bash
# All services
docker-compose logs

# Last 50 lines
docker-compose logs --tail=50

# Follow logs
docker-compose logs -f

# Specific service
docker-compose logs -f mutisaas-api
```

### Stop Services

```bash
# Stop (preserve volumes)
docker-compose stop

# Stop specific service
docker-compose stop mutisaas-api

# Restart
docker-compose restart

# Remove containers (keep volumes)
docker-compose down

# Remove everything (including volumes)
docker-compose down -v
```

### Execute Commands

```bash
# Open bash in API container
docker exec -it mutisaas-api bash

# Run dotnet CLI commands
docker exec mutisaas-api dotnet --version

# Query database from host
sqlcmd -S localhost,1433 -U sa -P YourPassword123! -Q "SELECT @@VERSION"
```

## Configuration Management

### Environment Variables

Override environment variables without editing docker-compose.yml:

```bash
# Set variable before running
export JWT_SECRET="your-new-secret"
docker-compose up

# Or use .env file
cp .env.example .env
# Edit .env with your values
docker-compose up
```

### Connection Strings

**Development (Local):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MutiSaaSAppDb;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;"
  }
}
```

**Docker (Container-to-Container):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=MutiSaaSAppDb;User Id=sa;Password=YourPassword123!;Encrypt=false;TrustServerCertificate=true;"
  }
}
```

## Production Deployment

### Security Considerations

1. **Change Default Passwords:**
   ```yaml
   environment:
     SA_PASSWORD: "YourNewSecurePassword123!"
   ```

2. **Use Secrets Management:**
   ```bash
   docker secret create db_password /path/to/password.txt
   ```

3. **Network Security:**
   - Don't expose ports unnecessarily
   - Use firewall rules
   - Implement SSL/TLS

4. **Image Security:**
   - Use specific image versions (not `latest`)
   - Scan images for vulnerabilities
   - Use private registries

### Scale Configuration

For production, adjust resource limits:

```yaml
services:
  mutisaas-api:
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M
```

### High Availability

For production clusters:

```yaml
# Use swarm or Kubernetes
# - Multiple API replicas
# - Database replication
# - Redis clustering
# - Load balancer
```

## Troubleshooting

### Containers Not Starting

```bash
# Check logs
docker-compose logs sqlserver

# Common issues:
# - Port already in use: Change port mapping
# - Insufficient memory: Increase Docker memory
# - Volume permission errors: Check file ownership
```

### Database Connection Failed

```bash
# Verify SQL Server is running
docker ps | grep sqlserver

# Test connection
docker-compose exec sqlserver sqlcmd -U sa -P YourPassword123! -Q "SELECT 1"

# Check connection string
docker-compose logs mutisaas-api | grep Connection
```

### Cache Connection Failed

```bash
# Verify Redis is running
docker ps | grep redis

# Test Redis
docker-compose exec redis redis-cli ping

# Check Redis connection string
docker-compose logs mutisaas-api | grep Redis
```

### Performance Issues

```bash
# Check resource usage
docker stats

# Check container logs
docker-compose logs -f mutisaas-api

# Increase resource limits
# Edit docker-compose.yml memory settings
```

## Cleanup Commands

```bash
# Remove all containers
docker-compose down

# Remove containers and volumes
docker-compose down -v

# Remove images
docker rmi $(docker images -q)

# Prune unused resources
docker system prune -a
```

## Integration with Kubernetes

For Kubernetes deployment:

1. **Create image registry:**
   ```bash
   docker tag mutisaasapp:latest myregistry.azurecr.io/mutisaasapp:latest
   docker push myregistry.azurecr.io/mutisaasapp:latest
   ```

2. **Use generated Kubernetes manifests:**
   ```bash
   kompose convert -f docker-compose.yml -o k8s/
   kubectl apply -f k8s/
   ```

## Files Included

### Created
- `Dockerfile` - Multi-stage build configuration
- `docker-compose.yml` - Full stack orchestration
- `.env.example` - Environment variable template
- `MutiSaaSApp/appsettings.Production.json` - Production configuration

### Modified
- `MutiSaaSApp/Program.cs` - Serilog integration (Feature #15)
- `docker-compose.yml` - Replaced stub with full configuration

## Next Steps

### Local Development
```bash
# Start stack
docker-compose up -d

# Apply migrations (if needed)
docker exec mutisaas-api dotnet ef database update

# View logs
docker-compose logs -f
```

### Production Deployment
```bash
# Build image
docker build -t mutisaasapp:1.0 .

# Push to registry
docker push myregistry.azurecr.io/mutisaasapp:1.0

# Deploy to cloud (AWS/Azure/GCP)
# Use docker-compose or Kubernetes
```

### Monitoring
```bash
# Set up log aggregation
# - ELK Stack
# - Splunk
# - DataDog
# - Azure Monitor

# Configure alerts
# - Unhealthy services
# - High resource usage
# - Failed deployments
```

## Summary

Feature #17 provides:
- ✅ Production-ready Dockerfile with multi-stage build
- ✅ Complete docker-compose.yml with all dependencies
- ✅ SQL Server, Redis, and API services
- ✅ Persistent volumes for data
- ✅ Health checks on all services
- ✅ Networking isolation
- ✅ Environment variable configuration
- ✅ Development and production settings
- ✅ .env template for easy configuration
- ✅ Comprehensive documentation

**Ready for:** Local development, CI/CD pipelines, and cloud deployment.
