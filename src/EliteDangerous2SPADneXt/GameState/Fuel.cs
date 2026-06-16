using JetBrains.Annotations;

namespace EliteDangerous2SPADneXt.GameState
{
    /// <summary>
    /// Represents the fuel-related attributes of a ship in the game.
    /// </summary>
    [UsedImplicitly]
    public class Fuel
    {
        public double FuelMain { get; set; }
        public double FuelReservoir { get; set; }
    }
}