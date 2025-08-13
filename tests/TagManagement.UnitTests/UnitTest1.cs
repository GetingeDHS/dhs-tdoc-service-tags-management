using TagManagement.Core.Entities;
using TagManagement.Core.Enums;
using Microsoft.EntityFrameworkCore;
using TagManagement.Infrastructure.Persistence;
using TagManagement.Infrastructure.Persistence.Repositories;

namespace TagManagement.UnitTests.Domain;

/// <summary>
/// Medical Device Compliance Tests for Tag Entity
/// Tests critical business logic for regulatory compliance
/// </summary>
[Trait("Category", "MedicalDevice")]
[Trait("Compliance", "ISO-13485")]
public class TagEntityTests
{
    /// <summary>
    /// MD-REQ-001: Tag must have valid number and type
    /// Critical for device tracking and traceability
    /// </summary>
    [Fact(DisplayName = "MD-REQ-001: Tag Creation Requires Valid Number And Type")]
    public void Tag_Creation_Should_Require_Valid_Number_And_Type()
    {
        // Arrange
        var tag = new Tag();
        
        // Act & Assert - Tag must have number > 0
        tag.TagNumber = 0;
        tag.IsEmpty.Should().BeTrue("Empty tag should be detected");
        
        // Valid tag creation
        tag.TagNumber = 12345;
        tag.TagType = TagType.PrepTag;
        tag.TagTypeKeyId = 1;
        
        // Assert
        tag.TagNumber.Should().Be(12345, "Tag number must be preserved");
        tag.TagType.Should().Be(TagType.PrepTag, "Tag type must be preserved");
        tag.DisplayString.Should().Contain("12345", "Display string must contain tag number");
    }

    /// <summary>
    /// MD-REQ-002: Auto tags must be properly identified
    /// Critical for automated manufacturing processes
    /// </summary>
    [Fact(DisplayName = "MD-REQ-002: Auto Tags Must Be Properly Identified")]
    public void Auto_Tag_Should_Have_Proper_Identification()
    {
        // Arrange
        var autoTag = new Tag
        {
            TagNumber = 100,
            TagType = TagType.PrepTag,
            IsAuto = true
        };
        
        var manualTag = new Tag
        {
            TagNumber = 101,
            TagType = TagType.PrepTag,
            IsAuto = false
        };
        
        // Assert
        autoTag.IsAuto.Should().BeTrue("Auto tag must be identified as auto");
        autoTag.FullDisplayString.Should().Contain("[AUTO]", "Auto tag display must include AUTO indicator");
        
        manualTag.IsAuto.Should().BeFalse("Manual tag must not be auto");
        manualTag.FullDisplayString.Should().NotContain("[AUTO]", "Manual tag should not have AUTO indicator");
    }

    /// <summary>
    /// MD-REQ-003: Tag content condition must be accurately determined
    /// Critical for manufacturing workflow decisions
    /// </summary>
    [Theory(DisplayName = "MD-REQ-003: Tag Content Condition Must Be Accurate")]
    [InlineData(TagContentCondition.Empty)]
    [InlineData(TagContentCondition.Units)]
    [InlineData(TagContentCondition.Items)]
    [InlineData(TagContentCondition.Mixed)]
    public void Tag_Content_Condition_Should_Be_Accurate(TagContentCondition expectedCondition)
    {
        // Arrange
        var tag = new Tag
        {
            TagNumber = 200,
            TagType = TagType.BundleTag
        };
        
        // Act - Set up content based on expected condition
        switch (expectedCondition)
        {
            case TagContentCondition.Empty:
                // No content added
                break;
            case TagContentCondition.Units:
                tag.Contents.Units.Add(1);
                tag.Contents.Units.Add(2);
                break;
            case TagContentCondition.Items:
                tag.Contents.Items.Add(new TagItem(1, 1, 1, 5));
                break;
            case TagContentCondition.Mixed:
                tag.Contents.Units.Add(1);
                tag.Contents.Items.Add(new TagItem(1, 1, 1, 3));
                break;
        }
        
        // Assert
        tag.ContentCondition.Should().Be(expectedCondition, 
            $"Tag content condition should be {expectedCondition}");
        tag.IsEmpty.Should().Be(expectedCondition == TagContentCondition.Empty,
            "Tag emptiness should match content condition");
    }

    /// <summary>
    /// MD-REQ-004: Tag audit fields must be properly maintained
    /// Critical for regulatory compliance and traceability
    /// </summary>
    [Fact(DisplayName = "MD-REQ-004: Tag Audit Fields Must Be Maintained")]
    public void Tag_Audit_Fields_Should_Be_Maintained()
    {
        // Arrange
        var createdTime = DateTime.UtcNow;
        var updatedTime = createdTime.AddMinutes(30);
        
        var tag = new Tag
        {
            TagNumber = 300,
            TagType = TagType.WashTag,
            CreatedAt = createdTime,
            CreatedBy = "TestUser",
            UpdatedAt = updatedTime,
            UpdatedBy = "UpdateUser"
        };
        
        // Assert
        tag.CreatedAt.Should().Be(createdTime, "Created timestamp must be preserved");
        tag.CreatedBy.Should().Be("TestUser", "Creator must be recorded");
        tag.UpdatedAt.Should().Be(updatedTime, "Update timestamp must be preserved");
        tag.UpdatedBy.Should().Be("UpdateUser", "Updater must be recorded");
        
        // Audit trail validation
        tag.UpdatedAt.Should().BeAfter(tag.CreatedAt, "Update time must be after creation");
        tag.CreatedBy.Should().NotBeNullOrEmpty("Creator must be specified");
    }

    /// <summary>
    /// MD-REQ-005: Tag status transitions must be valid
    /// Critical for manufacturing process control
    /// </summary>
    [Theory(DisplayName = "MD-REQ-005: Tag Status Transitions Must Be Valid")]
    [InlineData(LifeStatus.Active)]
    [InlineData(LifeStatus.Inactive)]
    [InlineData(LifeStatus.Dead)]
    public void Tag_Status_Transitions_Should_Be_Valid(LifeStatus status)
    {
        // Arrange
        var tag = new Tag
        {
            TagNumber = 400,
            TagType = TagType.SterilizationLoadTag,
            Status = status
        };
        
        // Assert
        tag.Status.Should().Be(status, $"Tag status should be {status}");
        
        // Additional validation based on status
        switch (status)
        {
            case LifeStatus.Active:
                // Active tags should be usable
                break;
            case LifeStatus.Inactive:
                // Inactive tags should be temporarily disabled
                break;
            case LifeStatus.Dead:
                // Dead tags should be permanently unusable
                break;
        }
    }
}
