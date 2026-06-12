namespace EliteDangerous2SPADneXt.GameState
{
    /// <summary>
    /// Represents the fuel-related attributes of a ship in the game.
    /// </summary>
    public class Fuel
    {
        /// <summary>
        /// the current level of fuel in the ship's main fuel tank.
        /// </summary>
        public double FuelMain { get; set; }

        /// <summary>
        /// the current level of fuel in the ship's reserve fuel tank.
        /// </summary>
        public double FuelReservoir { get; set; }
    }
}