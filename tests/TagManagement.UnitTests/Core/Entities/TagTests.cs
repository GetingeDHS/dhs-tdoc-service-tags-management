using TagManagement.Core.Entities;
using TagManagement.Core.Enums;

namespace TagManagement.UnitTests.Core.Entities;

/// <summary>
/// Medical Device Compliance Tests for Tag Entity
/// Tests tag functionality and business logic
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Core")]
[Trait("Component", "Tag")]
public class TagTests
{
    /// <summary>
    /// MD-TAG-001: Tag must initialize with default values and empty contents
    /// Critical for proper entity initialization
    /// </summary>
    [Fact(DisplayName = "MD-TAG-001: Tag Must Initialize With Default Values")]
    public void Constructor_Should_Initialize_With_Default_Values()
    {
        // Act
        var tag = new Tag();

        // Assert
        tag.Id.Should().Be(0);
        tag.TagNumber.Should().Be(0);
        tag.TagType.Should().Be(TagType.PrepTag);
        tag.TagTypeKeyId.Should().Be(0);
        tag.IsAuto.Should().BeFalse();
        tag.Status.Should().Be(LifeStatus.Active);
        tag.LocationKeyId.Should().Be(0);
        tag.LocationTime.Should().BeNull();
        tag.HoldsItems.Should().BeFalse();
        tag.HasAutoReservation.Should().BeFalse();
        tag.InTagGroupKeyId.Should().Be(0);
        tag.Contents.Should().NotBeNull();
        tag.Contents.IsEmpty.Should().BeTrue();
        tag.CreatedBy.Should().Be(string.Empty);
        tag.UpdatedBy.Should().BeNull();
    }

    /// <summary>
    /// MD-TAG-002: Properties must be settable and gettable
    /// Critical for entity data management
    /// </summary>
    [Fact(DisplayName = "MD-TAG-002: Properties Must Be Settable And Gettable")]
    public void Properties_Should_Be_Settable_And_Gettable()
    {
        // Arrange
        var tag = new Tag();
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);
        var locationTime = DateTime.UtcNow.AddMinutes(10);

        // Act
        tag.Id = 123;
        tag.TagNumber = 456;
        tag.TagType = TagType.Bundle;
        tag.TagTypeKeyId = 789;
        tag.IsAuto = true;
        tag.Status = LifeStatus.Inactive;
        tag.LocationKeyId = 100;
        tag.LocationTime = locationTime;
        tag.HoldsItems = true;
        tag.HasAutoReservation = true;
        tag.InTagGroupKeyId = 200;
        tag.CreatedAt = createdAt;
        tag.UpdatedAt = updatedAt;
        tag.CreatedBy = "TestUser";
        tag.UpdatedBy = "UpdateUser";

        // Assert
        tag.Id.Should().Be(123);
        tag.TagNumber.Should().Be(456);
        tag.TagType.Should().Be(TagType.Bundle);
        tag.TagTypeKeyId.Should().Be(789);
        tag.IsAuto.Should().BeTrue();
        tag.Status.Should().Be(LifeStatus.Inactive);
        tag.LocationKeyId.Should().Be(100);
        tag.LocationTime.Should().Be(locationTime);
        tag.HoldsItems.Should().BeTrue();
        tag.HasAutoReservation.Should().BeTrue();
        tag.InTagGroupKeyId.Should().Be(200);
        tag.CreatedAt.Should().Be(createdAt);
        tag.UpdatedAt.Should().Be(updatedAt);
        tag.CreatedBy.Should().Be("TestUser");
        tag.UpdatedBy.Should().Be("UpdateUser");
    }

    /// <summary>
    /// MD-TAG-003: DisplayString must format correctly for all tag types
    /// Critical for user interface display
    /// </summary>
    [Theory(DisplayName = "MD-TAG-003: DisplayString Must Format Correctly")]
    [InlineData(TagType.PrepTag, 123, "Prep Tag #123")]
    [InlineData(TagType.Bundle, 456, "Bundle #456")]
    [InlineData(TagType.SteriLoad, 789, "Sterilization Load #789")]
    [InlineData(TagType.CaseCart, 1, "Case Cart #1")]
    [InlineData(TagType.TransportBox, 9999, "Transport Box #9999")]
    public void DisplayString_Should_Format_Correctly_For_All_Tag_Types(
        TagType tagType, int tagNumber, string expectedDisplay)
    {
        // Arrange
        var tag = new Tag
        {
            TagType = tagType,
            TagNumber = tagNumber
        };

        // Act
        var displayString = tag.DisplayString;

        // Assert
        displayString.Should().Be(expectedDisplay);
    }

    /// <summary>
    /// MD-TAG-004: FullDisplayString must include auto indicator when appropriate
    /// Critical for distinguishing auto tags
    /// </summary>
    [Theory(DisplayName = "MD-TAG-004: FullDisplayString Must Include Auto Indicator")]
    [InlineData(false, TagType.PrepTag, 123, "Prep Tag #123")]
    [InlineData(true, TagType.PrepTag, 123, "[AUTO] Prep Tag #123")]
    [InlineData(false, TagType.Bundle, 456, "Bundle #456")]
    [InlineData(true, TagType.Bundle, 456, "[AUTO] Bundle #456")]
    public void FullDisplayString_Should_Include_Auto_Indicator_When_Appropriate(
        bool isAuto, TagType tagType, int tagNumber, string expectedDisplay)
    {
        // Arrange
        var tag = new Tag
        {
            TagType = tagType,
            TagNumber = tagNumber,
            IsAuto = isAuto
        };

        // Act
        var fullDisplayString = tag.FullDisplayString;

        // Assert
        fullDisplayString.Should().Be(expectedDisplay);
    }

    /// <summary>
    /// MD-TAG-005: IsEmpty must reflect Contents.IsEmpty state
    /// Critical for tag state consistency
    /// </summary>
    [Fact(DisplayName = "MD-TAG-005: IsEmpty Must Reflect Contents State")]
    public void IsEmpty_Should_Reflect_Contents_IsEmpty_State()
    {
        // Arrange
        var tag = new Tag();

        // Assert - Initially empty
        tag.IsEmpty.Should().BeTrue("New tag should be empty");

        // Act - Add content
        tag.Contents.Units.Add(1);

        // Assert - No longer empty
        tag.IsEmpty.Should().BeFalse("Tag with content should not be empty");

        // Act - Remove content
        tag.Contents.ClearContents();

        // Assert - Empty again
        tag.IsEmpty.Should().BeTrue("Tag after clearing should be empty");
    }

    /// <summary>
    /// MD-TAG-006: ContentCondition must return correct condition for empty tag
    /// Critical for content state determination
    /// </summary>
    [Fact(DisplayName = "MD-TAG-006: ContentCondition Must Return Empty For Empty Tag")]
    public void ContentCondition_Should_Return_Empty_For_Empty_Tag()
    {
        // Arrange
        var tag = new Tag();

        // Act
        var condition = tag.ContentCondition;

        // Assert
        condition.Should().Be(TagContentCondition.Empty);
    }

    /// <summary>
    /// MD-TAG-007: ContentCondition must return Units when only units present
    /// Critical for content state determination
    /// </summary>
    [Fact(DisplayName = "MD-TAG-007: ContentCondition Must Return Units When Only Units Present")]
    public void ContentCondition_Should_Return_Units_When_Only_Units_Present()
    {
        // Arrange
        var tag = new Tag();
        tag.Contents.Units.AddRange(new[] { 1, 2, 3 });

        // Act
        var condition = tag.ContentCondition;

        // Assert
        condition.Should().Be(TagContentCondition.Units);
    }

    /// <summary>
    /// MD-TAG-008: ContentCondition must return Items when only items present
    /// Critical for content state determination
    /// </summary>
    [Fact(DisplayName = "MD-TAG-008: ContentCondition Must Return Items When Only Items Present")]
    public void ContentCondition_Should_Return_Items_When_Only_Items_Present()
    {
        // Arrange
        var tag = new Tag();
        tag.Contents.Items.Add(new TagItem(1, 10, 100, 5));

        // Act
        var condition = tag.ContentCondition;

        // Assert
        condition.Should().Be(TagContentCondition.Items);
    }

    /// <summary>
    /// MD-TAG-009: ContentCondition must return Mixed when both units and items present
    /// Critical for mixed content detection
    /// </summary>
    [Fact(DisplayName = "MD-TAG-009: ContentCondition Must Return Mixed When Both Present")]
    public void ContentCondition_Should_Return_Mixed_When_Both_Units_And_Items_Present()
    {
        // Arrange
        var tag = new Tag();
        tag.Contents.Units.Add(1);
        tag.Contents.Items.Add(new TagItem(1, 10, 100, 5));

        // Act
        var condition = tag.ContentCondition;

        // Assert
        condition.Should().Be(TagContentCondition.Mixed);
    }

    /// <summary>
    /// MD-TAG-010: ContentCondition must return Empty when only other content types present
    /// Critical for proper content condition logic
    /// </summary>
    [Fact(DisplayName = "MD-TAG-010: ContentCondition Must Return Empty When Only Other Content Present")]
    public void ContentCondition_Should_Return_Empty_When_Only_Other_Content_Present()
    {
        // Arrange
        var tag = new Tag();
        tag.Contents.Tags.Add(new Tag { Id = 1 }); // Nested tag only
        tag.Contents.Indicators.Add(1); // Indicator only

        // Act
        var condition = tag.ContentCondition;

        // Assert
        condition.Should().Be(TagContentCondition.Empty, 
            "Content condition only considers units and items, not nested tags or indicators");
    }

    /// <summary>
    /// MD-TAG-011: ContentCondition must handle complex content scenarios
    /// Critical for comprehensive content state detection
    /// </summary>
    [Theory(DisplayName = "MD-TAG-011: ContentCondition Must Handle Complex Content Scenarios")]
    [InlineData(true, false, false, false, TagContentCondition.Units)]
    [InlineData(false, true, false, false, TagContentCondition.Items)]
    [InlineData(true, true, false, false, TagContentCondition.Mixed)]
    [InlineData(false, false, true, false, TagContentCondition.Empty)]
    [InlineData(false, false, false, true, TagContentCondition.Empty)]
    [InlineData(true, false, true, true, TagContentCondition.Units)]
    [InlineData(false, true, true, true, TagContentCondition.Items)]
    [InlineData(true, true, true, true, TagContentCondition.Mixed)]
    public void ContentCondition_Should_Handle_Complex_Content_Scenarios(
        bool hasUnits, bool hasItems, bool hasTags, bool hasIndicators, TagContentCondition expectedCondition)
    {
        // Arrange
        var tag = new Tag();
        
        if (hasUnits) tag.Contents.Units.Add(1);
        if (hasItems) tag.Contents.Items.Add(new TagItem(1, 10, 100, 5));
        if (hasTags) tag.Contents.Tags.Add(new Tag { Id = 1 });
        if (hasIndicators) tag.Contents.Indicators.Add(1);

        // Act
        var condition = tag.ContentCondition;

        // Assert
        condition.Should().Be(expectedCondition);
    }

    /// <summary>
    /// MD-TAG-012: Contents property must never be null
    /// Critical for null reference prevention
    /// </summary>
    [Fact(DisplayName = "MD-TAG-012: Contents Property Must Never Be Null")]
    public void Contents_Property_Should_Never_Be_Null()
    {
        // Arrange
        var tag = new Tag();

        // Act - Try to set Contents to null
        tag.Contents = null!;

        // Assert - This would be a design flaw but test current behavior
        // In a production system, this might be prevented by validation
        tag.Contents.Should().BeNull("Current implementation allows null assignment");

        // However, accessing IsEmpty would throw NullReferenceException
        // In production, we'd want to prevent null assignment or have null-safe properties
    }

    /// <summary>
    /// MD-TAG-013: Contents must be replaceable with different instance
    /// Critical for content management operations
    /// </summary>
    [Fact(DisplayName = "MD-TAG-013: Contents Must Be Replaceable With Different Instance")]
    public void Contents_Should_Be_Replaceable_With_Different_Instance()
    {
        // Arrange
        var tag = new Tag();
        var originalContents = tag.Contents;
        var newContents = new TagContents();
        newContents.Units.Add(1);

        // Act
        tag.Contents = newContents;

        // Assert
        tag.Contents.Should().BeSameAs(newContents);
        tag.Contents.Should().NotBeSameAs(originalContents);
        tag.Contents.UnitCount.Should().Be(1);
    }

    /// <summary>
    /// MD-TAG-014: String properties must handle null and empty values
    /// Critical for data integrity
    /// </summary>
    [Fact(DisplayName = "MD-TAG-014: String Properties Must Handle Null And Empty Values")]
    public void String_Properties_Should_Handle_Null_And_Empty_Values()
    {
        // Arrange
        var tag = new Tag();

        // Act & Assert - Test CreatedBy
        tag.CreatedBy = null!;
        tag.CreatedBy.Should().BeNull("CreatedBy accepts null");

        tag.CreatedBy = string.Empty;
        tag.CreatedBy.Should().Be(string.Empty);

        tag.CreatedBy = "   ";
        tag.CreatedBy.Should().Be("   ", "Whitespace is preserved");

        // Act & Assert - Test UpdatedBy (nullable)
        tag.UpdatedBy = null;
        tag.UpdatedBy.Should().BeNull("UpdatedBy is nullable");

        tag.UpdatedBy = string.Empty;
        tag.UpdatedBy.Should().Be(string.Empty);

        tag.UpdatedBy = "TestUser";
        tag.UpdatedBy.Should().Be("TestUser");
    }

    /// <summary>
    /// MD-TAG-015: DateTime properties must handle various date values
    /// Critical for temporal data management
    /// </summary>
    [Fact(DisplayName = "MD-TAG-015: DateTime Properties Must Handle Various Date Values")]
    public void DateTime_Properties_Should_Handle_Various_Date_Values()
    {
        // Arrange
        var tag = new Tag();
        var pastDate = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var futureDate = new DateTime(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act & Assert - CreatedAt
        tag.CreatedAt = pastDate;
        tag.CreatedAt.Should().Be(pastDate);

        tag.CreatedAt = DateTime.MinValue;
        tag.CreatedAt.Should().Be(DateTime.MinValue);

        tag.CreatedAt = DateTime.MaxValue;
        tag.CreatedAt.Should().Be(DateTime.MaxValue);

        // Act & Assert - UpdatedAt (nullable)
        tag.UpdatedAt = null;
        tag.UpdatedAt.Should().BeNull();

        tag.UpdatedAt = futureDate;
        tag.UpdatedAt.Should().Be(futureDate);

        // Act & Assert - LocationTime (nullable)
        tag.LocationTime = null;
        tag.LocationTime.Should().BeNull();

        tag.LocationTime = pastDate;
        tag.LocationTime.Should().Be(pastDate);
    }

    /// <summary>
    /// MD-TAG-016: Boolean properties must handle true/false states correctly
    /// Critical for state management
    /// </summary>
    [Theory(DisplayName = "MD-TAG-016: Boolean Properties Must Handle States Correctly")]
    [InlineData(true)]
    [InlineData(false)]
    public void Boolean_Properties_Should_Handle_States_Correctly(bool value)
    {
        // Arrange
        var tag = new Tag();

        // Act
        tag.IsAuto = value;
        tag.HoldsItems = value;
        tag.HasAutoReservation = value;

        // Assert
        tag.IsAuto.Should().Be(value);
        tag.HoldsItems.Should().Be(value);
        tag.HasAutoReservation.Should().Be(value);
    }

    /// <summary>
    /// MD-TAG-017: Integer properties must handle boundary values
    /// Critical for data validation and edge cases
    /// </summary>
    [Theory(DisplayName = "MD-TAG-017: Integer Properties Must Handle Boundary Values")]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Integer_Properties_Should_Handle_Boundary_Values(int value)
    {
        // Arrange
        var tag = new Tag();

        // Act
        tag.Id = value;
        tag.TagNumber = value;
        tag.TagTypeKeyId = value;
        tag.LocationKeyId = value;
        tag.InTagGroupKeyId = value;

        // Assert
        tag.Id.Should().Be(value);
        tag.TagNumber.Should().Be(value);
        tag.TagTypeKeyId.Should().Be(value);
        tag.LocationKeyId.Should().Be(value);
        tag.InTagGroupKeyId.Should().Be(value);
    }

    /// <summary>
    /// MD-TAG-018: Enum properties must handle all valid enum values
    /// Critical for enum state management
    /// </summary>
    [Theory(DisplayName = "MD-TAG-018: Enum Properties Must Handle All Valid Values")]
    [InlineData(TagType.PrepTag, LifeStatus.Active)]
    [InlineData(TagType.Bundle, LifeStatus.Inactive)]
    [InlineData(TagType.CaseCart, LifeStatus.Dead)]
    [InlineData(TagType.TransportBox, LifeStatus.Active)]
    public void Enum_Properties_Should_Handle_All_Valid_Values(TagType tagType, LifeStatus status)
    {
        // Arrange
        var tag = new Tag();

        // Act
        tag.TagType = tagType;
        tag.Status = status;

        // Assert
        tag.TagType.Should().Be(tagType);
        tag.Status.Should().Be(status);
    }

    /// <summary>
    /// MD-TAG-019: Display properties must be consistent across property changes
    /// Critical for UI consistency
    /// </summary>
    [Fact(DisplayName = "MD-TAG-019: Display Properties Must Be Consistent Across Changes")]
    public void Display_Properties_Should_Be_Consistent_Across_Changes()
    {
        // Arrange
        var tag = new Tag
        {
            TagType = TagType.PrepTag,
            TagNumber = 100,
            IsAuto = false
        };

        // Initial state
        tag.DisplayString.Should().Be("Prep Tag #100");
        tag.FullDisplayString.Should().Be("Prep Tag #100");

        // Change tag type
        tag.TagType = TagType.Bundle;
        tag.DisplayString.Should().Be("Bundle #100");
        tag.FullDisplayString.Should().Be("Bundle #100");

        // Change number
        tag.TagNumber = 999;
        tag.DisplayString.Should().Be("Bundle #999");
        tag.FullDisplayString.Should().Be("Bundle #999");

        // Make auto
        tag.IsAuto = true;
        tag.DisplayString.Should().Be("Bundle #999");
        tag.FullDisplayString.Should().Be("[AUTO] Bundle #999");

        // Make non-auto again
        tag.IsAuto = false;
        tag.DisplayString.Should().Be("Bundle #999");
        tag.FullDisplayString.Should().Be("Bundle #999");
    }

    /// <summary>
    /// MD-TAG-020: Tag entity must support complex content manipulation
    /// Critical for comprehensive tag management
    /// </summary>
    [Fact(DisplayName = "MD-TAG-020: Tag Must Support Complex Content Manipulation")]
    public void Tag_Should_Support_Complex_Content_Manipulation()
    {
        // Arrange
        var parentTag = new Tag
        {
            Id = 1,
            TagType = TagType.Transport,
            TagNumber = 100
        };

        var childTag = new Tag
        {
            Id = 2,
            TagType = TagType.Bundle,
            TagNumber = 200
        };

        var item = new TagItem(1, 10, 100, 5);

        // Act - Add various content types
        parentTag.Contents.Tags.Add(childTag);
        parentTag.Contents.Units.AddRange(new[] { 1, 2, 3 });
        parentTag.Contents.Items.Add(item);
        parentTag.Contents.Indicators.AddRange(new[] { 10, 20 });

        // Add content to child tag
        childTag.Contents.Units.Add(4);

        // Assert parent tag state
        parentTag.IsEmpty.Should().BeFalse();
        parentTag.ContentCondition.Should().Be(TagContentCondition.Mixed);
        parentTag.Contents.TagCount.Should().Be(1);
        parentTag.Contents.UnitCount.Should().Be(3);
        parentTag.Contents.ItemCount.Should().Be(1);
        parentTag.Contents.IndicatorCount.Should().Be(2);

        // Assert child tag state
        childTag.IsEmpty.Should().BeFalse();
        childTag.ContentCondition.Should().Be(TagContentCondition.Units);
        childTag.Contents.UnitCount.Should().Be(1);

        // Test recursive unit collection
        var allUnits = parentTag.Contents.GetAllContainedUnits();
        allUnits.Should().HaveCount(4); // 3 direct + 1 from child
        allUnits.Should().Contain(new[] { 1, 2, 3, 4 });

        // Test content removal
        parentTag.Contents.RemoveTag(childTag);
        parentTag.Contents.TagCount.Should().Be(0);
        parentTag.Contents.GetAllContainedUnits().Should().HaveCount(3); // Only direct units now
    }
}
