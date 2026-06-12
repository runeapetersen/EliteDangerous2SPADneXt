using System;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.ChangeHandling;
using EliteDangerous2SPADneXt.GameState;
using Xunit;

namespace EliteDangerous2SPADneXt.Tests
{
    public class EnumVariableNameHelperTest
    {
        [Fact]
        public async Task Build_GetExectedResult()
        {
            Assert.Equal("EdFlags_Altitude_from_Average_radius",
                EnumVariableNameHelper.Build(EdFlags.Altitude_from_Average_radius));
            Assert.Throws<ArgumentException>(() =>
                EnumVariableNameHelper.Build(EdFlags.Altitude_from_Average_radius | EdFlags.Being_Interdicted));
        }
    }
}