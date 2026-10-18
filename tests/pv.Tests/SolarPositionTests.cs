using System.Text.Json.Serialization;
using pv;

namespace pv.Tests;

public class SolarPositionTests
{
    [Fact]
    public void SunRiseSetTransitGeometricMatchesPvlibAndPublishedSpaValues()
    {
        var testCase = LoadCase("golden_mst");
        var inputs = testCase.Inputs!;
        var expected = testCase.Expected!;
        var times = inputs.Times;
        var utcDayOfYear = Array.ConvertAll(
            times, time => (double)time.UtcDateTime.DayOfYear);

        FixtureAssert.AssertAllClose(
            inputs.Declination,
            SolarPosition.DeclinationSpencer71(utcDayOfYear),
            rtol: 1e-9,
            atol: 0);
        FixtureAssert.AssertAllClose(
            inputs.EquationOfTime,
            SolarPosition.EquationOfTimeSpencer71(utcDayOfYear),
            rtol: 1e-9,
            atol: 0);

        var (sunrise, sunset, transit) = SolarPosition.SunRiseSetTransitGeometric(
            times, inputs.Latitude, inputs.Longitude, inputs.Declination, inputs.EquationOfTime);
        FixtureAssert.AssertTimesClose(expected.Sunrise, sunrise, toleranceSeconds: 1);
        FixtureAssert.AssertTimesClose(expected.Sunset, sunset, toleranceSeconds: 1);
        FixtureAssert.AssertTimesClose(expected.Transit, transit, toleranceSeconds: 1);
        Assert.All(sunrise, time => Assert.Equal(times[0].Offset, time.Offset));
        Assert.All(sunset, time => Assert.Equal(times[0].Offset, time.Offset));
        Assert.All(transit, time => Assert.Equal(times[0].Offset, time.Offset));

        var sunriseSpaHours = SolarPosition.TimesToHoursAfterLocalMidnight(
            expected.PublishedSpa.Sunrise);
        var sunsetSpaHours = SolarPosition.TimesToHoursAfterLocalMidnight(
            expected.PublishedSpa.Sunset);
        var transitSpaHours = SolarPosition.TimesToHoursAfterLocalMidnight(
            expected.PublishedSpa.Transit);
        FixtureAssert.AssertAllClose(
            sunriseSpaHours,
            TruncatedToMicrosecondHours(sunrise),
            rtol: 0,
            atol: 0.07910089555555544);
        FixtureAssert.AssertAllClose(
            sunsetSpaHours,
            TruncatedToMicrosecondHours(sunset),
            rtol: 0,
            atol: 0.1036246955555562);
        FixtureAssert.AssertAllClose(
            transitSpaHours,
            TruncatedToMicrosecondHours(transit),
            rtol: 0,
            atol: 0.011150788888889096);
    }

    [Theory]
    [InlineData("microsecond_index_utc")]
    [InlineData("microsecond_index_us_eastern")]
    public void SunRiseSetTransitGeometricMicrosecondIndex(string name)
    {
        var testCase = LoadCase(name);
        var inputs = testCase.Inputs!;
        var expected = testCase.Expected!;
        var (sunrise, sunset, transit) = SolarPosition.SunRiseSetTransitGeometric(
            inputs.Times,
            inputs.Latitude,
            inputs.Longitude,
            inputs.Declination,
            inputs.EquationOfTime);

        AssertRepeatedEvent(expected.Ns.Sunrise[0], sunrise);
        AssertRepeatedEvent(expected.Ns.Sunset[0], sunset);
        AssertRepeatedEvent(expected.Ns.Transit[0], transit);
        Assert.Equal(expected.Ns.Sunrise, expected.Us.Sunrise);
        Assert.Equal(expected.Ns.Sunset, expected.Us.Sunset);
        Assert.Equal(expected.Ns.Transit, expected.Us.Transit);
    }

    [Fact]
    public void TimeHelpersMatchPvlibFixture()
    {
        var testCase = LoadCase("golden_mst");
        var inputs = testCase.Inputs!;
        var expected = testCase.Expected!;

        FixtureAssert.AssertAllClose(
            expected.TimesToHoursAfterLocalMidnight,
            SolarPosition.TimesToHoursAfterLocalMidnight(inputs.HelperTimes),
            rtol: 0,
            atol: 0);

        var hours = SolarPosition.HourAngleToHours(
            inputs.HelperTimes,
            inputs.HelperHourAngle,
            inputs.Longitude,
            inputs.HelperEquationOfTime);
        FixtureAssert.AssertAllClose(expected.HourAngleToHours, hours, rtol: 0, atol: 1e-12);
        FixtureAssert.AssertTimesClose(
            expected.LocalTimesFromHoursSinceMidnight,
            SolarPosition.LocalTimesFromHoursSinceMidnight(inputs.HelperTimes, hours),
            toleranceSeconds: 1e-6);
    }

    [Fact]
    public void SunRiseSetTransitGeometricValidatesArrayLengths()
    {
        var times = new[] { new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero) };

        Assert.Throws<ArgumentException>(() =>
            SolarPosition.SunRiseSetTransitGeometric(times, 0, 0, [], [0]));
        Assert.Throws<ArgumentException>(() =>
            SolarPosition.HourAngleToHours(times, [], 0, [0]));
        Assert.Throws<ArgumentException>(() =>
            SolarPosition.LocalTimesFromHoursSinceMidnight(times, []));
    }

    [Fact]
    public void HourAngleToHoursUsesEachTimestampOffset()
    {
        DateTimeOffset[] times =
        [
            new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new(2024, 1, 1, 12, 0, 0, TimeSpan.FromHours(-5))
        ];

        FixtureAssert.AssertAllClose(
            [12, 7],
            SolarPosition.HourAngleToHours(times, [0, 0], 0, [0, 0]),
            rtol: 0,
            atol: 0);
        var localTimes = SolarPosition.LocalTimesFromHoursSinceMidnight(times, [12, 7]);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero), localTimes[0]);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 7, 0, 0, TimeSpan.FromHours(-5)), localTimes[1]);
        Assert.Equal(times[0].Offset, localTimes[0].Offset);
        Assert.Equal(times[1].Offset, localTimes[1].Offset);
    }

    private static FixtureCase<GeometricInputs, GeometricExpected> LoadCase(string name)
    {
        var fixture = FixtureLoader.Load<GeometricInputs, GeometricExpected>(
            "sun_rise_set_transit_geometric.json");
        Assert.Equal("0.16.1", fixture.PvlibVersion);
        return Assert.Single(fixture.Cases, testCase => testCase.Name == name);
    }

    private static void AssertRepeatedEvent(DateTimeOffset expected, DateTimeOffset[] actual)
    {
        Assert.NotEmpty(actual);
        Assert.All(actual, time => FixtureAssert.AssertTimesClose([expected], [time], 1));
    }

    private static double[] TruncatedToMicrosecondHours(DateTimeOffset[] times) =>
        Array.ConvertAll(
            times,
            time => new DateTimeOffset(time.Ticks - time.Ticks % 10, time.Offset)
                .TimeOfDay.TotalHours);

    public sealed class GeometricInputs
    {
        [JsonPropertyName("times")]
        public DateTimeOffset[] Times { get; init; } = [];

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("declination")]
        public double[] Declination { get; init; } = [];

        [JsonPropertyName("equation_of_time")]
        public double[] EquationOfTime { get; init; } = [];

        [JsonPropertyName("helper_times")]
        public DateTimeOffset[] HelperTimes { get; init; } = [];

        [JsonPropertyName("helper_hour_angle")]
        public double[] HelperHourAngle { get; init; } = [];

        [JsonPropertyName("helper_equation_of_time")]
        public double[] HelperEquationOfTime { get; init; } = [];
    }

    public sealed class GeometricExpected
    {
        [JsonPropertyName("sunrise")]
        public DateTimeOffset[] Sunrise { get; init; } = [];

        [JsonPropertyName("sunset")]
        public DateTimeOffset[] Sunset { get; init; } = [];

        [JsonPropertyName("transit")]
        public DateTimeOffset[] Transit { get; init; } = [];

        [JsonPropertyName("published_spa")]
        public PublishedSpaTimes PublishedSpa { get; init; } = new();

        [JsonPropertyName("times_to_hours_after_local_midnight")]
        public double[] TimesToHoursAfterLocalMidnight { get; init; } = [];

        [JsonPropertyName("hour_angle_to_hours")]
        public double[] HourAngleToHours { get; init; } = [];

        [JsonPropertyName("local_times_from_hours_since_midnight")]
        public DateTimeOffset[] LocalTimesFromHoursSinceMidnight { get; init; } = [];

        [JsonPropertyName("ns")]
        public EventTimes Ns { get; init; } = new();

        [JsonPropertyName("us")]
        public EventTimes Us { get; init; } = new();
    }

    public sealed class PublishedSpaTimes
    {
        [JsonPropertyName("sunrise")]
        public DateTimeOffset[] Sunrise { get; init; } = [];

        [JsonPropertyName("sunset")]
        public DateTimeOffset[] Sunset { get; init; } = [];

        [JsonPropertyName("transit")]
        public DateTimeOffset[] Transit { get; init; } = [];
    }

    public sealed class EventTimes
    {
        [JsonPropertyName("sunrise")]
        public DateTimeOffset[] Sunrise { get; init; } = [];

        [JsonPropertyName("sunset")]
        public DateTimeOffset[] Sunset { get; init; } = [];

        [JsonPropertyName("transit")]
        public DateTimeOffset[] Transit { get; init; } = [];
    }
}
