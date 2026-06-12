using System;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Provides helper methods for handling and generating variable names
    /// based on the values of enumerations. This class is particularly
    /// useful in scenarios where enum values need to be converted into
    /// standardized and predictable string representations.
    /// </summary>
    /// <remarks>
    /// The utility assumes that the enum values are not combined flag values.
    /// If combined flags are provided, an exception will be thrown to signal
    /// unsupported usage.
    /// </remarks>
    public static class EnumVariableNameHelper
    {
        public static string Build<T>(T value) where T : Enum
        {
            var t = typeof(T);
            var name = Enum.GetName(t, value);
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid enum value. Combined flags are not supported.");
            return $"{t.Name}_{name}";
        }
    }
}