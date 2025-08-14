using TagManagement.Core.Entities;

namespace TagManagement.UnitTests.Core.Entities;

/// <summary>
/// Medical Device Compliance Tests for Customer Entity
/// Tests core domain entity for regulatory compliance
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Core")]
[Trait("Entity", "Customer")]
public class CustomerTests
{
    /// <summary>
    /// MD-CUSTOMER-001: Customer entity must initialize with correct default values
    /// Critical for data integrity and traceability
    /// </summary>
    [Fact(DisplayName = "MD-CUSTOMER-001: Customer Must Initialize With Default Values")]
    public void Customer_Should_Initialize_With_Default_Values()
    {
        // Act
        var customer = new Customer();

        // Assert
        customer.Id.Should().Be(0, "Id should have default value");
        customer.Name.Should().Be(string.Empty, "Name should be empty string by default");
        customer.Code.Should().Be(string.Empty, "Code should be empty string by default");
        customer.CreatedAt.Should().Be(default(DateTime), "CreatedAt should have default value");
        customer.UpdatedAt.Should().BeNull("UpdatedAt should be null by default");
    }

    /// <summary>
    /// MD-CUSTOMER-002: Customer properties must accept and return correct values
    /// Critical for data integrity and customer tracking
    /// </summary>
    [Fact(DisplayName = "MD-CUSTOMER-002: Customer Properties Must Accept And Return Values")]
    public void Customer_Properties_Should_Accept_And_Return_Values()
    {
        // Arrange
        var customer = new Customer();
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow.AddHours(1);

        // Act & Assert - Test all properties
        customer.Id = 12345;
        customer.Id.Should().Be(12345, "Id property should store and return value");

        customer.Name = "Test Customer Inc.";
        customer.Name.Should().Be("Test Customer Inc.", "Name property should store and return value");

        customer.Code = "TC001";
        customer.Code.Should().Be("TC001", "Code property should store and return value");

        customer.CreatedAt = createdDate;
        customer.CreatedAt.Should().Be(createdDate, "CreatedAt property should store and return value");

        customer.UpdatedAt = updatedDate;
        customer.UpdatedAt.Should().Be(updatedDate, "UpdatedAt property should store and return value");
    }

    /// <summary>
    /// MD-CUSTOMER-003: Customer entity must handle string properties correctly
    /// Critical for data validation and customer identification
    /// </summary>
    [Theory(DisplayName = "MD-CUSTOMER-003: Customer Must Handle String Properties")]
    [InlineData("", "")]
    [InlineData("Single Customer", "SC")]
    [InlineData("Very Long Customer Name With Special Characters !@#$%^&*()", "VLCNWSC123")]
    [InlineData("Unicode Customer ñáméß", "UCP001")]
    [InlineData("Customer with\nNewlines\tand\rCarriageReturns", "CNACR")]
    public void Customer_Should_Handle_String_Properties(string name, string code)
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Name = name;
        customer.Code = code;

        // Assert
        customer.Name.Should().Be(name, "Name should handle various string formats");
        customer.Code.Should().Be(code, "Code should handle various string formats");
    }

    /// <summary>
    /// MD-CUSTOMER-004: Customer entity must handle null string values gracefully
    /// Critical for data validation and system stability
    /// </summary>
    [Fact(DisplayName = "MD-CUSTOMER-004: Customer Must Handle Null String Values")]
    public void Customer_Should_Handle_Null_String_Values()
    {
        // Arrange
        var customer = new Customer();

        // Act & Assert - Setting null values should work
        customer.Name = null!;
        customer.Code = null!;

        customer.Name.Should().BeNull("Name should accept null values");
        customer.Code.Should().BeNull("Code should accept null values");
    }

    /// <summary>
    /// MD-CUSTOMER-005: Customer entity must handle boundary values correctly
    /// Critical for data validation and system stability
    /// </summary>
    [Fact(DisplayName = "MD-CUSTOMER-005: Customer Must Handle Boundary Values")]
    public void Customer_Should_Handle_Boundary_Values()
    {
        // Arrange & Act & Assert - Test boundary values
        var customer = new Customer();

        // Test minimum values
        customer.Id = int.MinValue;
        customer.Id.Should().Be(int.MinValue, "Should handle minimum integer value");

        // Test maximum values
        customer.Id = int.MaxValue;
        customer.Id.Should().Be(int.MaxValue, "Should handle maximum integer value");

        // Test DateTime boundary values
        customer.CreatedAt = DateTime.MinValue;
        customer.CreatedAt.Should().Be(DateTime.MinValue, "Should handle minimum DateTime");

        customer.CreatedAt = DateTime.MaxValue;
        customer.CreatedAt.Should().Be(DateTime.MaxValue, "Should handle maximum DateTime");

        // Test nullable properties
        customer.UpdatedAt = null;
        customer.UpdatedAt.Should().BeNull("UpdatedAt should accept null values");

        customer.UpdatedAt = DateTime.MaxValue;
        customer.UpdatedAt.Should().Be(DateTime.MaxValue, "UpdatedAt should accept DateTime values");
    }

    /// <summary>
    /// MD-CUSTOMER-006: Customer entity must support full lifecycle data tracking
    /// Critical for medical device traceability requirements
    /// </summary>
    [Fact(DisplayName = "MD-CUSTOMER-006: Customer Must Support Full Lifecycle Tracking")]
    public void Customer_Should_Support_Full_Lifecycle_Tracking()
    {
        // Arrange
        var customer = new Customer();
        var baselineTime = DateTime.UtcNow;

        // Act - Simulate customer lifecycle
        
        // 1. Creation
        customer.Id = 1001;
        customer.Name = "Medical Center Alpha";
        customer.Code = "MCA001";
        customer.CreatedAt = baselineTime;

        // 2. Update customer information
        customer.Name = "Medical Center Alpha - Updated";
        customer.UpdatedAt = baselineTime.AddDays(30);

        // 3. Code change
        customer.Code = "MCA002";
        customer.UpdatedAt = baselineTime.AddDays(60);

        // Assert - Verify lifecycle tracking
        customer.Id.Should().Be(1001, "Customer ID must be preserved throughout lifecycle");
        customer.Name.Should().Be("Medical Center Alpha - Updated", "Name should reflect final update");
        customer.Code.Should().Be("MCA002", "Code should reflect final update");
        customer.CreatedAt.Should().Be(baselineTime, "Creation time must be preserved");
        customer.UpdatedAt.Should().Be(baselineTime.AddDays(60), "Update time should reflect last change");
    }

    /// <summary>
    /// MD-CUSTOMER-007: Customer entity must maintain data consistency across operations
    /// Critical for customer identification and business continuity
    /// </summary>
    [Fact(DisplayName = "MD-CUSTOMER-007: Customer Must Maintain Data Consistency")]
    public void Customer_Should_Maintain_Data_Consistency()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 2001,
            Name = "Consistent Customer Corp",
            Code = "CCC001",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var originalId = customer.Id;
        var originalName = customer.Name;
        var originalCode = customer.Code;
        var originalCreatedAt = customer.CreatedAt;
        var originalUpdatedAt = customer.UpdatedAt;

        // Act - Perform various operations
        var modifiedCustomer = customer; // Simulate passing around the reference
        modifiedCustomer.Name = "Updated " + modifiedCustomer.Name;
        modifiedCustomer.UpdatedAt = DateTime.UtcNow;

        // Assert - Verify consistency
        customer.Should().BeSameAs(modifiedCustomer, "Should be same reference");
        customer.Id.Should().Be(originalId, "ID must remain consistent");
        customer.Name.Should().StartWith("Updated ", "Name modification should be reflected");
        customer.Code.Should().Be(originalCode, "Code should remain unchanged");
        customer.CreatedAt.Should().Be(originalCreatedAt, "Creation time must remain unchanged");
        customer.UpdatedAt.Should().NotBe(originalUpdatedAt, "Update time should change");
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt.Value, "Update time should be more recent");
    }

    /// <summary>
    /// MD-CUSTOMER-008: Customer entity must handle edge cases for string properties
    /// Critical for data robustness and internationalization support
    /// </summary>
    [Theory(DisplayName = "MD-CUSTOMER-008: Customer Must Handle String Edge Cases")]
    [InlineData("   Padded Customer   ", "P001")]
    [InlineData("Customer\u0000WithNull", "CNUL")]
    [InlineData("🏥 Hospital Emoji 🩺", "HE01")]
    [InlineData("Customer with very long name that exceeds typical database field lengths and contains various special characters including @#$%^&*()[]{}|\\:;\"'<>,.?/~`", "LONGCODE")]
    public void Customer_Should_Handle_String_Edge_Cases(string name, string code)
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Name = name;
        customer.Code = code;

        // Assert
        customer.Name.Should().Be(name, "Should preserve exact string content including edge cases");
        customer.Code.Should().Be(code, "Should preserve exact code content including edge cases");
    }
}
