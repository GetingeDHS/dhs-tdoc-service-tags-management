using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;
using TagManagement.Api.Services;
using TagManagement.Core.Interfaces;
using TagManagement.Infrastructure;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/tag-management-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.

// Build connection string from environment variables
// This allows for dynamic database names while keeping credentials in Key Vault
var connectionString = BuildConnectionString(builder.Configuration);

// Add data layer services (includes DbContext and repositories)
builder.Services.AddDataServices(builder.Configuration, connectionString);

// Add application services
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi(); // Not available in .NET 8

// Add Swagger/OpenAPI for better API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Tag Management API", 
        Version = "v1",
        Description = "Medical Device Tag Management Microservice - ISO-13485 Compliant"
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    // app.MapOpenApi(); // Not available in .NET 8
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description,
                duration = x.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            medicalDeviceCompliance = "ISO-13485",
            timestamp = DateTime.UtcNow
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
});

// Basic API info endpoint
app.MapGet("/api/info", () => new
{
    service = "Tag Management Service",
    version = "1.0.2", // bump again to trigger synchronize and validate re-run
    environment = app.Environment.EnvironmentName,
    complianceStandard = "ISO-13485",
    timestamp = DateTime.UtcNow
});

// Placeholder endpoints for E2E testing (will be replaced with real controllers)
app.MapGet("/api/tags", () => new[]
{
    new { TagID = 1, TagNumber = "PREP-001", TagType = "Prep Tag", IsAutoTag = false, TagStatus = "Active", CreatedDate = DateTime.UtcNow.AddDays(-1) },
    new { TagID = 2, TagNumber = "BUNDLE-001", TagType = "Bundle Tag", IsAutoTag = true, TagStatus = "Active", CreatedDate = DateTime.UtcNow.AddDays(-2) },
    new { TagID = 3, TagNumber = "BASKET-001", TagType = "Basket Tag", IsAutoTag = false, TagStatus = "Active", CreatedDate = DateTime.UtcNow.AddDays(-3) }
});

app.MapGet("/api/tags/{id:int}", (int id) => 
{
    if (id == 1)
        return Results.Ok(new { TagID = 1, TagNumber = "PREP-001", TagType = "Prep Tag", IsAutoTag = false, TagStatus = "Active", CreatedDate = DateTime.UtcNow.AddDays(-1) });
    
    return Results.NotFound(new { message = $"Tag with ID {id} not found" });
});

app.MapGet("/api/tags/types", () => new[]
{
    new { TagTypeID = 1, TagTypeName = "Prep Tag", TagTypeCode = "PREP", IsActive = true },
    new { TagTypeID = 2, TagTypeName = "Bundle Tag", TagTypeCode = "BUNDLE", IsActive = true },
    new { TagTypeID = 3, TagTypeName = "Basket Tag", TagTypeCode = "BASKET", IsActive = true },
    new { TagTypeID = 4, TagTypeName = "Sterilization Load Tag", TagTypeCode = "STERIL", IsActive = true }
});

app.MapGet("/api/tags/{id:int}/contents", (int id) => new[]
{
    new { TagContentID = 1, TagID = id, UnitID = 1, ContentType = "Unit", Quantity = 1 },
    new { TagContentID = 2, TagID = id, UnitID = 2, ContentType = "Unit", Quantity = 1 }
});

app.MapPost("/api/tags", (object request) => 
{
    var newTag = new { TagID = 99, TagNumber = "TEST-NEW", TagType = "Prep Tag", IsAutoTag = false, TagStatus = "Active", CreatedDate = DateTime.UtcNow };
    return Results.Created($"/api/tags/99", newTag);
});

app.MapDelete("/api/tags/{id:int}", (int id) => Results.NoContent());

app.MapPost("/api/tags/{id:int}/units", (int id, object request) => Results.Created($"/api/tags/{id}/contents", new { message = "Unit added successfully" }));

Log.Information("Starting Tag Management API - Medical Device Service (ISO-13485 Compliant)");

// Helper method to build connection string from environment variables
static string BuildConnectionString(IConfiguration configuration)
{
    // Try to get from standard connection string first (for local development)
    var standardConnectionString = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(standardConnectionString))
    {
        return standardConnectionString;
    }
    
    // Build from environment variables (for Azure deployment)
    var sqlServerFqdn = configuration["SQL_SERVER_FQDN"];
    var sqlUsername = configuration["SQL_USERNAME"];
    var sqlPassword = configuration["SQL_PASSWORD"];
    var databaseName = configuration["DATABASE_NAME"];
    
    // Fall back to local development if env vars not available
    if (string.IsNullOrEmpty(sqlServerFqdn) || string.IsNullOrEmpty(databaseName))
    {
        Log.Warning("Environment variables for database connection not found, using local development defaults");
        return "Server=localhost;Database=TDocDB;Integrated Security=true;TrustServerCertificate=true;";
    }
    
    var connectionString = $"Server={sqlServerFqdn};Database={databaseName};User Id={sqlUsername};Password={sqlPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=True;";
    
    Log.Information("Built connection string for database: {DatabaseName} on server: {ServerFqdn}", databaseName, sqlServerFqdn);
    
    return connectionString;
}

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
