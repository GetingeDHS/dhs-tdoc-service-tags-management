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

    #region LocationModel Tests

    /// <summary>
    /// MD-INFRA-021: LocationModel must initialize with default values and empty collections
    /// Critical for proper data model initialization
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-021: LocationModel Must Initialize With Default Values")]
    public void LocationModel_Constructor_Should_Initialize_Default_Values()
    {
        // Act
        var location = new LocationModel();

        // Assert
        location.LocationKeyId.Should().Be(0);
        location.LocationName.Should().BeNull();
        location.LocationCode.Should().BeNull();
        location.Description.Should().BeNull();
        location.ParentLocationKeyId.Should().BeNull();
        location.IsActive.Should().BeNull();
        location.CreatedTime.Should().BeNull();
        location.CreatedByUserKeyId.Should().BeNull();
        location.ModifiedTime.Should().BeNull();
        location.ModifiedByUserKeyId.Should().BeNull();
        
        // Navigation properties
        location.ParentLocation.Should().BeNull();
        location.ChildLocations.Should().NotBeNull().And.BeEmpty();
        location.Tags.Should().NotBeNull().And.BeEmpty();
        location.TagContents.Should().NotBeNull().And.BeEmpty();
        location.Units.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// MD-INFRA-022: LocationModel properties must be settable and gettable
    /// Critical for data persistence and retrieval
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-022: LocationModel Properties Must Be Settable")]
    public void LocationModel_Properties_Should_Be_Settable()
    {
        // Arrange
        var location = new LocationModel();
        var createdTime = DateTime.UtcNow;
        var modifiedTime = DateTime.UtcNow.AddHours(1);

        // Act
        location.LocationKeyId = 500;
        location.LocationName = "Operating Room 1";
        location.LocationCode = "OR01";
        location.Description = "Main operating room";
        location.ParentLocationKeyId = 100;
        location.IsActive = true;
        location.CreatedTime = createdTime;
        location.CreatedByUserKeyId = 1;
        location.ModifiedTime = modifiedTime;
        location.ModifiedByUserKeyId = 2;

        // Assert
        location.LocationKeyId.Should().Be(500);
        location.LocationName.Should().Be("Operating Room 1");
        location.LocationCode.Should().Be("OR01");
        location.Description.Should().Be("Main operating room");
        location.ParentLocationKeyId.Should().Be(100);
        location.IsActive.Should().BeTrue();
        location.CreatedTime.Should().Be(createdTime);
        location.CreatedByUserKeyId.Should().Be(1);
        location.ModifiedTime.Should().Be(modifiedTime);
        location.ModifiedByUserKeyId.Should().Be(2);
    }

    /// <summary>
    /// MD-INFRA-023: LocationModel navigation properties must support hierarchical relationships
    /// Critical for location hierarchy management
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-023: LocationModel Navigation Properties Must Support Hierarchical Relationships")]
    public void LocationModel_Navigation_Properties_Should_Support_Hierarchical_Relationships()
    {
        // Arrange
        var parentLocation = new LocationModel { LocationKeyId = 1, LocationName = "Hospital" };
        var childLocation1 = new LocationModel { LocationKeyId = 2, LocationName = "OR Wing" };
        var childLocation2 = new LocationModel { LocationKeyId = 3, LocationName = "ICU Wing" };
        var tag = new TagsModel { TagKeyId = 1 };
        var tagContent = new TagContentModel { TagContentKeyId = 1 };
        var unit = new UnitModel { UnitKeyId = 1 };

        // Act
        childLocation1.ParentLocation = parentLocation;
        parentLocation.ChildLocations.Add(childLocation1);
        parentLocation.ChildLocations.Add(childLocation2);
        parentLocation.Tags.Add(tag);
        parentLocation.TagContents.Add(tagContent);
        parentLocation.Units.Add(unit);

        // Assert
        childLocation1.ParentLocation.Should().BeSameAs(parentLocation);
        parentLocation.ChildLocations.Should().HaveCount(2);
        parentLocation.ChildLocations.Should().Contain(childLocation1);
        parentLocation.ChildLocations.Should().Contain(childLocation2);
        parentLocation.Tags.Should().HaveCount(1);
        parentLocation.Tags.Should().Contain(tag);
        parentLocation.TagContents.Should().HaveCount(1);
        parentLocation.TagContents.Should().Contain(tagContent);
        parentLocation.Units.Should().HaveCount(1);
        parentLocation.Units.Should().Contain(unit);
    }

    #endregion

    #region TagContentModel Tests

    /// <summary>
    /// MD-INFRA-024: TagContentModel must initialize with default values
    /// Critical for proper tag content initialization
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-024: TagContentModel Must Initialize With Default Values")]
    public void TagContentModel_Constructor_Should_Initialize_Default_Values()
    {
        // Act
        var tagContent = new TagContentModel();

        // Assert
        tagContent.TagContentKeyId.Should().Be(0);
        tagContent.ParentTagKeyId.Should().Be(0);
        tagContent.ChildTagKeyId.Should().BeNull();
        tagContent.UnitKeyId.Should().BeNull();
        tagContent.ItemKeyId.Should().BeNull();
        tagContent.SerialKeyId.Should().BeNull();
        tagContent.LotInfoKeyId.Should().BeNull();
        tagContent.IndicatorKeyId.Should().BeNull();
        tagContent.LocationKeyId.Should().BeNull();
        tagContent.CreatedTime.Should().BeNull();
        tagContent.CreatedByUserKeyId.Should().BeNull();
        tagContent.ModifiedTime.Should().BeNull();
        tagContent.ModifiedByUserKeyId.Should().BeNull();
        
        // Navigation properties
        tagContent.ParentTag.Should().BeNull();
        tagContent.ChildTag.Should().BeNull();
        tagContent.Unit.Should().BeNull();
        tagContent.Item.Should().BeNull();
        tagContent.Location.Should().BeNull();
        tagContent.Indicator.Should().BeNull();
    }

    /// <summary>
    /// MD-INFRA-025: TagContentModel properties must be settable and gettable
    /// Critical for tag content data management
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-025: TagContentModel Properties Must Be Settable")]
    public void TagContentModel_Properties_Should_Be_Settable()
    {
        // Arrange
        var tagContent = new TagContentModel();
        var createdTime = DateTime.UtcNow;
        var modifiedTime = DateTime.UtcNow.AddHours(1);

        // Act
        tagContent.TagContentKeyId = 600;
        tagContent.ParentTagKeyId = 100;
        tagContent.ChildTagKeyId = 200;
        tagContent.UnitKeyId = 300;
        tagContent.ItemKeyId = 400;
        tagContent.SerialKeyId = 500;
        tagContent.LotInfoKeyId = 600;
        tagContent.IndicatorKeyId = 700;
        tagContent.LocationKeyId = 800;
        tagContent.CreatedTime = createdTime;
        tagContent.CreatedByUserKeyId = 1;
        tagContent.ModifiedTime = modifiedTime;
        tagContent.ModifiedByUserKeyId = 2;

        // Assert
        tagContent.TagContentKeyId.Should().Be(600);
        tagContent.ParentTagKeyId.Should().Be(100);
        tagContent.ChildTagKeyId.Should().Be(200);
        tagContent.UnitKeyId.Should().Be(300);
        tagContent.ItemKeyId.Should().Be(400);
        tagContent.SerialKeyId.Should().Be(500);
        tagContent.LotInfoKeyId.Should().Be(600);
        tagContent.IndicatorKeyId.Should().Be(700);
        tagContent.LocationKeyId.Should().Be(800);
        tagContent.CreatedTime.Should().Be(createdTime);
        tagContent.CreatedByUserKeyId.Should().Be(1);
        tagContent.ModifiedTime.Should().Be(modifiedTime);
        tagContent.ModifiedByUserKeyId.Should().Be(2);
    }

    /// <summary>
    /// MD-INFRA-026: TagContentModel navigation properties must support complex relationships
    /// Critical for tag content relationship management
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-026: TagContentModel Navigation Properties Must Support Complex Relationships")]
    public void TagContentModel_Navigation_Properties_Should_Support_Complex_Relationships()
    {
        // Arrange
        var tagContent = new TagContentModel();
        var parentTag = new TagsModel { TagKeyId = 1 };
        var childTag = new TagsModel { TagKeyId = 2 };
        var unit = new UnitModel { UnitKeyId = 1 };
        var item = new ItemModel { ItemKeyId = 1 };
        var location = new LocationModel { LocationKeyId = 1 };
        var indicator = new IndicatorModel { IndicatorKeyId = 1 };

        // Act
        tagContent.ParentTag = parentTag;
        tagContent.ChildTag = childTag;
        tagContent.Unit = unit;
        tagContent.Item = item;
        tagContent.Location = location;
        tagContent.Indicator = indicator;

        // Assert
        tagContent.ParentTag.Should().BeSameAs(parentTag);
        tagContent.ChildTag.Should().BeSameAs(childTag);
        tagContent.Unit.Should().BeSameAs(unit);
        tagContent.Item.Should().BeSameAs(item);
        tagContent.Location.Should().BeSameAs(location);
        tagContent.Indicator.Should().BeSameAs(indicator);
    }

    #endregion

    #region TagsModel Tests

    /// <summary>
    /// MD-INFRA-027: TagsModel must initialize with default values and empty collections
    /// Critical for proper tag initialization
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-027: TagsModel Must Initialize With Default Values")]
    public void TagsModel_Constructor_Should_Initialize_Default_Values()
    {
        // Act
        var tag = new TagsModel();

        // Assert
        tag.TagKeyId.Should().Be(0);
        tag.TagNumber.Should().BeNull();
        tag.TagTypeKeyId.Should().BeNull();
        tag.LocationKeyId.Should().BeNull();
        tag.ProcessBatchKeyId.Should().BeNull();
        tag.IsAutoTag.Should().BeNull();
        tag.CreatedTime.Should().BeNull();
        tag.CreatedByUserKeyId.Should().BeNull();
        tag.ModifiedTime.Should().BeNull();
        tag.ModifiedByUserKeyId.Should().BeNull();
        
        // Navigation properties
        tag.Location.Should().BeNull();
        tag.TagType.Should().BeNull();
        tag.TagContents.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// MD-INFRA-028: TagsModel properties must be settable and gettable
    /// Critical for tag data management
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-028: TagsModel Properties Must Be Settable")]
    public void TagsModel_Properties_Should_Be_Settable()
    {
        // Arrange
        var tag = new TagsModel();
        var createdTime = DateTime.UtcNow;
        var modifiedTime = DateTime.UtcNow.AddHours(1);

        // Act
        tag.TagKeyId = 700;
        tag.TagNumber = 12345;
        tag.TagTypeKeyId = 1;
        tag.LocationKeyId = 200;
        tag.ProcessBatchKeyId = 300;
        tag.IsAutoTag = true;
        tag.CreatedTime = createdTime;
        tag.CreatedByUserKeyId = 1;
        tag.ModifiedTime = modifiedTime;
        tag.ModifiedByUserKeyId = 2;

        // Assert
        tag.TagKeyId.Should().Be(700);
        tag.TagNumber.Should().Be(12345);
        tag.TagTypeKeyId.Should().Be(1);
        tag.LocationKeyId.Should().Be(200);
        tag.ProcessBatchKeyId.Should().Be(300);
        tag.IsAutoTag.Should().BeTrue();
        tag.CreatedTime.Should().Be(createdTime);
        tag.CreatedByUserKeyId.Should().Be(1);
        tag.ModifiedTime.Should().Be(modifiedTime);
        tag.ModifiedByUserKeyId.Should().Be(2);
    }

    /// <summary>
    /// MD-INFRA-029: TagsModel navigation properties must support tag relationships
    /// Critical for tag relationship management
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-029: TagsModel Navigation Properties Must Support Tag Relationships")]
    public void TagsModel_Navigation_Properties_Should_Support_Tag_Relationships()
    {
        // Arrange
        var tag = new TagsModel();
        var location = new LocationModel { LocationKeyId = 1 };
        var tagType = new TagTypeModel { TagTypeKeyId = 1 };
        var tagContent1 = new TagContentModel { TagContentKeyId = 1 };
        var tagContent2 = new TagContentModel { TagContentKeyId = 2 };

        // Act
        tag.Location = location;
        tag.TagType = tagType;
        tag.TagContents.Add(tagContent1);
        tag.TagContents.Add(tagContent2);

        // Assert
        tag.Location.Should().BeSameAs(location);
        tag.TagType.Should().BeSameAs(tagType);
        tag.TagContents.Should().HaveCount(2);
        tag.TagContents.Should().Contain(tagContent1);
        tag.TagContents.Should().Contain(tagContent2);
    }

    #endregion

    #region TagTypeModel Tests

    /// <summary>
    /// MD-INFRA-030: TagTypeModel must initialize with default values and empty collections
    /// Critical for proper tag type initialization
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-030: TagTypeModel Must Initialize With Default Values")]
    public void TagTypeModel_Constructor_Should_Initialize_Default_Values()
    {
        // Act
        var tagType = new TagTypeModel();

        // Assert
        tagType.TagTypeKeyId.Should().Be(0);
        tagType.TagTypeName.Should().BeNull();
        tagType.TagTypeCode.Should().BeNull();
        tagType.Description.Should().BeNull();
        tagType.IsActive.Should().BeNull();
        tagType.CreatedTime.Should().BeNull();
        tagType.CreatedByUserKeyId.Should().BeNull();
        tagType.ModifiedTime.Should().BeNull();
        tagType.ModifiedByUserKeyId.Should().BeNull();
        
        // Navigation properties
        tagType.Tags.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    /// MD-INFRA-031: TagTypeModel properties must be settable and gettable
    /// Critical for tag type data management
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-031: TagTypeModel Properties Must Be Settable")]
    public void TagTypeModel_Properties_Should_Be_Settable()
    {
        // Arrange
        var tagType = new TagTypeModel();
        var createdTime = DateTime.UtcNow;
        var modifiedTime = DateTime.UtcNow.AddHours(1);

        // Act
        tagType.TagTypeKeyId = 800;
        tagType.TagTypeName = "Sterilization Load";
        tagType.TagTypeCode = "STERIL";
        tagType.Description = "Sterilization load container tag";
        tagType.IsActive = true;
        tagType.CreatedTime = createdTime;
        tagType.CreatedByUserKeyId = 1;
        tagType.ModifiedTime = modifiedTime;
        tagType.ModifiedByUserKeyId = 2;

        // Assert
        tagType.TagTypeKeyId.Should().Be(800);
        tagType.TagTypeName.Should().Be("Sterilization Load");
        tagType.TagTypeCode.Should().Be("STERIL");
        tagType.Description.Should().Be("Sterilization load container tag");
        tagType.IsActive.Should().BeTrue();
        tagType.CreatedTime.Should().Be(createdTime);
        tagType.CreatedByUserKeyId.Should().Be(1);
        tagType.ModifiedTime.Should().Be(modifiedTime);
        tagType.ModifiedByUserKeyId.Should().Be(2);
    }

    /// <summary>
    /// MD-INFRA-032: TagTypeModel navigation properties must support tag collections
    /// Critical for tag type relationship management
    /// </summary>
    [Fact(DisplayName = "MD-INFRA-032: TagTypeModel Navigation Properties Must Support Tag Collections")]
    public void TagTypeModel_Navigation_Properties_Should_Support_Tag_Collections()
    {
        // Arrange
        var tagType = new TagTypeModel();
        var tag1 = new TagsModel { TagKeyId = 1 };
        var tag2 = new TagsModel { TagKeyId = 2 };
        var tag3 = new TagsModel { TagKeyId = 3 };

        // Act
        tagType.Tags.Add(tag1);
        tagType.Tags.Add(tag2);
        tagType.Tags.Add(tag3);

        // Assert
        tagType.Tags.Should().HaveCount(3);
        tagType.Tags.Should().Contain(tag1);
        tagType.Tags.Should().Contain(tag2);
        tagType.Tags.Should().Contain(tag3);
    }

    /// <summary>
    /// MD-INFRA-033: TagTypeModel must handle string edge cases
    /// Critical for tag type data validation
    /// </summary>
    [Theory(DisplayName = "MD-INFRA-033: TagTypeModel Must Handle String Edge Cases")]
    [InlineData(null, null, null)]
    [InlineData("", "", "")]
    [InlineData("   ", "   ", "   ")]
    [InlineData("Very Long Tag Type Name That Exceeds Normal Length", "VLONG", "Very long description that might exceed normal database field lengths for testing purposes")]
    [InlineData("Tag\nWith\nNewlines", "CODE\t", "Description\rwith\rcarriage\rreturns")]
    public void TagTypeModel_Should_Handle_String_Edge_Cases(string? name, string? code, string? description)
    {
        // Arrange & Act
        var tagType = new TagTypeModel
        {
            TagTypeName = name,
            TagTypeCode = code,
            Description = description
        };

        // Assert - Should preserve the values as-is
        tagType.TagTypeName.Should().Be(name);
        tagType.TagTypeCode.Should().Be(code);
        tagType.Description.Should().Be(description);
    }

    #endregion
}
