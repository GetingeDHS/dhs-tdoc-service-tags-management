using Microsoft.EntityFrameworkCore;
using TagManagement.Core.Entities;
using TagManagement.Core.Enums;
using TagManagement.Core.Interfaces;
using TagManagement.Infrastructure.Persistence.Models;
using System.Linq.Expressions;

namespace TagManagement.Infrastructure.Persistence.Repositories
{
    public class TDocTagRepository : ITagRepository
    {
        private readonly TagManagementDbContext _context;

        public TDocTagRepository(TagManagementDbContext context)
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
                .FirstOrDefaultAsync(t => t.TagNumber == tagNumber && t.TagTypeKeyId == (int)tagType);
            
            if (tagsModel == null) return null;

            var tag = MapToTag(tagsModel);
            await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
            return tag;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<IEnumerable<Tag>> GetPagedAsync(int page, int pageSize)
        {
            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<Tag> AddAsync(Tag tag)
        {
            var tagsModel = MapToTagsModel(tag);
            tagsModel.CreatedTime = DateTime.UtcNow;
            tagsModel.CreatedByUserKeyId = 1; // Default system user
            
            _context.Tags.Add(tagsModel);
            await _context.SaveChangesAsync();
            
            tag.Id = tagsModel.TagKeyId;
            return tag;
        }

        public async Task<Tag> UpdateAsync(Tag tag)
        {
            var tagsModel = await _context.Tags.FindAsync(tag.Id);
            if (tagsModel == null) throw new ArgumentException("Tag not found");

            UpdateTagsModel(tagsModel, tag);
            tagsModel.ModifiedTime = DateTime.UtcNow;
            tagsModel.ModifiedByUserKeyId = 1; // Default system user

            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tagsModel = await _context.Tags.FindAsync(id);
            if (tagsModel == null) return false;

            // Also need to delete related TagContent records
            var tagContents = await _context.TagContents
                .Where(tc => tc.ParentTagKeyId == id)
                .ToListAsync();
            
            _context.TagContents.RemoveRange(tagContents);
            _context.Tags.Remove(tagsModel);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Tag-specific Queries

        public async Task<IEnumerable<Tag>> GetTagsByTypeAsync(TagType tagType)
        {
            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Where(t => t.TagTypeKeyId == (int)tagType)
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<IEnumerable<Tag>> GetTagsByLocationAsync(int locationKeyId)
        {
            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Where(t => t.LocationKeyId == locationKeyId)
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<IEnumerable<Tag>> GetAutoTagsAsync()
        {
            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Where(t => t.IsAutoTag == true)
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<IEnumerable<Tag>> GetTagsWithReservationsAsync()
        {
            // This would need to be implemented based on TDOC business logic
            // For now, return empty collection
            return new List<Tag>();
        }

        public async Task<Tag?> GetEmptyAutoTagAsync(TagType tagType, int locationKeyId)
        {
            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Where(t => t.TagTypeKeyId == (int)tagType && 
                           t.IsAutoTag == true && 
                           t.LocationKeyId == locationKeyId)
                .ToListAsync();

            foreach (var tagsModel in tagsModels)
            {
                var hasContent = await _context.TagContents
                    .AnyAsync(tc => tc.ParentTagKeyId == tagsModel.TagKeyId);
                
                if (!hasContent)
                {
                    var tag = MapToTag(tagsModel);
                    await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                    return tag;
                }
            }

            return null;
        }

        #endregion

        #region Content-related Queries

        public async Task<IEnumerable<Tag>> GetTagsContainingUnitAsync(int unitId)
        {
            // Need to query TagContent for units
            var tagContentIds = await _context.TagContents
                .Where(tc => tc.UnitKeyId == unitId)
                .Select(tc => tc.ParentTagKeyId)
                .Distinct()
                .ToListAsync();

            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Where(t => tagContentIds.Contains(t.TagKeyId))
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<IEnumerable<Tag>> GetTagsContainingItemAsync(int itemKeyId, int serialKeyId)
        {
            var tagContentIds = await _context.TagContents
                .Where(tc => tc.ItemKeyId == itemKeyId && tc.SerialKeyId == serialKeyId)
                .Select(tc => tc.ParentTagKeyId)
                .Distinct()
                .ToListAsync();

            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Where(t => tagContentIds.Contains(t.TagKeyId))
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<bool> IsUnitInAnyTagAsync(int unitId)
        {
            return await _context.TagContents.AnyAsync(tc => tc.UnitKeyId == unitId);
        }

        public async Task<bool> IsItemInAnyTagAsync(int itemKeyId, int serialKeyId)
        {
            return await _context.TagContents
                .AnyAsync(tc => tc.ItemKeyId == itemKeyId && tc.SerialKeyId == serialKeyId);
        }

        public async Task<int> GetTagContentCountAsync(int tagId)
        {
            return await _context.TagContents.CountAsync(tc => tc.ParentTagKeyId == tagId);
        }

        public async Task<bool> IsTagEmptyAsync(int tagId)
        {
            return await GetTagContentCountAsync(tagId) == 0;
        }

        #endregion

        #region Tag Hierarchy Queries

        public async Task<IEnumerable<Tag>> GetChildTagsAsync(int parentTagId)
        {
            var childTagIds = await _context.TagContents
                .Where(tc => tc.ParentTagKeyId == parentTagId && tc.ChildTagKeyId != null)
                .Select(tc => tc.ChildTagKeyId!.Value)
                .ToListAsync();

            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Where(t => childTagIds.Contains(t.TagKeyId))
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<Tag?> GetParentTagAsync(int childTagId)
        {
            var parentTagId = await _context.TagContents
                .Where(tc => tc.ChildTagKeyId == childTagId)
                .Select(tc => tc.ParentTagKeyId)
                .FirstOrDefaultAsync();

            return parentTagId > 0 ? await GetByIdAsync(parentTagId) : null;
        }

        public async Task<IEnumerable<Tag>> GetRootTagsAsync()
        {
            var childTagIds = await _context.TagContents
                .Where(tc => tc.ChildTagKeyId != null)
                .Select(tc => tc.ChildTagKeyId!.Value)
                .ToListAsync();

            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Where(t => !childTagIds.Contains(t.TagKeyId))
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        public async Task<int> GetRootTagIdAsync(int tagId)
        {
            var parentTag = await GetParentTagAsync(tagId);
            if (parentTag == null)
                return tagId;

            return await GetRootTagIdAsync(parentTag.Id);
        }

        #endregion

        #region Split Tag Queries (Simplified for TDOC integration)

        public async Task<IEnumerable<Tag>> GetLinkedSplitTagsAsync(int tagId)
        {
            // This would need complex business logic from TDOC
            // For now, return empty collection
            return new List<Tag>();
        }

        public async Task<int?> GetSplitUnitSerialNumberSplitTagAsync(int unitId)
        {
            // This would need complex business logic from TDOC
            return null;
        }

        #endregion

        #region Tag Relations (Using TagContent table)

        public async Task<bool> AddUnitToTagAsync(int tagId, int unitId, DateTime time, int locationKeyId, bool markAsSplit = false)
        {
            try
            {
                // Remove unit from any other tags first if not marking as split
                if (!markAsSplit)
                {
                    var existingContent = await _context.TagContents
                        .Where(tc => tc.UnitKeyId == unitId)
                        .ToListAsync();
                    _context.TagContents.RemoveRange(existingContent);
                }

                var tagContent = new TagContentModel
                {
                    ParentTagKeyId = tagId,
                    UnitKeyId = unitId,
                    LocationKeyId = locationKeyId,
                    CreatedTime = time,
                    CreatedByUserKeyId = 1 // Default system user
                };

                _context.TagContents.Add(tagContent);
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
            var tagContent = await _context.TagContents
                .FirstOrDefaultAsync(tc => tc.ParentTagKeyId == tagId && tc.UnitKeyId == unitId);

            if (tagContent == null) return false;

            _context.TagContents.Remove(tagContent);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddItemToTagAsync(int tagId, TagItem item, DateTime time, int locationKeyId)
        {
            try
            {
                // Remove item from any other tags first
                var existingContent = await _context.TagContents
                    .Where(tc => tc.ItemKeyId == item.ItemKeyId && tc.SerialKeyId == item.SerialKeyId)
                    .ToListAsync();
                _context.TagContents.RemoveRange(existingContent);

                var tagContent = new TagContentModel
                {
                    ParentTagKeyId = tagId,
                    ItemKeyId = item.ItemKeyId,
                    SerialKeyId = item.SerialKeyId,
                    LotInfoKeyId = item.LotInfoKeyId,
                    LocationKeyId = locationKeyId,
                    CreatedTime = time,
                    CreatedByUserKeyId = 1 // Default system user
                };

                _context.TagContents.Add(tagContent);
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
            var tagContent = await _context.TagContents
                .FirstOrDefaultAsync(tc => tc.ParentTagKeyId == tagId && 
                                          tc.ItemKeyId == item.ItemKeyId && 
                                          tc.SerialKeyId == item.SerialKeyId);

            if (tagContent == null) return false;

            _context.TagContents.Remove(tagContent);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddTagToTagAsync(int parentTagId, int childTagId, DateTime time, int locationKeyId)
        {
            try
            {
                // Remove child tag from any other parent tags first
                var existingContent = await _context.TagContents
                    .Where(tc => tc.ChildTagKeyId == childTagId)
                    .ToListAsync();
                _context.TagContents.RemoveRange(existingContent);

                var tagContent = new TagContentModel
                {
                    ParentTagKeyId = parentTagId,
                    ChildTagKeyId = childTagId,
                    LocationKeyId = locationKeyId,
                    CreatedTime = time,
                    CreatedByUserKeyId = 1 // Default system user
                };

                _context.TagContents.Add(tagContent);
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
            var tagContent = await _context.TagContents
                .FirstOrDefaultAsync(tc => tc.ParentTagKeyId == parentTagId && tc.ChildTagKeyId == childTagId);

            if (tagContent == null) return false;

            _context.TagContents.Remove(tagContent);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddIndicatorToTagAsync(int tagId, int indicatorId, DateTime time, int locationKeyId)
        {
            try
            {
                // Remove indicator from any other tags first
                var existingContent = await _context.TagContents
                    .Where(tc => tc.IndicatorKeyId == indicatorId)
                    .ToListAsync();
                _context.TagContents.RemoveRange(existingContent);

                var tagContent = new TagContentModel
                {
                    ParentTagKeyId = tagId,
                    IndicatorKeyId = indicatorId,
                    LocationKeyId = locationKeyId,
                    CreatedTime = time,
                    CreatedByUserKeyId = 1 // Default system user
                };

                _context.TagContents.Add(tagContent);
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
            var tagContent = await _context.TagContents
                .FirstOrDefaultAsync(tc => tc.ParentTagKeyId == tagId && tc.IndicatorKeyId == indicatorId);

            if (tagContent == null) return false;

            _context.TagContents.Remove(tagContent);
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
                var tagContents = await _context.TagContents
                    .Where(tc => tc.ParentTagKeyId == tagId)
                    .ToListAsync();

                _context.TagContents.RemoveRange(tagContents);
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
                var tagContents = await _context.TagContents
                    .Where(tc => tc.ParentTagKeyId == sourceTagId)
                    .ToListAsync();

                foreach (var tagContent in tagContents)
                {
                    tagContent.ParentTagKeyId = transportTagId;
                    tagContent.LocationKeyId = locationKeyId;
                    tagContent.ModifiedTime = time;
                    tagContent.ModifiedByUserKeyId = 1; // Default system user
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
            var tagNumber = await GenerateNextTagNumberAsync(tagType);
            
            var tagsModel = new TagsModel
            {
                TagTypeKeyId = (int)tagType,
                TagNumber = tagNumber,
                IsAutoTag = true,
                LocationKeyId = locationKeyId,
                CreatedTime = DateTime.UtcNow,
                CreatedByUserKeyId = 1 // Default system user
            };

            _context.Tags.Add(tagsModel);
            await _context.SaveChangesAsync();
            return tagsModel.TagKeyId;
        }

        public async Task<bool> ReleaseAutoTagReservationAsync(int tagId)
        {
            var tagsModel = await _context.Tags.FindAsync(tagId);
            if (tagsModel == null || tagsModel.IsAutoTag != true) return false;

            tagsModel.IsAutoTag = false;
            tagsModel.ModifiedTime = DateTime.UtcNow;
            tagsModel.ModifiedByUserKeyId = 1; // Default system user

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Tag>> GetReservedAutoTagsAsync()
        {
            var tagsModels = await _context.Tags
                .Include(t => t.Location)
                .Include(t => t.TagType)
                .Where(t => t.IsAutoTag == true)
                .ToListAsync();

            var tags = new List<Tag>();
            foreach (var tagsModel in tagsModels)
            {
                var tag = MapToTag(tagsModel);
                await LoadTagContentsAsync(tag, tagsModel.TagKeyId);
                tags.Add(tag);
            }
            return tags;
        }

        #endregion

        #region Private Helper Methods

        private async Task LoadTagContentsAsync(Tag tag, int tagId)
        {
            var tagContents = await _context.TagContents
                .Where(tc => tc.ParentTagKeyId == tagId)
                .ToListAsync();

            foreach (var content in tagContents)
            {
                if (content.UnitKeyId.HasValue)
                {
                    tag.Contents.Units.Add(content.UnitKeyId.Value);
                }
                else if (content.ItemKeyId.HasValue && content.SerialKeyId.HasValue)
                {
                    var tagItem = new TagItem(
                        content.ItemKeyId.Value,
                        content.SerialKeyId.Value,
                        content.LotInfoKeyId ?? 0,
                        1 // Default count since TDOC doesn't store this in TagContent
                    );
                    tag.Contents.Items.Add(tagItem);
                }
                else if (content.ChildTagKeyId.HasValue)
                {
                    var childTag = await GetByIdAsync(content.ChildTagKeyId.Value);
                    if (childTag != null)
                    {
                        tag.Contents.Tags.Add(childTag);
                    }
                }
                else if (content.IndicatorKeyId.HasValue)
                {
                    tag.Contents.Indicators.Add(content.IndicatorKeyId.Value);
                }
            }
        }

        private Tag MapToTag(TagsModel tagsModel)
        {
            return new Tag
            {
                Id = tagsModel.TagKeyId,
                TagNumber = tagsModel.TagNumber ?? 0,
                TagType = (TagType)(tagsModel.TagTypeKeyId ?? 0),
                TagTypeKeyId = tagsModel.TagTypeKeyId ?? 0,
                IsAuto = tagsModel.IsAutoTag ?? false,
                Status = LifeStatus.Active, // Default status
                LocationKeyId = tagsModel.LocationKeyId ?? 0,
                LocationTime = tagsModel.CreatedTime ?? DateTime.UtcNow,
                HasAutoReservation = tagsModel.IsAutoTag ?? false,
                CreatedAt = tagsModel.CreatedTime ?? DateTime.UtcNow,
                CreatedBy = "TDOC System",
                UpdatedAt = tagsModel.ModifiedTime,
                UpdatedBy = "TDOC System"
            };
        }

        private TagsModel MapToTagsModel(Tag tag)
        {
            return new TagsModel
            {
                TagNumber = tag.TagNumber,
                TagTypeKeyId = tag.TagTypeKeyId,
                IsAutoTag = tag.IsAuto,
                LocationKeyId = tag.LocationKeyId,
                CreatedTime = tag.CreatedAt,
                CreatedByUserKeyId = 1, // Default system user
                ModifiedTime = tag.UpdatedAt,
                ModifiedByUserKeyId = 1 // Default system user
            };
        }

        private void UpdateTagsModel(TagsModel tagsModel, Tag tag)
        {
            tagsModel.TagNumber = tag.TagNumber;
            tagsModel.TagTypeKeyId = tag.TagTypeKeyId;
            tagsModel.IsAutoTag = tag.IsAuto;
            tagsModel.LocationKeyId = tag.LocationKeyId;
            tagsModel.ModifiedTime = tag.UpdatedAt;
            tagsModel.ModifiedByUserKeyId = 1; // Default system user
        }

        private async Task<int> GenerateNextTagNumberAsync(TagType tagType)
        {
            var lastTag = await _context.Tags
                .Where(t => t.TagTypeKeyId == (int)tagType)
                .OrderByDescending(t => t.TagNumber)
                .FirstOrDefaultAsync();

            return (lastTag?.TagNumber ?? 0) + 1;
        }

        #endregion
    }
}
