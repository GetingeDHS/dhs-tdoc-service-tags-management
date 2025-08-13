using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using Xunit;

namespace TagManagement.E2ETests;

/// <summary>
/// Test fixture for End-to-End tests
/// Manages test environment setup for medical device compliance testing
/// </summary>
public class E2ETestFixture : IDisposable
{
    private bool _disposed = false;

    public IServiceProvider ServiceProvider { get; private set; }
    public IConfiguration Configuration { get; private set; }
    public RestClient ApiClient { get; private set; }
    public string ApiBaseUrl { get; private set; }

    public E2ETestFixture()
    {
        InitializeConfiguration();
        InitializeServices();
        InitializeApiClient();
    }

    private void InitializeConfiguration()
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .AddEnvironmentVariables();

        Configuration = configBuilder.Build();
    }

    private void InitializeServices()
    {
        var services = new ServiceCollection();

        // Add configuration
        services.AddSingleton(Configuration);

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        ServiceProvider = services.BuildServiceProvider();
    }

    private void InitializeApiClient()
    {
        // Get API base URL from environment or configuration
        ApiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") 
                    ?? Configuration["ApiSettings:BaseUrl"] 
                    ?? "http://localhost:5000";

        var options = new RestClientOptions(ApiBaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        ApiClient = new RestClient(options);

        // Add default headers
        ApiClient.AddDefaultHeader("Accept", "application/json");
        ApiClient.AddDefaultHeader("Content-Type", "application/json");
        ApiClient.AddDefaultHeader("User-Agent", "TagManagement-E2E-Tests/1.0");

        var logger = ServiceProvider.GetRequiredService<ILogger<E2ETestFixture>>();
        logger.LogInformation($"E2E Test Fixture initialized with API base URL: {ApiBaseUrl}");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            ApiClient?.Dispose();
            (ServiceProvider as IDisposable)?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Collection definition for E2E tests to ensure proper test isolation
/// </summary>
[CollectionDefinition("E2E Tests")]
public class E2ETestCollection : ICollectionFixture<E2ETestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
