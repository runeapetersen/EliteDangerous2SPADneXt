using System;

namespace EliteDangerous2SPADneXt.GameState
{
    /// <summary>
    /// Represents various status flags indicating the player's current state in the game, such as
    /// being on foot, in a taxi, experiencing extreme temperatures, or oxygen depletion.
    /// Flags can be combined using bitwise operations as this enumeration has the Flags attribute.
    /// </summary>
    [Flags]
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    public enum EdFlags2
    {
        OnFoot = 1 << 0,
        InTaxi = 1 << 1,
        InMulticrew = 1 << 2,
        OnFootInStation = 1 << 3,
        OnFootOnPlanet = 1 << 4,
        AimDownSights = 1 << 5,
        LowOxygen = 1 << 6,
        LowHealth = 1 << 7,
        Cold = 1 << 8,
        Hot = 1 << 9,
        VeryCold = 1 << 10,
        VeryHot = 1 << 11,
        Glide_Mode = 1 << 12,
        OnFootInHangar = 1 << 13,
        OnFootSocialSpace = 1 << 14,
        OnFootExterior = 1 << 15,
        BreathableAtmosphere = 1 << 16,
        Telepresence_Multicrew = 1 << 17,
        Physical_Multicrew = 1 << 18,
        Fsd_Hyperdrive_Charging = 1 << 19,
    }
}