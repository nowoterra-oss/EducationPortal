# ============================================================
# EduPortal API - Multi-stage Dockerfile
# ============================================================
# Build: docker build -t eduportal-api .
# Run:   docker run -p 8080:8080 eduportal-api
# ============================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["EduPortal.sln", "./"]
COPY ["src/EduPortal.API/EduPortal.API.csproj", "src/EduPortal.API/"]
COPY ["src/EduPortal.Application/EduPortal.Application.csproj", "src/EduPortal.Application/"]
COPY ["src/EduPortal.Domain/EduPortal.Domain.csproj", "src/EduPortal.Domain/"]
COPY ["src/EduPortal.Infrastructure/EduPortal.Infrastructure.csproj", "src/EduPortal.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/EduPortal.API/EduPortal.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR /src/src/EduPortal.API
RUN dotnet build "EduPortal.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "EduPortal.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install required packages for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r eduportal && useradd -r -g eduportal eduportal

# Copy published files
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R eduportal:eduportal /app

# Switch to non-root user
USER eduportal

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "EduPortal.API.dll"]
