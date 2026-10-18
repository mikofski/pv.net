using System.Text.Json.Serialization;
using pv;

namespace pv.Tests;

public class SolarPositionAnalyticalTests
{
    [Fact]
    public void AnalyticalHelpersMatchPvlibFixtureForLeapYearAndNonHourlyInputs()
    {
        var fixture = FixtureLoader.Load<AnalyticalInputs, AnalyticalExpected>("solarposition_analytical.json");
        var testCase = Assert.Single(fixture.Cases);
        var inputs = testCase.Inputs!;
        var expected = testCase.Expected!;
        Assert.Equal(366, inputs.DayOfYear.Length);
        Assert.Equal(366.0, inputs.DayOfYear[^1]);
        Assert.Equal(new DateTimeOffset(2024, 2, 29, 8, 17, 0, TimeSpan.FromHours(5.5)), inputs.Times[0]);
        Assert.Equal(new DateTimeOffset(2024, 3, 1, 17, 5, 0, TimeSpan.FromHours(5.5)), inputs.Times[^1]);

        FixtureAssert.AssertAllClose(expected.DayAngle,
            SolarPosition.CalcSimpleDayAngleArray(inputs.DayOfYear), rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.EquationOfTimeSpencer71,
            SolarPosition.EquationOfTimeSpencer71(inputs.DayOfYear), rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.EquationOfTimePvcdrom,
            SolarPosition.EquationOfTimePvCdrom(inputs.DayOfYear), rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.DeclinationSpencer71,
            SolarPosition.DeclinationSpencer71(inputs.DayOfYear), rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.DeclinationCooper69,
            SolarPosition.DeclinationCooper69(inputs.DayOfYear), rtol: 1e-9, atol: 1e-15);

        var location = new Location(inputs.Latitude, inputs.Longitude, inputs.Altitude, inputs.Times[0].Offset);
        var hourAngle = SolarPosition.HourAngle(location, inputs.Times, inputs.EquationOfTimeForTimes);
        FixtureAssert.AssertAllClose(expected.HourAngle, hourAngle, rtol: 1e-9, atol: 0);

        var declination = new[] { expected.DeclinationSpencer71[59], expected.DeclinationSpencer71[59], expected.DeclinationSpencer71[60] };
        var hourAngleRadians = Array.ConvertAll(hourAngle, angle => angle * Math.PI / 180.0);
        FixtureAssert.AssertAllClose(expected.SolarZenith,
            SolarPosition.SolarZenithAnalytical(inputs.Latitude * Math.PI / 180.0, hourAngleRadians, declination),
            rtol: 1e-9, atol: 0);
    }

    [Fact]
    public void LocationValidatesCoordinatesAndFixedOffset()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Location(90.1, 0, 0, TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Location(0, 180.1, 0, TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Location(0, 0, 0, TimeSpan.FromHours(14.5)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Location(0, 0, 0, TimeSpan.FromSeconds(30)));
        Assert.Equal(TimeSpan.FromHours(5.5),
            new Location(0, 0, -10, TimeSpan.FromHours(5.5)).UtcOffset);
        Assert.Null(new Location(0, 0, 0, TimeSpan.Zero).Name);
        Assert.Equal(
            "Greenboro-NC",
            new Location(0, 0, 0, TimeSpan.Zero, "Greenboro-NC").Name);
    }

    [Fact]
    public void HourAngleRejectsTimestampsWithDifferentOffsets()
    {
        var location = new Location(0, 0, 0, TimeSpan.FromHours(5.5));
        var times = new[] { new DateTimeOffset(2024, 2, 29, 12, 0, 0, TimeSpan.Zero) };

        Assert.Throws<ArgumentException>(() => SolarPosition.HourAngle(location, times, [0]));
    }

    [Fact]
    public void AnalyticalHelpersPreserveNaNInputs()
    {
        double[] nan = [double.NaN];

        Assert.True(double.IsNaN(SolarPosition.CalcSimpleDayAngleArray(nan)[0]));
        Assert.True(double.IsNaN(SolarPosition.EquationOfTimeSpencer71(nan)[0]));
        Assert.True(double.IsNaN(SolarPosition.EquationOfTimePvCdrom(nan)[0]));
        Assert.True(double.IsNaN(SolarPosition.DeclinationSpencer71(nan)[0]));
        Assert.True(double.IsNaN(SolarPosition.DeclinationCooper69(nan)[0]));
        Assert.True(double.IsNaN(SolarPosition.SolarZenithAnalytical(0, nan, [0])[0]));
        Assert.True(double.IsNaN(SolarPosition.HourAngle(
            new Location(0, 0, 0, TimeSpan.Zero),
            [new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)],
            nan)[0]));
    }

    public sealed class AnalyticalInputs
    {
        [JsonPropertyName("day_of_year")]
        public double[] DayOfYear { get; init; } = [];

        [JsonPropertyName("times")]
        public DateTimeOffset[] Times { get; init; } = [];

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("altitude")]
        public double Altitude { get; init; }

        [JsonPropertyName("equation_of_time_for_times")]
        public double[] EquationOfTimeForTimes { get; init; } = [];

    }

    public sealed class AnalyticalExpected
    {
        [JsonPropertyName("day_angle")]
        public double[] DayAngle { get; init; } = [];

        [JsonPropertyName("equation_of_time_spencer71")]
        public double[] EquationOfTimeSpencer71 { get; init; } = [];

        [JsonPropertyName("equation_of_time_pvcdrom")]
        public double[] EquationOfTimePvcdrom { get; init; } = [];

        [JsonPropertyName("declination_spencer71")]
        public double[] DeclinationSpencer71 { get; init; } = [];

        [JsonPropertyName("declination_cooper69")]
        public double[] DeclinationCooper69 { get; init; } = [];

        [JsonPropertyName("hour_angle")]
        public double[] HourAngle { get; init; } = [];

        [JsonPropertyName("solar_zenith")]
        public double[] SolarZenith { get; init; } = [];
    }
}
