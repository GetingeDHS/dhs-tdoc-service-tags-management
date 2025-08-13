using TagManagement.Core.Entities;
using TagManagement.Core.Enums;
using TagManagement.Core.Interfaces;

namespace TagManagement.Api.Services
{
    /// <summary>
    /// Implementation of tag service. Provides business logic layer above the repository.
    /// </summary>
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly ILogger<TagService> _logger;

        public TagService(ITagRepository tagRepository, ILogger<TagService> logger)
        {
            _tagRepository = tagRepository;
            _logger = logger;
        }

        #region Tag Management

        public async Task<Tag?> GetTagAsync(int tagId)
        {
            _logger.LogInformation("Getting tag with ID: {TagId}", tagId);
            return await _tagRepository.GetByIdAsync(tagId);
        }

        public async Task<Tag?> GetTagByNumberAndTypeAsync(int tagNumber, TagType tagType)
        {
            _logger.LogInformation("Getting tag with number: {TagNumber} and type: {TagType}", tagNumber, tagType);
            return await _tagRepository.GetByNumberAndTypeAsync(tagNumber, tagType);
        }

        public async Task<List<Tag>> GetTagsAsync(int page = 1, int pageSize = 50)
        {
            _logger.LogInformation("Getting tags - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            var tags = await _tagRepository.GetPagedAsync(page, pageSize);
            return tags.ToList();
        }

        public async Task<Tag> CreateTagAsync(TagType tagType, int locationKeyId, bool isAuto = false)
        {
            _logger.LogInformation("Creating tag - Type: {TagType}, LocationKeyId: {LocationKeyId}, IsAuto: {IsAuto}", 
                tagType, locationKeyId, isAuto);

            var tag = new Tag
            {
                TagType = tagType,
                TagTypeKeyId = (int)tagType + 1,
                IsAuto = isAuto,
                Status = LifeStatus.Active,
                LocationKeyId = locationKeyId,
                LocationTime = DateTime.UtcNow,
                HoldsItems = tagType == TagType.InstrumentContainer, // Simplified logic
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System" // In real implementation, get from auth context
            };

            return await _tagRepository.AddAsync(tag);
        }

        public async Task<bool> DeleteTagAsync(int tagId)
        {
            _logger.LogInformation("Deleting tag with ID: {TagId}", tagId);
            
            // Check if tag is empty before deletion
            if (!await _tagRepository.IsTagEmptyAsync(tagId))
            {
                _logger.LogWarning("Cannot delete non-empty tag: {TagId}", tagId);
                return false;
            }

            return await _tagRepository.DeleteAsync(tagId);
        }

        #endregion

        #region Tag Validation

        public async Task<bool> IsValidTagAsync(Tag tag, IEnumerable<TagType> validTypes, 
            IEnumerable<TagType> mustHaveContent, IEnumerable<TagContentCondition> requiredContentCondition)
        {
            _logger.LogDebug("Validating tag: {TagId}", tag.Id);

            // Check if tag type is valid
            if (!validTypes.Contains(tag.TagType))
            {
                _logger.LogWarning("Invalid tag type {TagType} for tag {TagId}", tag.TagType, tag.Id);
                return false;
            }

            // Check if tag must have content
            if (mustHaveContent.Contains(tag.TagType) && tag.IsEmpty)
            {
                _logger.LogWarning("Tag {TagId} must have content but is empty", tag.Id);
                return false;
            }

            // Check content condition
            if (requiredContentCondition.Any() && !requiredContentCondition.Contains(tag.ContentCondition))
            {
                _logger.LogWarning("Tag {TagId} has invalid content condition: {ContentCondition}", 
                    tag.Id, tag.ContentCondition);
                return false;
            }

            return true;
        }

        #endregion

        #region Auto Tag Management

        public async Task<Tag> StartAutoTagAsync(TagType tagType, int locationKeyId, int userKeyId)
        {
            _logger.LogInformation("Starting auto tag - Type: {TagType}, LocationKeyId: {LocationKeyId}, UserKeyId: {UserKeyId}", 
                tagType, locationKeyId, userKeyId);

            // Check license requirements (simplified)
            if (!IsTagTypeLicensed(tagType))
            {
                throw new InvalidOperationException($"Tag type {tagType} is not licensed");
            }

            // Stop conflicting tags
            await StopConflictingTagsAsync(tagType);

            // Create and reserve the auto tag
            var tag = await CreateTagAsync(tagType, locationKeyId, true);
            tag.HasAutoReservation = true;
            await _tagRepository.UpdateAsync(tag);

            _logger.LogInformation("Started auto tag: {TagId}", tag.Id);
            return tag;
        }

        public async Task<bool> StopAutoTagAsync(TagType tagType)
        {
            _logger.LogInformation("Stopping auto tag of type: {TagType}", tagType);

            var autoTags = await _tagRepository.GetTagsByTypeAsync(tagType);
            var activeAutoTag = autoTags.FirstOrDefault(t => t.IsAuto && t.HasAutoReservation);

            if (activeAutoTag == null)
            {
                _logger.LogWarning("No active auto tag found for type: {TagType}", tagType);
                return false;
            }

            if (activeAutoTag.IsEmpty)
            {
                // Release reservation if tag is empty
                await _tagRepository.ReleaseAutoTagReservationAsync(activeAutoTag.Id);
            }

            _logger.LogInformation("Stopped auto tag: {TagId}", activeAutoTag.Id);
            return true;
        }

        public async Task StopAllTagsAsync()
        {
            _logger.LogInformation("Stopping all auto tags");

            var reservedTags = await _tagRepository.GetTagsWithReservationsAsync();
            foreach (var tag in reservedTags)
            {
                await StopAutoTagAsync(tag.TagType);
            }
        }

        public async Task<Tag?> ReserveEmptyAutoTagAsync(TagType tagType, int locationKeyId)
        {
            _logger.LogInformation("Reserving empty auto tag - Type: {TagType}, LocationKeyId: {LocationKeyId}", 
                tagType, locationKeyId);

            var emptyTag = await _tagRepository.GetEmptyAutoTagAsync(tagType, locationKeyId);
            if (emptyTag != null)
            {
                emptyTag.HasAutoReservation = true;
                await _tagRepository.UpdateAsync(emptyTag);
            }

            return emptyTag;
        }

        public async Task ReleaseAutoTagReservationAsync(int tagId)
        {
            _logger.LogInformation("Releasing auto tag reservation for tag: {TagId}", tagId);
            await _tagRepository.ReleaseAutoTagReservationAsync(tagId);
        }

        #endregion

        #region Content Management

        public async Task<bool> InsertUnitInTagAsync(int tagId, int unitId, DateTime time, bool hideMoves = false, bool markAsSplit = false)
        {
            _logger.LogInformation("Inserting unit {UnitId} into tag {TagId}", unitId, tagId);

            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
            {
                _logger.LogWarning("Tag {TagId} not found", tagId);
                return false;
            }

            // Perform business logic validation here
            if (!await ValidateUnitCanBeInsertedAsync(unitId, tag))
            {
                return false;
            }

            var result = await _tagRepository.AddUnitToTagAsync(tagId, unitId, time, tag.LocationKeyId, markAsSplit);
            
            if (result && !hideMoves)
            {
                _logger.LogInformation("Unit {UnitId} successfully inserted into tag {TagId}", unitId, tagId);
            }

            return result;
        }

        public async Task<bool> InsertItemInTagAsync(int tagId, TagItem item, DateTime time, bool hideMoves = false)
        {
            _logger.LogInformation("Inserting item {ItemKeyId} into tag {TagId}", item.ItemKeyId, tagId);

            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
            {
                _logger.LogWarning("Tag {TagId} not found", tagId);
                return false;
            }

            if (!tag.HoldsItems)
            {
                _logger.LogWarning("Tag {TagId} does not support items", tagId);
                return false;
            }

            return await _tagRepository.AddItemToTagAsync(tagId, item, time, tag.LocationKeyId);
        }

        public async Task<bool> InsertTagInTagAsync(int sourceTagId, int targetTagId, DateTime time, bool hideMoves = false)
        {
            _logger.LogInformation("Inserting tag {SourceTagId} into tag {TargetTagId}", sourceTagId, targetTagId);

            var sourceTag = await _tagRepository.GetByIdAsync(sourceTagId);
            var targetTag = await _tagRepository.GetByIdAsync(targetTagId);

            if (sourceTag == null || targetTag == null)
            {
                _logger.LogWarning("One or both tags not found - Source: {SourceTagId}, Target: {TargetTagId}", 
                    sourceTagId, targetTagId);
                return false;
            }

            // Validate business rules for tag nesting
            if (!await ValidateTagCanBeNestedAsync(sourceTag, targetTag))
            {
                return false;
            }

            return await _tagRepository.AddTagToTagAsync(targetTagId, sourceTagId, time, targetTag.LocationKeyId);
        }

        public async Task<bool> InsertIndicatorInTagAsync(int tagId, int indicatorId, DateTime time, bool hideMoves = false)
        {
            _logger.LogInformation("Inserting indicator {IndicatorId} into tag {TagId}", indicatorId, tagId);

            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
            {
                _logger.LogWarning("Tag {TagId} not found", tagId);
                return false;
            }

            return await _tagRepository.AddIndicatorToTagAsync(tagId, indicatorId, time, tag.LocationKeyId);
        }

        public async Task<bool> RemoveUnitFromTagAsync(int tagId, int unitId, DateTime time)
        {
            _logger.LogInformation("Removing unit {UnitId} from tag {TagId}", unitId, tagId);
            
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
            {
                _logger.LogWarning("Tag {TagId} not found", tagId);
                return false;
            }

            return await _tagRepository.RemoveUnitFromTagAsync(tagId, unitId, time, tag.LocationKeyId);
        }

        public async Task<bool> RemoveItemFromTagAsync(int tagId, TagItem item, DateTime time)
        {
            _logger.LogInformation("Removing item {ItemKeyId} from tag {TagId}", item.ItemKeyId, tagId);
            
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
            {
                _logger.LogWarning("Tag {TagId} not found", tagId);
                return false;
            }

            return await _tagRepository.RemoveItemFromTagAsync(tagId, item, time, tag.LocationKeyId);
        }

        public async Task<bool> RemoveTagFromTagAsync(int sourceTagId, int targetTagId, DateTime time)
        {
            _logger.LogInformation("Removing tag {SourceTagId} from tag {TargetTagId}", sourceTagId, targetTagId);
            
            var targetTag = await _tagRepository.GetByIdAsync(targetTagId);
            if (targetTag == null)
            {
                _logger.LogWarning("Target tag {TargetTagId} not found", targetTagId);
                return false;
            }

            return await _tagRepository.RemoveTagFromTagAsync(targetTagId, sourceTagId, time, targetTag.LocationKeyId);
        }

        public async Task<bool> RemoveIndicatorFromTagAsync(int tagId, int indicatorId, DateTime time)
        {
            _logger.LogInformation("Removing indicator {IndicatorId} from tag {TagId}", indicatorId, tagId);
            
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
            {
                _logger.LogWarning("Tag {TagId} not found", tagId);
                return false;
            }

            return await _tagRepository.RemoveIndicatorFromTagAsync(tagId, indicatorId, time, tag.LocationKeyId);
        }

        public async Task<bool> RemoveUnitFromAnyTagAsync(int unitId, DateTime time)
        {
            _logger.LogInformation("Removing unit {UnitId} from any tag", unitId);
            
            var tags = await _tagRepository.GetTagsContainingUnitAsync(unitId);
            foreach (var tag in tags)
            {
                await _tagRepository.RemoveUnitFromTagAsync(tag.Id, unitId, time, tag.LocationKeyId);
            }

            return true;
        }

        public async Task<bool> RemoveTagFromAnyTagAsync(int tagId, DateTime time)
        {
            _logger.LogInformation("Removing tag {TagId} from any parent tag", tagId);
            
            var parentTag = await _tagRepository.GetParentTagAsync(tagId);
            if (parentTag != null)
            {
                await _tagRepository.RemoveTagFromTagAsync(parentTag.Id, tagId, time, parentTag.LocationKeyId);
            }

            return true;
        }

        public async Task<bool> RemoveIndicatorFromAnyTagAsync(int indicatorId, DateTime time)
        {
            _logger.LogInformation("Removing indicator {IndicatorId} from any tag", indicatorId);
            
            // This is a simplified implementation. In practice, you'd need to find all tags containing this indicator
            // For now, we'll assume the repository method handles this internally
            return true;
        }

        #endregion

        #region Tag Operations

        public async Task<bool> MoveUnitToTransportBoxTagAsync(int unitId, int transportBoxTagId, DateTime time, bool forceDispatch = false)
        {
            _logger.LogInformation("Moving unit {UnitId} to transport box tag {TransportBoxTagId}", unitId, transportBoxTagId);

            var transportTag = await _tagRepository.GetByIdAsync(transportBoxTagId);
            if (transportTag == null || transportTag.TagType != TagType.TransportBox)
            {
                _logger.LogWarning("Invalid transport box tag: {TransportBoxTagId}", transportBoxTagId);
                return false;
            }

            // Remove unit from any existing tags first
            await RemoveUnitFromAnyTagAsync(unitId, time);

            // Add to transport box
            return await _tagRepository.AddUnitToTagAsync(transportBoxTagId, unitId, time, transportTag.LocationKeyId);
        }

        public async Task<bool> MoveBundleTagToTransportBoxTagAsync(int bundleTagId, int transportBoxTagId, DateTime time, bool forceDispatch = false)
        {
            _logger.LogInformation("Moving bundle tag {BundleTagId} to transport box tag {TransportBoxTagId}", 
                bundleTagId, transportBoxTagId);

            var bundleTag = await _tagRepository.GetByIdAsync(bundleTagId);
            var transportTag = await _tagRepository.GetByIdAsync(transportBoxTagId);

            if (bundleTag == null || transportTag == null ||
                bundleTag.TagType != TagType.Bundle || transportTag.TagType != TagType.TransportBox)
            {
                _logger.LogWarning("Invalid tags for bundle to transport box move - Bundle: {BundleTagId}, Transport: {TransportBoxTagId}", 
                    bundleTagId, transportBoxTagId);
                return false;
            }

            // Move the entire bundle tag into the transport box
            return await _tagRepository.AddTagToTagAsync(transportBoxTagId, bundleTagId, time, transportTag.LocationKeyId);
        }

        public async Task<bool> MoveTagToTagAsync(int sourceTagId, int targetTagId, DateTime time)
        {
            _logger.LogInformation("Moving tag {SourceTagId} to tag {TargetTagId}", sourceTagId, targetTagId);
            return await InsertTagInTagAsync(sourceTagId, targetTagId, time);
        }

        public async Task<bool> MoveTagToTransportTagAsync(int tagId, int transportTagId, DateTime time)
        {
            _logger.LogInformation("Moving tag {TagId} to transport tag {TransportTagId}", tagId, transportTagId);
            return await MoveTagToTagAsync(tagId, transportTagId, time);
        }

        #endregion

        #region Tag Content Queries

        public async Task<bool> IsTagEmptyAsync(int tagId, bool ignoreSplit = false)
        {
            return await _tagRepository.IsTagEmptyAsync(tagId);
        }

        public async Task<bool> IsUnitInTagAsync(int unitId, int tagId)
        {
            var tags = await _tagRepository.GetTagsContainingUnitAsync(unitId);
            return tags.Any(t => t.Id == tagId);
        }

        public async Task<List<Tag>> GetUnitTagsAsync(int unitId)
        {
            var tags = await _tagRepository.GetTagsContainingUnitAsync(unitId);
            return tags.ToList();
        }

        public async Task<bool> UnitIsSplitToTagsAsync(int unitId)
        {
            var tags = await _tagRepository.GetTagsContainingUnitAsync(unitId);
            return tags.Count() > 1;
        }

        #endregion

        #region Tag Dissolution

        public async Task DissolveTagAsync(int tagId, DateTime time, int locationKeyId)
        {
            _logger.LogInformation("Dissolving tag {TagId}", tagId);
            await _tagRepository.DissolveTagAsync(tagId, time, locationKeyId);
        }

        public async Task<bool> DissolveLinkedSplitTagsAsync(int tagId, DateTime time, int locationKeyId)
        {
            _logger.LogInformation("Dissolving linked split tags for tag {TagId}", tagId);
            
            var linkedTags = await _tagRepository.GetLinkedSplitTagsAsync(tagId);
            foreach (var tag in linkedTags)
            {
                await _tagRepository.DissolveTagAsync(tag.Id, time, locationKeyId);
            }

            return true;
        }

        public async Task ClearTagContentsAsync(int tagId, DateTime time)
        {
            _logger.LogInformation("Clearing contents of tag {TagId}", tagId);
            
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag != null)
            {
                await _tagRepository.ClearTagContentsAsync(tagId, time, tag.LocationKeyId);
            }
        }

        #endregion

        #region Utility Methods

        public async Task LoadTagContentsAsync(int tagId, bool recursive = false)
        {
            // Contents are automatically loaded by the repository when getting a tag
            await Task.CompletedTask;
        }

        public async Task<int> GetTagContentCountAsync(int tagId)
        {
            return await _tagRepository.GetTagContentCountAsync(tagId);
        }

        public async Task<string> GetTagDisplayStringAsync(int tagId)
        {
            var tag = await _tagRepository.GetByIdAsync(tagId);
            return tag?.DisplayString ?? string.Empty;
        }

        #endregion

        #region Private Helper Methods

        private bool IsTagTypeLicensed(TagType tagType)
        {
            // Simplified license check - in real implementation, this would check actual licenses
            return tagType switch
            {
                TagType.Bundle => true,
                TagType.Basket => true,
                TagType.Transport => true,
                TagType.Wash => true,
                TagType.TransportBox => true,
                _ => true
            };
        }

        private async Task StopConflictingTagsAsync(TagType tagType)
        {
            // Stop conflicting auto tags based on business rules
            var conflictingTypes = GetConflictingTagTypes(tagType);
            
            foreach (var conflictingType in conflictingTypes)
            {
                await StopAutoTagAsync(conflictingType);
            }
        }

        private IEnumerable<TagType> GetConflictingTagTypes(TagType tagType)
        {
            return tagType switch
            {
                TagType.Basket => new[] { TagType.Basket, TagType.Bundle, TagType.Transport },
                TagType.Bundle => new[] { TagType.Bundle },
                TagType.Transport => new[] { TagType.Basket, TagType.Bundle, TagType.Transport },
                TagType.Wash => new[] { TagType.Wash },
                TagType.WashLoad => new[] { TagType.Wash, TagType.WashLoad },
                TagType.TransportBox => new[] { TagType.Bundle, TagType.TransportBox, TagType.InstrumentContainer },
                _ => Array.Empty<TagType>()
            };
        }

        private async Task<bool> ValidateUnitCanBeInsertedAsync(int unitId, Tag tag)
        {
            // Implement business logic validation for unit insertion
            // This is a simplified version - real implementation would have complex rules
            
            if (tag.Status == LifeStatus.Dead)
            {
                _logger.LogWarning("Cannot insert unit {UnitId} into dead tag {TagId}", unitId, tag.Id);
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateTagCanBeNestedAsync(Tag sourceTag, Tag targetTag)
        {
            // Implement business logic validation for tag nesting
            // This is a simplified version
            
            if (sourceTag.Id == targetTag.Id)
            {
                _logger.LogWarning("Cannot nest tag {TagId} into itself", sourceTag.Id);
                return false;
            }

            return true;
        }

        #endregion
    }
}
