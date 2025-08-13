using TagManagement.Core.Entities;
using TagManagement.Core.Enums;

namespace TagManagement.Core.Interfaces
{
    /// <summary>
    /// Repository interface for tag data access
    /// </summary>
    public interface ITagRepository
    {
        // Basic CRUD operations
        Task<Tag?> GetByIdAsync(int id);
        Task<Tag?> GetByNumberAndTypeAsync(int tagNumber, TagType tagType);
        Task<IEnumerable<Tag>> GetAllAsync();
        Task<IEnumerable<Tag>> GetPagedAsync(int page, int pageSize);
        Task<Tag> AddAsync(Tag tag);
        Task<Tag> UpdateAsync(Tag tag);
        Task<bool> DeleteAsync(int id);
        
        // Tag-specific queries
        Task<IEnumerable<Tag>> GetTagsByTypeAsync(TagType tagType);
        Task<IEnumerable<Tag>> GetTagsByLocationAsync(int locationKeyId);
        Task<IEnumerable<Tag>> GetAutoTagsAsync();
        Task<IEnumerable<Tag>> GetTagsWithReservationsAsync();
        Task<Tag?> GetEmptyAutoTagAsync(TagType tagType, int locationKeyId);
        
        // Content-related queries
        Task<IEnumerable<Tag>> GetTagsContainingUnitAsync(int unitId);
        Task<IEnumerable<Tag>> GetTagsContainingItemAsync(int itemKeyId, int serialKeyId);
        Task<bool> IsUnitInAnyTagAsync(int unitId);
        Task<bool> IsItemInAnyTagAsync(int itemKeyId, int serialKeyId);
        Task<int> GetTagContentCountAsync(int tagId);
        Task<bool> IsTagEmptyAsync(int tagId);
        
        // Tag hierarchy queries
        Task<IEnumerable<Tag>> GetChildTagsAsync(int parentTagId);
        Task<Tag?> GetParentTagAsync(int childTagId);
        Task<IEnumerable<Tag>> GetRootTagsAsync();
        Task<int> GetRootTagIdAsync(int tagId);
        
        // Split tag queries
        Task<IEnumerable<Tag>> GetLinkedSplitTagsAsync(int tagId);
        Task<int?> GetSplitUnitSerialNumberSplitTagAsync(int unitId);
        
        // Tag relations
        Task<bool> AddUnitToTagAsync(int tagId, int unitId, DateTime time, int locationKeyId, bool markAsSplit = false);
        Task<bool> RemoveUnitFromTagAsync(int tagId, int unitId, DateTime time, int locationKeyId);
        Task<bool> AddItemToTagAsync(int tagId, TagItem item, DateTime time, int locationKeyId);
        Task<bool> RemoveItemFromTagAsync(int tagId, TagItem item, DateTime time, int locationKeyId);
        Task<bool> AddTagToTagAsync(int parentTagId, int childTagId, DateTime time, int locationKeyId);
        Task<bool> RemoveTagFromTagAsync(int parentTagId, int childTagId, DateTime time, int locationKeyId);
        Task<bool> AddIndicatorToTagAsync(int tagId, int indicatorId, DateTime time, int locationKeyId);
        Task<bool> RemoveIndicatorFromTagAsync(int tagId, int indicatorId, DateTime time, int locationKeyId);
        
        // Batch operations
        Task<bool> DissolveTagAsync(int tagId, DateTime time, int locationKeyId);
        Task<bool> ClearTagContentsAsync(int tagId, DateTime time, int locationKeyId);
        Task<bool> MoveTagContentToTransportTagAsync(int sourceTagId, int transportTagId, DateTime time, int locationKeyId);
        
        // Auto tag management
        Task<int> ReserveAutoTagAsync(TagType tagType, int locationKeyId);
        Task<bool> ReleaseAutoTagReservationAsync(int tagId);
        Task<IEnumerable<Tag>> GetReservedAutoTagsAsync();
    }
}
