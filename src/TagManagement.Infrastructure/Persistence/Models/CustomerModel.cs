using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagManagement.Infrastructure.Persistence.Models
{
    [Table("TCUSTOMER")]
    public class CustomerModel
    {
        [Key]
        [Column("CUSTOMERKEY")]
        public int CustomerKeyId { get; set; }

        [Column("CUSTOMERNUMBER")]
        [MaxLength(50)]
        public string? CustomerNumber { get; set; }

        [Column("CUSTOMERNAME")]
        [MaxLength(200)]
        public string? CustomerName { get; set; }

        [Column("CUSTOMERCODE")]
        [MaxLength(50)]
        public string? CustomerCode { get; set; }

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
        public virtual ICollection<ItemModel> Items { get; set; } = new List<ItemModel>();
        public virtual ICollection<UnitModel> Units { get; set; } = new List<UnitModel>();
    }
}
