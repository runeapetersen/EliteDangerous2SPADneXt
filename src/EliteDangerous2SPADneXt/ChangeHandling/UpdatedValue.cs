namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Represents a value that has been updated, with an associated name
    /// and data type for tracking changes in a system.
    /// </summary>
    public class UpdatedValue
    {
        public string Name { get; }
        public object Value { get; }
        public SpadDataType DataType { get; }

        public UpdatedValue(string name, object value, SpadDataType dataType)
        {
            Name = name;
            Value = value;
            DataType = dataType;
        }
    }
}