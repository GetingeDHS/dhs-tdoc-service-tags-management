namespace TagManagement.Core.Enums
{
    /// <summary>
    /// Represents the state of a tag split operation
    /// </summary>
    public enum SplitState
    {
        Off = 0,
        Split = 1,
        SplitAdd = 2
    }

    /// <summary>
    /// Represents the split mode for tags
    /// </summary>
    public enum SplitMode
    {
        None = 0,
        Simple = 1,
        Advanced = 2
    }

    /// <summary>
    /// Represents the content condition of a tag
    /// </summary>
    public enum TagContentCondition
    {
        Empty = 0,
        Units = 1,
        Items = 2,
        Mixed = 3
    }

    /// <summary>
    /// Represents the life status of a tag
    /// </summary>
    public enum LifeStatus
    {
        Active = 0,
        Inactive = 1,
        Dead = 2
    }

    /// <summary>
    /// Represents the content type in a tag
    /// </summary>
    public enum TagContentType
    {
        Unit = 0,
        Item = 1,
        Tag = 2,
        Indicator = 3
    }

    /// <summary>
    /// Represents scan options for tag processing
    /// </summary>
    public enum ScanOption
    {
        Normal = 0,
        FastTrack = 1,
        Advanced = 2
    }

    /// <summary>
    /// Represents the move state for tags
    /// </summary>
    public enum TagMoveState
    {
        None = 0,
        TargetTransport = 1,
        TransportSource = 2,
        Target = 3,
        Source = 4
    }

    /// <summary>
    /// Represents log types for the system
    /// </summary>
    public enum LogType
    {
        Normal = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        FastTrack = 4
    }

    /// <summary>
    /// Unit status enumeration
    /// </summary>
    public enum UnitStatus
    {
        New = 0,
        Dirty = 1,
        InWash = 2,
        Clean = 3,
        InSterilization = 4,
        Sterile = 5,
        InUse = 6,
        Expired = 7,
        Maintenance = 8
    }
}
