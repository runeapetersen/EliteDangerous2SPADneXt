using System;
using System.Diagnostics.CodeAnalysis;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Represents a value that has been updated, with an associated name
    /// and data type for tracking changes in a system.
    /// </summary>
    public class UpdatedValue : IComparable<UpdatedValue>
    {
        public string Name { get; }
        public IComparable Value { get; }
        public SpadDataType DataType => Value is string ? SpadDataType.STRING : SpadDataType.NUMBER;

        public UpdatedValue(string name, IComparable value)
        {
            Name = name;
            Value = value;
        }

        public int CompareTo(UpdatedValue other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            var nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
            if (nameComparison != 0) return nameComparison;
            return Value.CompareTo(other.Value);
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj as UpdatedValue) == 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum SpadDataType
    {
        NUMBER,
        STRING
    }
}