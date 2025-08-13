using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagManagement.Infrastructure.Persistence.Models
{
    [Table("TTAGS")]
    public class TagsModel
    {
        [Key]
        [Column("TAGKEY")]
        public int TagKeyId { get; set; }

        [Column("TAGNUMBER")]
        public int? TagNumber { get; set; }

        [Column("TAGTYPEKEY")]
        public int? TagTypeKeyId { get; set; }

        [Column("LOCATIONKEY")]
        public int? LocationKeyId { get; set; }

        [Column("PROCESSBATCHKEY")]
        public int? ProcessBatchKeyId { get; set; }

        [Column("ISAUTOTAG")]
        public bool? IsAutoTag { get; set; }

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
        public virtual TagTypeModel? TagType { get; set; }
        public virtual ICollection<TagContentModel> TagContents { get; set; } = new List<TagContentModel>();
    }
}
