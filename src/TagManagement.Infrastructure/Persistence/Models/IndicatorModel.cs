using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagManagement.Infrastructure.Persistence.Models
{
    [Table("TINDICATOR")]
    public class IndicatorModel
    {
        [Key]
        [Column("INDICATORKEY")]
        public int IndicatorKeyId { get; set; }

        [Column("INDICATORNUMBER")]
        [MaxLength(50)]
        public string? IndicatorNumber { get; set; }

        [Column("INDICATORNAME")]
        [MaxLength(200)]
        public string? IndicatorName { get; set; }

        [Column("DESCRIPTION")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("INDICATORTYPE")]
        public int? IndicatorType { get; set; }

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
        public virtual ICollection<TagContentModel> TagContents { get; set; } = new List<TagContentModel>();
    }
}
