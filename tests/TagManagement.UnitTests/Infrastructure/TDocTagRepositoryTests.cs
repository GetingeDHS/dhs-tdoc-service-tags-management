using TagManagement.Core.Entities;
using TagManagement.Core.Enums;
using TagManagement.Core.Interfaces;
using TagManagement.Infrastructure.Persistence;
using TagManagement.Infrastructure.Persistence.Models;
using TagManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TagManagement.UnitTests.Infrastructure;

/// <summary>
/// Medical Device Compliance Tests for TDocTagRepository
/// Tests data access layer for regulatory compliance
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Infrastructure")]
public class TDocTagRepositoryTests : IDisposable
{
    private readonly TagManagementDbContext _context;
    private readonly TDocTagRepository _repository;

    public TDocTagRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TagManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TagManagementDbContext(options);
        _repository = new TDocTagRepository(_context);
    }

    /// <summary>
    /// MD-DATA-001: Repository must handle TDOC database schema correctly
    /// Critical for data integrity and legacy system compatibility
    /// </summary>
    [Fact(DisplayName = "MD-DATA-001: Repository Must Handle TDOC Schema")]
    public async Task Repository_Should_Handle_TDOC_Schema_Correctly()
    {
        // Arrange - Create test data matching TDOC schema
        var tagsModel = new TagsModel
        {
            TagKeyId = 1,
            TagNumber = 12345,
            TagTypeKeyId = 1,
            IsAutoTag = false,
            LocationKeyId = 100,
            CreatedTime = DateTime.UtcNow,
            CreatedByUserKeyId = 1
        };

        var tagTypeModel = new TagTypeModel
        {
            TagTypeKeyId = 1,
            TagTypeName = "PrepTag",
            IsActive = true
        };

        var locationModel = new LocationModel
        {
            LocationKeyId = 100,
            LocationName = "Test Location",
            LocationCode = "TL01",
            IsActive = true
        };

        _context.TagTypes.Add(tagTypeModel);
        _context.Locations.Add(locationModel);
        _context.Tags.Add(tagsModel);
        await _context.SaveChangesAsync();

        // Act
        var retrievedTag = await _repository.GetByIdAsync(1);

        // Assert
        retrievedTag.Should().NotBeNull("Tag should be retrieved from TDOC schema");
        retrievedTag!.TagNumber.Should().Be(12345, "Tag number must match TDOC data");
        retrievedTag.TagType.Should().Be(TagType.PrepTag, "Tag type must be converted correctly");
        retrievedTag.LocationKeyId.Should().Be(100, "Location reference must be preserved");
    }

    /// <summary>
    /// MD-DATA-002: Repository must maintain data integrity during CRUD operations
    /// Critical for medical device traceability
    /// </summary>
    [Fact(DisplayName = "MD-DATA-002: Repository Must Maintain Data Integrity")]
    public async Task Repository_Should_Maintain_Data_Integrity_During_CRUD()
    {
        // Arrange
        var originalTag = new Tag
        {
            TagNumber = 54321,
            TagType = TagType.BundleTag,
            TagTypeKeyId = 2,
            IsAuto = true,
            LocationKeyId = 200,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        // Act - Create
        var createdTag = await _repository.AddAsync(originalTag);
        
        // Assert - Creation
        createdTag.Should().NotBeNull("Created tag should be returned");
        createdTag.Id.Should().BeGreaterThan(0, "Tag should have assigned ID");

        // Act - Read
        var retrievedTag = await _repository.GetByIdAsync(createdTag.Id);
        
        // Assert - Read integrity
        retrievedTag.Should().NotBeNull("Tag should be retrievable");
        retrievedTag!.TagNumber.Should().Be(54321, "Tag number must be preserved");
        retrievedTag.TagType.Should().Be(TagType.BundleTag, "Tag type must be preserved");
        retrievedTag.IsAuto.Should().BeTrue("Auto flag must be preserved");

        // Act - Update
        retrievedTag.TagNumber = 99999;
        retrievedTag.UpdatedAt = DateTime.UtcNow;
        retrievedTag.UpdatedBy = "UpdateSystem";
        var updatedTag = await _repository.UpdateAsync(retrievedTag);

        // Assert - Update integrity
        updatedTag.TagNumber.Should().Be(99999, "Updated tag number must be preserved");
        updatedTag.UpdatedBy.Should().Be("UpdateSystem", "Update audit must be preserved");

        // Act - Delete
        var deleteResult = await _repository.DeleteAsync(createdTag.Id);
        
        // Assert - Delete integrity
        deleteResult.Should().BeTrue("Delete should succeed");
        var deletedTag = await _repository.GetByIdAsync(createdTag.Id);
        deletedTag.Should().BeNull("Deleted tag should not be retrievable");
    }

    /// <summary>
    /// MD-DATA-003: Repository must handle tag content relationships correctly
    /// Critical for manufacturing process tracking
    /// </summary>
    [Fact(DisplayName = "MD-DATA-003: Repository Must Handle Tag Content Relationships")]
    public async Task Repository_Should_Handle_Tag_Content_Relationships()
    {
        // Arrange - Create tag with content
        var tag = new Tag
        {
            TagNumber = 77777,
            TagType = TagType.TransportTag,
            TagTypeKeyId = 3,
            LocationKeyId = 300,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        // Act - Add tag and content
        var createdTag = await _repository.AddAsync(tag);
        var currentTime = DateTime.UtcNow;
        
        // Add unit to tag
        var addUnitResult = await _repository.AddUnitToTagAsync(
            createdTag.Id, 1001, currentTime, 300, false);
        
        // Add item to tag
        var tagItem = new TagItem(2001, 3001, 4001, 5);
        var addItemResult = await _repository.AddItemToTagAsync(
            createdTag.Id, tagItem, currentTime, 300);

        // Assert - Content addition
        addUnitResult.Should().BeTrue("Unit should be added to tag");
        addItemResult.Should().BeTrue("Item should be added to tag");

        // Act - Retrieve tag with content
        var tagWithContent = await _repository.GetByIdAsync(createdTag.Id);

        // Assert - Content relationships
        tagWithContent.Should().NotBeNull("Tag should be retrievable with content");
        tagWithContent!.Contents.Units.Should().Contain(1001, "Unit should be in tag content");
        tagWithContent.Contents.Items.Should().HaveCount(1, "Item should be in tag content");
        tagWithContent.Contents.Items.First().ItemKeyId.Should().Be(2001, "Item details must be preserved");
        tagWithContent.IsEmpty.Should().BeFalse("Tag with content should not be empty");
    }

    /// <summary>
    /// MD-DATA-004: Repository must support tag hierarchy operations
    /// Critical for nested tag structures in manufacturing
    /// </summary>
    [Fact(DisplayName = "MD-DATA-004: Repository Must Support Tag Hierarchy")]
    public async Task Repository_Should_Support_Tag_Hierarchy()
    {
        // Arrange - Create parent and child tags
        var parentTag = new Tag
        {
            TagNumber = 10001,
            TagType = TagType.TransportBoxTag,
            TagTypeKeyId = 4,
            LocationKeyId = 400,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var childTag = new Tag
        {
            TagNumber = 10002,
            TagType = TagType.PrepTag,
            TagTypeKeyId = 1,
            LocationKeyId = 400,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        // Act - Create tags
        var createdParent = await _repository.AddAsync(parentTag);
        var createdChild = await _repository.AddAsync(childTag);

        // Add child to parent
        var hierarchyResult = await _repository.AddTagToTagAsync(
            createdParent.Id, createdChild.Id, DateTime.UtcNow, 400);

        // Assert - Hierarchy creation
        hierarchyResult.Should().BeTrue("Child tag should be added to parent");

        // Act - Query hierarchy
        var childTags = await _repository.GetChildTagsAsync(createdParent.Id);
        var parentOfChild = await _repository.GetParentTagAsync(createdChild.Id);

        // Assert - Hierarchy queries
        childTags.Should().HaveCount(1, "Parent should have one child");
        childTags.First().TagNumber.Should().Be(10002, "Child tag should be correct");
        
        parentOfChild.Should().NotBeNull("Child should have parent");
        parentOfChild!.TagNumber.Should().Be(10001, "Parent should be correct");
    }

    /// <summary>
    /// MD-DATA-005: Repository must handle concurrent operations safely
    /// Critical for multi-user manufacturing environment
    /// </summary>
    [Fact(DisplayName = "MD-DATA-005: Repository Must Handle Concurrent Operations")]
    public async Task Repository_Should_Handle_Concurrent_Operations_Safely()
    {
        // Arrange - Create base tag
        var baseTag = new Tag
        {
            TagNumber = 20001,
            TagType = TagType.WashTag,
            TagTypeKeyId = 5,
            LocationKeyId = 500,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var createdTag = await _repository.AddAsync(baseTag);

        // Act - Simulate concurrent unit additions
        var tasks = new List<Task<bool>>();
        for (int i = 1; i <= 5; i++)
        {
            int unitId = 2000 + i;
            tasks.Add(_repository.AddUnitToTagAsync(
                createdTag.Id, unitId, DateTime.UtcNow, 500, true)); // Mark as split for concurrent adds
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All operations should succeed
        results.Should().AllSatisfy(result => result.Should().BeTrue("Concurrent operations should succeed"));

        // Verify final state
        var finalTag = await _repository.GetByIdAsync(createdTag.Id);
        finalTag!.Contents.Units.Should().HaveCount(5, "All units should be added");
    }

    /// <summary>
    /// MD-DATA-006: Repository must support efficient querying for large datasets
    /// Critical for manufacturing system performance
    /// </summary>
    [Theory(DisplayName = "MD-DATA-006: Repository Must Support Efficient Querying")]
    [InlineData(100)]  // Small dataset
    [InlineData(1000)] // Large dataset
    public async Task Repository_Should_Support_Efficient_Querying(int tagCount)
    {
        // Arrange - Create multiple tags
        var tags = new List<Tag>();
        for (int i = 1; i <= tagCount; i++)
        {
            tags.Add(new Tag
            {
                TagNumber = 30000 + i,
                TagType = TagType.PrepTag,
                TagTypeKeyId = 1,
                LocationKeyId = 600,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                CreatedBy = "TestSystem"
            });
        }

        // Act - Bulk insert (simulate)
        foreach (var tag in tags)
        {
            await _repository.AddAsync(tag);
        }

        // Act - Query operations
        var startTime = DateTime.UtcNow;
        
        var allTags = await _repository.GetAllAsync();
        var pagedTags = await _repository.GetPagedAsync(1, 50);
        var tagsByType = await _repository.GetTagsByTypeAsync(TagType.PrepTag);
        var tagsByLocation = await _repository.GetTagsByLocationAsync(600);
        
        var endTime = DateTime.UtcNow;
        var queryDuration = endTime - startTime;

        // Assert - Query results and performance
        allTags.Should().HaveCount(tagCount, "All tags should be returned");
        pagedTags.Should().HaveCount(Math.Min(50, tagCount), "Paged results should be limited");
        tagsByType.Should().HaveCount(tagCount, "Type filter should work");
        tagsByLocation.Should().HaveCount(tagCount, "Location filter should work");
        
        queryDuration.Should().BeLessThan(TimeSpan.FromSeconds(5), 
            "Queries should complete within reasonable time");
    }

    /// <summary>
    /// MD-DATA-007: Repository must validate business rules
    /// Critical for data quality and process integrity
    /// </summary>
    [Fact(DisplayName = "MD-DATA-007: Repository Must Validate Business Rules")]
    public async Task Repository_Should_Validate_Business_Rules()
    {
        // Arrange - Create test tag
        var tag = new Tag
        {
            TagNumber = 40001,
            TagType = TagType.SterilizationLoadTag,
            TagTypeKeyId = 6,
            LocationKeyId = 700,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var createdTag = await _repository.AddAsync(tag);

        // Act & Assert - Unit can only be in one tag (non-split)
        var unit1Result = await _repository.AddUnitToTagAsync(
            createdTag.Id, 4001, DateTime.UtcNow, 700, false);
        unit1Result.Should().BeTrue("First unit addition should succeed");

        // Create another tag and try to add same unit
        var anotherTag = new Tag
        {
            TagNumber = 40002,
            TagType = TagType.PrepTag,
            TagTypeKeyId = 1,
            LocationKeyId = 700,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };
        var anotherCreatedTag = await _repository.AddAsync(anotherTag);

        var unit2Result = await _repository.AddUnitToTagAsync(
            anotherCreatedTag.Id, 4001, DateTime.UtcNow, 700, false);
        unit2Result.Should().BeTrue("Unit should be moved to new tag");

        // Verify unit was moved
        var originalTag = await _repository.GetByIdAsync(createdTag.Id);
        var newTag = await _repository.GetByIdAsync(anotherCreatedTag.Id);

        originalTag!.Contents.Units.Should().NotContain(4001, "Unit should be removed from original tag");
        newTag!.Contents.Units.Should().Contain(4001, "Unit should be in new tag");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
