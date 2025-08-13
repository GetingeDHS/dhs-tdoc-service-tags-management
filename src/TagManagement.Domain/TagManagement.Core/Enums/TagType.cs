namespace TagManagement.Core.Enums
{
    /// <summary>
    /// Represents different types of tags in the system.
    /// Converted from Delphi TTagType enum.
    /// </summary>
    public enum TagType
    {
        PrepTag = 0,
        Bundle = 1,
        Basket = 2,
        SteriLoad = 3,
        Wash = 4,
        WashLoad = 5,
        Transport = 6,
        CaseCart = 7,
        TransportBox = 8,
        InstrumentContainer = 9,
        
        // Aliases for backward compatibility with unit tests
        BundleTag = Bundle,
        WashTag = Wash,
        SterilizationLoadTag = SteriLoad,
        TransportTag = Transport,
        TransportBoxTag = TransportBox
    }

    /// <summary>
    /// Extension methods for TagType enum
    /// </summary>
    public static class TagTypeExtensions
    {
        public static bool IsAutoTag(this TagType tagType)
        {
            return tagType switch
            {
                TagType.PrepTag => true,
                TagType.Bundle => true,
                TagType.Basket => true,
                TagType.SteriLoad => true,
                TagType.Wash => true,
                TagType.WashLoad => true,
                TagType.Transport => true,
                TagType.TransportBox => true,
                _ => false
            };
        }

        public static string GetDisplayName(this TagType tagType)
        {
            return tagType switch
            {
                TagType.PrepTag => "Prep Tag",
                TagType.Bundle => "Bundle",
                TagType.Basket => "Basket",
                TagType.SteriLoad => "Sterilization Load",
                TagType.Wash => "Wash",
                TagType.WashLoad => "Wash Load",
                TagType.Transport => "Transport",
                TagType.CaseCart => "Case Cart",
                TagType.TransportBox => "Transport Box",
                TagType.InstrumentContainer => "Instrument Container",
                _ => tagType.ToString()
            };
        }
    }
}
