using System.Text.Json.Serialization;
using pv;

namespace pv.Tests;

public class EquationOfTimeSpencer71Tests
{
    [Fact]
    public void MatchesPvlibFixtureForAllDaysOf2015()
    {
        var fixture = FixtureLoader.Load<EquationInputs, double[]>("equation_of_time_spencer71.json");
        var testCase = Assert.Single(fixture.Cases);
        Assert.Equal(Enumerable.Range(1, 365).Select(day => (double)day), testCase.Inputs!.DayOfYear);
        var actual = SolarPosition.EquationOfTimeSpencer71(testCase.Inputs.DayOfYear);

        FixtureAssert.AssertAllClose(testCase.Expected!, actual, rtol: 1e-9, atol: 0);
    }

    public sealed class EquationInputs
    {
        [JsonPropertyName("day_of_year")]
        public double[] DayOfYear { get; init; } = [];
    }
}
