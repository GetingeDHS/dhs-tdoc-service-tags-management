using TagManagement.Core.Entities;

namespace TagManagement.UnitTests.Core.Entities;

/// <summary>
/// Medical Device Compliance Tests for TagContents Entity
/// Tests container functionality for tag content management
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Core")]
[Trait("Component", "TagContents")]
public class TagContentsTests
{
    /// <summary>
    /// MD-TAGCONT-001: TagContents must initialize with empty collections
    /// Critical for proper container state initialization
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-001: TagContents Must Initialize With Empty Collections")]
    public void Constructor_Should_Initialize_Empty_Collections()
    {
        // Act
        var tagContents = new TagContents();

        // Assert
        tagContents.Tags.Should().NotBeNull().And.BeEmpty();
        tagContents.Units.Should().NotBeNull().And.BeEmpty();
        tagContents.Items.Should().NotBeNull().And.BeEmpty();
        tagContents.Indicators.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// MD-TAGCONT-002: Count properties must return correct values
    /// Critical for content counting accuracy
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-002: Count Properties Must Return Correct Values")]
    public void Count_Properties_Should_Return_Correct_Values()
    {
        // Arrange
        var tagContents = new TagContents();
        var childTag = new Tag { Id = 1, TagNumber = 100 };
        var tagItem = new TagItem(1, 2, 3, 5);

        // Add content
        tagContents.Tags.Add(childTag);
        tagContents.Units.Add(10);
        tagContents.Units.Add(20);
        tagContents.Items.Add(tagItem);
        tagContents.Indicators.Add(1);
        tagContents.Indicators.Add(2);
        tagContents.Indicators.Add(3);

        // Assert
        tagContents.TagCount.Should().Be(1);
        tagContents.UnitCount.Should().Be(2);
        tagContents.ItemCount.Should().Be(1);
        tagContents.IndicatorCount.Should().Be(3);
    }

    /// <summary>
    /// MD-TAGCONT-003: IsEmpty must correctly identify empty containers
    /// Critical for tag state validation
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-003: IsEmpty Must Correctly Identify Empty Containers")]
    public void IsEmpty_Should_Return_True_When_All_Collections_Empty()
    {
        // Arrange
        var tagContents = new TagContents();

        // Assert
        tagContents.IsEmpty.Should().BeTrue("New TagContents should be empty");
    }

    /// <summary>
    /// MD-TAGCONT-004: IsEmpty must return false when any content exists
    /// Critical for proper content detection
    /// </summary>
    [Theory(DisplayName = "MD-TAGCONT-004: IsEmpty Must Return False When Content Exists")]
    [InlineData("tag")]
    [InlineData("unit")]
    [InlineData("item")]
    [InlineData("indicator")]
    public void IsEmpty_Should_Return_False_When_Content_Exists(string contentType)
    {
        // Arrange
        var tagContents = new TagContents();

        switch (contentType)
        {
            case "tag":
                tagContents.Tags.Add(new Tag { Id = 1 });
                break;
            case "unit":
                tagContents.Units.Add(1);
                break;
            case "item":
                tagContents.Items.Add(new TagItem(1, 2, 3, 1));
                break;
            case "indicator":
                tagContents.Indicators.Add(1);
                break;
        }

        // Assert
        tagContents.IsEmpty.Should().BeFalse($"TagContents with {contentType} should not be empty");
    }

    /// <summary>
    /// MD-TAGCONT-005: ClearContents must remove all content
    /// Critical for tag cleanup operations
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-005: ClearContents Must Remove All Content")]
    public void ClearContents_Should_Remove_All_Content()
    {
        // Arrange
        var tagContents = new TagContents();
        tagContents.Tags.Add(new Tag { Id = 1 });
        tagContents.Units.Add(10);
        tagContents.Units.Add(20);
        tagContents.Items.Add(new TagItem(1, 2, 3, 5));
        tagContents.Indicators.Add(1);

        // Verify content exists
        tagContents.IsEmpty.Should().BeFalse();

        // Act
        tagContents.ClearContents();

        // Assert
        tagContents.Tags.Should().BeEmpty();
        tagContents.Units.Should().BeEmpty();
        tagContents.Items.Should().BeEmpty();
        tagContents.Indicators.Should().BeEmpty();
        tagContents.IsEmpty.Should().BeTrue();
    }

    /// <summary>
    /// MD-TAGCONT-006: RemoveUnit must remove specified unit
    /// Critical for unit management operations
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-006: RemoveUnit Must Remove Specified Unit")]
    public void RemoveUnit_Should_Remove_Specified_Unit()
    {
        // Arrange
        var tagContents = new TagContents();
        tagContents.Units.AddRange(new[] { 10, 20, 30 });

        // Act
        tagContents.RemoveUnit(20);

        // Assert
        tagContents.Units.Should().HaveCount(2);
        tagContents.Units.Should().Contain(10);
        tagContents.Units.Should().Contain(30);
        tagContents.Units.Should().NotContain(20);
    }

    /// <summary>
    /// MD-TAGCONT-007: RemoveUnit must handle non-existent units gracefully
    /// Critical for robust error handling
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-007: RemoveUnit Must Handle Non-Existent Units")]
    public void RemoveUnit_Should_Handle_Non_Existent_Unit_Gracefully()
    {
        // Arrange
        var tagContents = new TagContents();
        tagContents.Units.AddRange(new[] { 10, 20, 30 });
        var originalCount = tagContents.UnitCount;

        // Act - Remove non-existent unit
        tagContents.RemoveUnit(999);

        // Assert - No change should occur
        tagContents.UnitCount.Should().Be(originalCount);
        tagContents.Units.Should().HaveCount(3);
        tagContents.Units.Should().Equal(10, 20, 30);
    }

    /// <summary>
    /// MD-TAGCONT-008: RemoveTag must remove specified tag
    /// Critical for nested tag management
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-008: RemoveTag Must Remove Specified Tag")]
    public void RemoveTag_Should_Remove_Specified_Tag()
    {
        // Arrange
        var tagContents = new TagContents();
        var tag1 = new Tag { Id = 1, TagNumber = 100 };
        var tag2 = new Tag { Id = 2, TagNumber = 200 };
        var tag3 = new Tag { Id = 3, TagNumber = 300 };
        
        tagContents.Tags.AddRange(new[] { tag1, tag2, tag3 });

        // Act
        tagContents.RemoveTag(tag2);

        // Assert
        tagContents.Tags.Should().HaveCount(2);
        tagContents.Tags.Should().Contain(tag1);
        tagContents.Tags.Should().Contain(tag3);
        tagContents.Tags.Should().NotContain(tag2);
    }

    /// <summary>
    /// MD-TAGCONT-009: RemoveTag must handle non-existent tags gracefully
    /// Critical for robust error handling
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-009: RemoveTag Must Handle Non-Existent Tags")]
    public void RemoveTag_Should_Handle_Non_Existent_Tag_Gracefully()
    {
        // Arrange
        var tagContents = new TagContents();
        var existingTag = new Tag { Id = 1, TagNumber = 100 };
        var nonExistentTag = new Tag { Id = 999, TagNumber = 999 };
        tagContents.Tags.Add(existingTag);
        var originalCount = tagContents.TagCount;

        // Act - Remove non-existent tag
        tagContents.RemoveTag(nonExistentTag);

        // Assert - No change should occur
        tagContents.TagCount.Should().Be(originalCount);
        tagContents.Tags.Should().HaveCount(1);
        tagContents.Tags.Should().Contain(existingTag);
    }

    /// <summary>
    /// MD-TAGCONT-010: RemoveItem must remove item with matching keys
    /// Critical for item management operations
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-010: RemoveItem Must Remove Item With Matching Keys")]
    public void RemoveItem_Should_Remove_Item_With_Matching_Keys()
    {
        // Arrange
        var tagContents = new TagContents();
        var item1 = new TagItem(1, 10, 100, 5);
        var item2 = new TagItem(2, 20, 200, 3);
        var item3 = new TagItem(3, 30, 300, 7);
        
        tagContents.Items.AddRange(new[] { item1, item2, item3 });

        // Act - Remove item2 by its keys
        tagContents.RemoveItem(2, 20, 200);

        // Assert
        tagContents.Items.Should().HaveCount(2);
        tagContents.Items.Should().Contain(item1);
        tagContents.Items.Should().Contain(item3);
        tagContents.Items.Should().NotContain(item2);
    }

    /// <summary>
    /// MD-TAGCONT-011: RemoveItem must handle non-existent items gracefully
    /// Critical for robust error handling
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-011: RemoveItem Must Handle Non-Existent Items")]
    public void RemoveItem_Should_Handle_Non_Existent_Item_Gracefully()
    {
        // Arrange
        var tagContents = new TagContents();
        var existingItem = new TagItem(1, 10, 100, 5);
        tagContents.Items.Add(existingItem);
        var originalCount = tagContents.ItemCount;

        // Act - Remove non-existent item
        tagContents.RemoveItem(999, 999, 999);

        // Assert - No change should occur
        tagContents.ItemCount.Should().Be(originalCount);
        tagContents.Items.Should().HaveCount(1);
        tagContents.Items.Should().Contain(existingItem);
    }

    /// <summary>
    /// MD-TAGCONT-012: RemoveItem must require all keys to match
    /// Critical for precise item identification
    /// </summary>
    [Theory(DisplayName = "MD-TAGCONT-012: RemoveItem Must Require All Keys To Match")]
    [InlineData(1, 999, 100)] // Wrong SerialKeyId
    [InlineData(999, 10, 100)] // Wrong ItemKeyId
    [InlineData(1, 10, 999)]   // Wrong LotInfoKeyId
    public void RemoveItem_Should_Require_All_Keys_To_Match(int itemKeyId, int serialKeyId, int lotInfoKeyId)
    {
        // Arrange
        var tagContents = new TagContents();
        var existingItem = new TagItem(1, 10, 100, 5);
        tagContents.Items.Add(existingItem);
        var originalCount = tagContents.ItemCount;

        // Act - Try to remove with partial matching keys
        tagContents.RemoveItem(itemKeyId, serialKeyId, lotInfoKeyId);

        // Assert - Item should not be removed
        tagContents.ItemCount.Should().Be(originalCount);
        tagContents.Items.Should().Contain(existingItem);
    }

    /// <summary>
    /// MD-TAGCONT-013: RemoveIndicator must remove specified indicator
    /// Critical for indicator management operations
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-013: RemoveIndicator Must Remove Specified Indicator")]
    public void RemoveIndicator_Should_Remove_Specified_Indicator()
    {
        // Arrange
        var tagContents = new TagContents();
        tagContents.Indicators.AddRange(new[] { 1, 2, 3, 4, 5 });

        // Act
        tagContents.RemoveIndicator(3);

        // Assert
        tagContents.Indicators.Should().HaveCount(4);
        tagContents.Indicators.Should().Contain(new[] { 1, 2, 4, 5 });
        tagContents.Indicators.Should().NotContain(3);
    }

    /// <summary>
    /// MD-TAGCONT-014: RemoveIndicator must handle non-existent indicators gracefully
    /// Critical for robust error handling
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-014: RemoveIndicator Must Handle Non-Existent Indicators")]
    public void RemoveIndicator_Should_Handle_Non_Existent_Indicator_Gracefully()
    {
        // Arrange
        var tagContents = new TagContents();
        tagContents.Indicators.AddRange(new[] { 1, 2, 3 });
        var originalCount = tagContents.IndicatorCount;

        // Act - Remove non-existent indicator
        tagContents.RemoveIndicator(999);

        // Assert - No change should occur
        tagContents.IndicatorCount.Should().Be(originalCount);
        tagContents.Indicators.Should().HaveCount(3);
        tagContents.Indicators.Should().Equal(1, 2, 3);
    }

    /// <summary>
    /// MD-TAGCONT-015: GetAllContainedUnits must return direct and nested units
    /// Critical for recursive unit collection
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-015: GetAllContainedUnits Must Return Direct And Nested Units")]
    public void GetAllContainedUnits_Should_Return_Direct_And_Nested_Units()
    {
        // Arrange
        var parentContents = new TagContents();
        parentContents.Units.AddRange(new[] { 10, 20 });

        // Create nested tag with units
        var nestedTag = new Tag { Id = 1, TagNumber = 100 };
        nestedTag.Contents.Units.AddRange(new[] { 30, 40 });
        parentContents.Tags.Add(nestedTag);

        // Create deeply nested tag
        var deepNestedTag = new Tag { Id = 2, TagNumber = 200 };
        deepNestedTag.Contents.Units.AddRange(new[] { 50 });
        nestedTag.Contents.Tags.Add(deepNestedTag);

        // Act
        var allUnits = parentContents.GetAllContainedUnits();

        // Assert
        allUnits.Should().HaveCount(5);
        allUnits.Should().Contain(new[] { 10, 20, 30, 40, 50 });
    }

    /// <summary>
    /// MD-TAGCONT-016: GetAllContainedUnits must handle empty nested tags
    /// Critical for robust recursive processing
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-016: GetAllContainedUnits Must Handle Empty Nested Tags")]
    public void GetAllContainedUnits_Should_Handle_Empty_Nested_Tags()
    {
        // Arrange
        var parentContents = new TagContents();
        parentContents.Units.AddRange(new[] { 10, 20 });

        // Add empty nested tag
        var emptyNestedTag = new Tag { Id = 1, TagNumber = 100 };
        parentContents.Tags.Add(emptyNestedTag);

        // Act
        var allUnits = parentContents.GetAllContainedUnits();

        // Assert
        allUnits.Should().HaveCount(2);
        allUnits.Should().Contain(new[] { 10, 20 });
    }

    /// <summary>
    /// MD-TAGCONT-017: GetAllContainedUnits must return empty list when no units
    /// Critical for boundary condition handling
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-017: GetAllContainedUnits Must Return Empty List When No Units")]
    public void GetAllContainedUnits_Should_Return_Empty_List_When_No_Units()
    {
        // Arrange - Contents with no units (but may have other content)
        var tagContents = new TagContents();
        tagContents.Items.Add(new TagItem(1, 2, 3, 1));
        tagContents.Indicators.Add(1);

        // Act
        var allUnits = tagContents.GetAllContainedUnits();

        // Assert
        allUnits.Should().NotBeNull();
        allUnits.Should().BeEmpty();
    }

    /// <summary>
    /// MD-TAGCONT-018: Count properties must remain consistent during operations
    /// Critical for data consistency validation
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-018: Count Properties Must Remain Consistent During Operations")]
    public void Count_Properties_Should_Remain_Consistent_During_Operations()
    {
        // Arrange
        var tagContents = new TagContents();

        // Add initial content
        tagContents.Units.AddRange(new[] { 1, 2, 3 });
        tagContents.Items.Add(new TagItem(1, 10, 100, 5));
        tagContents.Indicators.AddRange(new[] { 1, 2 });
        tagContents.Tags.Add(new Tag { Id = 1 });

        // Verify initial counts
        tagContents.UnitCount.Should().Be(3);
        tagContents.ItemCount.Should().Be(1);
        tagContents.IndicatorCount.Should().Be(2);
        tagContents.TagCount.Should().Be(1);

        // Act - Perform operations
        tagContents.RemoveUnit(2);
        tagContents.RemoveItem(1, 10, 100);
        tagContents.RemoveIndicator(1);

        // Assert - Counts should be updated
        tagContents.UnitCount.Should().Be(2);
        tagContents.ItemCount.Should().Be(0);
        tagContents.IndicatorCount.Should().Be(1);
        tagContents.TagCount.Should().Be(1);
        tagContents.IsEmpty.Should().BeFalse("Still has units, indicators, and tags");

        // Clear everything
        tagContents.ClearContents();
        
        // Final verification
        tagContents.UnitCount.Should().Be(0);
        tagContents.ItemCount.Should().Be(0);
        tagContents.IndicatorCount.Should().Be(0);
        tagContents.TagCount.Should().Be(0);
        tagContents.IsEmpty.Should().BeTrue();
    }

    /// <summary>
    /// MD-TAGCONT-019: Content collections must support multiple identical entries
    /// Critical for proper collection behavior
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-019: Content Collections Must Support Multiple Identical Entries")]
    public void Content_Collections_Should_Support_Multiple_Identical_Entries()
    {
        // Arrange
        var tagContents = new TagContents();

        // Act - Add duplicate units and indicators
        tagContents.Units.Add(10);
        tagContents.Units.Add(10); // Duplicate
        tagContents.Indicators.Add(1);
        tagContents.Indicators.Add(1); // Duplicate

        // Assert
        tagContents.UnitCount.Should().Be(2);
        tagContents.IndicatorCount.Should().Be(2);
        tagContents.Units.Should().Equal(10, 10);
        tagContents.Indicators.Should().Equal(1, 1);

        // Test removal behavior with duplicates
        tagContents.RemoveUnit(10); // Should only remove first occurrence
        tagContents.RemoveIndicator(1); // Should only remove first occurrence

        tagContents.UnitCount.Should().Be(1);
        tagContents.IndicatorCount.Should().Be(1);
    }

    /// <summary>
    /// MD-TAGCONT-020: Collections must be modifiable after initialization
    /// Critical for runtime content management
    /// </summary>
    [Fact(DisplayName = "MD-TAGCONT-020: Collections Must Be Modifiable After Initialization")]
    public void Collections_Should_Be_Modifiable_After_Initialization()
    {
        // Arrange
        var tagContents = new TagContents();

        // Act & Assert - Test all collection operations
        // Tags
        tagContents.Tags.Add(new Tag { Id = 1 });
        tagContents.Tags.Should().HaveCount(1);
        
        // Units
        tagContents.Units.Add(10);
        tagContents.Units.Should().HaveCount(1);
        
        // Items
        tagContents.Items.Add(new TagItem(1, 2, 3, 1));
        tagContents.Items.Should().HaveCount(1);
        
        // Indicators
        tagContents.Indicators.Add(1);
        tagContents.Indicators.Should().HaveCount(1);

        // Test direct collection manipulation
        tagContents.Units.Clear();
        tagContents.Units.Should().BeEmpty();
        
        tagContents.Units.AddRange(new[] { 1, 2, 3, 4, 5 });
        tagContents.Units.Should().HaveCount(5);
    }
}
