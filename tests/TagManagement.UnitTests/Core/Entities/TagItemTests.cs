using TagManagement.Core.Entities;

namespace TagManagement.UnitTests.Core.Entities;

/// <summary>
/// Medical Device Compliance Tests for TagItem Entity
/// Tests item representation within tags
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Core")]
[Trait("Component", "TagItem")]
public class TagItemTests
{
    /// <summary>
    /// MD-TAGITEM-001: Default constructor must initialize with zero values
    /// Critical for proper entity initialization
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-001: Default Constructor Must Initialize With Zero Values")]
    public void Default_Constructor_Should_Initialize_With_Zero_Values()
    {
        // Act
        var tagItem = new TagItem();

        // Assert
        tagItem.ItemKeyId.Should().Be(0);
        tagItem.SerialKeyId.Should().Be(0);
        tagItem.LotInfoKeyId.Should().Be(0);
        tagItem.Count.Should().Be(0);
    }

    /// <summary>
    /// MD-TAGITEM-002: Parameterized constructor must set all properties correctly
    /// Critical for proper entity initialization with data
    /// </summary>
    [Theory(DisplayName = "MD-TAGITEM-002: Parameterized Constructor Must Set Properties Correctly")]
    [InlineData(1, 10, 100, 5)]
    [InlineData(999, 888, 777, 25)]
    [InlineData(0, 0, 0, 0)]
    [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
    public void Parameterized_Constructor_Should_Set_All_Properties_Correctly(
        int itemKeyId, int serialKeyId, int lotInfoKeyId, int count)
    {
        // Act
        var tagItem = new TagItem(itemKeyId, serialKeyId, lotInfoKeyId, count);

        // Assert
        tagItem.ItemKeyId.Should().Be(itemKeyId);
        tagItem.SerialKeyId.Should().Be(serialKeyId);
        tagItem.LotInfoKeyId.Should().Be(lotInfoKeyId);
        tagItem.Count.Should().Be(count);
    }

    /// <summary>
    /// MD-TAGITEM-003: Properties must be settable after construction
    /// Critical for entity data modification
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-003: Properties Must Be Settable After Construction")]
    public void Properties_Should_Be_Settable_After_Construction()
    {
        // Arrange
        var tagItem = new TagItem();

        // Act
        tagItem.ItemKeyId = 123;
        tagItem.SerialKeyId = 456;
        tagItem.LotInfoKeyId = 789;
        tagItem.Count = 15;

        // Assert
        tagItem.ItemKeyId.Should().Be(123);
        tagItem.SerialKeyId.Should().Be(456);
        tagItem.LotInfoKeyId.Should().Be(789);
        tagItem.Count.Should().Be(15);
    }

    /// <summary>
    /// MD-TAGITEM-004: CreateCopy must create exact duplicate
    /// Critical for entity cloning operations
    /// </summary>
    [Theory(DisplayName = "MD-TAGITEM-004: CreateCopy Must Create Exact Duplicate")]
    [InlineData(1, 10, 100, 5)]
    [InlineData(999, 888, 777, 25)]
    [InlineData(0, 0, 0, 0)]
    public void CreateCopy_Should_Create_Exact_Duplicate(
        int itemKeyId, int serialKeyId, int lotInfoKeyId, int count)
    {
        // Arrange
        var original = new TagItem(itemKeyId, serialKeyId, lotInfoKeyId, count);

        // Act
        var copy = original.CreateCopy();

        // Assert
        copy.Should().NotBeSameAs(original, "Copy should be a different instance");
        copy.ItemKeyId.Should().Be(original.ItemKeyId);
        copy.SerialKeyId.Should().Be(original.SerialKeyId);
        copy.LotInfoKeyId.Should().Be(original.LotInfoKeyId);
        copy.Count.Should().Be(original.Count);
    }

    /// <summary>
    /// MD-TAGITEM-005: CreateCopy must be independent of original
    /// Critical for proper entity isolation
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-005: CreateCopy Must Be Independent Of Original")]
    public void CreateCopy_Should_Be_Independent_Of_Original()
    {
        // Arrange
        var original = new TagItem(1, 10, 100, 5);
        var copy = original.CreateCopy();

        // Act - Modify original
        original.ItemKeyId = 999;
        original.SerialKeyId = 888;
        original.LotInfoKeyId = 777;
        original.Count = 50;

        // Assert - Copy should remain unchanged
        copy.ItemKeyId.Should().Be(1);
        copy.SerialKeyId.Should().Be(10);
        copy.LotInfoKeyId.Should().Be(100);
        copy.Count.Should().Be(5);
    }

    /// <summary>
    /// MD-TAGITEM-006: Equals must compare based on key properties only
    /// Critical for proper entity comparison
    /// </summary>
    [Theory(DisplayName = "MD-TAGITEM-006: Equals Must Compare Based On Key Properties")]
    [InlineData(1, 10, 100)]
    [InlineData(999, 888, 777)]
    [InlineData(0, 0, 0)]
    public void Equals_Should_Compare_Based_On_Key_Properties_Only(
        int itemKeyId, int serialKeyId, int lotInfoKeyId)
    {
        // Arrange
        var item1 = new TagItem(itemKeyId, serialKeyId, lotInfoKeyId, 5);
        var item2 = new TagItem(itemKeyId, serialKeyId, lotInfoKeyId, 10); // Different count

        // Act & Assert
        item1.Equals(item2).Should().BeTrue("Items with same keys should be equal regardless of count");
        (item1 == item2).Should().BeFalse("== operator not overridden, should use reference equality");
    }

    /// <summary>
    /// MD-TAGITEM-007: Equals must return false for different key combinations
    /// Critical for proper entity differentiation
    /// </summary>
    [Theory(DisplayName = "MD-TAGITEM-007: Equals Must Return False For Different Keys")]
    [InlineData(1, 10, 100, 2, 10, 100)] // Different ItemKeyId
    [InlineData(1, 10, 100, 1, 20, 100)] // Different SerialKeyId
    [InlineData(1, 10, 100, 1, 10, 200)] // Different LotInfoKeyId
    [InlineData(1, 10, 100, 2, 20, 200)] // All different
    public void Equals_Should_Return_False_For_Different_Keys(
        int item1Key, int item1Serial, int item1Lot,
        int item2Key, int item2Serial, int item2Lot)
    {
        // Arrange
        var item1 = new TagItem(item1Key, item1Serial, item1Lot, 5);
        var item2 = new TagItem(item2Key, item2Serial, item2Lot, 5);

        // Act & Assert
        item1.Equals(item2).Should().BeFalse("Items with different keys should not be equal");
    }

    /// <summary>
    /// MD-TAGITEM-008: Equals must handle null and non-TagItem objects
    /// Critical for robust equality checking
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-008: Equals Must Handle Null And Non-TagItem Objects")]
    public void Equals_Should_Handle_Null_And_Non_TagItem_Objects()
    {
        // Arrange
        var tagItem = new TagItem(1, 10, 100, 5);

        // Act & Assert
        tagItem.Equals(null).Should().BeFalse("TagItem should not equal null");
        tagItem.Equals("string").Should().BeFalse("TagItem should not equal string");
        tagItem.Equals(123).Should().BeFalse("TagItem should not equal integer");
        tagItem.Equals(new object()).Should().BeFalse("TagItem should not equal generic object");
    }

    /// <summary>
    /// MD-TAGITEM-009: Equals must be reflexive
    /// Critical for proper equality contract
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-009: Equals Must Be Reflexive")]
    public void Equals_Should_Be_Reflexive()
    {
        // Arrange
        var tagItem = new TagItem(1, 10, 100, 5);

        // Act & Assert
        tagItem.Equals(tagItem).Should().BeTrue("Object should equal itself");
    }

    /// <summary>
    /// MD-TAGITEM-010: Equals must be symmetric
    /// Critical for proper equality contract
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-010: Equals Must Be Symmetric")]
    public void Equals_Should_Be_Symmetric()
    {
        // Arrange
        var item1 = new TagItem(1, 10, 100, 5);
        var item2 = new TagItem(1, 10, 100, 7); // Different count but same keys

        // Act & Assert
        item1.Equals(item2).Should().Be(item2.Equals(item1), "Equality should be symmetric");
    }

    /// <summary>
    /// MD-TAGITEM-011: Equals must be transitive
    /// Critical for proper equality contract
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-011: Equals Must Be Transitive")]
    public void Equals_Should_Be_Transitive()
    {
        // Arrange
        var item1 = new TagItem(1, 10, 100, 5);
        var item2 = new TagItem(1, 10, 100, 7);
        var item3 = new TagItem(1, 10, 100, 9);

        // Act & Assert
        var equals12 = item1.Equals(item2);
        var equals23 = item2.Equals(item3);
        var equals13 = item1.Equals(item3);

        // If item1 equals item2 and item2 equals item3, then item1 should equal item3
        if (equals12 && equals23)
        {
            equals13.Should().BeTrue("Equality should be transitive");
        }

        // All should be equal since they have same keys
        equals12.Should().BeTrue();
        equals23.Should().BeTrue();
        equals13.Should().BeTrue();
    }

    /// <summary>
    /// MD-TAGITEM-012: GetHashCode must be consistent with Equals
    /// Critical for proper hash-based collection behavior
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-012: GetHashCode Must Be Consistent With Equals")]
    public void GetHashCode_Should_Be_Consistent_With_Equals()
    {
        // Arrange
        var item1 = new TagItem(1, 10, 100, 5);
        var item2 = new TagItem(1, 10, 100, 10); // Different count but equal by key

        // Act
        var hash1 = item1.GetHashCode();
        var hash2 = item2.GetHashCode();

        // Assert
        item1.Equals(item2).Should().BeTrue("Items should be equal");
        hash1.Should().Be(hash2, "Equal objects must have equal hash codes");
    }

    /// <summary>
    /// MD-TAGITEM-013: GetHashCode must be deterministic
    /// Critical for consistent hash behavior
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-013: GetHashCode Must Be Deterministic")]
    public void GetHashCode_Should_Be_Deterministic()
    {
        // Arrange
        var tagItem = new TagItem(1, 10, 100, 5);

        // Act - Get hash code multiple times
        var hash1 = tagItem.GetHashCode();
        var hash2 = tagItem.GetHashCode();
        var hash3 = tagItem.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
        hash2.Should().Be(hash3);
        hash1.Should().Be(hash3);
    }

    /// <summary>
    /// MD-TAGITEM-014: Different key combinations must produce different hash codes (usually)
    /// Critical for good hash distribution
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-014: Different Keys Should Usually Produce Different Hash Codes")]
    public void Different_Keys_Should_Usually_Produce_Different_Hash_Codes()
    {
        // Arrange - Create items with different key combinations
        var items = new[]
        {
            new TagItem(1, 10, 100, 5),
            new TagItem(2, 10, 100, 5),
            new TagItem(1, 20, 100, 5),
            new TagItem(1, 10, 200, 5),
            new TagItem(2, 20, 200, 5),
        };

        // Act - Get all hash codes
        var hashCodes = items.Select(item => item.GetHashCode()).ToArray();

        // Assert - Most should be different (hash collisions are possible but rare)
        var uniqueHashes = hashCodes.Distinct().Count();
        uniqueHashes.Should().BeGreaterThan(3, "Most different key combinations should produce different hash codes");
    }

    /// <summary>
    /// MD-TAGITEM-015: HashSet operations must work correctly
    /// Critical for collection-based operations
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-015: HashSet Operations Must Work Correctly")]
    public void HashSet_Operations_Should_Work_Correctly()
    {
        // Arrange
        var item1 = new TagItem(1, 10, 100, 5);
        var item2 = new TagItem(1, 10, 100, 10); // Equal to item1 by keys
        var item3 = new TagItem(2, 20, 200, 5);   // Different from others

        var hashSet = new HashSet<TagItem>();

        // Act & Assert
        hashSet.Add(item1).Should().BeTrue("First add should succeed");
        hashSet.Add(item2).Should().BeFalse("Second add of equal item should fail");
        hashSet.Add(item3).Should().BeTrue("Add of different item should succeed");

        hashSet.Should().HaveCount(2);
        hashSet.Contains(item1).Should().BeTrue();
        hashSet.Contains(item2).Should().BeTrue("Equal item should be found");
        hashSet.Contains(item3).Should().BeTrue();
    }

    /// <summary>
    /// MD-TAGITEM-016: Properties must handle boundary values
    /// Critical for data validation and edge case handling
    /// </summary>
    [Theory(DisplayName = "MD-TAGITEM-016: Properties Must Handle Boundary Values")]
    [InlineData(int.MinValue, int.MinValue, int.MinValue, int.MinValue)]
    [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
    [InlineData(0, 0, 0, 0)]
    [InlineData(-1, -1, -1, -1)]
    public void Properties_Should_Handle_Boundary_Values(
        int itemKeyId, int serialKeyId, int lotInfoKeyId, int count)
    {
        // Act
        var tagItem = new TagItem(itemKeyId, serialKeyId, lotInfoKeyId, count);

        // Assert
        tagItem.ItemKeyId.Should().Be(itemKeyId);
        tagItem.SerialKeyId.Should().Be(serialKeyId);
        tagItem.LotInfoKeyId.Should().Be(lotInfoKeyId);
        tagItem.Count.Should().Be(count);
    }

    /// <summary>
    /// MD-TAGITEM-017: Count property modifications must not affect equality
    /// Critical for proper key-based equality behavior
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-017: Count Modifications Must Not Affect Equality")]
    public void Count_Modifications_Should_Not_Affect_Equality()
    {
        // Arrange
        var item1 = new TagItem(1, 10, 100, 5);
        var item2 = new TagItem(1, 10, 100, 5);

        // Verify initial equality
        item1.Equals(item2).Should().BeTrue();

        // Act - Modify counts
        item1.Count = 100;
        item2.Count = 1;

        // Assert - Should still be equal
        item1.Equals(item2).Should().BeTrue("Count changes should not affect equality");
        item1.GetHashCode().Should().Be(item2.GetHashCode(), "Hash codes should remain equal");
    }

    /// <summary>
    /// MD-TAGITEM-018: Key property modifications must affect equality
    /// Critical for proper key-based equality behavior
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-018: Key Property Modifications Must Affect Equality")]
    public void Key_Property_Modifications_Should_Affect_Equality()
    {
        // Arrange
        var item1 = new TagItem(1, 10, 100, 5);
        var item2 = new TagItem(1, 10, 100, 5);

        // Verify initial equality
        item1.Equals(item2).Should().BeTrue();

        // Act - Modify key properties
        item2.ItemKeyId = 2;

        // Assert - Should no longer be equal
        item1.Equals(item2).Should().BeFalse("Key changes should affect equality");
    }

    /// <summary>
    /// MD-TAGITEM-019: ToString behavior validation for debugging support
    /// Critical for development and debugging support
    /// </summary>
    [Fact(DisplayName = "MD-TAGITEM-019: ToString Should Provide Meaningful Output")]
    public void ToString_Should_Provide_Meaningful_Output()
    {
        // Arrange
        var tagItem = new TagItem(123, 456, 789, 10);

        // Act
        var result = tagItem.ToString();

        // Assert - Should use default object ToString (we're not overriding it)
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("TagItem", "Should contain class name");
    }
}
