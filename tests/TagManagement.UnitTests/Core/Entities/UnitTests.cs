using TagManagement.Core.Entities;
using TagManagement.Core.Enums;

namespace TagManagement.UnitTests.Core.Entities;

/// <summary>
/// Medical Device Compliance Tests for Unit Entity
/// Tests core domain entity for regulatory compliance
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Core")]
[Trait("Entity", "Unit")]
public class UnitTests
{
    /// <summary>
    /// MD-UNIT-001: Unit entity must initialize with correct default values
    /// Critical for data integrity and traceability
    /// </summary>
    [Fact(DisplayName = "MD-UNIT-001: Unit Must Initialize With Default Values")]
    public void Unit_Should_Initialize_With_Default_Values()
    {
        // Act
        var unit = new Unit();

        // Assert
        unit.Id.Should().Be(0, "Id should have default value");
        unit.UnitNumber.Should().Be(0, "UnitNumber should have default value");
        unit.Status.Should().Be(UnitStatus.New, "Status should default to New");
        unit.ProductKeyId.Should().Be(0, "ProductKeyId should have default value");
        unit.CustomerKeyId.Should().Be(0, "CustomerKeyId should have default value");
        unit.CreatedAt.Should().Be(default(DateTime), "CreatedAt should have default value");
        unit.UpdatedAt.Should().BeNull("UpdatedAt should be null by default");
        unit.Product.Should().BeNull("Product should be null by default");
    }

    /// <summary>
    /// MD-UNIT-002: Unit properties must accept and return correct values
    /// Critical for data integrity and manufacturing traceability
    /// </summary>
    [Fact(DisplayName = "MD-UNIT-002: Unit Properties Must Accept And Return Values")]
    public void Unit_Properties_Should_Accept_And_Return_Values()
    {
        // Arrange
        var unit = new Unit();
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow.AddHours(1);
        var product = new Product();

        // Act & Assert - Test all properties
        unit.Id = 12345;
        unit.Id.Should().Be(12345, "Id property should store and return value");

        unit.UnitNumber = 98765;
        unit.UnitNumber.Should().Be(98765, "UnitNumber property should store and return value");

        unit.Status = UnitStatus.Sterile;
        unit.Status.Should().Be(UnitStatus.Sterile, "Status property should store and return value");

        unit.ProductKeyId = 555;
        unit.ProductKeyId.Should().Be(555, "ProductKeyId property should store and return value");

        unit.CustomerKeyId = 777;
        unit.CustomerKeyId.Should().Be(777, "CustomerKeyId property should store and return value");

        unit.CreatedAt = createdDate;
        unit.CreatedAt.Should().Be(createdDate, "CreatedAt property should store and return value");

        unit.UpdatedAt = updatedDate;
        unit.UpdatedAt.Should().Be(updatedDate, "UpdatedAt property should store and return value");

        unit.Product = product;
        unit.Product.Should().BeSameAs(product, "Product property should store and return reference");
    }

    /// <summary>
    /// MD-UNIT-003: StatusDisplayString must return correct display text for all status values
    /// Critical for user interface and manufacturing process tracking
    /// </summary>
    [Theory(DisplayName = "MD-UNIT-003: StatusDisplayString Must Return Correct Display Text")]
    [InlineData(UnitStatus.New, "New")]
    [InlineData(UnitStatus.Dirty, "Dirty")]
    [InlineData(UnitStatus.InWash, "In Wash")]
    [InlineData(UnitStatus.Clean, "Clean")]
    [InlineData(UnitStatus.InSterilization, "In Sterilization")]
    [InlineData(UnitStatus.Sterile, "Sterile")]
    [InlineData(UnitStatus.InUse, "In Use")]
    [InlineData(UnitStatus.Expired, "Expired")]
    [InlineData(UnitStatus.Maintenance, "Maintenance")]
    public void StatusDisplayString_Should_Return_Correct_Display_Text(UnitStatus status, string expectedDisplay)
    {
        // Arrange
        var unit = new Unit { Status = status };

        // Act
        var displayString = unit.StatusDisplayString;

        // Assert
        displayString.Should().Be(expectedDisplay, 
            $"StatusDisplayString should return '{expectedDisplay}' for status {status}");
    }

    /// <summary>
    /// MD-UNIT-004: StatusDisplayString must handle invalid enum values gracefully
    /// Critical for system robustness and error handling
    /// </summary>
    [Fact(DisplayName = "MD-UNIT-004: StatusDisplayString Must Handle Invalid Enum Values")]
    public void StatusDisplayString_Should_Handle_Invalid_Enum_Values()
    {
        // Arrange - Force an invalid enum value
        var unit = new Unit();
        var invalidStatus = (UnitStatus)999; // Invalid enum value
        unit.Status = invalidStatus;

        // Act
        var displayString = unit.StatusDisplayString;

        // Assert
        displayString.Should().Be("999", 
            "StatusDisplayString should fall back to ToString() for invalid enum values");
    }

    /// <summary>
    /// MD-UNIT-005: Unit entity must support full lifecycle data tracking
    /// Critical for medical device traceability requirements
    /// </summary>
    [Fact(DisplayName = "MD-UNIT-005: Unit Must Support Full Lifecycle Tracking")]
    public void Unit_Should_Support_Full_Lifecycle_Tracking()
    {
        // Arrange
        var unit = new Unit();
        var baselineTime = DateTime.UtcNow;

        // Act - Simulate unit lifecycle
        
        // 1. Creation
        unit.Id = 1001;
        unit.UnitNumber = 54321;
        unit.Status = UnitStatus.New;
        unit.ProductKeyId = 100;
        unit.CustomerKeyId = 200;
        unit.CreatedAt = baselineTime;

        // 2. Processing
        unit.Status = UnitStatus.Dirty;
        unit.UpdatedAt = baselineTime.AddMinutes(30);

        // 3. Washing
        unit.Status = UnitStatus.InWash;
        unit.UpdatedAt = baselineTime.AddHours(1);

        // 4. Sterilization
        unit.Status = UnitStatus.InSterilization;
        unit.UpdatedAt = baselineTime.AddHours(2);

        // 5. Ready for use
        unit.Status = UnitStatus.Sterile;
        unit.UpdatedAt = baselineTime.AddHours(4);

        // Assert - Verify lifecycle tracking
        unit.Id.Should().Be(1001, "Unit ID must be preserved throughout lifecycle");
        unit.UnitNumber.Should().Be(54321, "Unit number must be preserved throughout lifecycle");
        unit.Status.Should().Be(UnitStatus.Sterile, "Final status should be Sterile");
        unit.StatusDisplayString.Should().Be("Sterile", "Display string should match final status");
        unit.ProductKeyId.Should().Be(100, "Product reference must be preserved");
        unit.CustomerKeyId.Should().Be(200, "Customer reference must be preserved");
        unit.CreatedAt.Should().Be(baselineTime, "Creation time must be preserved");
        unit.UpdatedAt.Should().Be(baselineTime.AddHours(4), "Update time should reflect last change");
    }

    /// <summary>
    /// MD-UNIT-006: Unit entity must handle null and boundary values correctly
    /// Critical for data validation and system stability
    /// </summary>
    [Fact(DisplayName = "MD-UNIT-006: Unit Must Handle Boundary Values")]
    public void Unit_Should_Handle_Boundary_Values()
    {
        // Arrange & Act & Assert - Test boundary values
        var unit = new Unit();

        // Test minimum values
        unit.Id = int.MinValue;
        unit.Id.Should().Be(int.MinValue, "Should handle minimum integer value");

        unit.UnitNumber = int.MinValue;
        unit.UnitNumber.Should().Be(int.MinValue, "Should handle minimum unit number");

        // Test maximum values
        unit.Id = int.MaxValue;
        unit.Id.Should().Be(int.MaxValue, "Should handle maximum integer value");

        unit.UnitNumber = int.MaxValue;
        unit.UnitNumber.Should().Be(int.MaxValue, "Should handle maximum unit number");

        // Test DateTime boundary values
        unit.CreatedAt = DateTime.MinValue;
        unit.CreatedAt.Should().Be(DateTime.MinValue, "Should handle minimum DateTime");

        unit.CreatedAt = DateTime.MaxValue;
        unit.CreatedAt.Should().Be(DateTime.MaxValue, "Should handle maximum DateTime");

        // Test nullable properties
        unit.UpdatedAt = null;
        unit.UpdatedAt.Should().BeNull("UpdatedAt should accept null values");

        unit.Product = null;
        unit.Product.Should().BeNull("Product should accept null values");
    }

    /// <summary>
    /// MD-UNIT-007: Unit entity must maintain consistency across all status transitions
    /// Critical for manufacturing process validation
    /// </summary>
    [Theory(DisplayName = "MD-UNIT-007: Unit Must Maintain Consistency Across Status Transitions")]
    [InlineData(UnitStatus.New, UnitStatus.Dirty)]
    [InlineData(UnitStatus.Dirty, UnitStatus.InWash)]
    [InlineData(UnitStatus.InWash, UnitStatus.Clean)]
    [InlineData(UnitStatus.Clean, UnitStatus.InSterilization)]
    [InlineData(UnitStatus.InSterilization, UnitStatus.Sterile)]
    [InlineData(UnitStatus.Sterile, UnitStatus.InUse)]
    [InlineData(UnitStatus.InUse, UnitStatus.Dirty)]
    [InlineData(UnitStatus.Sterile, UnitStatus.Expired)]
    [InlineData(UnitStatus.InUse, UnitStatus.Maintenance)]
    public void Unit_Should_Maintain_Consistency_Across_Status_Transitions(
        UnitStatus fromStatus, UnitStatus toStatus)
    {
        // Arrange
        var unit = new Unit
        {
            Id = 2001,
            UnitNumber = 12345,
            Status = fromStatus,
            ProductKeyId = 100,
            CustomerKeyId = 200,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var originalId = unit.Id;
        var originalUnitNumber = unit.UnitNumber;
        var originalProductKeyId = unit.ProductKeyId;
        var originalCustomerKeyId = unit.CustomerKeyId;
        var originalCreatedAt = unit.CreatedAt;
        var fromDisplayString = unit.StatusDisplayString;

        // Act - Change status
        unit.Status = toStatus;
        unit.UpdatedAt = DateTime.UtcNow;
        var toDisplayString = unit.StatusDisplayString;

        // Assert - Verify consistency
        unit.Id.Should().Be(originalId, "ID must remain consistent across status changes");
        unit.UnitNumber.Should().Be(originalUnitNumber, "Unit number must remain consistent");
        unit.ProductKeyId.Should().Be(originalProductKeyId, "Product reference must remain consistent");
        unit.CustomerKeyId.Should().Be(originalCustomerKeyId, "Customer reference must remain consistent");
        unit.CreatedAt.Should().Be(originalCreatedAt, "Creation time must remain consistent");
        unit.Status.Should().Be(toStatus, "Status should be updated to new value");
        unit.UpdatedAt.Should().NotBeNull("UpdatedAt should be set when status changes");
        
        fromDisplayString.Should().NotBe(toDisplayString, 
            "Display string should change when status changes");
        toDisplayString.Should().NotBeNullOrEmpty("New display string should have value");
    }
}
