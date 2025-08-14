using TagManagement.Core.Entities;

namespace TagManagement.UnitTests.Core.Entities;

/// <summary>
/// Medical Device Compliance Tests for Product Entity
/// Tests core domain entity for regulatory compliance
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Core")]
[Trait("Entity", "Product")]
public class ProductTests
{
    /// <summary>
    /// MD-PRODUCT-001: Product entity must initialize with correct default values
    /// Critical for data integrity and product traceability
    /// </summary>
    [Fact(DisplayName = "MD-PRODUCT-001: Product Must Initialize With Default Values")]
    public void Product_Should_Initialize_With_Default_Values()
    {
        // Act
        var product = new Product();

        // Assert
        product.Id.Should().Be(0, "Id should have default value");
        product.Name.Should().Be(string.Empty, "Name should be empty string by default");
        product.ItemText.Should().Be(string.Empty, "ItemText should be empty string by default");
        product.CustomerKeyId.Should().Be(0, "CustomerKeyId should have default value");
        product.StorageType.Should().Be(string.Empty, "StorageType should be empty string by default");
        product.CreatedAt.Should().Be(default(DateTime), "CreatedAt should have default value");
        product.UpdatedAt.Should().BeNull("UpdatedAt should be null by default");
        product.Customer.Should().BeNull("Customer should be null by default");
    }

    /// <summary>
    /// MD-PRODUCT-002: Product properties must accept and return correct values
    /// Critical for data integrity and product management
    /// </summary>
    [Fact(DisplayName = "MD-PRODUCT-002: Product Properties Must Accept And Return Values")]
    public void Product_Properties_Should_Accept_And_Return_Values()
    {
        // Arrange
        var product = new Product();
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow.AddHours(1);
        var customer = new Customer();

        // Act & Assert - Test all properties
        product.Id = 12345;
        product.Id.Should().Be(12345, "Id property should store and return value");

        product.Name = "Surgical Instruments Set";
        product.Name.Should().Be("Surgical Instruments Set", "Name property should store and return value");

        product.ItemText = "Complete sterile surgical instrument set";
        product.ItemText.Should().Be("Complete sterile surgical instrument set", "ItemText property should store and return value");

        product.CustomerKeyId = 555;
        product.CustomerKeyId.Should().Be(555, "CustomerKeyId property should store and return value");

        product.StorageType = "Sterile Storage";
        product.StorageType.Should().Be("Sterile Storage", "StorageType property should store and return value");

        product.CreatedAt = createdDate;
        product.CreatedAt.Should().Be(createdDate, "CreatedAt property should store and return value");

        product.UpdatedAt = updatedDate;
        product.UpdatedAt.Should().Be(updatedDate, "UpdatedAt property should store and return value");

        product.Customer = customer;
        product.Customer.Should().BeSameAs(customer, "Customer property should store and return reference");
    }

    /// <summary>
    /// MD-PRODUCT-003: Product entity must handle string properties correctly
    /// Critical for data validation and product identification
    /// </summary>
    [Theory(DisplayName = "MD-PRODUCT-003: Product Must Handle String Properties")]
    [InlineData("", "", "")]
    [InlineData("Basic Product", "Basic medical device", "Cold Storage")]
    [InlineData("Product with Special Characters !@#$%^&*()", "Item with special chars", "Ambient Storage")]
    [InlineData("Unicode Product ñáméß", "Unicóde item téxt", "Stérile Storagé")]
    [InlineData("Product with\nNewlines\tand\rCarriage Returns", "Item with\nformatting", "Storage\twith\rformatting")]
    public void Product_Should_Handle_String_Properties(string name, string itemText, string storageType)
    {
        // Arrange
        var product = new Product();

        // Act
        product.Name = name;
        product.ItemText = itemText;
        product.StorageType = storageType;

        // Assert
        product.Name.Should().Be(name, "Name should handle various string formats");
        product.ItemText.Should().Be(itemText, "ItemText should handle various string formats");
        product.StorageType.Should().Be(storageType, "StorageType should handle various string formats");
    }

    /// <summary>
    /// MD-PRODUCT-004: Product entity must handle null string values gracefully
    /// Critical for data validation and system stability
    /// </summary>
    [Fact(DisplayName = "MD-PRODUCT-004: Product Must Handle Null String Values")]
    public void Product_Should_Handle_Null_String_Values()
    {
        // Arrange
        var product = new Product();

        // Act & Assert - Setting null values should work
        product.Name = null!;
        product.ItemText = null!;
        product.StorageType = null!;

        product.Name.Should().BeNull("Name should accept null values");
        product.ItemText.Should().BeNull("ItemText should accept null values");
        product.StorageType.Should().BeNull("StorageType should accept null values");
    }

    /// <summary>
    /// MD-PRODUCT-005: Product entity must handle boundary values correctly
    /// Critical for data validation and system stability
    /// </summary>
    [Fact(DisplayName = "MD-PRODUCT-005: Product Must Handle Boundary Values")]
    public void Product_Should_Handle_Boundary_Values()
    {
        // Arrange & Act & Assert - Test boundary values
        var product = new Product();

        // Test minimum values
        product.Id = int.MinValue;
        product.Id.Should().Be(int.MinValue, "Should handle minimum integer value for Id");

        product.CustomerKeyId = int.MinValue;
        product.CustomerKeyId.Should().Be(int.MinValue, "Should handle minimum integer value for CustomerKeyId");

        // Test maximum values
        product.Id = int.MaxValue;
        product.Id.Should().Be(int.MaxValue, "Should handle maximum integer value for Id");

        product.CustomerKeyId = int.MaxValue;
        product.CustomerKeyId.Should().Be(int.MaxValue, "Should handle maximum integer value for CustomerKeyId");

        // Test DateTime boundary values
        product.CreatedAt = DateTime.MinValue;
        product.CreatedAt.Should().Be(DateTime.MinValue, "Should handle minimum DateTime");

        product.CreatedAt = DateTime.MaxValue;
        product.CreatedAt.Should().Be(DateTime.MaxValue, "Should handle maximum DateTime");

        // Test nullable properties
        product.UpdatedAt = null;
        product.UpdatedAt.Should().BeNull("UpdatedAt should accept null values");

        product.UpdatedAt = DateTime.MaxValue;
        product.UpdatedAt.Should().Be(DateTime.MaxValue, "UpdatedAt should accept DateTime values");

        product.Customer = null;
        product.Customer.Should().BeNull("Customer should accept null values");
    }

    /// <summary>
    /// MD-PRODUCT-006: Product entity must support full lifecycle data tracking
    /// Critical for medical device traceability requirements
    /// </summary>
    [Fact(DisplayName = "MD-PRODUCT-006: Product Must Support Full Lifecycle Tracking")]
    public void Product_Should_Support_Full_Lifecycle_Tracking()
    {
        // Arrange
        var product = new Product();
        var baselineTime = DateTime.UtcNow;
        var customer = new Customer { Id = 100, Name = "Test Hospital", Code = "TH001" };

        // Act - Simulate product lifecycle
        
        // 1. Creation
        product.Id = 1001;
        product.Name = "Scalpel Set Alpha";
        product.ItemText = "High precision surgical scalpel set";
        product.CustomerKeyId = 100;
        product.StorageType = "Sterile";
        product.CreatedAt = baselineTime;
        product.Customer = customer;

        // 2. Update product information
        product.Name = "Scalpel Set Alpha - Enhanced";
        product.ItemText = "Enhanced high precision surgical scalpel set with new features";
        product.UpdatedAt = baselineTime.AddDays(30);

        // 3. Storage type change
        product.StorageType = "Ultra-Sterile";
        product.UpdatedAt = baselineTime.AddDays(60);

        // Assert - Verify lifecycle tracking
        product.Id.Should().Be(1001, "Product ID must be preserved throughout lifecycle");
        product.Name.Should().Be("Scalpel Set Alpha - Enhanced", "Name should reflect final update");
        product.ItemText.Should().StartWith("Enhanced", "ItemText should reflect final update");
        product.CustomerKeyId.Should().Be(100, "Customer reference must be preserved");
        product.StorageType.Should().Be("Ultra-Sterile", "StorageType should reflect final update");
        product.CreatedAt.Should().Be(baselineTime, "Creation time must be preserved");
        product.UpdatedAt.Should().Be(baselineTime.AddDays(60), "Update time should reflect last change");
        product.Customer.Should().BeSameAs(customer, "Customer reference should be preserved");
    }

    /// <summary>
    /// MD-PRODUCT-007: Product entity must maintain data consistency across operations
    /// Critical for product identification and inventory management
    /// </summary>
    [Fact(DisplayName = "MD-PRODUCT-007: Product Must Maintain Data Consistency")]
    public void Product_Should_Maintain_Data_Consistency()
    {
        // Arrange
        var customer = new Customer { Id = 200, Name = "Medical Center", Code = "MC001" };
        var product = new Product
        {
            Id = 2001,
            Name = "Consistent Product",
            ItemText = "Product with consistent data",
            CustomerKeyId = 200,
            StorageType = "Standard",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5),
            Customer = customer
        };

        var originalId = product.Id;
        var originalName = product.Name;
        var originalCustomerKeyId = product.CustomerKeyId;
        var originalCreatedAt = product.CreatedAt;
        var originalUpdatedAt = product.UpdatedAt;
        var originalCustomer = product.Customer;

        // Act - Perform various operations
        var modifiedProduct = product; // Simulate passing around the reference
        modifiedProduct.ItemText = "Updated " + modifiedProduct.ItemText;
        modifiedProduct.StorageType = "Enhanced " + modifiedProduct.StorageType;
        modifiedProduct.UpdatedAt = DateTime.UtcNow;

        // Assert - Verify consistency
        product.Should().BeSameAs(modifiedProduct, "Should be same reference");
        product.Id.Should().Be(originalId, "ID must remain consistent");
        product.Name.Should().Be(originalName, "Name should remain unchanged");
        product.CustomerKeyId.Should().Be(originalCustomerKeyId, "CustomerKeyId should remain unchanged");
        product.ItemText.Should().StartWith("Updated ", "ItemText modification should be reflected");
        product.StorageType.Should().StartWith("Enhanced ", "StorageType modification should be reflected");
        product.CreatedAt.Should().Be(originalCreatedAt, "Creation time must remain unchanged");
        product.UpdatedAt.Should().NotBe(originalUpdatedAt, "Update time should change");
        product.UpdatedAt.Should().BeAfter(originalUpdatedAt.Value, "Update time should be more recent");
        product.Customer.Should().BeSameAs(originalCustomer, "Customer reference should remain unchanged");
    }

    /// <summary>
    /// MD-PRODUCT-008: Product entity must handle customer relationship correctly
    /// Critical for product-customer association and business logic
    /// </summary>
    [Fact(DisplayName = "MD-PRODUCT-008: Product Must Handle Customer Relationship")]
    public void Product_Should_Handle_Customer_Relationship()
    {
        // Arrange
        var product = new Product
        {
            Id = 3001,
            Name = "Test Product",
            CustomerKeyId = 300
        };

        var customer1 = new Customer { Id = 300, Name = "Customer One", Code = "C001" };
        var customer2 = new Customer { Id = 301, Name = "Customer Two", Code = "C002" };

        // Act & Assert - Test relationship scenarios

        // 1. Initially no customer
        product.Customer.Should().BeNull("Product should start with no customer reference");

        // 2. Set customer
        product.Customer = customer1;
        product.Customer.Should().BeSameAs(customer1, "Product should reference customer1");
        product.CustomerKeyId.Should().Be(300, "CustomerKeyId should remain consistent");

        // 3. Change customer
        product.Customer = customer2;
        product.Customer.Should().BeSameAs(customer2, "Product should reference customer2");
        product.CustomerKeyId.Should().Be(300, "CustomerKeyId should remain unchanged (business logic)");

        // 4. Clear customer
        product.Customer = null;
        product.Customer.Should().BeNull("Product should allow null customer reference");
        product.CustomerKeyId.Should().Be(300, "CustomerKeyId should remain unchanged");
    }

    /// <summary>
    /// MD-PRODUCT-009: Product entity must handle edge cases for string properties
    /// Critical for data robustness and internationalization support
    /// </summary>
    [Theory(DisplayName = "MD-PRODUCT-009: Product Must Handle String Edge Cases")]
    [InlineData("   Padded Product   ", "   Padded Item   ", "   Padded Storage   ")]
    [InlineData("Product\u0000WithNull", "Item\u0000WithNull", "Storage\u0000WithNull")]
    [InlineData("🔬 Medical Device 🩺", "📋 Device Description 💊", "❄️ Cold Storage 🧊")]
    [InlineData("Product with extremely long name that would exceed typical database field lengths and includes various special characters like @#$%^&*()[]{}|\\:;\"'<>,.?/~`", "Item text with extremely long description that contains detailed information about the medical device and its intended use in clinical settings", "Storage type with very detailed requirements and environmental conditions")]
    public void Product_Should_Handle_String_Edge_Cases(string name, string itemText, string storageType)
    {
        // Arrange
        var product = new Product();

        // Act
        product.Name = name;
        product.ItemText = itemText;
        product.StorageType = storageType;

        // Assert
        product.Name.Should().Be(name, "Should preserve exact name content including edge cases");
        product.ItemText.Should().Be(itemText, "Should preserve exact item text content including edge cases");
        product.StorageType.Should().Be(storageType, "Should preserve exact storage type content including edge cases");
    }

    /// <summary>
    /// MD-PRODUCT-010: Product entity must support complex product configurations
    /// Critical for diverse medical device management scenarios
    /// </summary>
    [Theory(DisplayName = "MD-PRODUCT-010: Product Must Support Complex Configurations")]
    [InlineData(1, "Basic Scalpel", "Single use scalpel", 100, "Sterile")]
    [InlineData(999999, "Advanced Surgical Robot", "Multi-arm robotic surgical system with AI assistance", 500, "Climate Controlled")]
    [InlineData(-1, "Legacy Product", "Legacy medical device", 0, "Deprecated")]
    public void Product_Should_Support_Complex_Configurations(int id, string name, string itemText, int customerKeyId, string storageType)
    {
        // Arrange & Act
        var product = new Product
        {
            Id = id,
            Name = name,
            ItemText = itemText,
            CustomerKeyId = customerKeyId,
            StorageType = storageType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Assert
        product.Id.Should().Be(id, "Product should support various ID values");
        product.Name.Should().Be(name, "Product should support various name formats");
        product.ItemText.Should().Be(itemText, "Product should support various item text lengths");
        product.CustomerKeyId.Should().Be(customerKeyId, "Product should support various customer IDs");
        product.StorageType.Should().Be(storageType, "Product should support various storage types");
        product.CreatedAt.Should().NotBe(default(DateTime), "CreatedAt should be set");
        product.UpdatedAt.Should().NotBeNull("UpdatedAt should be set");
    }
}
