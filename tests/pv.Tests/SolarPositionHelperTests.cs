using System.Text.Json.Serialization;
using pv;

namespace pv.Tests;

public class SolarPositionHelperTests
{
    [Theory]
    [InlineData("day_angle")]
    [InlineData("equation_of_time_spencer71")]
    [InlineData("equation_of_time_pvcdrom")]
    [InlineData("declination_spencer71")]
    [InlineData("declination_cooper69")]
    public void DayHelpersMatchPvlibForScalarAndArrayInputs(string helper)
    {
        var fixture = FixtureLoader.Load<DayInputs, Dictionary<string, double[]>>(
            "solarposition_day_helpers.json");
        Assert.NotEmpty(fixture.Cases);
        foreach (var testCase in fixture.Cases)
        {
            var inputs = testCase.Inputs!;
            (Func<double, double>, Func<double[], double[]>) functions = helper switch
            {
                "day_angle" => (
                    day => SolarPosition.CalcSimpleDayAngleArray(day, inputs.Offset),
                    days => SolarPosition.CalcSimpleDayAngleArray(days, inputs.Offset)),
                "equation_of_time_spencer71" => (
                    SolarPosition.EquationOfTimeSpencer71, SolarPosition.EquationOfTimeSpencer71),
                "equation_of_time_pvcdrom" => (
                    SolarPosition.EquationOfTimePvCdrom, SolarPosition.EquationOfTimePvCdrom),
                "declination_spencer71" => (
                    SolarPosition.DeclinationSpencer71, SolarPosition.DeclinationSpencer71),
                "declination_cooper69" => (
                    SolarPosition.DeclinationCooper69, SolarPosition.DeclinationCooper69),
                _ => throw new ArgumentException("Unknown helper.", nameof(helper))
            };
            var (scalar, array) = functions;
            var original = (double[])inputs.DayOfYear.Clone();
            var scalarResults = Array.ConvertAll(inputs.DayOfYear, day => scalar(day));
            var arrayResults = array(inputs.DayOfYear);

            FixtureAssert.AssertAllClose(testCase.Expected![helper], scalarResults, rtol: 1e-9, atol: 1e-15);
            FixtureAssert.AssertAllClose(testCase.Expected[helper], arrayResults, rtol: 1e-9, atol: 1e-15);
            Assert.Equal(scalarResults, arrayResults);
            Assert.Equal(original, inputs.DayOfYear);
            Assert.NotSame(inputs.DayOfYear, arrayResults);
            Assert.Throws<ArgumentNullException>(() => array(null!));
        }
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(60.5)]
    [InlineData(366.0)]
    public void DayAngleDefaultsToOffsetOne(double dayOfYear)
    {
        Assert.Equal(
            SolarPosition.CalcSimpleDayAngleArray(dayOfYear, offset: 1),
            SolarPosition.CalcSimpleDayAngleArray(dayOfYear));
        Assert.Equal(
            SolarPosition.CalcSimpleDayAngleArray([dayOfYear], offset: 1),
            SolarPosition.CalcSimpleDayAngleArray([dayOfYear]));
    }

    [Fact]
    public void HourAngleMatchesPvlibForScalarAndArrayInputs()
    {
        var fixture = FixtureLoader.Load<HourAngleInputs, double[]>("hour_angle.json");
        Assert.NotEmpty(fixture.Cases);
        foreach (var testCase in fixture.Cases)
        {
            var inputs = testCase.Inputs!;
            var location = new Location(0, inputs.Longitude, 0, inputs.Times[0].Offset);
            var originalTimes = (DateTimeOffset[])inputs.Times.Clone();
            var originalEquationOfTime = (double[])inputs.EquationOfTime.Clone();
            var scalarResults = inputs.Times.Select((time, i) =>
                SolarPosition.HourAngle(location, time, inputs.EquationOfTime[i])).ToArray();
            var arrayResults = SolarPosition.HourAngle(location, inputs.Times, inputs.EquationOfTime);

            FixtureAssert.AssertAllClose(testCase.Expected!, scalarResults, rtol: 1e-9, atol: 1e-12);
            FixtureAssert.AssertAllClose(testCase.Expected!, arrayResults, rtol: 1e-9, atol: 1e-12);
            Assert.Equal(scalarResults, arrayResults);
            Assert.Equal(originalTimes, inputs.Times);
            Assert.Equal(originalEquationOfTime, inputs.EquationOfTime);
        }
    }

    [Fact]
    public void HourAngleValidatesScalarAndArrayArguments()
    {
        var location = new Location(0, 0, 0, TimeSpan.Zero);
        var time = new DateTimeOffset(2024, 2, 29, 12, 0, 0, TimeSpan.Zero);
        var wrongOffset = time.ToOffset(TimeSpan.FromHours(5.5));

        Assert.Throws<ArgumentNullException>(() => SolarPosition.HourAngle(null!, time, 0));
        Assert.Equal("time",
            Assert.Throws<ArgumentException>(() => SolarPosition.HourAngle(location, wrongOffset, 0)).ParamName);
        Assert.Throws<ArgumentNullException>(() => SolarPosition.HourAngle(null!, [time], [0]));
        Assert.Throws<ArgumentNullException>(() => SolarPosition.HourAngle(location, null!, [0]));
        Assert.Throws<ArgumentNullException>(() => SolarPosition.HourAngle(location, [time], null!));
        Assert.Throws<ArgumentException>(() => SolarPosition.HourAngle(location, [time], []));
        Assert.Equal("times",
            Assert.Throws<ArgumentException>(() => SolarPosition.HourAngle(location, [wrongOffset], [0])).ParamName);
        Assert.Empty(SolarPosition.HourAngle(location, [], []));
    }

    public sealed class DayInputs
    {
        [JsonPropertyName("day_of_year")]
        public double[] DayOfYear { get; init; } = [];

        [JsonPropertyName("offset")]
        public int Offset { get; init; }
    }

    public sealed class HourAngleInputs
    {
        [JsonPropertyName("times")]
        public DateTimeOffset[] Times { get; init; } = [];

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("equation_of_time")]
        public double[] EquationOfTime { get; init; } = [];
    }
}
