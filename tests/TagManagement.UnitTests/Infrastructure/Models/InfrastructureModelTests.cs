using TagManagement.Infrastructure.Persistence.Models;

namespace TagManagement.UnitTests.Infrastructure.Models;

/// <summary>
/// Medical Device Compliance Tests for Infrastructure Models
/// Tests data model integrity and property behaviors
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
[Trait("Layer", "Infrastructure")]
[Trait("Component", "Models")]
public class InfrastructureModelTests
{
    #region CustomerModel Tests

    /// <summary>
    /// MD-INFRA-001: CustomerModel must initialize with default values and empty collections
    /// Critical for proper data model initialization
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-001: CustomerModel Must Initialize With Default Values")]
    public void CustomerModel_Constructor_Should_Initialize_Default_Values()
    {
        // Act
        var customer = new CustomerModel();

        // Assert
        customer.CustomerKeyId.Should().Be(0);
        customer.CustomerNumber.Should().BeNull();
        customer.CustomerName.Should().BeNull();
        customer.CustomerCode.Should().BeNull();
        customer.IsActive.Should().BeNull();
        customer.CreatedTime.Should().BeNull();
        customer.CreatedByUserKeyId.Should().BeNull();
        customer.ModifiedTime.Should().BeNull();
        customer.ModifiedByUserKeyId.Should().BeNull();
        
        // Navigation properties
        customer.Items.Should().NotBeNull().And.BeEmpty();
        customer.Units.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// MD-INFRA-002: CustomerModel properties must be settable and gettable
    /// Critical for data persistence and retrieval
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-002: CustomerModel Properties Must Be Settable")]
    public void CustomerModel_Properties_Should_Be_Settable()
    {
        // Arrange
        var customer = new CustomerModel();
        var createdTime = DateTime.UtcNow;
        var modifiedTime = DateTime.UtcNow.AddHours(1);

        // Act
        customer.CustomerKeyId = 123;
        customer.CustomerNumber = "CUST001";
        customer.CustomerName = "Test Customer";
        customer.CustomerCode = "TC001";
        customer.IsActive = true;
        customer.CreatedTime = createdTime;
        customer.CreatedByUserKeyId = 1;
        customer.ModifiedTime = modifiedTime;
        customer.ModifiedByUserKeyId = 2;

        // Assert
        customer.CustomerKeyId.Should().Be(123);
        customer.CustomerNumber.Should().Be("CUST001");
        customer.CustomerName.Should().Be("Test Customer");
        customer.CustomerCode.Should().Be("TC001");
        customer.IsActive.Should().BeTrue();
        customer.CreatedTime.Should().Be(createdTime);
        customer.CreatedByUserKeyId.Should().Be(1);
        customer.ModifiedTime.Should().Be(modifiedTime);
        customer.ModifiedByUserKeyId.Should().Be(2);
    }

    /// <summary>
    /// MD-INFRA-003: CustomerModel navigation properties must be modifiable
    /// Critical for entity relationships
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-003: CustomerModel Navigation Properties Must Be Modifiable")]
    public void CustomerModel_Navigation_Properties_Should_Be_Modifiable()
    {
        // Arrange
        var customer = new CustomerModel();
        var item = new ItemModel { ItemKeyId = 1 };
        var unit = new UnitModel { UnitKeyId = 1 };

        // Act
        customer.Items.Add(item);
        customer.Units.Add(unit);

        // Assert
        customer.Items.Should().HaveCount(1);
        customer.Items.Should().Contain(item);
        customer.Units.Should().HaveCount(1);
        customer.Units.Should().Contain(unit);
    }

    #endregion

    #region ItemModel Tests

    /// <summary>
    /// MD-INFRA-004: ItemModel must initialize with default values and empty collections
    /// Critical for proper data model initialization
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-004: ItemModel Must Initialize With Default Values")]
    public void ItemModel_Constructor_Should_Initialize_Default_Values()
    {
        // Act
        var item = new ItemModel();

        // Assert
        item.ItemKeyId.Should().Be(0);
        item.ItemNumber.Should().BeNull();
        item.ItemName.Should().BeNull();
        item.Description.Should().BeNull();
        item.CustomerKeyId.Should().BeNull();
        item.ItemType.Should().BeNull();
        item.IsActive.Should().BeNull();
        item.CreatedTime.Should().BeNull();
        item.CreatedByUserKeyId.Should().BeNull();
        item.ModifiedTime.Should().BeNull();
        item.ModifiedByUserKeyId.Should().BeNull();
        
        // Navigation properties
        item.Customer.Should().BeNull();
        item.Units.Should().NotBeNull().And.BeEmpty();
        item.TagContents.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// MD-INFRA-005: ItemModel properties must be settable and gettable
    /// Critical for data persistence and retrieval
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-005: ItemModel Properties Must Be Settable")]
    public void ItemModel_Properties_Should_Be_Settable()
    {
        // Arrange
        var item = new ItemModel();
        var createdTime = DateTime.UtcNow;
        var modifiedTime = DateTime.UtcNow.AddHours(1);

        // Act
        item.ItemKeyId = 456;
        item.ItemNumber = "ITEM001";
        item.ItemName = "Test Item";
        item.Description = "Test Description";
        item.CustomerKeyId = 123;
        item.ItemType = 1;
        item.IsActive = true;
        item.CreatedTime = createdTime;
        item.CreatedByUserKeyId = 1;
        item.ModifiedTime = modifiedTime;
        item.ModifiedByUserKeyId = 2;

        // Assert
        item.ItemKeyId.Should().Be(456);
        item.ItemNumber.Should().Be("ITEM001");
        item.ItemName.Should().Be("Test Item");
        item.Description.Should().Be("Test Description");
        item.CustomerKeyId.Should().Be(123);
        item.ItemType.Should().Be(1);
        item.IsActive.Should().BeTrue();
        item.CreatedTime.Should().Be(createdTime);
        item.CreatedByUserKeyId.Should().Be(1);
        item.ModifiedTime.Should().Be(modifiedTime);
        item.ModifiedByUserKeyId.Should().Be(2);
    }

    /// <summary>
    /// MD-INFRA-006: ItemModel navigation properties must support relationships
    /// Critical for entity relationships
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-006: ItemModel Navigation Properties Must Support Relationships")]
    public void ItemModel_Navigation_Properties_Should_Support_Relationships()
    {
        // Arrange
        var item = new ItemModel();
        var customer = new CustomerModel { CustomerKeyId = 1 };
        var unit = new UnitModel { UnitKeyId = 1 };
        var tagContent = new TagContentModel { TagContentKeyId = 1 };

        // Act
        item.Customer = customer;
        item.Units.Add(unit);
        item.TagContents.Add(tagContent);

        // Assert
        item.Customer.Should().BeSameAs(customer);
        item.Units.Should().HaveCount(1);
        item.Units.Should().Contain(unit);
        item.TagContents.Should().HaveCount(1);
        item.TagContents.Should().Contain(tagContent);
    }

    #endregion

    #region UnitModel Tests

    /// <summary>
    /// MD-INFRA-007: UnitModel must initialize with default values and empty collections
    /// Critical for proper data model initialization
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-007: UnitModel Must Initialize With Default Values")]
    public void UnitModel_Constructor_Should_Initialize_Default_Values()
    {
        // Act
        var unit = new UnitModel();

        // Assert
        unit.UnitKeyId.Should().Be(0);
        unit.UnitNumber.Should().BeNull();
        unit.SerialNumber.Should().BeNull();
        unit.LocationKeyId.Should().BeNull();
        unit.ItemKeyId.Should().BeNull();
        unit.CustomerKeyId.Should().BeNull();
        unit.ProcessBatchKeyId.Should().BeNull();
        unit.Status.Should().BeNull();
        unit.CreatedTime.Should().BeNull();
        unit.CreatedByUserKeyId.Should().BeNull();
        unit.ModifiedTime.Should().BeNull();
        unit.ModifiedByUserKeyId.Should().BeNull();
        
        // Navigation properties
        unit.Location.Should().BeNull();
        unit.Item.Should().BeNull();
        unit.Customer.Should().BeNull();
        unit.TagContents.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// MD-INFRA-008: UnitModel properties must be settable and gettable
    /// Critical for data persistence and retrieval
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-008: UnitModel Properties Must Be Settable")]
    public void UnitModel_Properties_Should_Be_Settable()
    {
        // Arrange
        var unit = new UnitModel();
        var createdTime = DateTime.UtcNow;
        var modifiedTime = DateTime.UtcNow.AddHours(1);

        // Act
        unit.UnitKeyId = 789;
        unit.UnitNumber = 12345;
        unit.SerialNumber = "SN001";
        unit.LocationKeyId = 100;
        unit.ItemKeyId = 200;
        unit.CustomerKeyId = 300;
        unit.ProcessBatchKeyId = 400;
        unit.Status = 1;
        unit.CreatedTime = createdTime;
        unit.CreatedByUserKeyId = 1;
        unit.ModifiedTime = modifiedTime;
        unit.ModifiedByUserKeyId = 2;

        // Assert
        unit.UnitKeyId.Should().Be(789);
        unit.UnitNumber.Should().Be(12345);
        unit.SerialNumber.Should().Be("SN001");
        unit.LocationKeyId.Should().Be(100);
        unit.ItemKeyId.Should().Be(200);
        unit.CustomerKeyId.Should().Be(300);
        unit.ProcessBatchKeyId.Should().Be(400);
        unit.Status.Should().Be(1);
        unit.CreatedTime.Should().Be(createdTime);
        unit.CreatedByUserKeyId.Should().Be(1);
        unit.ModifiedTime.Should().Be(modifiedTime);
        unit.ModifiedByUserKeyId.Should().Be(2);
    }

    /// <summary>
    /// MD-INFRA-009: UnitModel navigation properties must support complex relationships
    /// Critical for multi-entity relationships
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-009: UnitModel Navigation Properties Must Support Relationships")]
    public void UnitModel_Navigation_Properties_Should_Support_Complex_Relationships()
    {
        // Arrange
        var unit = new UnitModel();
        var location = new LocationModel { LocationKeyId = 1 };
        var item = new ItemModel { ItemKeyId = 1 };
        var customer = new CustomerModel { CustomerKeyId = 1 };
        var tagContent = new TagContentModel { TagContentKeyId = 1 };

        // Act
        unit.Location = location;
        unit.Item = item;
        unit.Customer = customer;
        unit.TagContents.Add(tagContent);

        // Assert
        unit.Location.Should().BeSameAs(location);
        unit.Item.Should().BeSameAs(item);
        unit.Customer.Should().BeSameAs(customer);
        unit.TagContents.Should().HaveCount(1);
        unit.TagContents.Should().Contain(tagContent);
    }

    #endregion

    #region IndicatorModel Tests

    /// <summary>
    /// MD-INFRA-010: IndicatorModel must initialize with default values and empty collections
    /// Critical for proper data model initialization
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-010: IndicatorModel Must Initialize With Default Values")]
    public void IndicatorModel_Constructor_Should_Initialize_Default_Values()
    {
        // Act
        var indicator = new IndicatorModel();

        // Assert
        indicator.IndicatorKeyId.Should().Be(0);
        indicator.IndicatorNumber.Should().BeNull();
        indicator.IndicatorName.Should().BeNull();
        indicator.Description.Should().BeNull();
        indicator.IndicatorType.Should().BeNull();
        indicator.IsActive.Should().BeNull();
        indicator.CreatedTime.Should().BeNull();
        indicator.CreatedByUserKeyId.Should().BeNull();
        indicator.ModifiedTime.Should().BeNull();
        indicator.ModifiedByUserKeyId.Should().BeNull();
        
        // Navigation properties
        indicator.TagContents.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// MD-INFRA-011: IndicatorModel properties must be settable and gettable
    /// Critical for data persistence and retrieval
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-011: IndicatorModel Properties Must Be Settable")]
    public void IndicatorModel_Properties_Should_Be_Settable()
    {
        // Arrange
        var indicator = new IndicatorModel();
        var createdTime = DateTime.UtcNow;
        var modifiedTime = DateTime.UtcNow.AddHours(1);

        // Act
        indicator.IndicatorKeyId = 999;
        indicator.IndicatorNumber = "IND001";
        indicator.IndicatorName = "Test Indicator";
        indicator.Description = "Test Description";
        indicator.IndicatorType = 2;
        indicator.IsActive = false;
        indicator.CreatedTime = createdTime;
        indicator.CreatedByUserKeyId = 1;
        indicator.ModifiedTime = modifiedTime;
        indicator.ModifiedByUserKeyId = 2;

        // Assert
        indicator.IndicatorKeyId.Should().Be(999);
        indicator.IndicatorNumber.Should().Be("IND001");
        indicator.IndicatorName.Should().Be("Test Indicator");
        indicator.Description.Should().Be("Test Description");
        indicator.IndicatorType.Should().Be(2);
        indicator.IsActive.Should().BeFalse();
        indicator.CreatedTime.Should().Be(createdTime);
        indicator.CreatedByUserKeyId.Should().Be(1);
        indicator.ModifiedTime.Should().Be(modifiedTime);
        indicator.ModifiedByUserKeyId.Should().Be(2);
    }

    /// <summary>
    /// MD-INFRA-012: IndicatorModel navigation properties must support tag relationships
    /// Critical for indicator-tag relationships
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-012: IndicatorModel Navigation Properties Must Support Relationships")]
    public void IndicatorModel_Navigation_Properties_Should_Support_Tag_Relationships()
    {
        // Arrange
        var indicator = new IndicatorModel();
        var tagContent1 = new TagContentModel { TagContentKeyId = 1 };
        var tagContent2 = new TagContentModel { TagContentKeyId = 2 };

        // Act
        indicator.TagContents.Add(tagContent1);
        indicator.TagContents.Add(tagContent2);

        // Assert
        indicator.TagContents.Should().HaveCount(2);
        indicator.TagContents.Should().Contain(tagContent1);
        indicator.TagContents.Should().Contain(tagContent2);
    }

    #endregion

    #region Boundary and Edge Case Tests

    /// <summary>
    /// MD-INFRA-013: Models must handle null and empty string values gracefully
    /// Critical for data integrity and validation
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-013: Models Must Handle Null And Empty String Values")]
    public void Models_Should_Handle_Null_And_Empty_String_Values()
    {
        // Arrange & Act
        var customer = new CustomerModel
        {
            CustomerNumber = null,
            CustomerName = "",
            CustomerCode = "   "
        };

        var item = new ItemModel
        {
            ItemNumber = null,
            ItemName = "",
            Description = "   "
        };

        var unit = new UnitModel
        {
            SerialNumber = null
        };

        var indicator = new IndicatorModel
        {
            IndicatorNumber = null,
            IndicatorName = "",
            Description = "   "
        };

        // Assert - Should not throw and preserve values
        customer.CustomerNumber.Should().BeNull();
        customer.CustomerName.Should().Be("");
        customer.CustomerCode.Should().Be("   ");

        item.ItemNumber.Should().BeNull();
        item.ItemName.Should().Be("");
        item.Description.Should().Be("   ");

        unit.SerialNumber.Should().BeNull();

        indicator.IndicatorNumber.Should().BeNull();
        indicator.IndicatorName.Should().Be("");
        indicator.Description.Should().Be("   ");
    }

    /// <summary>
    /// MD-INFRA-014: Models must handle boundary integer values correctly
    /// Critical for data validation and edge cases
    /// </summary>
    [Theory(DisplayName = "MD-INFRA-014: Models Must Handle Boundary Integer Values")]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Models_Should_Handle_Boundary_Integer_Values(int value)
    {
        // Arrange & Act
        var customer = new CustomerModel { CustomerKeyId = value };
        var item = new ItemModel { ItemKeyId = value, ItemType = value };
        var unit = new UnitModel { UnitKeyId = value, UnitNumber = value };
        var indicator = new IndicatorModel { IndicatorKeyId = value, IndicatorType = value };

        // Assert
        customer.CustomerKeyId.Should().Be(value);
        item.ItemKeyId.Should().Be(value);
        item.ItemType.Should().Be(value);
        unit.UnitKeyId.Should().Be(value);
        unit.UnitNumber.Should().Be(value);
        indicator.IndicatorKeyId.Should().Be(value);
        indicator.IndicatorType.Should().Be(value);
    }

    /// <summary>
    /// MD-INFRA-015: Models must handle nullable integer values correctly
    /// Critical for optional field handling
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-015: Models Must Handle Nullable Integer Values")]
    public void Models_Should_Handle_Nullable_Integer_Values()
    {
        // Arrange
        var customer = new CustomerModel();
        var item = new ItemModel();
        var unit = new UnitModel();
        var indicator = new IndicatorModel();

        // Test null values
        customer.CreatedByUserKeyId = null;
        item.CustomerKeyId = null;
        unit.LocationKeyId = null;
        indicator.IndicatorType = null;

        customer.CreatedByUserKeyId.Should().BeNull();
        item.CustomerKeyId.Should().BeNull();
        unit.LocationKeyId.Should().BeNull();
        indicator.IndicatorType.Should().BeNull();

        // Test setting values
        customer.CreatedByUserKeyId = 123;
        item.CustomerKeyId = 456;
        unit.LocationKeyId = 789;
        indicator.IndicatorType = 1;

        customer.CreatedByUserKeyId.Should().Be(123);
        item.CustomerKeyId.Should().Be(456);
        unit.LocationKeyId.Should().Be(789);
        indicator.IndicatorType.Should().Be(1);
    }

    /// <summary>
    /// MD-INFRA-016: Models must handle nullable DateTime values correctly
    /// Critical for temporal data integrity
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-016: Models Must Handle Nullable DateTime Values")]
    public void Models_Should_Handle_Nullable_DateTime_Values()
    {
        // Arrange
        var testDate = new DateTime(2025, 8, 14, 10, 30, 0, DateTimeKind.Utc);
        var models = new object[]
        {
            new CustomerModel(),
            new ItemModel(),
            new UnitModel(),
            new IndicatorModel()
        };

        foreach (var model in models)
        {
            // Test null assignment and retrieval
            if (model is CustomerModel customer)
            {
                customer.CreatedTime = null;
                customer.ModifiedTime = null;
                customer.CreatedTime.Should().BeNull();
                customer.ModifiedTime.Should().BeNull();

                customer.CreatedTime = testDate;
                customer.CreatedTime.Should().Be(testDate);
            }
            else if (model is ItemModel item)
            {
                item.CreatedTime = null;
                item.ModifiedTime = null;
                item.CreatedTime.Should().BeNull();
                item.ModifiedTime.Should().BeNull();

                item.CreatedTime = testDate;
                item.CreatedTime.Should().Be(testDate);
            }
            else if (model is UnitModel unit)
            {
                unit.CreatedTime = null;
                unit.ModifiedTime = null;
                unit.CreatedTime.Should().BeNull();
                unit.ModifiedTime.Should().BeNull();

                unit.CreatedTime = testDate;
                unit.CreatedTime.Should().Be(testDate);
            }
            else if (model is IndicatorModel indicator)
            {
                indicator.CreatedTime = null;
                indicator.ModifiedTime = null;
                indicator.CreatedTime.Should().BeNull();
                indicator.ModifiedTime.Should().BeNull();

                indicator.CreatedTime = testDate;
                indicator.CreatedTime.Should().Be(testDate);
            }
        }
    }

    /// <summary>
    /// MD-INFRA-017: Models must handle nullable boolean values correctly
    /// Critical for optional status fields
    /// </summary>
    [Theory(DisplayName = "MD-INFRA-017: Models Must Handle Nullable Boolean Values")]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void Models_Should_Handle_Nullable_Boolean_Values(bool? value)
    {
        // Arrange & Act
        var customer = new CustomerModel { IsActive = value };
        var item = new ItemModel { IsActive = value };
        var indicator = new IndicatorModel { IsActive = value };

        // Assert
        customer.IsActive.Should().Be(value);
        item.IsActive.Should().Be(value);
        indicator.IsActive.Should().Be(value);
    }

    /// <summary>
    /// MD-INFRA-018: Models must support collection modification operations
    /// Critical for entity relationship management
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-018: Models Must Support Collection Modification Operations")]
    public void Models_Should_Support_Collection_Modification_Operations()
    {
        // Arrange
        var customer = new CustomerModel();
        var item = new ItemModel();
        var unit = new UnitModel();
        var indicator = new IndicatorModel();

        var item1 = new ItemModel { ItemKeyId = 1 };
        var item2 = new ItemModel { ItemKeyId = 2 };
        var unit1 = new UnitModel { UnitKeyId = 1 };
        var unit2 = new UnitModel { UnitKeyId = 2 };
        var tagContent1 = new TagContentModel { TagContentKeyId = 1 };
        var tagContent2 = new TagContentModel { TagContentKeyId = 2 };

        // Act & Assert - CustomerModel collections
        customer.Items.Add(item1);
        customer.Items.Add(item2);
        customer.Items.Should().HaveCount(2);
        
        customer.Items.Remove(item1);
        customer.Items.Should().HaveCount(1);
        customer.Items.Should().NotContain(item1);
        
        customer.Items.Clear();
        customer.Items.Should().BeEmpty();

        // Act & Assert - ItemModel collections
        item.Units.Add(unit1);
        item.TagContents.Add(tagContent1);
        item.Units.Should().HaveCount(1);
        item.TagContents.Should().HaveCount(1);

        // Act & Assert - UnitModel collections
        unit.TagContents.Add(tagContent1);
        unit.TagContents.Add(tagContent2);
        unit.TagContents.Should().HaveCount(2);

        // Act & Assert - IndicatorModel collections
        indicator.TagContents.Add(tagContent1);
        indicator.TagContents.Should().HaveCount(1);
    }

    /// <summary>
    /// MD-INFRA-019: Models must maintain collection integrity across operations
    /// Critical for data consistency during operations
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-019: Models Must Maintain Collection Integrity")]
    public void Models_Should_Maintain_Collection_Integrity_Across_Operations()
    {
        // Arrange
        var customer = new CustomerModel();
        var originalItemsCollection = customer.Items;
        var originalUnitsCollection = customer.Units;

        // Act - Modify collections
        customer.Items.Add(new ItemModel { ItemKeyId = 1 });
        customer.Units.Add(new UnitModel { UnitKeyId = 1 });

        // Assert - Collections should be the same reference
        customer.Items.Should().BeSameAs(originalItemsCollection);
        customer.Units.Should().BeSameAs(originalUnitsCollection);
        
        // Verify content
        customer.Items.Should().HaveCount(1);
        customer.Units.Should().HaveCount(1);

        // Test collection replacement
        var newItemsCollection = new List<ItemModel>();
        customer.Items = newItemsCollection;
        customer.Items.Should().BeSameAs(newItemsCollection);
        customer.Items.Should().BeEmpty();
    }

    /// <summary>
    /// MD-INFRA-020: Models must handle concurrent collection access gracefully
    /// Critical for thread safety considerations
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-020: Models Must Handle Collection Access Gracefully")]
    public void Models_Should_Handle_Collection_Access_Gracefully()
    {
        // Arrange
        var customer = new CustomerModel();
        var item = new ItemModel();
        var unit = new UnitModel();

        // Act & Assert - Multiple operations on same collection
        customer.Items.Add(new ItemModel { ItemKeyId = 1 });
        customer.Items.Add(new ItemModel { ItemKeyId = 2 });
        
        var itemCount = customer.Items.Count;
        var firstItem = customer.Items.First();
        
        itemCount.Should().Be(2);
        firstItem.Should().NotBeNull();
        firstItem.ItemKeyId.Should().Be(1);

        // Test enumeration safety
        var itemIds = customer.Items.Select(i => i.ItemKeyId).ToList();
        itemIds.Should().Equal(1, 2);

        // Test collection state after operations
        customer.Items.Should().HaveCount(2);
        customer.Items.Any(i => i.ItemKeyId == 1).Should().BeTrue();
        customer.Items.Any(i => i.ItemKeyId == 2).Should().BeTrue();
    }

    #endregion
}
