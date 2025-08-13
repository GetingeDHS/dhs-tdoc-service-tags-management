namespace TagManagement.Core.Entities
{
    /// <summary>
    /// Represents a product in the system
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ItemText { get; set; } = string.Empty;
        public int CustomerKeyId { get; set; }
        public string StorageType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Customer information
        /// </summary>
        public Customer? Customer { get; set; }
    }
}
