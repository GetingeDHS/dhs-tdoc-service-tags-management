using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagManagement.Infrastructure.Persistence.Models
{
    [Table("TITEM")]
    public class ItemModel
    {
        [Key]
        [Column("ITEMKEY")]
        public int ItemKeyId { get; set; }

        [Column("ITEMNUMBER")]
        [MaxLength(100)]
        public string? ItemNumber { get; set; }

        [Column("ITEMNAME")]
        [MaxLength(200)]
        public string? ItemName { get; set; }

        [Column("DESCRIPTION")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("CUSTOMERKEY")]
        public int? CustomerKeyId { get; set; }

        [Column("ITEMTYPE")]
        public int? ItemType { get; set; }

        [Column("ISACTIVE")]
        public bool? IsActive { get; set; }

        [Column("CREATEDTIME")]
        public DateTime? CreatedTime { get; set; }

        [Column("CREATEDUSERKEY")]
        public int? CreatedByUserKeyId { get; set; }

        [Column("MODIFIEDTIME")]
        public DateTime? ModifiedTime { get; set; }

        [Column("MODIFIEDUSERKEY")]
        public int? ModifiedByUserKeyId { get; set; }

        // Navigation properties
        public virtual CustomerModel? Customer { get; set; }
        public virtual ICollection<UnitModel> Units { get; set; } = new List<UnitModel>();
        public virtual ICollection<TagContentModel> TagContents { get; set; } = new List<TagContentModel>();
    }
}
