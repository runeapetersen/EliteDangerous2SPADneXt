using System;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Represents an interface for managing and monitoring value changes.
    /// Provides mechanisms for handling updates and retrieving the current value.
    /// </summary>
    public interface IValueDetails
    {
        bool HandleUpdate(IComparable newValue);
        IComparable CurrentValue { get; }
    }
}