using Microsoft.AspNetCore.Mvc;
using TagManagement.Core.Entities;
using TagManagement.Core.Enums;
using TagManagement.Core.Interfaces;

namespace TagManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ITagService tagService, ILogger<TagsController> logger)
        {
            _tagService = tagService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all tags with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var tags = await _tagService.GetTagsAsync(page, pageSize);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags");
                return StatusCode(500, "An error occurred while retrieving tags");
            }
        }

        /// <summary>
        /// Gets a specific tag by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTag(int id)
        {
            try
            {
                var tag = await _tagService.GetTagAsync(id);
                if (tag == null)
                {
                    return NotFound($"Tag with ID {id} not found");
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag {TagId}", id);
                return StatusCode(500, "An error occurred while retrieving the tag");
            }
        }

        /// <summary>
        /// Gets a tag by number and type
        /// </summary>
        [HttpGet("number/{tagNumber}/type/{tagType}")]
        public async Task<ActionResult<Tag>> GetTagByNumberAndType(int tagNumber, TagType tagType)
        {
            try
            {
                var tag = await _tagService.GetTagByNumberAndTypeAsync(tagNumber, tagType);
                if (tag == null)
                {
                    return NotFound($"Tag with number {tagNumber} and type {tagType} not found");
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by number {TagNumber} and type {TagType}", tagNumber, tagType);
                return StatusCode(500, "An error occurred while retrieving the tag");
            }
        }

        /// <summary>
        /// Creates a new tag
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Tag>> CreateTag([FromBody] CreateTagRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tag = await _tagService.CreateTagAsync(request.TagType, request.LocationKeyId, request.IsAuto);
                return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return StatusCode(500, "An error occurred while creating the tag");
            }
        }

        /// <summary>
        /// Deletes a tag
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTag(int id)
        {
            try
            {
                var result = await _tagService.DeleteTagAsync(id);
                if (!result)
                {
                    return BadRequest("Cannot delete tag. It may not exist or may not be empty.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag {TagId}", id);
                return StatusCode(500, "An error occurred while deleting the tag");
            }
        }

        /// <summary>
        /// Starts an auto tag
        /// </summary>
        [HttpPost("auto/start")]
        public async Task<ActionResult<Tag>> StartAutoTag([FromBody] StartAutoTagRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tag = await _tagService.StartAutoTagAsync(request.TagType, request.LocationKeyId, request.UserKeyId);
                return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting auto tag");
                return StatusCode(500, "An error occurred while starting the auto tag");
            }
        }

        /// <summary>
        /// Stops an auto tag
        /// </summary>
        [HttpPost("auto/stop/{tagType}")]
        public async Task<ActionResult> StopAutoTag(TagType tagType)
        {
            try
            {
                var result = await _tagService.StopAutoTagAsync(tagType);
                if (!result)
                {
                    return NotFound($"No active auto tag found for type {tagType}");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping auto tag {TagType}", tagType);
                return StatusCode(500, "An error occurred while stopping the auto tag");
            }
        }

        /// <summary>
        /// Stops all auto tags
        /// </summary>
        [HttpPost("auto/stop-all")]
        public async Task<ActionResult> StopAllTags()
        {
            try
            {
                await _tagService.StopAllTagsAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping all auto tags");
                return StatusCode(500, "An error occurred while stopping all auto tags");
            }
        }

        /// <summary>
        /// Inserts a unit into a tag
        /// </summary>
        [HttpPost("{tagId}/units")]
        public async Task<ActionResult> InsertUnitInTag(int tagId, [FromBody] InsertUnitRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _tagService.InsertUnitInTagAsync(
                    tagId, request.UnitId, request.Time, request.HideMoves, request.MarkAsSplit);

                if (!result)
                {
                    return BadRequest("Failed to insert unit into tag");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting unit {UnitId} into tag {TagId}", request.UnitId, tagId);
                return StatusCode(500, "An error occurred while inserting the unit");
            }
        }

        /// <summary>
        /// Removes a unit from a tag
        /// </summary>
        [HttpDelete("{tagId}/units/{unitId}")]
        public async Task<ActionResult> RemoveUnitFromTag(int tagId, int unitId)
        {
            try
            {
                var result = await _tagService.RemoveUnitFromTagAsync(tagId, unitId, DateTime.UtcNow);
                if (!result)
                {
                    return BadRequest("Failed to remove unit from tag");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing unit {UnitId} from tag {TagId}", unitId, tagId);
                return StatusCode(500, "An error occurred while removing the unit");
            }
        }

        /// <summary>
        /// Inserts an item into a tag
        /// </summary>
        [HttpPost("{tagId}/items")]
        public async Task<ActionResult> InsertItemInTag(int tagId, [FromBody] TagItem item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _tagService.InsertItemInTagAsync(tagId, item, DateTime.UtcNow);
                if (!result)
                {
                    return BadRequest("Failed to insert item into tag");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting item {ItemKeyId} into tag {TagId}", item.ItemKeyId, tagId);
                return StatusCode(500, "An error occurred while inserting the item");
            }
        }

        /// <summary>
        /// Removes an item from a tag
        /// </summary>
        [HttpDelete("{tagId}/items")]
        public async Task<ActionResult> RemoveItemFromTag(int tagId, [FromBody] TagItem item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _tagService.RemoveItemFromTagAsync(tagId, item, DateTime.UtcNow);
                if (!result)
                {
                    return BadRequest("Failed to remove item from tag");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {ItemKeyId} from tag {TagId}", item.ItemKeyId, tagId);
                return StatusCode(500, "An error occurred while removing the item");
            }
        }

        /// <summary>
        /// Inserts a tag into another tag
        /// </summary>
        [HttpPost("{targetTagId}/tags/{sourceTagId}")]
        public async Task<ActionResult> InsertTagInTag(int targetTagId, int sourceTagId, [FromQuery] bool hideMoves = false)
        {
            try
            {
                var result = await _tagService.InsertTagInTagAsync(sourceTagId, targetTagId, DateTime.UtcNow, hideMoves);
                if (!result)
                {
                    return BadRequest("Failed to insert tag into tag");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting tag {SourceTagId} into tag {TargetTagId}", sourceTagId, targetTagId);
                return StatusCode(500, "An error occurred while inserting the tag");
            }
        }

        /// <summary>
        /// Moves a unit to a transport box tag
        /// </summary>
        [HttpPost("transport-box/{transportBoxTagId}/units/{unitId}")]
        public async Task<ActionResult> MoveUnitToTransportBoxTag(int transportBoxTagId, int unitId, [FromQuery] bool forceDispatch = false)
        {
            try
            {
                var result = await _tagService.MoveUnitToTransportBoxTagAsync(unitId, transportBoxTagId, DateTime.UtcNow, forceDispatch);
                if (!result)
                {
                    return BadRequest("Failed to move unit to transport box tag");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving unit {UnitId} to transport box tag {TransportBoxTagId}", unitId, transportBoxTagId);
                return StatusCode(500, "An error occurred while moving the unit");
            }
        }

        /// <summary>
        /// Gets tags containing a specific unit
        /// </summary>
        [HttpGet("units/{unitId}/tags")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetUnitTags(int unitId)
        {
            try
            {
                var tags = await _tagService.GetUnitTagsAsync(unitId);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags for unit {UnitId}", unitId);
                return StatusCode(500, "An error occurred while retrieving unit tags");
            }
        }

        /// <summary>
        /// Checks if a tag is empty
        /// </summary>
        [HttpGet("{tagId}/is-empty")]
        public async Task<ActionResult<bool>> IsTagEmpty(int tagId, [FromQuery] bool ignoreSplit = false)
        {
            try
            {
                var isEmpty = await _tagService.IsTagEmptyAsync(tagId, ignoreSplit);
                return Ok(isEmpty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if tag {TagId} is empty", tagId);
                return StatusCode(500, "An error occurred while checking tag status");
            }
        }

        /// <summary>
        /// Gets the content count of a tag
        /// </summary>
        [HttpGet("{tagId}/content-count")]
        public async Task<ActionResult<int>> GetTagContentCount(int tagId)
        {
            try
            {
                var count = await _tagService.GetTagContentCountAsync(tagId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content count for tag {TagId}", tagId);
                return StatusCode(500, "An error occurred while getting tag content count");
            }
        }

        /// <summary>
        /// Dissolves a tag
        /// </summary>
        [HttpDelete("{tagId}/dissolve")]
        public async Task<ActionResult> DissolveTag(int tagId, [FromQuery] int locationKeyId = 1)
        {
            try
            {
                await _tagService.DissolveTagAsync(tagId, DateTime.UtcNow, locationKeyId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dissolving tag {TagId}", tagId);
                return StatusCode(500, "An error occurred while dissolving the tag");
            }
        }

        /// <summary>
        /// Clears tag contents
        /// </summary>
        [HttpDelete("{tagId}/contents")]
        public async Task<ActionResult> ClearTagContents(int tagId)
        {
            try
            {
                await _tagService.ClearTagContentsAsync(tagId, DateTime.UtcNow);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing contents of tag {TagId}", tagId);
                return StatusCode(500, "An error occurred while clearing tag contents");
            }
        }
    }

    // Request/Response DTOs
    public class CreateTagRequest
    {
        public TagType TagType { get; set; }
        public int LocationKeyId { get; set; }
        public bool IsAuto { get; set; } = false;
    }

    public class StartAutoTagRequest
    {
        public TagType TagType { get; set; }
        public int LocationKeyId { get; set; }
        public int UserKeyId { get; set; }
    }

    public class InsertUnitRequest
    {
        public int UnitId { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public bool HideMoves { get; set; } = false;
        public bool MarkAsSplit { get; set; } = false;
    }
}
