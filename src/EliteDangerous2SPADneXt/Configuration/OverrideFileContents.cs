namespace EliteDangerous2SPADneXt.Configuration
{
    /// <summary>
    /// Represents configuration settings for overriding the default file containing the status information
    /// for the Elite Dangerous game. This class is used to specify an alternate file path for the status file
    /// location through the "StatusFilePathOverride" property.
    /// </summary>
    public class OverrideFileContents
    {
        public string StatusFilePathOverride { get; set; }
    }
}