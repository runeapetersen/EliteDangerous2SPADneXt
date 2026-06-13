using JetBrains.Annotations;

namespace EliteDangerous2SPADneXt.GameState
{
    /// <summary>
    /// Represents a destination in the game state, including system, body, and name details.
    /// </summary>
    
    [UsedImplicitly]
    public class Destination
    {
        public string System { get; set; }
        public string Body { get; set; }
        public string Name { get; set; }
    }
}