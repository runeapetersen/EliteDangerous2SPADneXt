using System;
using System.Collections.Generic;
using EliteDangerous2SPADneXt.GameState;
using SPAD.neXt.Interfaces.Logging;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Represents a container that manages and tracks the state of various status values,
    /// providing mechanisms for accessing and applying updates to these values.
    /// </summary>
    public class StateContainer
    {
        private readonly IDictionary<string, IValueDetails> _statusValues = new Dictionary<string, IValueDetails>();
        private readonly FlagChangeHandler<EdFlags> _edFlagsHandler = new FlagChangeHandler<EdFlags>();
        private readonly FlagChangeHandler<EdFlags2> _edFlags2Handler = new FlagChangeHandler<EdFlags2>();
        private bool _isInitialSyncCompleted;
        private readonly ILogger _logger;

        public StateContainer(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<UpdatedValue> Apply(Status status)
        {
            var isFirstRun = !_isInitialSyncCompleted;
            var updateBucket = new List<UpdatedValue>(128);
            foreach (var (name, extract, dataType) in FieldMap)
            {
                try
                {
                    object value = extract(status);

                    bool isUpdate;
                    if (!_statusValues.TryGetValue(name, out var statusValue))
                    {
                        _statusValues[name] = CreateValueDetails(value);
                        isUpdate = true;
                    }
                    else
                    {
                        // this fails at runtime in interesting ways. It seems there's a difference in runtime handling???    
                        isUpdate = statusValue.HandleUpdate(value);
                    }

                    if (isUpdate)
                        updateBucket.Add(new UpdatedValue(name, value, dataType));
                }
                catch (Exception e)
                {
                    _logger.Error($"Caught exception {e} while trying to handle {name}");
                }
            }

            updateBucket.AddRange(_edFlagsHandler.HandleUpdate(status.Flags, isFirstRun));
            updateBucket.AddRange(_edFlags2Handler.HandleUpdate(status.Flags2, isFirstRun));

            if (isFirstRun) _isInitialSyncCompleted = true;

            return updateBucket;
        }

        private static IValueDetails CreateValueDetails(object value)
        {
            if (value is string)
                return new StringValueDetails(value);
            if (value is bool)
                return new BoolValueDetails(value);
            if (value is double || value is int)
                return new DoubleValueDetails(value);
            else throw new NotSupportedException("Unsupported value type");
        }

        private static readonly (string Name, Func<Status, object> Extractor, SpadDataType targetDataType)[] FieldMap =
            {
                (StatusVariableNames.Pips.Sys, s => s.Pips?[0] ?? 0.0, SpadDataType.NUMBER),
                (StatusVariableNames.Pips.Eng, s => s.Pips?[1] ?? 0.0, SpadDataType.NUMBER),
                (StatusVariableNames.Pips.Wep, s => s.Pips?[2] ?? 0.0, SpadDataType.NUMBER),
                (StatusVariableNames.FireGroup, s => s.FireGroup, SpadDataType.NUMBER),
                (StatusVariableNames.GuiFocus, s => s.GuiFocus, SpadDataType.NUMBER),
                (StatusVariableNames.Fuel.FuelMain, s => s.Fuel?.FuelMain ?? 0.0, SpadDataType.NUMBER),
                (StatusVariableNames.Fuel.FuelReservoir, s => s.Fuel?.FuelReservoir ?? 0.0, SpadDataType.NUMBER),
                (StatusVariableNames.Cargo, s => s.Cargo, SpadDataType.NUMBER),
                (StatusVariableNames.LegalState, s => s.LegalState.ToString(), SpadDataType.NUMBER),
                (StatusVariableNames.Latitude, s => s.Latitude, SpadDataType.NUMBER),
                (StatusVariableNames.Altitude, s => s.Altitude, SpadDataType.NUMBER),
                (StatusVariableNames.Longitude, s => s.Longitude, SpadDataType.NUMBER),
                (StatusVariableNames.Heading, s => s.Heading, SpadDataType.NUMBER),
                (StatusVariableNames.BodyName, s => s.BodyName ?? string.Empty, SpadDataType.STRING),
                (StatusVariableNames.PlanetRadius, s => s.PlanetRadius, SpadDataType.NUMBER),
                (StatusVariableNames.Balance, s => s.Balance, SpadDataType.NUMBER),
                (StatusVariableNames.Destination.Name, s => s.Destination?.Name ?? string.Empty, SpadDataType.STRING),
                (StatusVariableNames.Destination.System, s => s.Destination?.System ?? string.Empty, SpadDataType.STRING),
                (StatusVariableNames.Destination.Body, s => s.Destination?.Body ?? string.Empty, SpadDataType.STRING),
                (StatusVariableNames.Oxygen, s => s.Oxygen, SpadDataType.NUMBER),
                (StatusVariableNames.Health, s => s.Health, SpadDataType.NUMBER),
                (StatusVariableNames.Temperature, s => s.Temperature, SpadDataType.NUMBER),
                (StatusVariableNames.SelectedWeapon, s => s.SelectedWeapon ?? string.Empty, SpadDataType.STRING),
                (StatusVariableNames.Gravity, s => s.Gravity, SpadDataType.NUMBER)
                // add further mappings as necessary
            };
    }
}