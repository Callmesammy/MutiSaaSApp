# Multi-stage build for MutiSaaSApp
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-stage

WORKDIR /src

# Copy solution and project files
COPY ["MutiSaaSApp/MutiSaaSApp.csproj", "MutiSaaSApp/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infastructure/Infastructure.csproj", "Infastructure/"]

# Restore dependencies
RUN dotnet restore "MutiSaaSApp/MutiSaaSApp.csproj"

# Copy all source code
COPY . .

# Build the application
RUN dotnet build "MutiSaaSApp/MutiSaaSApp.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "MutiSaaSApp/MutiSaaSApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime-stage

WORKDIR /app

# Copy published application from build stage
COPY --from=build-stage /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:5000/api/health || exit 1

# Expose port
EXPOSE 5000

# Run the application
ENTRYPOINT ["dotnet", "MutiSaaSApp.dll"]
