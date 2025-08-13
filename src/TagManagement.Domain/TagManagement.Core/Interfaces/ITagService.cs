using TagManagement.Core.Entities;
using TagManagement.Core.Enums;

namespace TagManagement.Core.Interfaces
{
    /// <summary>
    /// Tag service interface. Converted from Delphi TProcessHandlerTagHandler functionality.
    /// </summary>
    public interface ITagService
    {
        // Tag Management
        Task<Tag?> GetTagAsync(int tagId);
        Task<Tag?> GetTagByNumberAndTypeAsync(int tagNumber, TagType tagType);
        Task<List<Tag>> GetTagsAsync(int page = 1, int pageSize = 50);
        Task<Tag> CreateTagAsync(TagType tagType, int locationKeyId, bool isAuto = false);
        Task<bool> DeleteTagAsync(int tagId);

        // Tag Validation
        Task<bool> IsValidTagAsync(Tag tag, IEnumerable<TagType> validTypes, 
            IEnumerable<TagType> mustHaveContent, 
            IEnumerable<TagContentCondition> requiredContentCondition);

        // Auto Tag Management
        Task<Tag> StartAutoTagAsync(TagType tagType, int locationKeyId, int userKeyId);
        Task<bool> StopAutoTagAsync(TagType tagType);
        Task StopAllTagsAsync();
        Task<Tag?> ReserveEmptyAutoTagAsync(TagType tagType, int locationKeyId);
        Task ReleaseAutoTagReservationAsync(int tagId);

        // Content Management
        Task<bool> InsertUnitInTagAsync(int tagId, int unitId, DateTime time, bool hideMoves = false, bool markAsSplit = false);
        Task<bool> InsertItemInTagAsync(int tagId, TagItem item, DateTime time, bool hideMoves = false);
        Task<bool> InsertTagInTagAsync(int sourceTagId, int targetTagId, DateTime time, bool hideMoves = false);
        Task<bool> InsertIndicatorInTagAsync(int tagId, int indicatorId, DateTime time, bool hideMoves = false);

        Task<bool> RemoveUnitFromTagAsync(int tagId, int unitId, DateTime time);
        Task<bool> RemoveItemFromTagAsync(int tagId, TagItem item, DateTime time);
        Task<bool> RemoveTagFromTagAsync(int sourceTagId, int targetTagId, DateTime time);
        Task<bool> RemoveIndicatorFromTagAsync(int tagId, int indicatorId, DateTime time);

        Task<bool> RemoveUnitFromAnyTagAsync(int unitId, DateTime time);
        Task<bool> RemoveTagFromAnyTagAsync(int tagId, DateTime time);
        Task<bool> RemoveIndicatorFromAnyTagAsync(int indicatorId, DateTime time);

        // Tag Operations
        Task<bool> MoveUnitToTransportBoxTagAsync(int unitId, int transportBoxTagId, DateTime time, bool forceDispatch = false);
        Task<bool> MoveBundleTagToTransportBoxTagAsync(int bundleTagId, int transportBoxTagId, DateTime time, bool forceDispatch = false);
        Task<bool> MoveTagToTagAsync(int sourceTagId, int targetTagId, DateTime time);
        Task<bool> MoveTagToTransportTagAsync(int tagId, int transportTagId, DateTime time);
        
        // Tag Content Queries
        Task<bool> IsTagEmptyAsync(int tagId, bool ignoreSplit = false);
        Task<bool> IsUnitInTagAsync(int unitId, int tagId);
        Task<List<Tag>> GetUnitTagsAsync(int unitId);
        Task<bool> UnitIsSplitToTagsAsync(int unitId);
        
        // Tag Dissolution
        Task DissolveTagAsync(int tagId, DateTime time, int locationKeyId);
        Task<bool> DissolveLinkedSplitTagsAsync(int tagId, DateTime time, int locationKeyId);
        Task ClearTagContentsAsync(int tagId, DateTime time);

        // Utility Methods
        Task LoadTagContentsAsync(int tagId, bool recursive = false);
        Task<int> GetTagContentCountAsync(int tagId);
        Task<string> GetTagDisplayStringAsync(int tagId);
    }
}
