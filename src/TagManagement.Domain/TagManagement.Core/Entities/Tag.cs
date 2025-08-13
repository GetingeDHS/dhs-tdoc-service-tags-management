using TagManagement.Core.Enums;

namespace TagManagement.Core.Entities
{
    /// <summary>
    /// Represents a tag in the system. Converted from Delphi TBOTags class.
    /// </summary>
    public class Tag
    {
        public int Id { get; set; }
        public int TagNumber { get; set; }
        public TagType TagType { get; set; }
        public int TagTypeKeyId { get; set; }
        public bool IsAuto { get; set; }
        public LifeStatus Status { get; set; }
        public int LocationKeyId { get; set; }
        public DateTime? LocationTime { get; set; }
        public bool HoldsItems { get; set; }
        public bool HasAutoReservation { get; set; }
        public int InTagGroupKeyId { get; set; }
        
        // Content tracking
        public TagContents Contents { get; set; } = new();
        
        // Audit fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Gets the display string for the tag
        /// </summary>
        public string DisplayString => $"{TagType.GetDisplayName()} #{TagNumber}";

        /// <summary>
        /// Gets the full display string for the tag including auto indicator
        /// </summary>
        public string FullDisplayString => IsAuto ? $"[AUTO] {DisplayString}" : DisplayString;

        /// <summary>
        /// Checks if the tag is empty (no content)
        /// </summary>
        public bool IsEmpty => Contents.IsEmpty;

        /// <summary>
        /// Gets the content condition of the tag
        /// </summary>
        public TagContentCondition ContentCondition
        {
            get
            {
                if (Contents.IsEmpty)
                    return TagContentCondition.Empty;

                bool hasUnits = Contents.Units.Any();
                bool hasItems = Contents.Items.Any();

                if (hasUnits && hasItems)
                    return TagContentCondition.Mixed;
                if (hasUnits)
                    return TagContentCondition.Units;
                if (hasItems)
                    return TagContentCondition.Items;

                return TagContentCondition.Empty;
            }
        }
    }
}
