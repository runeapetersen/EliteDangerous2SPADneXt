using System;
using System.Collections.Generic;
using System.Linq;
using EliteDangerous2SPADneXt.GameState;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Represents a container that manages and tracks the state of various status values,
    /// providing mechanisms for accessing and applying updates to these values.
    /// </summary>
    public class StateContainer
    {
        public IDictionary<string, IValueDetails> StatusValues { get; } = new Dictionary<string, IValueDetails>();

        public StateContainer()
        {
            SeedStatusValues();
        }

        public DateTimeOffset LastUpdateTimeStamp { get; set; } = DateTimeOffset.MinValue;

        private void SeedStatusValues()
        {
            StatusValues.Add(StatusVariableNames.Pips.One, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Pips.Two, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Pips.Three, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.FireGroup, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.GuiFocus, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Fuel.FuelMain, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Fuel.FuelReservoir, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Cargo, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.LegalState, new ValueDetails<string>());
            StatusValues.Add(StatusVariableNames.Latitude, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Altitude, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Longitude, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Heading, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.BodyName, new ValueDetails<string>());
            StatusValues.Add(StatusVariableNames.PlanetRadius, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Balance, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Destination.Body, new ValueDetails<string>());
            StatusValues.Add(StatusVariableNames.Destination.Name, new ValueDetails<string>());
            StatusValues.Add(StatusVariableNames.Destination.System, new ValueDetails<string>());
            StatusValues.Add(StatusVariableNames.Oxygen, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Health, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.Temperature, new ValueDetails<double>());
            StatusValues.Add(StatusVariableNames.SelectedWeapon, new ValueDetails<string>());
            StatusValues.Add(StatusVariableNames.Gravity, new ValueDetails<double>());
        }

        public IEnumerable<UpdatedValue> GetCurrentValues()
        {
            return StatusValues.Select(v => new UpdatedValue(v.Key, v.Value.CurrentValue));
        }

        public IEnumerable<UpdatedValue> Apply(Status status)
        {
            var updateBucket = new List<UpdatedValue>();
            HandleField(StatusVariableNames.Pips.One, status.Pips?[0] ?? 0, updateBucket);
            HandleField(StatusVariableNames.Pips.Two, status.Pips?[1] ?? 0, updateBucket);
            HandleField(StatusVariableNames.Pips.Three, status.Pips?[2] ?? 0, updateBucket);
            HandleField(StatusVariableNames.FireGroup, status.FireGroup, updateBucket);
            HandleField(StatusVariableNames.GuiFocus, status.GuiFocus, updateBucket);
            HandleField(StatusVariableNames.Fuel.FuelMain, status.Fuel?.FuelMain ?? 0, updateBucket);
            HandleField(StatusVariableNames.Fuel.FuelReservoir, status.Fuel?.FuelReservoir ?? 0, updateBucket);
            HandleField(StatusVariableNames.Cargo, status.Cargo, updateBucket);
            HandleField(StatusVariableNames.LegalState, status.LegalState ?? string.Empty, updateBucket);
            HandleField(StatusVariableNames.Latitude, status.Latitude, updateBucket);
            HandleField(StatusVariableNames.Altitude, status.Altitude, updateBucket);
            HandleField(StatusVariableNames.Longitude, status.Longitude, updateBucket);
            HandleField(StatusVariableNames.Heading, status.Heading, updateBucket);
            HandleField(StatusVariableNames.BodyName, status.BodyName ?? string.Empty, updateBucket);
            HandleField(StatusVariableNames.PlanetRadius, status.PlanetRadius, updateBucket);
            HandleField(StatusVariableNames.Balance, status.Balance, updateBucket);
            HandleField(StatusVariableNames.Destination.Name, status.Destination?.Name ?? string.Empty, updateBucket);
            HandleField(StatusVariableNames.Destination.System, status.Destination?.System ?? string.Empty, updateBucket);
            HandleField(StatusVariableNames.Destination.Body, status.Destination?.Body ?? string.Empty, updateBucket);
            HandleField(StatusVariableNames.Oxygen, status.Oxygen, updateBucket);
            HandleField(StatusVariableNames.Health, status.Health, updateBucket);
            HandleField(StatusVariableNames.Temperature, status.Temperature, updateBucket);
            HandleField(StatusVariableNames.SelectedWeapon, status.SelectedWeapon??string.Empty, updateBucket);
            HandleField(StatusVariableNames.Gravity, status.Gravity, updateBucket);
            return updateBucket;
        }

        private void HandleField(string name, IComparable value, List<UpdatedValue> updateBucket)
        {
            var isUpdate = StatusValues[name].HandleUpdate(value);
            if (isUpdate)
                updateBucket.Add(new UpdatedValue(name, value));
        }
    }
}