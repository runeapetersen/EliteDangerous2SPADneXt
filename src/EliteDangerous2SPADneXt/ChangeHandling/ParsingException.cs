using System;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Represents an exception that occurs during the parsing of input data.
    /// </summary>
    /// <remarks>
    /// This exception is used to indicate an error encountered when attempting to parse data,
    /// such as a game state file that contains invalid or improperly formatted content.
    /// </remarks>
    public class ParsingException : Exception
    {
        public ParsingException(string message) : base(message)
        {
        }
    }
}