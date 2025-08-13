# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/TagManagement.Api/TagManagement.Api.csproj", "src/TagManagement.Api/"]
COPY ["src/TagManagement.Domain/TagManagement.Core/TagManagement.Core.csproj", "src/TagManagement.Domain/TagManagement.Core/"]
COPY ["src/TagManagement.Infrastructure/TagManagement.Infrastructure.csproj", "src/TagManagement.Infrastructure/"]
COPY ["src/TagManagement.Application/TagManagement.Application.csproj", "src/TagManagement.Application/"]

RUN dotnet restore "src/TagManagement.Api/TagManagement.Api.csproj"

# Copy source code
COPY . .

# Run tests during build for quality gate
FROM build AS test
WORKDIR /src
RUN dotnet test "tests/TagManagement.UnitTests/TagManagement.UnitTests.csproj" \
    --logger "trx;LogFileName=test-results.trx" \
    --collect:"XPlat Code Coverage" \
    --results-directory /testresults

# Build application
FROM build AS publish
WORKDIR /src/src/TagManagement.Api
RUN dotnet publish "TagManagement.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security (medical device compliance)
RUN addgroup --system --gid 1001 tagmanagement \
    && adduser --system --uid 1001 --gid 1001 --no-create-home tagmanagement

# Copy published app
COPY --from=publish /app/publish .

# Set ownership and permissions
RUN chown -R tagmanagement:tagmanagement /app
USER tagmanagement

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Environment variables for production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=8081

# Entry point
ENTRYPOINT ["dotnet", "TagManagement.Api.dll"]

# Labels for medical device compliance and container metadata
LABEL maintainer="Medical Device Engineering Team"
LABEL version="1.0.0"
LABEL description="Tag Management Microservice for Medical Device Manufacturing"
LABEL org.opencontainers.image.title="Tag Management Service"
LABEL org.opencontainers.image.description="Microservice for managing tags in medical device systems"
LABEL org.opencontainers.image.vendor="Your Medical Device Company"
LABEL org.opencontainers.image.licenses="MIT"
LABEL org.opencontainers.image.source="https://github.com/your-org/tag-management"
LABEL medical-device.compliance="ISO-13485"
LABEL medical-device.classification="Class-II"
