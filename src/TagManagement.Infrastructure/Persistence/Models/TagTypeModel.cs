using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagManagement.Infrastructure.Persistence.Models
{
    [Table("TTAGTYPE")]
    public class TagTypeModel
    {
        [Key]
        [Column("TAGTYPEKEY")]
        public int TagTypeKeyId { get; set; }

        [Column("TAGTYPENAME")]
        [MaxLength(50)]
        public string? TagTypeName { get; set; }

        [Column("TAGTYPECODE")]
        [MaxLength(10)]
        public string? TagTypeCode { get; set; }

        [Column("DESCRIPTION")]
        [MaxLength(255)]
        public string? Description { get; set; }

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
        public virtual ICollection<TagsModel> Tags { get; set; } = new List<TagsModel>();
    }
}
