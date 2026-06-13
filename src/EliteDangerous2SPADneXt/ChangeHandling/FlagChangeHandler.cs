using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Handles the detection and processing of changes in a flag-based enumeration.
    /// This class is specifically designed to work with enumerations marked with the
    /// <c>Flags</c> attribute, enabling granular tracking of flag state transitions.
    /// </summary>
    /// <typeparam name="T">
    /// An enumeration type that represents the set of flags to be handled.
    /// The type must be adorned with the <c>Flags</c> attribute.
    /// </typeparam>
    /// <exception cref="ArgumentException">
    /// Thrown when the type parameter <typeparamref name="T"/> is not marked with the <c>Flags</c> attribute.
    /// </exception>
    public class FlagChangeHandler<T> where T : Enum
    {
        private readonly T _currentFlags;
        private readonly T[] _allValues;
        private readonly Dictionary<T, string> _variableNames;

        public FlagChangeHandler(T initialValues) : this()
        {
            _currentFlags = initialValues;
        }

        public FlagChangeHandler()
        {
            if (!typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Any())
            {
                throw new ArgumentException("Type T must have a Flags attribute for this handler to work");
            }

            _currentFlags = default;
            var valueCount = Enum.GetNames(typeof(T)).Length;
            List<T> allValues = new List<T>(valueCount);
            for (int i = 0; i < valueCount; i++)
            {
                allValues.Add((T)Enum.ToObject(typeof(T), 1 << i));
            }

            _allValues = allValues.ToArray();
            _variableNames = allValues.ToDictionary(v => v, EnumVariableNameHelper.Build);
        }

        public IEnumerable<UpdatedValue> HandleUpdate(T flagsEnum)
        {
            var changedFields = new List<UpdatedValue>();
            foreach (var v in _allValues)
            {
                if (!_currentFlags.HasFlag(v) && flagsEnum.HasFlag(v))
                {
                    changedFields.Add(new UpdatedValue(_variableNames[v], 1));
                }
                else if (_currentFlags.HasFlag(v) && !flagsEnum.HasFlag(v))
                {
                    changedFields.Add(new UpdatedValue(_variableNames[v], 0));
                }
            }

            return changedFields;
        }

        public IEnumerable<UpdatedValue> GetCurrentValues()
        {
            return _variableNames.Select(v => new UpdatedValue(v.Value, _currentFlags.HasFlag(v.Key) ? 1 : 0));
        }
    }
}