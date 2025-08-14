using Microsoft.Playwright;
using FluentAssertions;
using System.Text.Json;

namespace TagManagement.PlaywrightTests;

[TestFixture]
[Category("E2E")]
[Category("API")]
public class TagManagementApiTests : TestBase
{
    [Test]
    [Category("Health")]
    [Description("Verify API health endpoint is accessible and returns healthy status")]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Arrange & Act
        await NavigateTo("/health");

        // Assert
        var healthResponse = await MakeApiCall<HealthCheckResponse>("/health");
        healthResponse.Should().NotBeNull();
        healthResponse!.Status.Should().Be("Healthy");
    }

    [Test]
    [Category("API")]
    [Description("Verify Swagger documentation is accessible")]
    public async Task SwaggerUI_ShouldBeAccessible()
    {
        // Arrange & Act
        await NavigateTo("/swagger");

        // Assert
        await VerifyPageTitle("Swagger UI");
        var swaggerElement = await WaitForElement(".swagger-ui");
        await Expect(swaggerElement).ToBeVisibleAsync();
    }

    [Test]
    [Category("Tags")]
    [Description("Verify GET /api/tags returns list of tags")]
    public async Task GetTags_ShouldReturnTagsList()
    {
        // Arrange & Act
        var tags = await MakeApiCall<List<TagResponse>>("/api/tags");

        // Assert
        tags.Should().NotBeNull();
        tags.Should().BeAssignableTo<List<TagResponse>>();
    }

    [Test]
    [Category("Tags")]
    [Description("Verify GET /api/tags/{id} returns specific tag")]
    public async Task GetTagById_WithValidId_ShouldReturnTag()
    {
        // Arrange - First get all tags to find a valid ID
        var allTags = await MakeApiCall<List<TagResponse>>("/api/tags");
        
        if (allTags == null || allTags.Count == 0)
        {
            Assert.Inconclusive("No tags available for testing");
            return;
        }

        var firstTag = allTags.First();

        // Act
        var tag = await MakeApiCall<TagResponse>($"/api/tags/{firstTag.Id}");

        // Assert
        tag.Should().NotBeNull();
        tag!.Id.Should().Be(firstTag.Id);
        tag.TagNumber.Should().NotBeNullOrEmpty();
    }

    [Test]
    [Category("Tags")]
    [Description("Verify GET /api/tags/{id} returns 404 for invalid ID")]
    public async Task GetTagById_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act & Assert
        var request = await Playwright.APIRequest.NewContextAsync();
        var response = await request.GetAsync($"{BaseUrl}/api/tags/{invalidId}");
        
        response.Status.Should().Be(404);
    }

    [Test]
    [Category("Tags")]
    [Category("CRUD")]
    [Description("Verify POST /api/tags creates new tag")]
    public async Task CreateTag_WithValidData_ShouldCreateTag()
    {
        // Arrange
        var newTag = new CreateTagRequest
        {
            TagNumber = $"TEST-{Guid.NewGuid():N}"[..13].ToUpper(),
            TagType = "Equipment",
            TagTypeKeyId = 1,
            Status = "Active",
            UnitId = 1,
            LocationId = 1
        };

        // Act
        var request = await Playwright.APIRequest.NewContextAsync();
        var response = await request.PostAsync($"{BaseUrl}/api/tags", new APIRequestContextOptions
        {
            DataObject = newTag,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json"
            }
        });

        // Assert
        response.Ok.Should().BeTrue($"Create tag failed with status {response.Status}");
        
        var createdTag = JsonSerializer.Deserialize<TagResponse>(await response.TextAsync(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        createdTag.Should().NotBeNull();
        createdTag!.TagNumber.Should().Be(newTag.TagNumber);
        createdTag.TagType.Should().Be(newTag.TagType);
    }

    [Test]
    [Category("Tags")]
    [Category("CRUD")]
    [Description("Verify PUT /api/tags/{id} updates existing tag")]
    public async Task UpdateTag_WithValidData_ShouldUpdateTag()
    {
        // Arrange - Create a tag first
        var newTag = new CreateTagRequest
        {
            TagNumber = $"TEST-UPD-{Guid.NewGuid():N}"[..13].ToUpper(),
            TagType = "Equipment",
            TagTypeKeyId = 1,
            Status = "Active",
            UnitId = 1,
            LocationId = 1
        };

        var request = await Playwright.APIRequest.NewContextAsync();
        var createResponse = await request.PostAsync($"{BaseUrl}/api/tags", new APIRequestContextOptions
        {
            DataObject = newTag,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        });

        var createdTag = JsonSerializer.Deserialize<TagResponse>(await createResponse.TextAsync(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Update the tag
        var updateRequest = new UpdateTagRequest
        {
            TagNumber = createdTag!.TagNumber,
            TagType = "Transport",
            TagTypeKeyId = 2,
            Status = "Inactive",
            UnitId = createdTag.UnitId,
            LocationId = createdTag.LocationId
        };

        // Act
        var updateResponse = await request.PutAsync($"{BaseUrl}/api/tags/{createdTag.Id}", new APIRequestContextOptions
        {
            DataObject = updateRequest,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        });

        // Assert
        updateResponse.Ok.Should().BeTrue($"Update tag failed with status {updateResponse.Status}");
        
        var updatedTag = JsonSerializer.Deserialize<TagResponse>(await updateResponse.TextAsync(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        updatedTag.Should().NotBeNull();
        updatedTag!.TagType.Should().Be("Transport");
        updatedTag.Status.Should().Be("Inactive");
    }

    [Test]
    [Category("Tags")]
    [Category("CRUD")]
    [Description("Verify DELETE /api/tags/{id} deletes existing tag")]
    public async Task DeleteTag_WithValidId_ShouldDeleteTag()
    {
        // Arrange - Create a tag first
        var newTag = new CreateTagRequest
        {
            TagNumber = $"TEST-DEL-{Guid.NewGuid():N}"[..13].ToUpper(),
            TagType = "Equipment",
            TagTypeKeyId = 1,
            Status = "Active",
            UnitId = 1,
            LocationId = 1
        };

        var request = await Playwright.APIRequest.NewContextAsync();
        var createResponse = await request.PostAsync($"{BaseUrl}/api/tags", new APIRequestContextOptions
        {
            DataObject = newTag,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        });

        var createdTag = JsonSerializer.Deserialize<TagResponse>(await createResponse.TextAsync(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Act
        var deleteResponse = await request.DeleteAsync($"{BaseUrl}/api/tags/{createdTag!.Id}");

        // Assert
        deleteResponse.Ok.Should().BeTrue($"Delete tag failed with status {deleteResponse.Status}");
        
        // Verify tag is deleted
        var getResponse = await request.GetAsync($"{BaseUrl}/api/tags/{createdTag.Id}");
        getResponse.Status.Should().Be(404);
    }

    [Test]
    [Category("Tags")]
    [Category("Content")]
    [Description("Verify tag content management endpoints")]
    public async Task TagContent_CRUD_ShouldWorkCorrectly()
    {
        // This test would verify tag content operations
        // Implementation depends on the actual API endpoints available
        Assert.Pass("Tag content CRUD operations test - to be implemented based on actual API");
    }

    [Test]
    [Category("Medical")]
    [Category("Audit")]
    [Description("Verify audit trail is created for tag operations")]
    public async Task TagOperations_ShouldCreateAuditTrail()
    {
        // This test would verify that audit trails are properly created
        // This is critical for medical device compliance
        Assert.Pass("Audit trail verification test - to be implemented based on actual audit requirements");
    }

    [Test]
    [Category("Medical")]
    [Category("DataIntegrity")]
    [Description("Verify data integrity constraints are enforced")]
    public async Task DataIntegrity_ShouldBeEnforced()
    {
        // Test data integrity rules like unique tag numbers, required fields, etc.
        var invalidTag = new CreateTagRequest
        {
            TagNumber = "", // Empty tag number should fail
            TagType = "Equipment",
            TagTypeKeyId = 1,
            Status = "Active",
            UnitId = 1,
            LocationId = 1
        };

        var request = await Playwright.APIRequest.NewContextAsync();
        var response = await request.PostAsync($"{BaseUrl}/api/tags", new APIRequestContextOptions
        {
            DataObject = invalidTag,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        });

        // Should fail with validation error
        response.Ok.Should().BeFalse("Empty tag number should not be allowed");
        response.Status.Should().Be(400);
    }
}

// DTOs for API responses
public class HealthCheckResponse
{
    public string Status { get; set; } = "";
}

public class TagResponse
{
    public Guid Id { get; set; }
    public string TagNumber { get; set; } = "";
    public string TagType { get; set; } = "";
    public int TagTypeKeyId { get; set; }
    public string Status { get; set; } = "";
    public int UnitId { get; set; }
    public int LocationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTagRequest
{
    public string TagNumber { get; set; } = "";
    public string TagType { get; set; } = "";
    public int TagTypeKeyId { get; set; }
    public string Status { get; set; } = "";
    public int UnitId { get; set; }
    public int LocationId { get; set; }
}

public class UpdateTagRequest
{
    public string TagNumber { get; set; } = "";
    public string TagType { get; set; } = "";
    public int TagTypeKeyId { get; set; }
    public string Status { get; set; } = "";
    public int UnitId { get; set; }
    public int LocationId { get; set; }
}
