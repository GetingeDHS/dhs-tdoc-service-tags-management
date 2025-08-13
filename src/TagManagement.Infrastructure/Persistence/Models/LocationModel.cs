using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagManagement.Infrastructure.Persistence.Models
{
    [Table("TLOCATION")]
    public class LocationModel
    {
        [Key]
        [Column("LOCATIONKEY")]
        public int LocationKeyId { get; set; }

        [Column("LOCATIONNAME")]
        [MaxLength(100)]
        public string? LocationName { get; set; }

        [Column("LOCATIONCODE")]
        [MaxLength(50)]
        public string? LocationCode { get; set; }

        [Column("DESCRIPTION")]
        [MaxLength(255)]
        public string? Description { get; set; }

        [Column("PARENTLOCATIONKEY")]
        public int? ParentLocationKeyId { get; set; }

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
        public virtual LocationModel? ParentLocation { get; set; }
        public virtual ICollection<LocationModel> ChildLocations { get; set; } = new List<LocationModel>();
        public virtual ICollection<TagsModel> Tags { get; set; } = new List<TagsModel>();
        public virtual ICollection<TagContentModel> TagContents { get; set; } = new List<TagContentModel>();
        public virtual ICollection<UnitModel> Units { get; set; } = new List<UnitModel>();
    }
}
