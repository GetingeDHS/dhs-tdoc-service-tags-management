using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagManagement.Infrastructure.Persistence.Models
{
    [Table("TTAGCONTENT")]
    public class TagContentModel
    {
        [Key]
        [Column("TAGCONTENTKEY")]
        public int TagContentKeyId { get; set; }

        [Column("PARENTTAGKEY")]
        public int ParentTagKeyId { get; set; }

        [Column("CHILDTAGKEY")]
        public int? ChildTagKeyId { get; set; }

        [Column("UNITKEY")]
        public int? UnitKeyId { get; set; }

        [Column("ITEMKEY")]
        public int? ItemKeyId { get; set; }

        [Column("SERIALKEY")]
        public int? SerialKeyId { get; set; }

        [Column("LOTINFOKEY")]
        public int? LotInfoKeyId { get; set; }

        [Column("INDICATORKEY")]
        public int? IndicatorKeyId { get; set; }

        [Column("LOCATIONKEY")]
        public int? LocationKeyId { get; set; }

        [Column("CREATEDTIME")]
        public DateTime? CreatedTime { get; set; }

        [Column("CREATEDUSERKEY")]
        public int? CreatedByUserKeyId { get; set; }

        [Column("MODIFIEDTIME")]
        public DateTime? ModifiedTime { get; set; }

        [Column("MODIFIEDUSERKEY")]
        public int? ModifiedByUserKeyId { get; set; }

        // Navigation properties
        public virtual TagsModel ParentTag { get; set; } = null!;
        public virtual TagsModel? ChildTag { get; set; }
        public virtual UnitModel? Unit { get; set; }
        public virtual ItemModel? Item { get; set; }
        public virtual LocationModel? Location { get; set; }
        public virtual IndicatorModel? Indicator { get; set; }
    }
}
