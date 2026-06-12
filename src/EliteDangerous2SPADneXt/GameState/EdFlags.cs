using System;

namespace EliteDangerous2SPADneXt.GameState
{
    /// <summary>
    /// Represents various flags indicating the current state or condition of a ship or player
    /// within Elite Dangerous. These flags provide information about the ship's systems,
    /// environment, and other statuses.
    /// </summary>
    /// <remarks>
    /// This enumeration is decorated with the <see cref="FlagsAttribute"/>, allowing
    /// bitwise combination of its member values for representing multiple states simultaneously.
    /// </remarks>
    [Flags]
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    public enum EdFlags
    {
        Docked = 1 << 0,
        Landed_Planet = 1 << 1,
        Landing_Gear_Down = 1 << 2,
        Shields_Up = 1 << 3,
        Supercruise = 1 << 4,
        FlightAssist_Off = 1 << 5,
        Hardpoints_Deployed = 1 << 6,
        In_Wing = 1 << 7,
        LightsOn = 1 << 8,
        Cargo_Scoop_Deployed = 1 << 9,
        Silent_Running = 1 << 10,
        Scooping_Fuel = 1 << 11,
        Srv_Handbrake = 1 << 12,
        Srv_using_Turret_view = 1 << 13,
        Srv_Turret_retracted_close_to_ship = 1 << 14,
        Srv_DriveAssist = 1 << 15,
        Fsd_MassLocked = 1 << 16,
        Fsd_Charging = 1 << 17,
        Fsd_Cooldown = 1 << 18,
        Low_Fuel_less_than_25_percent = 1 << 19,
        Over_Heating_over_100_percent = 1 << 20,
        Has_Lat_Long = 1 << 21,
        IsInDanger = 1 << 22,
        Being_Interdicted = 1 << 23,
        In_MainShip = 1 << 24,
        In_Fighter = 1 << 25,
        In_SRV = 1 << 26,
        Hud_in_Analysis_mode = 1 << 27,
        Night_Vision = 1 << 28,
        Altitude_from_Average_radius = 1 << 29,
        fsdJump = 1 << 30,
        srvHighBeam = 1 << 31
    }
}