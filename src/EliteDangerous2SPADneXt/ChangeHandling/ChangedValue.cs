namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Represents a value that has been identified as changed.
    /// </summary>
    /// <remarks>
    /// Instances of this class are used to encapsulate both the name of the property or value
    /// that has changed and its corresponding new value. It is primarily intended for use in
    /// change-tracking and update-handling mechanisms within the application.
    /// </remarks>
    public class ChangedValue
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}