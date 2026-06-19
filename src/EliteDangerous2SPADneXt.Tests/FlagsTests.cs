using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EliteDangerous2SPADneXt.ChangeHandling;
using EliteDangerous2SPADneXt.GameState;
using Xunit;

namespace EliteDangerous2SPADneXt.Tests
{
    public class FlagChangeHandlerTests
    {
        private readonly UpdatedValueComparer _comparer;

        public FlagChangeHandlerTests()
        {
            _comparer = new UpdatedValueComparer();
        }

        [Fact]
        public async Task Flags_InitialState_UpdatesProcessed()
        {
            var initialValues = EdFlags.Docked | EdFlags.FlightAssist_Off;
            var updatedValues = EdFlags.Docked | EdFlags.Being_Interdicted;

            var sut = new FlagChangeHandler<EdFlags>(initialValues);
            var outcome = sut.HandleUpdate(updatedValues);
            Assert.Equal(2, outcome.Count());
            Assert.Contains(
                new UpdatedValue(EnumVariableNameHelper.Build(EdFlags.Being_Interdicted), 1, SpadDataType.BOOL),
                outcome, _comparer);
            Assert.Contains(
                new UpdatedValue(EnumVariableNameHelper.Build(EdFlags.FlightAssist_Off), 0, SpadDataType.BOOL), outcome,
                _comparer);
        }

        internal class UpdatedValueComparer : IComparer<UpdatedValue>, IEqualityComparer<UpdatedValue>
        {
            public int Compare(UpdatedValue x, UpdatedValue y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (y is null) return 1;
                if (x is null) return -1;
                if (x.Value.GetType() != y.Value.GetType())
                    return -1;
                if (x.Equals(y))
                    return 0;
                var nameComparison = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                if (nameComparison != 0) return nameComparison;
                return x.DataType.CompareTo(y.DataType);
            }

            public bool Equals(UpdatedValue x, UpdatedValue y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null) return false;
                if (y is null) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Name == y.Name && Equals(x.Value, y.Value) && x.DataType == y.DataType;
            }

            public int GetHashCode(UpdatedValue obj)
            {
                if (obj == null) return 0;

                // Standard prime multiplier pattern compatible with C# 7.3 / .NET Framework 4.8
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + (obj.Name != null ? obj.Name.GetHashCode() : 0);
                    hash = hash * 31 + (obj.Value != null ? obj.Value.GetHashCode() : 0);
                    hash = hash * 31 + obj.DataType.GetHashCode();

                    return hash;
                }
            }
        }

        [Fact]
        public async Task Flags_DefaultState_UpdatesProcessed()
        {
            var updatedValues = EdFlags.Docked | EdFlags.Being_Interdicted;

            var sut = new FlagChangeHandler<EdFlags>();
            var outcome = sut.HandleUpdate(updatedValues);
            Assert.Equal(2, outcome.Count());
            Assert.Contains(
                new UpdatedValue(EnumVariableNameHelper.Build(EdFlags.Being_Interdicted), 1, SpadDataType.BOOL),
                outcome, _comparer);
            Assert.Contains(new UpdatedValue(EnumVariableNameHelper.Build(EdFlags.Docked), 1, SpadDataType.BOOL),
                outcome, _comparer);
        }

        [Fact]
        public async Task Flags_NoChange_NoUpdates()
        {
            var updatedValues = EdFlags.Docked;
            var sut = new FlagChangeHandler<EdFlags>(updatedValues);
            var outcome = sut.HandleUpdate(updatedValues);
            Assert.Empty(outcome);
        }

        [Fact]
        public async Task Flags_ToZero_UpdatesProcessed()
        {
            var initialValues = EdFlags.Docked | EdFlags.Being_Interdicted;

            var sut = new FlagChangeHandler<EdFlags>(initialValues);
            var outcome = sut.HandleUpdate(0);
            Assert.Equal(2, outcome.Count());
            Assert.Contains(
                new UpdatedValue(EnumVariableNameHelper.Build(EdFlags.Being_Interdicted), 0, SpadDataType.BOOL),
                outcome, _comparer);
            Assert.Contains(new UpdatedValue(EnumVariableNameHelper.Build(EdFlags.Docked), 0, SpadDataType.BOOL),
                outcome, _comparer);
        }

        [Fact]
        public async Task GetAllVariables()
        {
            var sut = new FlagChangeHandler<EdFlags>(0);
            var result = sut.GetCurrentValues();
            Assert.Equal(32, result.Count());
            Assert.All(result, v => Assert.Equal(0, v.Value));
        }
    }
}