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
            TagTypeKeyId = 0, // PrepTag has enum value 0
            IsAutoTag = false,
            LocationKeyId = 100,
            CreatedTime = DateTime.UtcNow,
            CreatedByUserKeyId = 1
        };

        var tagTypeModel = new TagTypeModel
        {
            TagTypeKeyId = 0,
            TagTypeName = "PrepTag", // Use correct name for TagTypeKeyId = 0
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
            TagTypeKeyId = 1, // BundleTag has enum value 1
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
            TagTypeKeyId = 6, // TransportTag has enum value 6
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
            TagTypeKeyId = 8, // TransportBoxTag has enum value 8
            LocationKeyId = 400,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var childTag = new Tag
        {
            TagNumber = 10002,
            TagType = TagType.PrepTag,
            TagTypeKeyId = 0, // PrepTag has enum value 0
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
            TagTypeKeyId = 4, // WashTag has enum value 4
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
                TagTypeKeyId = 0, // PrepTag has enum value 0
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
            TagTypeKeyId = 3, // SterilizationLoadTag has enum value 3
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
            TagTypeKeyId = 0, // PrepTag has enum value 0
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

    /// <summary>
    /// MD-REPO-021: Repository must handle GetEmptyAutoTagAsync correctly
    /// Critical for auto tag management
    /// </summary>
    [Fact(DisplayName = "MD-REPO-021: Repository Must Handle GetEmptyAutoTagAsync")]
    public async Task Repository_Should_Handle_GetEmptyAutoTagAsync()
    {
        // Arrange - Create auto tags with and without content
        var emptyAutoTag = new Tag
        {
            TagNumber = 50001,
            TagType = TagType.PrepTag,
            TagTypeKeyId = 0,
            IsAuto = true,
            LocationKeyId = 800,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };
        
        var fullAutoTag = new Tag
        {
            TagNumber = 50002,
            TagType = TagType.PrepTag,
            TagTypeKeyId = 0,
            IsAuto = true,
            LocationKeyId = 800,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var createdEmptyTag = await _repository.AddAsync(emptyAutoTag);
        var createdFullTag = await _repository.AddAsync(fullAutoTag);
        
        // Add content to full tag
        await _repository.AddUnitToTagAsync(createdFullTag.Id, 5001, DateTime.UtcNow, 800, false);

        // Act
        var emptyTag = await _repository.GetEmptyAutoTagAsync(TagType.PrepTag, 800);

        // Assert
        emptyTag.Should().NotBeNull("Empty auto tag should be found");
        emptyTag!.Id.Should().Be(createdEmptyTag.Id, "Should return the empty auto tag");
    }

    /// <summary>
    /// MD-REPO-022: Repository must handle content query methods correctly
    /// Critical for tag content management
    /// </summary>
    [Fact(DisplayName = "MD-REPO-022: Repository Must Handle Content Query Methods")]
    public async Task Repository_Should_Handle_Content_Query_Methods()
    {
        // Arrange - Create tags with specific content
        var tag1 = new Tag
        {
            TagNumber = 60001,
            TagType = TagType.BundleTag,
            TagTypeKeyId = 1,
            LocationKeyId = 900,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var tag2 = new Tag
        {
            TagNumber = 60002,
            TagType = TagType.BundleTag,
            TagTypeKeyId = 1,
            LocationKeyId = 900,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var createdTag1 = await _repository.AddAsync(tag1);
        var createdTag2 = await _repository.AddAsync(tag2);

        // Add specific unit and item to tags
        await _repository.AddUnitToTagAsync(createdTag1.Id, 6001, DateTime.UtcNow, 900, false);
        var tagItem = new TagItem(7001, 8001, 9001, 3);
        await _repository.AddItemToTagAsync(createdTag2.Id, tagItem, DateTime.UtcNow, 900);

        // Act & Assert - Content queries
        var tagsWithUnit = await _repository.GetTagsContainingUnitAsync(6001);
        tagsWithUnit.Should().HaveCount(1, "Only one tag should contain the unit");
        tagsWithUnit.First().Id.Should().Be(createdTag1.Id);

        var tagsWithItem = await _repository.GetTagsContainingItemAsync(7001, 8001);
        tagsWithItem.Should().HaveCount(1, "Only one tag should contain the item");
        tagsWithItem.First().Id.Should().Be(createdTag2.Id);

        var isUnitInTag = await _repository.IsUnitInAnyTagAsync(6001);
        isUnitInTag.Should().BeTrue("Unit should be found in tags");

        var isItemInTag = await _repository.IsItemInAnyTagAsync(7001, 8001);
        isItemInTag.Should().BeTrue("Item should be found in tags");

        var tag1ContentCount = await _repository.GetTagContentCountAsync(createdTag1.Id);
        tag1ContentCount.Should().Be(1, "Tag1 should have one content item");

        var isTag2Empty = await _repository.IsTagEmptyAsync(createdTag2.Id);
        isTag2Empty.Should().BeFalse("Tag2 should not be empty");
    }

    /// <summary>
    /// MD-REPO-023: Repository must handle hierarchy query methods correctly
    /// Critical for tag hierarchy management
    /// </summary>
    [Fact(DisplayName = "MD-REPO-023: Repository Must Handle Hierarchy Query Methods")]
    public async Task Repository_Should_Handle_Hierarchy_Query_Methods()
    {
        // Arrange - Create tag hierarchy
        var rootTag = new Tag
        {
            TagNumber = 70001,
            TagType = TagType.TransportBoxTag,
            TagTypeKeyId = 8,
            LocationKeyId = 1000,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var childTag1 = new Tag
        {
            TagNumber = 70002,
            TagType = TagType.PrepTag,
            TagTypeKeyId = 0,
            LocationKeyId = 1000,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var childTag2 = new Tag
        {
            TagNumber = 70003,
            TagType = TagType.BundleTag,
            TagTypeKeyId = 1,
            LocationKeyId = 1000,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var createdRoot = await _repository.AddAsync(rootTag);
        var createdChild1 = await _repository.AddAsync(childTag1);
        var createdChild2 = await _repository.AddAsync(childTag2);

        // Create hierarchy
        await _repository.AddTagToTagAsync(createdRoot.Id, createdChild1.Id, DateTime.UtcNow, 1000);
        await _repository.AddTagToTagAsync(createdRoot.Id, createdChild2.Id, DateTime.UtcNow, 1000);

        // Act & Assert - Hierarchy queries
        var rootTags = await _repository.GetRootTagsAsync();
        rootTags.Should().Contain(tag => tag.Id == createdRoot.Id, "Root tag should be in root tags collection");

        var rootTagId = await _repository.GetRootTagIdAsync(createdChild1.Id);
        rootTagId.Should().Be(createdRoot.Id, "Root of child should be the parent tag");
    }

    /// <summary>
    /// MD-REPO-024: Repository must handle content manipulation methods
    /// Critical for tag content operations
    /// </summary>
    [Fact(DisplayName = "MD-REPO-024: Repository Must Handle Content Manipulation Methods")]
    public async Task Repository_Should_Handle_Content_Manipulation_Methods()
    {
        // Arrange - Create tag for content operations
        var tag = new Tag
        {
            TagNumber = 80001,
            TagType = TagType.PrepTag,
            TagTypeKeyId = 0,
            LocationKeyId = 1100,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var createdTag = await _repository.AddAsync(tag);
        var currentTime = DateTime.UtcNow;

        // Act & Assert - Add and remove operations
        // Test unit operations
        var addUnitResult = await _repository.AddUnitToTagAsync(createdTag.Id, 8001, currentTime, 1100, false);
        addUnitResult.Should().BeTrue("Unit should be added successfully");

        var removeUnitResult = await _repository.RemoveUnitFromTagAsync(createdTag.Id, 8001, currentTime, 1100);
        removeUnitResult.Should().BeTrue("Unit should be removed successfully");

        // Test item operations
        var tagItem = new TagItem(8002, 8003, 8004, 2);
        var addItemResult = await _repository.AddItemToTagAsync(createdTag.Id, tagItem, currentTime, 1100);
        addItemResult.Should().BeTrue("Item should be added successfully");

        var removeItemResult = await _repository.RemoveItemFromTagAsync(createdTag.Id, tagItem, currentTime, 1100);
        removeItemResult.Should().BeTrue("Item should be removed successfully");

        // Test indicator operations
        var addIndicatorResult = await _repository.AddIndicatorToTagAsync(createdTag.Id, 8005, currentTime, 1100);
        addIndicatorResult.Should().BeTrue("Indicator should be added successfully");

        var removeIndicatorResult = await _repository.RemoveIndicatorFromTagAsync(createdTag.Id, 8005, currentTime, 1100);
        removeIndicatorResult.Should().BeTrue("Indicator should be removed successfully");

        // Test tag-to-tag operations
        var childTag = new Tag
        {
            TagNumber = 80002,
            TagType = TagType.BundleTag,
            TagTypeKeyId = 1,
            LocationKeyId = 1100,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };
        var createdChildTag = await _repository.AddAsync(childTag);

        var addTagResult = await _repository.AddTagToTagAsync(createdTag.Id, createdChildTag.Id, currentTime, 1100);
        addTagResult.Should().BeTrue("Child tag should be added successfully");

        var removeTagResult = await _repository.RemoveTagFromTagAsync(createdTag.Id, createdChildTag.Id, currentTime, 1100);
        removeTagResult.Should().BeTrue("Child tag should be removed successfully");
    }

    /// <summary>
    /// MD-REPO-025: Repository must handle batch operations correctly
    /// Critical for bulk tag operations
    /// </summary>
    [Fact(DisplayName = "MD-REPO-025: Repository Must Handle Batch Operations")]
    public async Task Repository_Should_Handle_Batch_Operations()
    {
        // Arrange - Create tags with content
        var sourceTag = new Tag
        {
            TagNumber = 90001,
            TagType = TagType.PrepTag,
            TagTypeKeyId = 0,
            LocationKeyId = 1200,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var transportTag = new Tag
        {
            TagNumber = 90002,
            TagType = TagType.TransportTag,
            TagTypeKeyId = 6,
            LocationKeyId = 1200,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var createdSourceTag = await _repository.AddAsync(sourceTag);
        var createdTransportTag = await _repository.AddAsync(transportTag);
        var currentTime = DateTime.UtcNow;

        // Add content to source tag
        await _repository.AddUnitToTagAsync(createdSourceTag.Id, 9001, currentTime, 1200, false);
        await _repository.AddUnitToTagAsync(createdSourceTag.Id, 9002, currentTime, 1200, false);

        // Act & Assert - Batch operations
        var moveResult = await _repository.MoveTagContentToTransportTagAsync(
            createdSourceTag.Id, createdTransportTag.Id, currentTime, 1200);
        moveResult.Should().BeTrue("Content should be moved successfully");

        // Verify content was moved
        var sourceTagAfterMove = await _repository.GetByIdAsync(createdSourceTag.Id);
        var transportTagAfterMove = await _repository.GetByIdAsync(createdTransportTag.Id);

        sourceTagAfterMove!.IsEmpty.Should().BeTrue("Source tag should be empty after move");
        transportTagAfterMove!.Contents.Units.Should().HaveCount(2, "Transport tag should have moved units");

        // Test dissolve operation
        var dissolveResult = await _repository.DissolveTagAsync(createdTransportTag.Id, currentTime, 1200);
        dissolveResult.Should().BeTrue("Tag should be dissolved successfully");

        var dissolvedTag = await _repository.GetByIdAsync(createdTransportTag.Id);
        dissolvedTag!.IsEmpty.Should().BeTrue("Dissolved tag should be empty");

        // Test clear contents (which should call dissolve)
        await _repository.AddUnitToTagAsync(createdTransportTag.Id, 9003, currentTime, 1200, false);
        var clearResult = await _repository.ClearTagContentsAsync(createdTransportTag.Id, currentTime, 1200);
        clearResult.Should().BeTrue("Tag contents should be cleared successfully");
    }

    /// <summary>
    /// MD-REPO-026: Repository must handle auto tag management correctly
    /// Critical for auto tag lifecycle
    /// </summary>
    [Fact(DisplayName = "MD-REPO-026: Repository Must Handle Auto Tag Management")]
    public async Task Repository_Should_Handle_Auto_Tag_Management()
    {
        // Act & Assert - Auto tag operations
        var reservedTagId = await _repository.ReserveAutoTagAsync(TagType.PrepTag, 1300);
        reservedTagId.Should().BeGreaterThan(0, "Reserved tag should have valid ID");

        var reservedTag = await _repository.GetByIdAsync(reservedTagId);
        reservedTag.Should().NotBeNull("Reserved tag should be retrievable");
        reservedTag!.IsAuto.Should().BeTrue("Reserved tag should be marked as auto");

        var releaseResult = await _repository.ReleaseAutoTagReservationAsync(reservedTagId);
        releaseResult.Should().BeTrue("Auto tag reservation should be released successfully");

        var releasedTag = await _repository.GetByIdAsync(reservedTagId);
        releasedTag!.IsAuto.Should().BeFalse("Released tag should no longer be auto");

        // Test getting reserved auto tags
        await _repository.ReserveAutoTagAsync(TagType.BundleTag, 1300);
        await _repository.ReserveAutoTagAsync(TagType.PrepTag, 1300);
        
        var reservedTags = await _repository.GetReservedAutoTagsAsync();
        reservedTags.Should().HaveCountGreaterOrEqualTo(2, "Should have reserved auto tags");
    }

    /// <summary>
    /// MD-REPO-027: Repository must handle error scenarios gracefully
    /// Critical for system reliability
    /// </summary>
    [Fact(DisplayName = "MD-REPO-027: Repository Must Handle Error Scenarios Gracefully")]
    public async Task Repository_Should_Handle_Error_Scenarios_Gracefully()
    {
        // Act & Assert - Non-existent operations
        var nonExistentTag = await _repository.GetByIdAsync(999999);
        nonExistentTag.Should().BeNull("Non-existent tag should return null");

        var nonExistentByNumber = await _repository.GetByNumberAndTypeAsync(999999, TagType.PrepTag);
        nonExistentByNumber.Should().BeNull("Non-existent tag by number should return null");

        var deleteNonExistent = await _repository.DeleteAsync(999999);
        deleteNonExistent.Should().BeFalse("Delete non-existent should return false");

        var removeNonExistentUnit = await _repository.RemoveUnitFromTagAsync(999999, 999999, DateTime.UtcNow, 1400);
        removeNonExistentUnit.Should().BeFalse("Remove non-existent unit should return false");

        var removeNonExistentItem = await _repository.RemoveItemFromTagAsync(999999, new TagItem(999, 999, 999, 1), DateTime.UtcNow, 1400);
        removeNonExistentItem.Should().BeFalse("Remove non-existent item should return false");

        var removeNonExistentTagFromTag = await _repository.RemoveTagFromTagAsync(999999, 999998, DateTime.UtcNow, 1400);
        removeNonExistentTagFromTag.Should().BeFalse("Remove non-existent tag from tag should return false");

        var removeNonExistentIndicator = await _repository.RemoveIndicatorFromTagAsync(999999, 999999, DateTime.UtcNow, 1400);
        removeNonExistentIndicator.Should().BeFalse("Remove non-existent indicator should return false");

        var releaseNonExistentReservation = await _repository.ReleaseAutoTagReservationAsync(999999);
        releaseNonExistentReservation.Should().BeFalse("Release non-existent reservation should return false");
    }

    /// <summary>
    /// MD-REPO-028: Repository must handle stub methods correctly
    /// Critical for incomplete implementations
    /// </summary>
    [Fact(DisplayName = "MD-REPO-028: Repository Must Handle Stub Methods Correctly")]
    public async Task Repository_Should_Handle_Stub_Methods_Correctly()
    {
        // Act & Assert - Stub implementations
        var tagsWithReservations = await _repository.GetTagsWithReservationsAsync();
        tagsWithReservations.Should().NotBeNull().And.BeEmpty("Stub method should return empty collection");

        var linkedSplitTags = await _repository.GetLinkedSplitTagsAsync(1);
        linkedSplitTags.Should().NotBeNull().And.BeEmpty("Stub method should return empty collection");

        var splitUnitSerial = await _repository.GetSplitUnitSerialNumberSplitTagAsync(1);
        splitUnitSerial.Should().BeNull("Stub method should return null");
    }

    /// <summary>
    /// MD-REPO-029: Repository must handle GetAutoTagsAsync correctly
    /// Critical for auto tag management
    /// </summary>
    [Fact(DisplayName = "MD-REPO-029: Repository Must Handle GetAutoTagsAsync")]
    public async Task Repository_Should_Handle_GetAutoTagsAsync()
    {
        // Arrange - Create auto and regular tags
        var autoTag = new Tag
        {
            TagNumber = 95001,
            TagType = TagType.PrepTag,
            TagTypeKeyId = 0,
            IsAuto = true,
            LocationKeyId = 1500,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        var regularTag = new Tag
        {
            TagNumber = 95002,
            TagType = TagType.BundleTag,
            TagTypeKeyId = 1,
            IsAuto = false,
            LocationKeyId = 1500,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestSystem"
        };

        await _repository.AddAsync(autoTag);
        await _repository.AddAsync(regularTag);

        // Act
        var autoTags = await _repository.GetAutoTagsAsync();

        // Assert
        autoTags.Should().HaveCountGreaterOrEqualTo(1, "Should find auto tags");
        autoTags.Should().AllSatisfy(tag => tag.IsAuto.Should().BeTrue("All returned tags should be auto tags"));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
