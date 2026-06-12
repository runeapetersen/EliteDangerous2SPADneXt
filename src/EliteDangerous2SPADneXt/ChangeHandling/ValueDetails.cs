using System;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Represents a generic class for handling and monitoring value changes
    /// for a specific type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value being handled. The type must implement the
    /// <see cref="IComparable"/> interface.
    /// </typeparam>
    public class ValueDetails<T> : IValueDetails where T : IComparable
    {
        public ValueDetails()
        {
            var wrappedType = typeof(T);
            if (wrappedType == typeof(string))
                CurrentValue = string.Empty;
            else
                CurrentValue = default(T);
        }
        
        public ValueDetails(T defaultValue = default(T))
        {
            CurrentValue = defaultValue;
        }

        public IComparable CurrentValue { get; set; }

        public bool HandleUpdate(IComparable newValue)
        {
            if (newValue == null)
                throw new ArgumentException(
                    "New value cannot be null when nullability is disallowed for this instance.");
            if (!newValue.GetType().IsAssignableFrom(WrappedType))
                throw new ArgumentException("New value type does not match the default value type.");
            if (newValue.CompareTo(CurrentValue) == 0)
                return false;
            CurrentValue = newValue;
            return true;
        }

        private Type WrappedType => typeof(T);
    }
}