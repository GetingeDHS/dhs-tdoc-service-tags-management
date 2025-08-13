namespace TagManagement.Core.Entities
{
    /// <summary>
    /// Represents the contents of a tag. Converted from Delphi TTagContents class.
    /// </summary>
    public class TagContents
    {
        public List<Tag> Tags { get; set; } = new();
        public List<int> Units { get; set; } = new();
        public List<TagItem> Items { get; set; } = new();
        public List<int> Indicators { get; set; } = new();

        /// <summary>
        /// Gets the count of units in the tag
        /// </summary>
        public int UnitCount => Units.Count;

        /// <summary>
        /// Gets the count of nested tags
        /// </summary>
        public int TagCount => Tags.Count;

        /// <summary>
        /// Gets the count of items in the tag
        /// </summary>
        public int ItemCount => Items.Count;

        /// <summary>
        /// Gets the count of indicators in the tag
        /// </summary>
        public int IndicatorCount => Indicators.Count;

        /// <summary>
        /// Checks if the tag contents are empty
        /// </summary>
        public bool IsEmpty => UnitCount == 0 && TagCount == 0 && ItemCount == 0 && IndicatorCount == 0;

        /// <summary>
        /// Clears all contents
        /// </summary>
        public void ClearContents()
        {
            Tags.Clear();
            Units.Clear();
            Items.Clear();
            Indicators.Clear();
        }

        /// <summary>
        /// Removes a unit from the contents
        /// </summary>
        public void RemoveUnit(int unitId)
        {
            Units.Remove(unitId);
        }

        /// <summary>
        /// Removes a tag from the contents
        /// </summary>
        public void RemoveTag(Tag tag)
        {
            Tags.Remove(tag);
        }

        /// <summary>
        /// Removes an item from the contents
        /// </summary>
        public void RemoveItem(int itemKeyId, int serialKeyId, int lotInfoKeyId)
        {
            var itemToRemove = Items.FirstOrDefault(i => 
                i.ItemKeyId == itemKeyId && 
                i.SerialKeyId == serialKeyId && 
                i.LotInfoKeyId == lotInfoKeyId);
            
            if (itemToRemove != null)
            {
                Items.Remove(itemToRemove);
            }
        }

        /// <summary>
        /// Removes an indicator from the contents
        /// </summary>
        public void RemoveIndicator(int indicatorId)
        {
            Indicators.Remove(indicatorId);
        }

        /// <summary>
        /// Gets all contained units recursively
        /// </summary>
        public List<int> GetAllContainedUnits()
        {
            var allUnits = new List<int>(Units);
            
            foreach (var nestedTag in Tags)
            {
                allUnits.AddRange(nestedTag.Contents.GetAllContainedUnits());
            }
            
            return allUnits;
        }
    }
}
