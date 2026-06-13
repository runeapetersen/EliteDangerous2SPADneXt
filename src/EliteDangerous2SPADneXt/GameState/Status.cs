using System;

// ReSharper disable UnusedMember.Global
namespace EliteDangerous2SPADneXt.GameState
{
    /// <summary>
    /// Represents the current state of the game, encapsulating various attributes like position, status flags, fuel levels,
    /// environmental data, and player-related information.
    /// </summary>
    public class Status
    {
        public DateTimeOffset TimeStamp { get; set; } // do not map to SPAD. It is presented as a string in SPAD and has no inherent usefulness
        public string Event { get; set; } // do not map to SPAD. It is presented as a string in SPAD and has no inherent usefulness

        public EdFlags Flags { get; set; }
        public EdFlags2 Flags2 { get; set; }
        public double[] Pips { get; set; }
        public double FireGroup { get; set; }
        public double GuiFocus { get; set; }
        public Fuel Fuel { get; set; }
        public double Cargo { get; set; }
        public string LegalState { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }
        public double Longitude { get; set; }
        public double Heading { get; set; }
        public string BodyName { get; set; }
        public double PlanetRadius { get; set; }
        public double Balance { get; set; }
        public Destination Destination { get; set; }
        public double Oxygen { get; set; }
        public double Health { get; set; }
        public double Temperature { get; set; }
        public string SelectedWeapon { get; set; }
        public double Gravity { get; set; }
    }
}