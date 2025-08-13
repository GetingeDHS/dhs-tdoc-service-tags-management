# Multi-stage Dockerfile for Tag Management API Service
# Medical Device Compliance: ISO-13485
# Build optimized for security and medical device requirements

# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file and project files for better layer caching
COPY *.sln ./
COPY src/TagManagement.Api/*.csproj ./src/TagManagement.Api/
COPY src/TagManagement.Domain/TagManagement.Core/*.csproj ./src/TagManagement.Domain/TagManagement.Core/
COPY src/TagManagement.Infrastructure/*.csproj ./src/TagManagement.Infrastructure/
COPY src/TagManagement.Application/*.csproj ./src/TagManagement.Application/
COPY tests/TagManagement.UnitTests/*.csproj ./tests/TagManagement.UnitTests/
COPY tests/TagManagement.E2ETests/*.csproj ./tests/TagManagement.E2ETests/

# Restore NuGet packages
RUN dotnet restore

# Copy all source code
COPY . .

# Build the solution
RUN dotnet build --configuration Release --no-restore

# Test Stage (Medical Device Compliance Requirement)
FROM build AS test
WORKDIR /app
# Run unit tests to ensure medical device compliance before containerization
RUN dotnet test --configuration Release --no-build --verbosity normal \
    --collect:"XPlat Code Coverage" \
    --results-directory /app/TestResults \
    tests/TagManagement.UnitTests/TagManagement.UnitTests.csproj

# Publish Stage
FROM build AS publish
WORKDIR /app
RUN dotnet publish src/TagManagement.Api/TagManagement.Api.csproj \
    --configuration Release \
    --no-restore \
    --no-build \
    --output /app/publish

# Runtime Stage - Use minimal runtime image for security
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Medical Device Security: Create non-root user
RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -D appuser

# Install required packages for medical device environments
RUN apk add --no-cache \
    curl \
    ca-certificates \
    tzdata

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Set ownership to appuser
RUN chown -R appuser:appuser /app

# Medical Device Metadata Labels
LABEL maintainer="DHS TDOC Team"
LABEL version="1.0.0"
LABEL description="Tag Management Service for Medical Device Management"
LABEL compliance.standard="ISO-13485"
LABEL security.scan.required="true"
LABEL environment.type="medical-device"
LABEL service.name="tag-management-api"
LABEL service.component="microservice"
LABEL data.classification="medical"

# Health check for medical device monitoring
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Switch to non-root user
USER appuser

# Expose application port
EXPOSE 8080

# Set environment variables for medical device compliance
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV Logging__LogLevel__Default=Information
ENV HealthChecks__UI__HealthChecksUri=http://localhost:8080/health

# Configure medical device specific settings
ENV MedicalDevice__ComplianceStandard="ISO-13485"
ENV MedicalDevice__AuditingEnabled="true"
ENV MedicalDevice__SecurityScanRequired="true"

ENTRYPOINT ["dotnet", "TagManagement.Api.dll"]
