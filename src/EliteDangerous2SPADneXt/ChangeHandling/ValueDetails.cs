using System;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    public class StringValueDetails : IValueDetails
    {
        public object CurrentValue { get; private set; }
        public SpadDataType DataType => SpadDataType.STRING;

        public StringValueDetails(object defaultValue = null)
        {
            CurrentValue = defaultValue ?? throw new ArgumentException("Default value cannot be null.", nameof(defaultValue));
        }

        public bool HandleUpdate(object newValue)
        {
            if (!(newValue is string stringValue))
                throw new ArgumentException("Expected a string value.", nameof(newValue));

            if (string.CompareOrdinal((string)CurrentValue, stringValue) == 0)
                return false;

            CurrentValue = stringValue;
            return true;
        }
    }
    public class DoubleValueDetails : IValueDetails
    {
        public object CurrentValue { get; private set; }
        public SpadDataType DataType => SpadDataType.NUMBER;

        public DoubleValueDetails(object defaultValue)
        {
            CurrentValue = defaultValue;
        }

        public bool HandleUpdate(object newValue)
        {
            if (!(newValue is double doubleValue))
                throw new ArgumentException("Expected a double value.", nameof(newValue));

            if (((double)CurrentValue).CompareTo(doubleValue) == 0)
                return false;

            CurrentValue = doubleValue;
            return true;
        }
    }
    public class BoolValueDetails : IValueDetails
    {
        public object CurrentValue { get; private set; }
        public SpadDataType DataType => SpadDataType.BOOL;

        public BoolValueDetails(object defaultValue)
        {
            CurrentValue = defaultValue;
        }

        public bool HandleUpdate(object newValue)
        {
            if (!(newValue is bool boolValue))
                throw new ArgumentException("Expected a boolean value.", nameof(newValue));

            if (((bool)CurrentValue).Equals(boolValue))
                return false;

            CurrentValue = boolValue;
            return true;
        }
    }
}