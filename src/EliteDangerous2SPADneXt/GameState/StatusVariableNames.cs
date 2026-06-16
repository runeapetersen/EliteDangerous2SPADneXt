namespace EliteDangerous2SPADneXt.GameState
{
    /// <summary>
    /// Provides a collection of string constants representing the names of various
    /// status variables in the game state. These constants are used for mapping
    /// game state properties to their respective identifiers.
    /// </summary>
    public static class StatusVariableNames
    {
        // note: EdFlags enumerations are elided from this convenience class.
        // The names are provided by the enumeration change handlers.
        public static class Pips
        {
            public static string Sys => $"{nameof(Status.Pips)}_{nameof(Sys)}";
            public static string Eng => $"{nameof(Status.Pips)}_{nameof(Eng)}";
            public static string Wep => $"{nameof(Status.Pips)}_{nameof(Wep)}";
        }

        public static string FireGroup => nameof(Status.FireGroup);
        public static string GuiFocus => nameof(Status.GuiFocus);

        public static class Fuel
        {
            public static string FuelMain => $"{nameof(Status.Fuel)}_{nameof(FuelMain)}";
            public static string FuelReservoir => $"{nameof(Status.Fuel)}_{nameof(FuelReservoir)}";
        }

        public static string Cargo => nameof(Status.Cargo);
        public static string LegalState => nameof(Status.LegalState);
        public static string Latitude => nameof(Status.Latitude);
        public static string Altitude => nameof(Status.Altitude);
        public static string Longitude => nameof(Status.Longitude);
        public static string Heading => nameof(Status.Heading);
        public static string BodyName => nameof(Status.BodyName);
        public static string PlanetRadius => nameof(Status.PlanetRadius);
        public static string Balance => nameof(Status.Balance);

        public static class Destination
        {
            public static string System => $"{nameof(Status.Destination)}_{nameof(System)}";
            public static string Body => $"{nameof(Status.Destination)}_{nameof(Body)}";
            public static string Name => $"{nameof(Status.Destination)}_{nameof(Name)}";
        }

        public static string Oxygen => nameof(Status.Oxygen);
        public static string Health => nameof(Status.Health);
        public static string Temperature => nameof(Status.Temperature);
        public static string SelectedWeapon => nameof(Status.SelectedWeapon);
        public static string Gravity => nameof(Status.Gravity);
    }
}