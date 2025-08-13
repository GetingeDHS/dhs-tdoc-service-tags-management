using Microsoft.EntityFrameworkCore;
using TagManagement.Core.Entities;
using TagManagement.Core.Enums;
using TagManagement.Core.Interfaces;
using Foundation.Core.Data.Models.EntityFramework;
using Foundation.Core.Data.Repositories;
using System.Linq.Expressions;

namespace TagManagement.Data.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly TagManagementDbContext _context;

        public TagRepository(TagManagementDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD Operations

        public async Task<Tag?> GetByIdAsync(int id)
        {
            var tagsModel = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .FirstOrDefaultAsync(t => t.TagKeyId == id);
            
            if (tagsModel == null) return null;

            var tag = MapToTag(tagsModel);
            await LoadTagContentsAsync(tag, id);
            return tag;
        }

        public async Task<Tag?> GetByNumberAndTypeAsync(int tagNumber, TagType tagType)
        {
            var tagsModel = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .FirstOrDefaultAsync(t => t.TagNumber == tagNumber && t.TagType.TagTypeKeyId == (int)tagType);
            
            if (tagsModel == null) return null;

            var tag = MapToTag(tagsModel);
            await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
            return tag;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            return await _context.Tags.ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Tags
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Tag> AddAsync(Tag tag)
        {
            tag.CreatedAt = DateTime.UtcNow;
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<Tag> UpdateAsync(Tag tag)
        {
            tag.UpdatedAt = DateTime.UtcNow;
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return false;

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Tag-specific Queries

        public async Task<IEnumerable<Tag>> GetTagsByTypeAsync(TagType tagType)
        {
            return await _context.Tags
                .Where(t => t.TagType == tagType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsByLocationAsync(int locationKeyId)
        {
            return await _context.Tags
                .Where(t => t.LocationKeyId == locationKeyId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetAutoTagsAsync()
        {
            return await _context.Tags
                .Where(t => t.IsAuto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsWithReservationsAsync()
        {
            return await _context.Tags
                .Where(t => t.HasAutoReservation)
                .ToListAsync();
        }

        public async Task<Tag?> GetEmptyAutoTagAsync(TagType tagType, int locationKeyId)
        {
            var tagIds = await _context.Tags
                .Where(t => t.TagType == tagType && t.IsAuto && t.LocationKeyId == locationKeyId)
                .Select(t => t.Id)
                .ToListAsync();

            foreach (var tagId in tagIds)
            {
                if (await IsTagEmptyAsync(tagId))
                {
                    return await GetByIdAsync(tagId);
                }
            }

            return null;
        }

        #endregion

        #region Content-related Queries

        public async Task<IEnumerable<Tag>> GetTagsContainingUnitAsync(int unitId)
        {
            var tagIds = await _context.TagUnits
                .Where(tu => tu.UnitId == unitId)
                .Select(tu => tu.TagId)
                .ToListAsync();

            var tags = await _context.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();

            foreach (var tag in tags)
            {
                await LoadTagContentsAsync(tag);
            }

            return tags;
        }

        public async Task<IEnumerable<Tag>> GetTagsContainingItemAsync(int itemKeyId, int serialKeyId)
        {
            var tagIds = await _context.TagItems
                .Where(ti => ti.ItemKeyId == itemKeyId && ti.SerialKeyId == serialKeyId)
                .Select(ti => ti.TagId)
                .ToListAsync();

            var tags = await _context.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();

            foreach (var tag in tags)
            {
                await LoadTagContentsAsync(tag);
            }

            return tags;
        }

        public async Task<bool> IsUnitInAnyTagAsync(int unitId)
        {
            return await _context.TagUnits.AnyAsync(tu => tu.UnitId == unitId);
        }

        public async Task<bool> IsItemInAnyTagAsync(int itemKeyId, int serialKeyId)
        {
            return await _context.TagItems
                .AnyAsync(ti => ti.ItemKeyId == itemKeyId && ti.SerialKeyId == serialKeyId);
        }

        public async Task<int> GetTagContentCountAsync(int tagId)
        {
            var unitCount = await _context.TagUnits.CountAsync(tu => tu.TagId == tagId);
            var itemCount = await _context.TagItems.CountAsync(ti => ti.TagId == tagId);
            var tagCount = await _context.TagTags.CountAsync(tt => tt.ParentTagId == tagId);
            var indicatorCount = await _context.TagIndicators.CountAsync(ti => ti.TagId == tagId);

            return unitCount + itemCount + tagCount + indicatorCount;
        }

        public async Task<bool> IsTagEmptyAsync(int tagId)
        {
            return await GetTagContentCountAsync(tagId) == 0;
        }

        #endregion

        #region Tag Hierarchy Queries

        public async Task<IEnumerable<Tag>> GetChildTagsAsync(int parentTagId)
        {
            var childTagIds = await _context.TagTags
                .Where(tt => tt.ParentTagId == parentTagId)
                .Select(tt => tt.ChildTagId)
                .ToListAsync();

            var tags = await _context.Tags
                .Where(t => childTagIds.Contains(t.Id))
                .ToListAsync();

            foreach (var tag in tags)
            {
                await LoadTagContentsAsync(tag);
            }

            return tags;
        }

        public async Task<Tag?> GetParentTagAsync(int childTagId)
        {
            var parentTagId = await _context.TagTags
                .Where(tt => tt.ChildTagId == childTagId)
                .Select(tt => tt.ParentTagId)
                .FirstOrDefaultAsync();

            return parentTagId > 0 ? await GetByIdAsync(parentTagId) : null;
        }

        public async Task<IEnumerable<Tag>> GetRootTagsAsync()
        {
            var childTagIds = await _context.TagTags
                .Select(tt => tt.ChildTagId)
                .ToListAsync();

            return await _context.Tags
                .Where(t => !childTagIds.Contains(t.Id))
                .ToListAsync();
        }

        public async Task<int> GetRootTagIdAsync(int tagId)
        {
            var parentTag = await GetParentTagAsync(tagId);
            if (parentTag == null)
                return tagId;

            return await GetRootTagIdAsync(parentTag.Id);
        }

        #endregion

        #region Split Tag Queries

        public async Task<IEnumerable<Tag>> GetLinkedSplitTagsAsync(int tagId)
        {
            // This is a simplified implementation. In the original Delphi code, 
            // split tags are identified through complex business logic
            // For now, we'll return tags that are split and related to the same unit
            var unitIds = await _context.TagUnits
                .Where(tu => tu.TagId == tagId && tu.IsSplit)
                .Select(tu => tu.UnitId)
                .ToListAsync();

            var tagIds = await _context.TagUnits
                .Where(tu => unitIds.Contains(tu.UnitId) && tu.IsSplit && tu.TagId != tagId)
                .Select(tu => tu.TagId)
                .ToListAsync();

            return await _context.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();
        }

        public async Task<int?> GetSplitUnitSerialNumberSplitTagAsync(int unitId)
        {
            // Simplified implementation - find a split tag containing this unit
            var tagId = await _context.TagUnits
                .Where(tu => tu.UnitId == unitId && tu.IsSplit)
                .Select(tu => tu.TagId)
                .FirstOrDefaultAsync();

            return tagId > 0 ? tagId : null;
        }

        #endregion

        #region Tag Relations

        public async Task<bool> AddUnitToTagAsync(int tagId, int unitId, DateTime time, int locationKeyId, bool markAsSplit = false)
        {
            try
            {
                // Remove unit from any other tags first if not marking as split
                if (!markAsSplit)
                {
                    await RemoveUnitFromAnyTagAsync(unitId, time, locationKeyId);
                }

                var tagUnit = new TagUnit
                {
                    TagId = tagId,
                    UnitId = unitId,
                    AddedAt = time,
                    LocationKeyId = locationKeyId,
                    IsSplit = markAsSplit
                };

                _context.TagUnits.Add(tagUnit);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveUnitFromTagAsync(int tagId, int unitId, DateTime time, int locationKeyId)
        {
            var tagUnit = await _context.TagUnits
                .FirstOrDefaultAsync(tu => tu.TagId == tagId && tu.UnitId == unitId);

            if (tagUnit == null) return false;

            _context.TagUnits.Remove(tagUnit);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddItemToTagAsync(int tagId, TagItem item, DateTime time, int locationKeyId)
        {
            try
            {
                // Remove item from any other tags first
                await RemoveItemFromAnyTagAsync(item, time, locationKeyId);

                var tagItem = new TagItemEntity
                {
                    TagId = tagId,
                    ItemKeyId = item.ItemKeyId,
                    SerialKeyId = item.SerialKeyId,
                    LotInfoKeyId = item.LotInfoKeyId,
                    Count = item.Count,
                    AddedAt = time,
                    LocationKeyId = locationKeyId
                };

                _context.TagItems.Add(tagItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveItemFromTagAsync(int tagId, TagItem item, DateTime time, int locationKeyId)
        {
            var tagItem = await _context.TagItems
                .FirstOrDefaultAsync(ti => ti.TagId == tagId && 
                                          ti.ItemKeyId == item.ItemKeyId && 
                                          ti.SerialKeyId == item.SerialKeyId && 
                                          ti.LotInfoKeyId == item.LotInfoKeyId);

            if (tagItem == null) return false;

            _context.TagItems.Remove(tagItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddTagToTagAsync(int parentTagId, int childTagId, DateTime time, int locationKeyId)
        {
            try
            {
                // Remove child tag from any other parent tags first
                await RemoveTagFromAnyParentAsync(childTagId, time, locationKeyId);

                var tagTag = new TagTag
                {
                    ParentTagId = parentTagId,
                    ChildTagId = childTagId,
                    AddedAt = time,
                    LocationKeyId = locationKeyId
                };

                _context.TagTags.Add(tagTag);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveTagFromTagAsync(int parentTagId, int childTagId, DateTime time, int locationKeyId)
        {
            var tagTag = await _context.TagTags
                .FirstOrDefaultAsync(tt => tt.ParentTagId == parentTagId && tt.ChildTagId == childTagId);

            if (tagTag == null) return false;

            _context.TagTags.Remove(tagTag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddIndicatorToTagAsync(int tagId, int indicatorId, DateTime time, int locationKeyId)
        {
            try
            {
                // Remove indicator from any other tags first
                await RemoveIndicatorFromAnyTagAsync(indicatorId, time, locationKeyId);

                var tagIndicator = new TagIndicator
                {
                    TagId = tagId,
                    IndicatorId = indicatorId,
                    AddedAt = time,
                    LocationKeyId = locationKeyId
                };

                _context.TagIndicators.Add(tagIndicator);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveIndicatorFromTagAsync(int tagId, int indicatorId, DateTime time, int locationKeyId)
        {
            var tagIndicator = await _context.TagIndicators
                .FirstOrDefaultAsync(ti => ti.TagId == tagId && ti.IndicatorId == indicatorId);

            if (tagIndicator == null) return false;

            _context.TagIndicators.Remove(tagIndicator);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Batch Operations

        public async Task<bool> DissolveTagAsync(int tagId, DateTime time, int locationKeyId)
        {
            try
            {
                // Remove all content from the tag
                var tagUnits = await _context.TagUnits.Where(tu => tu.TagId == tagId).ToListAsync();
                var tagItems = await _context.TagItems.Where(ti => ti.TagId == tagId).ToListAsync();
                var tagTags = await _context.TagTags.Where(tt => tt.ParentTagId == tagId).ToListAsync();
                var tagIndicators = await _context.TagIndicators.Where(ti => ti.TagId == tagId).ToListAsync();

                _context.TagUnits.RemoveRange(tagUnits);
                _context.TagItems.RemoveRange(tagItems);
                _context.TagTags.RemoveRange(tagTags);
                _context.TagIndicators.RemoveRange(tagIndicators);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ClearTagContentsAsync(int tagId, DateTime time, int locationKeyId)
        {
            return await DissolveTagAsync(tagId, time, locationKeyId);
        }

        public async Task<bool> MoveTagContentToTransportTagAsync(int sourceTagId, int transportTagId, DateTime time, int locationKeyId)
        {
            try
            {
                // Move all units
                var tagUnits = await _context.TagUnits.Where(tu => tu.TagId == sourceTagId).ToListAsync();
                foreach (var tagUnit in tagUnits)
                {
                    tagUnit.TagId = transportTagId;
                    tagUnit.AddedAt = time;
                    tagUnit.LocationKeyId = locationKeyId;
                }

                // Move all items
                var tagItems = await _context.TagItems.Where(ti => ti.TagId == sourceTagId).ToListAsync();
                foreach (var tagItem in tagItems)
                {
                    tagItem.TagId = transportTagId;
                    tagItem.AddedAt = time;
                    tagItem.LocationKeyId = locationKeyId;
                }

                // Move all nested tags
                var tagTags = await _context.TagTags.Where(tt => tt.ParentTagId == sourceTagId).ToListAsync();
                foreach (var tagTag in tagTags)
                {
                    tagTag.ParentTagId = transportTagId;
                    tagTag.AddedAt = time;
                    tagTag.LocationKeyId = locationKeyId;
                }

                // Move all indicators
                var tagIndicators = await _context.TagIndicators.Where(ti => ti.TagId == sourceTagId).ToListAsync();
                foreach (var tagIndicator in tagIndicators)
                {
                    tagIndicator.TagId = transportTagId;
                    tagIndicator.AddedAt = time;
                    tagIndicator.LocationKeyId = locationKeyId;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Auto Tag Management

        public async Task<int> ReserveAutoTagAsync(TagType tagType, int locationKeyId)
        {
            var tag = new Tag
            {
                TagType = tagType,
                TagNumber = await GenerateNextTagNumberAsync(tagType),
                TagTypeKeyId = (int)tagType + 1, // Simplified conversion
                IsAuto = true,
                Status = LifeStatus.Active,
                LocationKeyId = locationKeyId,
                LocationTime = DateTime.UtcNow,
                HasAutoReservation = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return tag.Id;
        }

        public async Task<bool> ReleaseAutoTagReservationAsync(int tagId)
        {
            var tag = await _context.Tags.FindAsync(tagId);
            if (tag == null || !tag.HasAutoReservation) return false;

            tag.HasAutoReservation = false;
            tag.UpdatedAt = DateTime.UtcNow;
            tag.UpdatedBy = "System";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Tag>> GetReservedAutoTagsAsync()
        {
            return await _context.Tags
                .Where(t => t.HasAutoReservation)
                .ToListAsync();
        }

        #endregion

        #region Private Helper Methods

        private async Task LoadTagContentsAsync(Tag tag)
        {
            // Load units
            var unitIds = await _context.TagUnits
                .Where(tu => tu.TagId == tag.Id)
                .Select(tu => tu.UnitId)
                .ToListAsync();
            tag.Contents.Units.AddRange(unitIds);

            // Load items
            var items = await _context.TagItems
                .Where(ti => ti.TagId == tag.Id)
                .Select(ti => new TagItem(ti.ItemKeyId, ti.SerialKeyId, ti.LotInfoKeyId, ti.Count))
                .ToListAsync();
            tag.Contents.Items.AddRange(items);

            // Load nested tags
            var childTags = await GetChildTagsAsync(tag.Id);
            tag.Contents.Tags.AddRange(childTags);

            // Load indicators
            var indicatorIds = await _context.TagIndicators
                .Where(ti => ti.TagId == tag.Id)
                .Select(ti => ti.IndicatorId)
                .ToListAsync();
            tag.Contents.Indicators.AddRange(indicatorIds);
        }

        private async Task<bool> RemoveUnitFromAnyTagAsync(int unitId, DateTime time, int locationKeyId)
        {
            var tagUnits = await _context.TagUnits.Where(tu => tu.UnitId == unitId).ToListAsync();
            _context.TagUnits.RemoveRange(tagUnits);
            return true;
        }

        private async Task<bool> RemoveItemFromAnyTagAsync(TagItem item, DateTime time, int locationKeyId)
        {
            var tagItems = await _context.TagItems
                .Where(ti => ti.ItemKeyId == item.ItemKeyId && 
                            ti.SerialKeyId == item.SerialKeyId && 
                            ti.LotInfoKeyId == item.LotInfoKeyId)
                .ToListAsync();
            _context.TagItems.RemoveRange(tagItems);
            return true;
        }

        private async Task<bool> RemoveTagFromAnyParentAsync(int childTagId, DateTime time, int locationKeyId)
        {
            var tagTags = await _context.TagTags.Where(tt => tt.ChildTagId == childTagId).ToListAsync();
            _context.TagTags.RemoveRange(tagTags);
            return true;
        }

        private async Task<bool> RemoveIndicatorFromAnyTagAsync(int indicatorId, DateTime time, int locationKeyId)
        {
            var tagIndicators = await _context.TagIndicators.Where(ti => ti.IndicatorId == indicatorId).ToListAsync();
            _context.TagIndicators.RemoveRange(tagIndicators);
            return true;
        }

        private async Task<int> GenerateNextTagNumberAsync(TagType tagType)
        {
            var lastTag = await _context.Tags
                .Where(t => t.TagType == tagType)
                .OrderByDescending(t => t.TagNumber)
                .FirstOrDefaultAsync();

            return lastTag?.TagNumber + 1 ?? 1;
        }

        #endregion
    }
}
