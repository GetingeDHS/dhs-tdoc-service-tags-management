namespace TagManagement.Core.Entities
{
    /// <summary>
    /// Represents an item within a tag. Converted from Delphi TTagItem class.
    /// </summary>
    public class TagItem
    {
        public int ItemKeyId { get; set; }
        public int SerialKeyId { get; set; }
        public int LotInfoKeyId { get; set; }
        public int Count { get; set; }

        public TagItem()
        {
        }

        public TagItem(int itemKeyId, int serialKeyId, int lotInfoKeyId, int count)
        {
            ItemKeyId = itemKeyId;
            SerialKeyId = serialKeyId;
            LotInfoKeyId = lotInfoKeyId;
            Count = count;
        }

        /// <summary>
        /// Creates a copy of this TagItem
        /// </summary>
        public TagItem CreateCopy()
        {
            return new TagItem(ItemKeyId, SerialKeyId, LotInfoKeyId, Count);
        }

        public override bool Equals(object? obj)
        {
            return obj is TagItem other &&
                   ItemKeyId == other.ItemKeyId &&
                   SerialKeyId == other.SerialKeyId &&
                   LotInfoKeyId == other.LotInfoKeyId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemKeyId, SerialKeyId, LotInfoKeyId);
        }
    }
}
