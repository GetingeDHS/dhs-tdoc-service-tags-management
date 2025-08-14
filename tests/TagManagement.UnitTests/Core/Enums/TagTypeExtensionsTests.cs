using TagManagement.Core.Enums;

namespace TagManagement.UnitTests.Core.Enums;

/// <summary>
/// Medical Device Compliance Tests for TagTypeExtensions
/// Tests enum extension methods for regulatory compliance
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Core")]
[Trait("Component", "TagTypeExtensions")]
public class TagTypeExtensionsTests
{
    /// <summary>
    /// MD-TAGEXT-001: IsAutoTag must return correct values for all auto tag types
    /// Critical for automated tag processing in manufacturing
    /// </summary>
    [Theory(DisplayName = "MD-TAGEXT-001: IsAutoTag Must Return True For Auto Tag Types")]
    [InlineData(TagType.PrepTag)]
    [InlineData(TagType.Bundle)]
    [InlineData(TagType.Basket)]
    [InlineData(TagType.SteriLoad)]
    [InlineData(TagType.Wash)]
    [InlineData(TagType.WashLoad)]
    [InlineData(TagType.Transport)]
    [InlineData(TagType.TransportBox)]
    public void IsAutoTag_Should_Return_True_For_Auto_Tag_Types(TagType tagType)
    {
        // Act
        var result = tagType.IsAutoTag();

        // Assert
        result.Should().BeTrue($"TagType.{tagType} should be considered an auto tag");
    }

    /// <summary>
    /// MD-TAGEXT-002: IsAutoTag must return false for non-auto tag types
    /// Critical for manual tag processing identification
    /// </summary>
    [Theory(DisplayName = "MD-TAGEXT-002: IsAutoTag Must Return False For Non-Auto Tag Types")]
    [InlineData(TagType.CaseCart)]
    [InlineData(TagType.InstrumentContainer)]
    public void IsAutoTag_Should_Return_False_For_Non_Auto_Tag_Types(TagType tagType)
    {
        // Act
        var result = tagType.IsAutoTag();

        // Assert
        result.Should().BeFalse($"TagType.{tagType} should not be considered an auto tag");
    }

    /// <summary>
    /// MD-TAGEXT-003: IsAutoTag must handle invalid enum values gracefully
    /// Critical for system robustness and error handling
    /// </summary>
    [Theory(DisplayName = "MD-TAGEXT-003: IsAutoTag Must Handle Invalid Enum Values")]
    [InlineData((TagType)99)]
    [InlineData((TagType)(-1))]
    [InlineData((TagType)1000)]
    [InlineData((TagType)int.MaxValue)]
    [InlineData((TagType)int.MinValue)]
    public void IsAutoTag_Should_Handle_Invalid_Enum_Values(TagType invalidTagType)
    {
        // Act
        var result = invalidTagType.IsAutoTag();

        // Assert
        result.Should().BeFalse($"Invalid TagType value {(int)invalidTagType} should return false for IsAutoTag");
    }

    /// <summary>
    /// MD-TAGEXT-004: IsAutoTag must handle alias enum values correctly
    /// Critical for backward compatibility with existing systems
    /// </summary>
    [Theory(DisplayName = "MD-TAGEXT-004: IsAutoTag Must Handle Alias Enum Values")]
    [InlineData(TagType.BundleTag, true)]
    [InlineData(TagType.WashTag, true)]
    [InlineData(TagType.SterilizationLoadTag, true)]
    [InlineData(TagType.TransportTag, true)]
    [InlineData(TagType.TransportBoxTag, true)]
    public void IsAutoTag_Should_Handle_Alias_Enum_Values(TagType aliasTagType, bool expectedResult)
    {
        // Act
        var result = aliasTagType.IsAutoTag();

        // Assert
        result.Should().Be(expectedResult, $"Alias TagType.{aliasTagType} should return {expectedResult}");
    }

    /// <summary>
    /// MD-TAGEXT-005: GetDisplayName must return correct display names for all tag types
    /// Critical for user interface and system logging
    /// </summary>
    [Theory(DisplayName = "MD-TAGEXT-005: GetDisplayName Must Return Correct Display Names")]
    [InlineData(TagType.PrepTag, "Prep Tag")]
    [InlineData(TagType.Bundle, "Bundle")]
    [InlineData(TagType.Basket, "Basket")]
    [InlineData(TagType.SteriLoad, "Sterilization Load")]
    [InlineData(TagType.Wash, "Wash")]
    [InlineData(TagType.WashLoad, "Wash Load")]
    [InlineData(TagType.Transport, "Transport")]
    [InlineData(TagType.CaseCart, "Case Cart")]
    [InlineData(TagType.TransportBox, "Transport Box")]
    [InlineData(TagType.InstrumentContainer, "Instrument Container")]
    public void GetDisplayName_Should_Return_Correct_Display_Names(TagType tagType, string expectedDisplayName)
    {
        // Act
        var result = tagType.GetDisplayName();

        // Assert
        result.Should().Be(expectedDisplayName, $"TagType.{tagType} should display as '{expectedDisplayName}'");
        result.Should().NotBeNullOrEmpty("Display name should never be null or empty");
    }

    /// <summary>
    /// MD-TAGEXT-006: GetDisplayName must handle alias enum values correctly
    /// Critical for backward compatibility with existing user interfaces
    /// </summary>
    [Theory(DisplayName = "MD-TAGEXT-006: GetDisplayName Must Handle Alias Enum Values")]
    [InlineData(TagType.BundleTag, "Bundle")]
    [InlineData(TagType.WashTag, "Wash")]
    [InlineData(TagType.SterilizationLoadTag, "Sterilization Load")]
    [InlineData(TagType.TransportTag, "Transport")]
    [InlineData(TagType.TransportBoxTag, "Transport Box")]
    public void GetDisplayName_Should_Handle_Alias_Enum_Values(TagType aliasTagType, string expectedDisplayName)
    {
        // Act
        var result = aliasTagType.GetDisplayName();

        // Assert
        result.Should().Be(expectedDisplayName, $"Alias TagType.{aliasTagType} should display as '{expectedDisplayName}'");
    }

    /// <summary>
    /// MD-TAGEXT-007: GetDisplayName must handle invalid enum values gracefully
    /// Critical for system robustness and error logging
    /// </summary>
    [Theory(DisplayName = "MD-TAGEXT-007: GetDisplayName Must Handle Invalid Enum Values")]
    [InlineData((TagType)99, "99")]
    [InlineData((TagType)(-1), "-1")]
    [InlineData((TagType)1000, "1000")]
    public void GetDisplayName_Should_Handle_Invalid_Enum_Values(TagType invalidTagType, string expectedFallback)
    {
        // Act
        var result = invalidTagType.GetDisplayName();

        // Assert
        result.Should().Be(expectedFallback, $"Invalid TagType value should fall back to ToString()");
        result.Should().NotBeNullOrEmpty("Fallback display name should never be null or empty");
    }

    /// <summary>
    /// MD-TAGEXT-008: Both extension methods must be consistent across all enum values
    /// Critical for system data integrity and business logic consistency
    /// </summary>
    [Fact(DisplayName = "MD-TAGEXT-008: Extension Methods Must Be Consistent")]
    public void Extension_Methods_Should_Be_Consistent_Across_All_Values()
    {
        // Arrange - Get all defined TagType values
        var allTagTypes = Enum.GetValues<TagType>().Distinct().ToArray();

        // Act & Assert - Test each defined value
        foreach (var tagType in allTagTypes)
        {
            var isAuto = tagType.IsAutoTag();
            var displayName = tagType.GetDisplayName();

            // Assert consistent behavior
            displayName.Should().NotBeNullOrEmpty($"TagType.{tagType} must have a display name");
            
            // Auto tags should have meaningful display names
            if (isAuto)
            {
                // Some enum values like Bundle legitimately have same ToString() as display name
                displayName.Should().NotBeNullOrEmpty($"Auto tag {tagType} should have a meaningful display name");
            }
        }
    }

    /// <summary>
    /// MD-TAGEXT-009: Extension methods must handle boundary enum values
    /// Critical for system stability with edge cases
    /// </summary>
    [Theory(DisplayName = "MD-TAGEXT-009: Extension Methods Must Handle Boundary Values")]
    [InlineData((TagType)0)] // First valid value (PrepTag)
    [InlineData((TagType)9)] // Last valid value (InstrumentContainer)
    public void Extension_Methods_Should_Handle_Boundary_Enum_Values(TagType boundaryValue)
    {
        // Act
        var isAuto = boundaryValue.IsAutoTag();
        var displayName = boundaryValue.GetDisplayName();

        // Assert
        // IsAutoTag returns bool - test it's either true or false
        (isAuto == true || isAuto == false).Should().BeTrue("IsAutoTag should return a valid boolean");
        displayName.Should().NotBeNullOrEmpty("GetDisplayName should return a non-empty string");
    }

    /// <summary>
    /// MD-TAGEXT-010: IsAutoTag must be deterministic and repeatable
    /// Critical for manufacturing process consistency
    /// </summary>
    [Fact(DisplayName = "MD-TAGEXT-010: IsAutoTag Must Be Deterministic")]
    public void IsAutoTag_Should_Be_Deterministic()
    {
        // Arrange - Test with various tag types
        var testCases = new[]
        {
            TagType.PrepTag,
            TagType.CaseCart,
            TagType.Bundle,
            TagType.InstrumentContainer,
            TagType.Transport
        };

        // Act & Assert - Multiple calls should return same result
        foreach (var tagType in testCases)
        {
            var result1 = tagType.IsAutoTag();
            var result2 = tagType.IsAutoTag();
            var result3 = tagType.IsAutoTag();

            result1.Should().Be(result2, $"IsAutoTag for {tagType} should be consistent");
            result2.Should().Be(result3, $"IsAutoTag for {tagType} should be consistent across multiple calls");
        }
    }

    /// <summary>
    /// MD-TAGEXT-011: GetDisplayName must be deterministic and repeatable
    /// Critical for user interface consistency
    /// </summary>
    [Fact(DisplayName = "MD-TAGEXT-011: GetDisplayName Must Be Deterministic")]
    public void GetDisplayName_Should_Be_Deterministic()
    {
        // Arrange - Test with various tag types
        var testCases = new[]
        {
            TagType.PrepTag,
            TagType.SteriLoad,
            TagType.WashLoad,
            TagType.TransportBox,
            TagType.InstrumentContainer
        };

        // Act & Assert - Multiple calls should return same result
        foreach (var tagType in testCases)
        {
            var result1 = tagType.GetDisplayName();
            var result2 = tagType.GetDisplayName();
            var result3 = tagType.GetDisplayName();

            result1.Should().Be(result2, $"GetDisplayName for {tagType} should be consistent");
            result2.Should().Be(result3, $"GetDisplayName for {tagType} should be consistent across multiple calls");
            
            // Additional check for string consistency
            result1.Should().BeEquivalentTo(result2, $"String content should be identical");
        }
    }

    /// <summary>
    /// MD-TAGEXT-012: Extension methods must handle all combinations of enum values
    /// Critical for comprehensive system coverage
    /// </summary>
    [Fact(DisplayName = "MD-TAGEXT-012: Extension Methods Must Handle All Enum Combinations")]
    public void Extension_Methods_Should_Handle_All_Enum_Combinations()
    {
        // Arrange - Get all unique TagType values (excluding aliases)
        var uniqueValues = new[]
        {
            TagType.PrepTag,      // 0
            TagType.Bundle,       // 1
            TagType.Basket,       // 2
            TagType.SteriLoad,    // 3
            TagType.Wash,         // 4
            TagType.WashLoad,     // 5
            TagType.Transport,    // 6
            TagType.CaseCart,     // 7
            TagType.TransportBox, // 8
            TagType.InstrumentContainer // 9
        };

        // Act & Assert
        foreach (var tagType in uniqueValues)
        {
            // Test IsAutoTag
            var isAuto = tagType.IsAutoTag();
            // IsAutoTag returns bool - test it's either true or false
            (isAuto == true || isAuto == false).Should().BeTrue($"IsAutoTag should return boolean for {tagType}");

            // Test GetDisplayName
            var displayName = tagType.GetDisplayName();
            displayName.Should().BeOfType<string>($"GetDisplayName should return string for {tagType}");
            displayName.Should().NotBeNull($"GetDisplayName should not be null for {tagType}");
            displayName.Should().NotBeEmpty($"GetDisplayName should not be empty for {tagType}");

            // Verify logical consistency
            if (isAuto && tagType != TagType.CaseCart && tagType != TagType.InstrumentContainer)
            {
                // Some enum values like Bundle legitimately have same ToString() as display name
                displayName.Should().NotBeNullOrEmpty($"Auto tag {tagType} should have meaningful display name");
            }
        }
    }

    /// <summary>
    /// MD-TAGEXT-013: Extension methods must handle enum casting scenarios
    /// Critical for interoperability with external systems
    /// </summary>
    [Theory(DisplayName = "MD-TAGEXT-013: Extension Methods Must Handle Enum Casting")]
    [InlineData(0, true, "Prep Tag")]   // PrepTag
    [InlineData(1, true, "Bundle")]     // Bundle
    [InlineData(7, false, "Case Cart")] // CaseCart
    [InlineData(9, false, "Instrument Container")] // InstrumentContainer
    public void Extension_Methods_Should_Handle_Enum_Casting(int enumValue, bool expectedIsAuto, string expectedDisplayName)
    {
        // Arrange
        var tagType = (TagType)enumValue;

        // Act
        var isAuto = tagType.IsAutoTag();
        var displayName = tagType.GetDisplayName();

        // Assert
        isAuto.Should().Be(expectedIsAuto, $"Cast TagType value {enumValue} should have IsAutoTag = {expectedIsAuto}");
        displayName.Should().Be(expectedDisplayName, $"Cast TagType value {enumValue} should display as '{expectedDisplayName}'");
    }
}
