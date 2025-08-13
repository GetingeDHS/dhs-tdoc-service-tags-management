using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagManagement.Infrastructure.Persistence.Models
{
    [Table("TUNIT")]
    public class UnitModel
    {
        [Key]
        [Column("UNITKEY")]
        public int UnitKeyId { get; set; }

        [Column("UNITNUMBER")]
        public int? UnitNumber { get; set; }

        [Column("SERIALNUMBER")]
        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [Column("LOCATIONKEY")]
        public int? LocationKeyId { get; set; }

        [Column("ITEMKEY")]
        public int? ItemKeyId { get; set; }

        [Column("CUSTOMERKEY")]
        public int? CustomerKeyId { get; set; }

        [Column("PROCESSBATCHKEY")]
        public int? ProcessBatchKeyId { get; set; }

        [Column("STATUS")]
        public int? Status { get; set; }

        [Column("CREATEDTIME")]
        public DateTime? CreatedTime { get; set; }

        [Column("CREATEDUSERKEY")]
        public int? CreatedByUserKeyId { get; set; }

        [Column("MODIFIEDTIME")]
        public DateTime? ModifiedTime { get; set; }

        [Column("MODIFIEDUSERKEY")]
        public int? ModifiedByUserKeyId { get; set; }

        // Navigation properties
        public virtual LocationModel? Location { get; set; }
        public virtual ItemModel? Item { get; set; }
        public virtual CustomerModel? Customer { get; set; }
        public virtual ICollection<TagContentModel> TagContents { get; set; } = new List<TagContentModel>();
    }
}
