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
        Assert.Equal(Enumerable.Range(1, 365), testCase.Inputs!.DayOfYear);

        var times = Enumerable.Range(0, 365)
            .Select(day => new DateTime(2015, 1, 1).AddDays(day).ToString("yyyy-MM-dd'T'HH:mm:ss"))
            .ToArray();
        var position = new SolarPosition(times, 0, 0, 0);
        var actual = position.EquationOfTimeSpencer71(position.CalcSimpleDayAngleArray());

        FixtureAssert.AssertAllClose(testCase.Expected!, actual, rtol: 1e-9, atol: 0);
    }

    public sealed class EquationInputs
    {
        [JsonPropertyName("day_of_year")]
        public int[] DayOfYear { get; init; } = [];
    }
}
