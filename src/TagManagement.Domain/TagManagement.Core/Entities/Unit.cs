using TagManagement.Core.Enums;

namespace TagManagement.Core.Entities
{
    /// <summary>
    /// Represents a unit in the system. Simplified version based on Delphi TDOUnit.
    /// </summary>
    public class Unit
    {
        public int Id { get; set; }
        public int UnitNumber { get; set; }
        public UnitStatus Status { get; set; }
        public int ProductKeyId { get; set; }
        public int CustomerKeyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets the status as a display string
        /// </summary>
        public string StatusDisplayString => Status switch
        {
            UnitStatus.New => "New",
            UnitStatus.Dirty => "Dirty",
            UnitStatus.InWash => "In Wash",
            UnitStatus.Clean => "Clean",
            UnitStatus.InSterilization => "In Sterilization",
            UnitStatus.Sterile => "Sterile",
            UnitStatus.InUse => "In Use",
            UnitStatus.Expired => "Expired",
            UnitStatus.Maintenance => "Maintenance",
            _ => Status.ToString()
        };

        /// <summary>
        /// Product information (simplified)
        /// </summary>
        public Product? Product { get; set; }
    }
}
