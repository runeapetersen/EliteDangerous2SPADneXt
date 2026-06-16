using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.ChangeHandling;
using Xunit;

namespace EliteDangerous2SPADneXt.Tests
{
    public class ValueDetailsTests
    {
        [Theory]
        [MemberData(nameof(TestData_String))]
        public async Task ValueDetails_String_HandleUpdate(string oldValue, string newValue, bool expectedResult)
        {
            var sut = new StringValueDetails(oldValue);
            var result = sut.HandleUpdate(newValue);
            Assert.Equal(expectedResult, result);
            Assert.Equal(newValue, sut.CurrentValue);
        }

        public static IEnumerable<object[]> TestData_String()
        {
            yield return new object[] { "A", "B", true };
            yield return new object[] { "A", "A", false };
        }

        [Theory]
        [MemberData(nameof(TestData_Double))]
        public async Task ValueDetails_Double_HandleUpdate(double oldValue, double newValue, bool expectedResult)
        {
            var sut = new DoubleValueDetails(oldValue);
            var result = sut.HandleUpdate(newValue);
            Assert.Equal(expectedResult, result);
            Assert.Equal(newValue, sut.CurrentValue);
        }

        public static IEnumerable<object[]> TestData_Double()
        {
            yield return new object[] { 1, 2, true };
            yield return new object[] { 1, 1, false };
        }

        [Fact]
        public async Task ValueDetails_Double_FailScenario()
        {
            var sut = new DoubleValueDetails(0);
            Action act = () => sut.HandleUpdate(null);
            Assert.Throws<ArgumentException>(act);
        }
    }
}