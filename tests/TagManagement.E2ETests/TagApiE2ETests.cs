using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace TagManagement.E2ETests;

/// <summary>
/// End-to-End tests for Tag Management API
/// Medical Device Compliance: ISO-13485
/// </summary>
[Collection("E2E Tests")]
public class TagApiE2ETests : IClassFixture<E2ETestFixture>
{
    private readonly E2ETestFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly RestClient _apiClient;
    private readonly ILogger<TagApiE2ETests> _logger;

    public TagApiE2ETests(E2ETestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _apiClient = _fixture.ApiClient;
        _logger = _fixture.ServiceProvider.GetRequiredService<ILogger<TagApiE2ETests>>();
    }

    private bool ShouldSkipE2ETest()
    {
        if (Environment.GetEnvironmentVariable("CI") != null && 
            Environment.GetEnvironmentVariable("E2E_TESTS_ENABLED") != "true")
        {
            _output.WriteLine("Skipping E2E test - running in CI environment without service");
            return true;
        }
        return false;
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "MedicalDevice")]
    [Trait("Priority", "High")]
    public async Task MD_E2E_001_HealthCheck_ShouldReturnHealthy()
    {
        if (ShouldSkipE2ETest()) return;

        // Arrange
        _logger.LogInformation("Testing API health endpoint for medical device compliance");

        // Act
        var request = new RestRequest("/health", Method.Get);
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Health endpoint must be accessible for medical device monitoring");
        response.Content.Should().NotBeNullOrEmpty("Health check must return status information");
        
        _output.WriteLine($"Health Check Response: {response.Content}");
        _logger.LogInformation("Health check passed - API is operational");
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "MedicalDevice")]
    [Trait("Priority", "Critical")]
    public async Task MD_E2E_002_GetAllTags_ShouldReturnTagList()
    {
        if (ShouldSkipE2ETest()) return;

        // Arrange
        _logger.LogInformation("Testing get all tags endpoint with medical device test data");

        // Act
        var request = new RestRequest("/api/tags", Method.Get);
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Tags endpoint must be accessible");
        response.Content.Should().NotBeNullOrEmpty("Tags endpoint must return data");

        var tags = JsonConvert.DeserializeObject<List<TagDto>>(response.Content);
        tags.Should().NotBeNull("Response should deserialize to tag list");
        tags.Should().HaveCountGreaterThan(0, "Test database should contain sample tags");

        _output.WriteLine($"Retrieved {tags.Count} tags from the system");
        _logger.LogInformation("Successfully retrieved tag list - basic CRUD operation verified");
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "MedicalDevice")]
    [Trait("Priority", "Critical")]
    public async Task MD_E2E_003_GetTagById_ShouldReturnSpecificTag()
    {
        if (ShouldSkipE2ETest()) return;

        // Arrange
        _logger.LogInformation("Testing get tag by ID endpoint for medical device traceability");
        const int testTagId = 1; // From our test data

        // Act
        var request = new RestRequest($"/api/tags/{testTagId}", Method.Get);
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Individual tag retrieval is critical for medical device traceability");
        response.Content.Should().NotBeNullOrEmpty();

        var tag = JsonConvert.DeserializeObject<TagDto>(response.Content);
        tag.Should().NotBeNull("Tag should be deserializable");
        tag.TagID.Should().Be(testTagId, "Should return the requested tag");
        tag.TagNumber.Should().NotBeNullOrEmpty("Tag number is required for medical device tracking");

        _output.WriteLine($"Retrieved tag: {tag.TagNumber} of type {tag.TagType}");
        _logger.LogInformation("Tag retrieval by ID successful - traceability requirement met");
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "MedicalDevice")]
    [Trait("Priority", "High")]
    public async Task MD_E2E_004_CreateNewTag_ShouldReturnCreatedTag()
    {
        if (ShouldSkipE2ETest()) return;

        // Arrange
        _logger.LogInformation("Testing tag creation for medical device workflow");
        var newTag = new CreateTagRequest
        {
            TagNumber = $"TEST-{Guid.NewGuid():N}"[..13].ToUpper(),
            TagTypeID = 1, // Prep Tag from test data
            IsAutoTag = false
        };

        // Act
        var request = new RestRequest("/api/tags", Method.Post);
        request.AddJsonBody(newTag);
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "Tag creation is essential for medical device operations");
        response.Content.Should().NotBeNullOrEmpty();

        var createdTag = JsonConvert.DeserializeObject<TagDto>(response.Content);
        createdTag.Should().NotBeNull("Created tag should be returned");
        createdTag.TagNumber.Should().Be(newTag.TagNumber, "Tag number should match request");
        createdTag.TagID.Should().BeGreaterThan(0, "Created tag should have valid ID");

        _output.WriteLine($"Created tag: {createdTag.TagNumber} with ID {createdTag.TagID}");
        _logger.LogInformation("Tag creation successful - medical device workflow enabled");

        // Cleanup - Delete the test tag
        var deleteRequest = new RestRequest($"/api/tags/{createdTag.TagID}", Method.Delete);
        await _apiClient.ExecuteAsync(deleteRequest);
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "MedicalDevice")]
    [Trait("Priority", "Critical")]
    public async Task MD_E2E_005_TagContentManagement_ShouldMaintainDataIntegrity()
    {
        if (ShouldSkipE2ETest()) return;

        // Arrange
        _logger.LogInformation("Testing tag content management for medical device compliance");
        const int testTagId = 1; // PREP-001 from test data
        const int testUnitId = 1; // UNIT-001 from test data

        // Act & Assert - Get tag contents before
        var getRequest = new RestRequest($"/api/tags/{testTagId}/contents", Method.Get);
        var initialResponse = await _apiClient.ExecuteAsync(getRequest);
        
        initialResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Tag contents must be retrievable for audit");
        var initialContents = JsonConvert.DeserializeObject<List<TagContentDto>>(initialResponse.Content);
        var initialCount = initialContents?.Count ?? 0;

        _output.WriteLine($"Initial tag contents count: {initialCount}");

        // Test adding unit to tag (if not already present)
        var addUnitRequest = new RestRequest($"/api/tags/{testTagId}/units", Method.Post);
        addUnitRequest.AddJsonBody(new { UnitId = testUnitId });
        var addResponse = await _apiClient.ExecuteAsync(addUnitRequest);
        
        // Should either succeed (201) or conflict (409) if already exists
        (addResponse.StatusCode == HttpStatusCode.Created || 
         addResponse.StatusCode == HttpStatusCode.Conflict)
        .Should().BeTrue("Unit addition should either succeed or indicate existing relationship");

        // Verify tag contents after addition
        var afterAddResponse = await _apiClient.ExecuteAsync(getRequest);
        afterAddResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterAddContents = JsonConvert.DeserializeObject<List<TagContentDto>>(afterAddResponse.Content);
        
        afterAddContents.Should().NotBeNull("Contents should be retrievable after modification");
        afterAddContents.Should().ContainSingle(c => c.UnitID == testUnitId, 
            "Tag should contain the unit for medical device traceability");

        _output.WriteLine($"Tag contents after addition: {afterAddContents.Count}");
        _logger.LogInformation("Tag content management validated - data integrity maintained");
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "MedicalDevice")]
    [Trait("Priority", "Medium")]
    public async Task MD_E2E_006_TagTypeValidation_ShouldEnforceMedicalDeviceRules()
    {
        if (ShouldSkipE2ETest()) return;

        // Arrange
        _logger.LogInformation("Testing tag type validation for medical device compliance");

        // Act - Get available tag types
        var request = new RestRequest("/api/tags/types", Method.Get);
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Tag types must be retrievable for validation");
        response.Content.Should().NotBeNullOrEmpty();

        var tagTypes = JsonConvert.DeserializeObject<List<TagTypeDto>>(response.Content);
        tagTypes.Should().NotBeNull("Tag types should be deserializable");
        tagTypes.Should().HaveCountGreaterThan(0, "System must have defined tag types");

        // Verify medical device specific tag types exist
        tagTypes.Should().Contain(tt => tt.TagTypeCode == "PREP", 
            "Prep tags are required for medical device workflows");
        tagTypes.Should().Contain(tt => tt.TagTypeCode == "STERIL", 
            "Sterilization tags are required for medical device compliance");

        _output.WriteLine($"Available tag types: {string.Join(", ", tagTypes.Select(t => t.TagTypeCode))}");
        _logger.LogInformation("Tag type validation successful - medical device rules enforced");
    }

    [Fact]
    [Trait("Category", "E2E")]
    [Trait("Category", "MedicalDevice")]
    [Trait("Priority", "High")]
    public async Task MD_E2E_007_DatabaseConnectivity_ShouldMaintainDataPersistence()
    {
        if (ShouldSkipE2ETest()) return;

        // Arrange
        _logger.LogInformation("Testing database connectivity and data persistence");

        // Act - Perform multiple operations to test persistence
        var operations = new[]
        {
            new RestRequest("/api/tags", Method.Get),
            new RestRequest("/api/tags/1", Method.Get),
            new RestRequest("/api/tags/types", Method.Get)
        };

        var results = new List<RestResponse>();
        foreach (var operation in operations)
        {
            var response = await _apiClient.ExecuteAsync(operation);
            results.Add(response);
            
            // Small delay to test connection stability
            await Task.Delay(100);
        }

        // Assert
        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK), 
            "All database operations should succeed for medical device reliability");

        results.Should().AllSatisfy(r => r.Content.Should().NotBeNullOrEmpty(), 
            "All operations should return data indicating persistent storage");

        _output.WriteLine($"Executed {results.Count} database operations successfully");
        _logger.LogInformation("Database connectivity and persistence validated");
    }
}

// DTOs for E2E testing (simplified versions)
public class TagDto
{
    public int TagID { get; set; }
    public string TagNumber { get; set; } = string.Empty;
    public int TagTypeID { get; set; }
    public string TagType { get; set; } = string.Empty;
    public bool IsAutoTag { get; set; }
    public string TagStatus { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class TagContentDto
{
    public int TagContentID { get; set; }
    public int TagID { get; set; }
    public int? UnitID { get; set; }
    public int? ChildTagID { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public int Quantity { get; set; }
}

public class TagTypeDto
{
    public int TagTypeID { get; set; }
    public string TagTypeName { get; set; } = string.Empty;
    public string TagTypeCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateTagRequest
{
    public string TagNumber { get; set; } = string.Empty;
    public int TagTypeID { get; set; }
    public bool IsAutoTag { get; set; }
}
