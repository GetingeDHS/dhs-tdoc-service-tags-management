using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using System.Text.Json;

namespace TagManagement.PlaywrightTests;

public abstract class TestBase : PageTest
{
    protected IConfiguration Configuration { get; private set; } = null!;
    protected string BaseUrl { get; private set; } = null!;
    protected TestSettings TestSettings { get; private set; } = null!;

    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        // Load configuration
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Get test settings
        TestSettings = Configuration.GetSection("TestSettings").Get<TestSettings>()
            ?? throw new InvalidOperationException("TestSettings configuration is missing");

        // Set base URL - use Azure URL if available, otherwise local
        BaseUrl = !string.IsNullOrEmpty(TestSettings.AzureTestUrl) 
            ? TestSettings.AzureTestUrl 
            : TestSettings.BaseUrl;

        Console.WriteLine($"Running tests against: {BaseUrl}");
    }

    [SetUp]
    public virtual async Task SetUp()
    {
        // Set default timeout for page operations
        Page.SetDefaultTimeout(TestSettings.Timeout);
        
        // Set base URL for all requests
        if (!string.IsNullOrEmpty(BaseUrl))
        {
            await Page.GotoAsync(BaseUrl);
        }
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        // Take screenshot on failure
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var screenshot = await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = $"test-results/failure-{TestContext.CurrentContext.Test.Name}-{DateTime.Now:yyyyMMdd-HHmmss}.png",
                FullPage = true
            });
        }
    }

    /// <summary>
    /// Helper method to wait for API response and verify status
    /// </summary>
    protected async Task<IResponse> WaitForApiResponse(string endpoint, int expectedStatus = 200)
    {
        var response = await Page.WaitForResponseAsync(
            response => response.Url.Contains(endpoint) && response.Status == expectedStatus,
            new PageWaitForResponseOptions { Timeout = TestSettings.Timeout }
        );
        
        response.Should().NotBeNull();
        response.Status.Should().Be(expectedStatus);
        
        return response;
    }

    /// <summary>
    /// Helper method to make API calls directly
    /// </summary>
    protected async Task<T?> MakeApiCall<T>(string endpoint, string method = "GET", object? body = null)
    {
        var request = await Playwright.APIRequest.NewContextAsync();
        
        var options = new APIRequestContextOptions
        {
            Method = method,
            Headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["Content-Type"] = "application/json"
            }
        };

        if (body != null)
        {
            options.DataObject = body;
        }

        var response = await request.FetchAsync($"{BaseUrl}{endpoint}", options);
        response.Ok.Should().BeTrue($"API call to {endpoint} failed with status {response.Status}");
        
        var content = await response.TextAsync();
        
        if (typeof(T) == typeof(string))
        {
            return (T)(object)content;
        }
        
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Helper method to wait for element to be visible and enabled
    /// </summary>
    protected async Task<ILocator> WaitForElement(string selector, int timeout = 0)
    {
        var element = Page.Locator(selector);
        await element.WaitForAsync(new LocatorWaitForOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = timeout > 0 ? timeout : TestSettings.Timeout
        });
        
        return element;
    }

    /// <summary>
    /// Helper method to navigate to a specific route
    /// </summary>
    protected async Task NavigateTo(string route)
    {
        await Page.GotoAsync($"{BaseUrl}{route}");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Helper method to verify page title
    /// </summary>
    protected async Task VerifyPageTitle(string expectedTitle)
    {
        await Expect(Page).ToHaveTitleAsync(expectedTitle);
    }
}

public class TestSettings
{
    public string BaseUrl { get; set; } = "https://localhost:7001";
    public string AzureTestUrl { get; set; } = "";
    public int Timeout { get; set; } = 30000;
    public string BrowserType { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public int SlowMo { get; set; } = 0;
    public bool VideoRecording { get; set; } = true;
    public bool Screenshots { get; set; } = true;
}
