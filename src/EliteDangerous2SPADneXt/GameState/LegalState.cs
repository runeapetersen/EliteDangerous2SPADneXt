// ReSharper disable UnusedMember.Global

namespace EliteDangerous2SPADneXt.GameState
{
    /// <summary>
    /// Represents the legal status, faction standing, or criminal record of the player within the game universe.
    /// </summary>
    /// <remarks>
    /// Tracks compliance with local laws, faction alignments, bounty statuses, and special Thargoid designations.
    /// </remarks>
    public enum LegalState
    {
        None,
        Clean,
        IllegalCargo,
        Speeding,
        Wanted,
        Hostile,
        PassengerWanted,
        Warrant,
        Allied,
        Thargoid
    }
}