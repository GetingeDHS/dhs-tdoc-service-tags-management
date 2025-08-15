using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TagManagement.Core.Entities;
using TagManagement.Core.Enums;
using TagManagement.Infrastructure.Persistence;
using TagManagement.Infrastructure.Persistence.Models;
using TagManagement.Infrastructure.Persistence.Repositories;

namespace TagManagement.UnitTests.Infrastructure;

/// <summary>
/// Medical Device Compliance Tests for TagRepository
/// Tests primary tag repository for regulatory compliance
/// Addresses Issue #7: TagRepository has 0% coverage
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Infrastructure")]
[Trait("Component", "TagRepository")]
public class TagRepositoryTests : IDisposable
{
    private readonly TagManagementDbContext _context;
    private readonly TagRepository _repository;
    private readonly Mock<ILogger<TagRepository>> _loggerMock;

    public TagRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TagManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TagManagementDbContext(options);
        _loggerMock = new Mock<ILogger<TagRepository>>();
        _repository = new TagRepository(_context, _loggerMock.Object);
    }

    #region Constructor Tests

    /// <summary>
    /// MD-REPO-001: Repository constructor must validate dependencies
    /// Critical for medical device dependency injection
    /// </summary>
    [Fact(DisplayName = "MD-REPO-001: Constructor Must Validate Dependencies")]
    public void Constructor_Should_Validate_Dependencies()
    {
        // Act & Assert - Null context should throw
        var act = () => new TagRepository(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");

        // Act & Assert - Null logger should throw
        var act2 = () => new TagRepository(_context, null!);
        act2.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region GetByIdAsync Tests

    /// <summary>
    /// MD-REPO-002: GetByIdAsync must return tag with navigation properties
    /// Critical for complete tag data retrieval
    /// </summary>
    [Fact(DisplayName = "MD-REPO-002: GetByIdAsync Must Return Tag With Navigation Properties")]
    public async Task GetByIdAsync_Should_Return_Tag_With_Navigation_Properties()
    {
        // Arrange
        var testData = await CreateTestDataAsync();
        
        // Act
        var result = await _repository.GetByIdAsync(testData.TagId);

        // Assert
        result.Should().NotBeNull("Tag should be found");
        result!.Id.Should().Be(testData.TagId);
        result.TagNumber.Should().Be(12345);
        result.TagType.Should().Be(TagType.PrepTag);
        result.LocationKeyId.Should().Be(100);
        result.IsAuto.Should().BeFalse();
    }

    /// <summary>
    /// MD-REPO-003: GetByIdAsync must return null for non-existent tag
    /// </summary>
    [Fact(DisplayName = "MD-REPO-003: GetByIdAsync Must Return Null For Non-Existent Tag")]
    public async Task GetByIdAsync_Should_Return_Null_For_NonExistent_Tag()
    {
        // Act
        var result = await _repository.GetByIdAsync(999999);

        // Assert
        result.Should().BeNull("Non-existent tag should return null");
    }

    /// <summary>
    /// MD-REPO-004: GetByIdAsync must handle exceptions and log errors
    /// </summary>
    [Fact(DisplayName = "MD-REPO-004: GetByIdAsync Must Handle Exceptions")]
    public async Task GetByIdAsync_Should_Handle_Exceptions_And_Log()
    {
        // Arrange
        await _context.DisposeAsync();
        
        // Act & Assert
        var act = async () => await _repository.GetByIdAsync(1);
        await act.Should().ThrowAsync<Exception>("Disposed context should throw");

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting tag with ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Error should be logged");
    }

    #endregion

    #region GetByNumberAndTypeAsync Tests

    /// <summary>
    /// MD-REPO-005: GetByNumberAndTypeAsync must find tag by number and type
    /// </summary>
    [Fact(DisplayName = "MD-REPO-005: GetByNumberAndTypeAsync Must Find Tag By Number And Type")]
    public async Task GetByNumberAndTypeAsync_Should_Find_Tag_By_Number_And_Type()
    {
        // Arrange
        var testData = await CreateTestDataAsync();
        
        // Act
        var result = await _repository.GetByNumberAndTypeAsync(12345, TagType.PrepTag);

        // Assert
        result.Should().NotBeNull("Tag should be found by number and type");
        result!.TagNumber.Should().Be(12345);
        result.TagType.Should().Be(TagType.PrepTag);
    }

    /// <summary>
    /// MD-REPO-006: GetByNumberAndTypeAsync must return null for non-matching criteria
    /// </summary>
    [Fact(DisplayName = "MD-REPO-006: GetByNumberAndTypeAsync Must Return Null For Non-Matching")]
    public async Task GetByNumberAndTypeAsync_Should_Return_Null_For_NonMatching()
    {
        // Arrange
        await CreateTestDataAsync();
        
        // Act
        var result = await _repository.GetByNumberAndTypeAsync(12345, TagType.BundleTag);

        // Assert
        result.Should().BeNull("Non-matching type should return null");
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// MD-REPO-007: GetAllAsync must return all tags with navigation properties
    /// </summary>
    [Fact(DisplayName = "MD-REPO-007: GetAllAsync Must Return All Tags")]
    public async Task GetAllAsync_Should_Return_All_Tags()
    {
        // Arrange
        await CreateTestDataAsync();
        await CreateSecondTagAsync();
        
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2, "Should return both test tags");
        result.All(t => t.TagNumber > 0).Should().BeTrue("All tags should have valid tag numbers");
    }

    /// <summary>
    /// MD-REPO-008: GetAllAsync must return empty collection when no tags exist
    /// </summary>
    [Fact(DisplayName = "MD-REPO-008: GetAllAsync Must Return Empty Collection")]
    public async Task GetAllAsync_Should_Return_Empty_Collection_When_No_Tags()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty("No tags should return empty collection");
    }

    #endregion

    #region GetPagedAsync Tests

    /// <summary>
    /// MD-REPO-009: GetPagedAsync must return correct page of tags
    /// </summary>
    [Fact(DisplayName = "MD-REPO-009: GetPagedAsync Must Return Correct Page")]
    public async Task GetPagedAsync_Should_Return_Correct_Page()
    {
        // Arrange
        await CreateTestDataAsync();
        await CreateSecondTagAsync();
        await CreateThirdTagAsync();
        
        // Act
        var result = await _repository.GetPagedAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2, "Should return page size number of tags");
    }

    /// <summary>
    /// MD-REPO-010: GetPagedAsync must handle pagination correctly
    /// </summary>
    [Fact(DisplayName = "MD-REPO-010: GetPagedAsync Must Handle Pagination")]
    public async Task GetPagedAsync_Should_Handle_Pagination()
    {
        // Arrange
        await CreateTestDataAsync();
        await CreateSecondTagAsync();
        
        // Act
        var page1 = await _repository.GetPagedAsync(1, 1);
        var page2 = await _repository.GetPagedAsync(2, 1);

        // Assert
        page1.Should().HaveCount(1, "First page should have 1 tag");
        page2.Should().HaveCount(1, "Second page should have 1 tag");
        page1.First().Id.Should().NotBe(page2.First().Id, "Pages should contain different tags");
    }

    #endregion

    #region AddAsync Tests

    /// <summary>
    /// MD-REPO-011: AddAsync must create new tag and return with ID
    /// </summary>
    [Fact(DisplayName = "MD-REPO-011: AddAsync Must Create New Tag")]
    public async Task AddAsync_Should_Create_New_Tag()
    {
        // Arrange
        await CreateLocationAsync();
        await CreateTagTypeAsync();
        
        var newTag = new Tag
        {
            TagNumber = 55555,
            TagType = TagType.PrepTag,
            IsAuto = true,
            LocationKeyId = 100
        };

        // Act
        var result = await _repository.AddAsync(newTag);

        // Assert
        result.Should().NotBeNull("Created tag should be returned");
        result.Id.Should().BeGreaterThan(0, "Created tag should have ID");
        result.TagNumber.Should().Be(55555);
        result.TagType.Should().Be(TagType.PrepTag);
        result.IsAuto.Should().BeTrue();
    }

    /// <summary>
    /// MD-REPO-012: AddAsync must set creation audit fields
    /// </summary>
    [Fact(DisplayName = "MD-REPO-012: AddAsync Must Set Creation Audit Fields")]
    public async Task AddAsync_Should_Set_Creation_Audit_Fields()
    {
        // Arrange
        await CreateLocationAsync();
        await CreateTagTypeAsync();
        
        var newTag = new Tag
        {
            TagNumber = 66666,
            TagType = TagType.PrepTag,
            LocationKeyId = 100
        };

        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _repository.AddAsync(newTag);

        // Assert
        result.CreatedAt.Should().BeAfter(beforeCreate.AddSeconds(-1));
        result.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// MD-REPO-013: UpdateAsync must update existing tag
    /// </summary>
    [Fact(DisplayName = "MD-REPO-013: UpdateAsync Must Update Existing Tag")]
    public async Task UpdateAsync_Should_Update_Existing_Tag()
    {
        // Arrange
        var testData = await CreateTestDataAsync();
        var existingTag = await _repository.GetByIdAsync(testData.TagId);
        existingTag!.TagNumber = 99999;
        existingTag.IsAuto = true;

        // Act
        var result = await _repository.UpdateAsync(existingTag);

        // Assert
        result.Should().NotBeNull();
        result.TagNumber.Should().Be(99999, "Tag number should be updated");
        result.IsAuto.Should().BeTrue("Auto flag should be updated");
    }

    /// <summary>
    /// MD-REPO-014: UpdateAsync must throw for non-existent tag
    /// </summary>
    [Fact(DisplayName = "MD-REPO-014: UpdateAsync Must Throw For Non-Existent Tag")]
    public async Task UpdateAsync_Should_Throw_For_NonExistent_Tag()
    {
        // Arrange
        var nonExistentTag = new Tag
        {
            Id = 999999,
            TagNumber = 99999,
            TagType = TagType.PrepTag,
            LocationKeyId = 100
        };

        // Act & Assert
        var act = async () => await _repository.UpdateAsync(nonExistentTag);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999999*");
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// MD-REPO-015: DeleteAsync must delete existing tag
    /// </summary>
    [Fact(DisplayName = "MD-REPO-015: DeleteAsync Must Delete Existing Tag")]
    public async Task DeleteAsync_Should_Delete_Existing_Tag()
    {
        // Arrange
        var testData = await CreateTestDataAsync();

        // Act
        var result = await _repository.DeleteAsync(testData.TagId);

        // Assert
        result.Should().BeTrue("Delete should succeed");
        var deletedTag = await _repository.GetByIdAsync(testData.TagId);
        deletedTag.Should().BeNull("Deleted tag should not be found");
    }

    /// <summary>
    /// MD-REPO-016: DeleteAsync must return false for non-existent tag
    /// </summary>
    [Fact(DisplayName = "MD-REPO-016: DeleteAsync Must Return False For Non-Existent")]
    public async Task DeleteAsync_Should_Return_False_For_NonExistent_Tag()
    {
        // Act
        var result = await _repository.DeleteAsync(999999);

        // Assert
        result.Should().BeFalse("Delete of non-existent tag should return false");
    }

    #endregion

    #region GetTagsByTypeAsync Tests

    /// <summary>
    /// MD-REPO-017: GetTagsByTypeAsync must return tags of specified type
    /// </summary>
    [Fact(DisplayName = "MD-REPO-017: GetTagsByTypeAsync Must Return Tags Of Type")]
    public async Task GetTagsByTypeAsync_Should_Return_Tags_Of_Type()
    {
        // Arrange
        await CreateTestDataAsync(); // PrepTag
        await CreateSecondTagAsync(); // BundleTag

        // Act
        var prepTags = await _repository.GetTagsByTypeAsync(TagType.PrepTag);
        var bundleTags = await _repository.GetTagsByTypeAsync(TagType.BundleTag);

        // Assert
        prepTags.Should().HaveCount(1, "Should find 1 PrepTag");
        prepTags.First().TagType.Should().Be(TagType.PrepTag);
        
        bundleTags.Should().HaveCount(1, "Should find 1 BundleTag");
        bundleTags.First().TagType.Should().Be(TagType.BundleTag);
    }

    #endregion

    #region GetTagsByLocationAsync Tests

    /// <summary>
    /// MD-REPO-018: GetTagsByLocationAsync must return tags at location
    /// </summary>
    [Fact(DisplayName = "MD-REPO-018: GetTagsByLocationAsync Must Return Tags At Location")]
    public async Task GetTagsByLocationAsync_Should_Return_Tags_At_Location()
    {
        // Arrange
        await CreateTestDataAsync(); // LocationKeyId = 100
        await CreateThirdTagAsync(); // LocationKeyId = 300

        // Act
        var location100Tags = await _repository.GetTagsByLocationAsync(100);
        var location300Tags = await _repository.GetTagsByLocationAsync(300);

        // Assert
        location100Tags.Should().HaveCount(1, "Should find 1 tag at location 100");
        location100Tags.First().LocationKeyId.Should().Be(100);
        
        location300Tags.Should().HaveCount(1, "Should find 1 tag at location 300");
        location300Tags.First().LocationKeyId.Should().Be(300);
    }

    #endregion

    #region GetAutoTagsAsync Tests

    /// <summary>
    /// MD-REPO-019: GetAutoTagsAsync must return only auto tags
    /// </summary>
    [Fact(DisplayName = "MD-REPO-019: GetAutoTagsAsync Must Return Only Auto Tags")]
    public async Task GetAutoTagsAsync_Should_Return_Only_Auto_Tags()
    {
        // Arrange
        await CreateTestDataAsync(); // IsAuto = false
        await CreateAutoTagAsync(); // IsAuto = true

        // Act
        var autoTags = await _repository.GetAutoTagsAsync();

        // Assert
        autoTags.Should().HaveCount(1, "Should find 1 auto tag");
        autoTags.First().IsAuto.Should().BeTrue("Auto tag should have IsAuto = true");
    }

    #endregion

    #region Stub Methods Coverage Tests

    /// <summary>
    /// MD-REPO-020: Stub methods must return expected default values
    /// </summary>
    [Fact(DisplayName = "MD-REPO-020: Stub Methods Must Return Default Values")]
    public async Task Stub_Methods_Should_Return_Expected_Defaults()
    {
        // Test GetTagsWithReservationsAsync
        var reservationTags = await _repository.GetTagsWithReservationsAsync();
        reservationTags.Should().BeEmpty("Stub should return empty collection");

        // Test GetEmptyAutoTagAsync  
        var emptyAutoTag = await _repository.GetEmptyAutoTagAsync(TagType.PrepTag, 100);
        emptyAutoTag.Should().BeNull("Stub should return null");

        // Test other stub methods
        var tagsContainingUnit = await _repository.GetTagsContainingUnitAsync(1);
        tagsContainingUnit.Should().BeEmpty();

        var tagsContainingItem = await _repository.GetTagsContainingItemAsync(1, 2);
        tagsContainingItem.Should().BeEmpty();

        var isUnitInTag = await _repository.IsUnitInAnyTagAsync(1);
        isUnitInTag.Should().BeFalse();

        var isItemInTag = await _repository.IsItemInAnyTagAsync(1, 2);
        isItemInTag.Should().BeFalse();

        var contentCount = await _repository.GetTagContentCountAsync(1);
        contentCount.Should().Be(0);

        var isEmpty = await _repository.IsTagEmptyAsync(1);
        isEmpty.Should().BeTrue();

        var childTags = await _repository.GetChildTagsAsync(1);
        childTags.Should().BeEmpty();

        var parentTag = await _repository.GetParentTagAsync(1);
        parentTag.Should().BeNull();

        var rootTags = await _repository.GetRootTagsAsync();
        rootTags.Should().BeEmpty();

        var rootTagId = await _repository.GetRootTagIdAsync(1);
        rootTagId.Should().Be(1);

        var linkedSplitTags = await _repository.GetLinkedSplitTagsAsync(1);
        linkedSplitTags.Should().BeEmpty();

        var splitUnitSerial = await _repository.GetSplitUnitSerialNumberSplitTagAsync(1);
        splitUnitSerial.Should().BeNull();

        var addUnitResult = await _repository.AddUnitToTagAsync(1, 1, DateTime.UtcNow, 100);
        addUnitResult.Should().BeTrue();

        var removeUnitResult = await _repository.RemoveUnitFromTagAsync(1, 1, DateTime.UtcNow, 100);
        removeUnitResult.Should().BeTrue();

        var tagItem = new TagItem(1, 1, 1, 1);
        var addItemResult = await _repository.AddItemToTagAsync(1, tagItem, DateTime.UtcNow, 100);
        addItemResult.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private async Task<(int TagId, int TagTypeId, int LocationId)> CreateTestDataAsync()
    {
        var tagType = await CreateTagTypeAsync();
        var location = await CreateLocationAsync();

        var tag = new TagsModel
        {
            TagNumber = 12345,
            TagTypeKeyId = 0, // PrepTag
            IsAutoTag = false,
            LocationKeyId = 100,
            CreatedTime = DateTime.UtcNow,
            CreatedByUserKeyId = 1
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return (tag.TagKeyId, tagType.TagTypeKeyId, location.LocationKeyId);
    }

    private async Task<TagsModel> CreateSecondTagAsync()
    {
        // Check if BundleTagType already exists to avoid tracking conflicts
        if (!await _context.TagTypes.AnyAsync(t => t.TagTypeKeyId == 1))
        {
            await CreateBundleTagTypeAsync();
        }
        
        var tag = new TagsModel
        {
            TagNumber = 54321,
            TagTypeKeyId = 1, // BundleTag
            IsAutoTag = false,
            LocationKeyId = 100,
            CreatedTime = DateTime.UtcNow,
            CreatedByUserKeyId = 1
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    private async Task<TagsModel> CreateThirdTagAsync()
    {
        // Ensure Location 300 exists
        if (!await _context.Locations.AnyAsync(l => l.LocationKeyId == 300))
        {
            var location = new LocationModel
            {
                LocationKeyId = 300,
                LocationName = "Test Location 300",
                LocationCode = "TL03",
                IsActive = true
            };
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
        }

        var tag = new TagsModel
        {
            TagNumber = 77777,
            TagTypeKeyId = 0, // PrepTag
            IsAutoTag = false,
            LocationKeyId = 300,
            CreatedTime = DateTime.UtcNow,
            CreatedByUserKeyId = 1
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    private async Task<TagsModel> CreateAutoTagAsync()
    {
        var tag = new TagsModel
        {
            TagNumber = 88888,
            TagTypeKeyId = 0, // PrepTag
            IsAutoTag = true,
            LocationKeyId = 100,
            CreatedTime = DateTime.UtcNow,
            CreatedByUserKeyId = 1
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    private async Task<TagTypeModel> CreateTagTypeAsync()
    {
        var tagType = new TagTypeModel
        {
            TagTypeKeyId = 0,
            TagTypeName = "PrepTag",
            TagTypeCode = "PREP",
            IsActive = true
        };

        _context.TagTypes.Add(tagType);
        await _context.SaveChangesAsync();
        return tagType;
    }

    private async Task<TagTypeModel> CreateBundleTagTypeAsync()
    {
        var tagType = new TagTypeModel
        {
            TagTypeKeyId = 1,
            TagTypeName = "BundleTag", 
            TagTypeCode = "BUNDLE",
            IsActive = true
        };

        _context.TagTypes.Add(tagType);
        await _context.SaveChangesAsync();
        return tagType;
    }

    private async Task<LocationModel> CreateLocationAsync()
    {
        var location = new LocationModel
        {
            LocationKeyId = 100,
            LocationName = "Test Location",
            LocationCode = "TL01",
            IsActive = true
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
