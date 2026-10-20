using System.Text.Json.Serialization;
using pv;

namespace pv.Tests;

public class SolarPositionTests
{
    [Fact]
    public void SolarAzimuthAnalyticalMatchesPvlibAndSpaFixtures()
    {
        var fixture = FixtureLoader.Load<SolarAzimuthInputs, SolarAzimuthExpected>(
            "solar_azimuth_analytical.json");
        Assert.Equal("0.16.1", fixture.PvlibVersion);
        Assert.Equal(3, fixture.Cases.Count);

        foreach (var testCase in fixture.Cases.Where(testCase =>
                     testCase.Name != "test_angles_boundary"))
        {
            var inputs = testCase.Inputs!;
            var expected = testCase.Expected!;
            var vectorActual = SolarPosition.SolarAzimuthAnalytical(
                inputs.Latitude,
                inputs.HourAngle,
                inputs.Declination,
                inputs.Zenith);

            FixtureAssert.AssertAllClose(
                expected.Azimuth, vectorActual, rtol: 1e-9, atol: 0);

            Assert.All(inputs.Latitude, latitude =>
                Assert.Equal(inputs.Latitude[0], latitude));
            var scalarLatitudeActual = SolarPosition.SolarAzimuthAnalytical(
                inputs.Latitude[0],
                inputs.HourAngle,
                inputs.Declination,
                inputs.Zenith);
            FixtureAssert.AssertAllClose(
                expected.Azimuth, scalarLatitudeActual, rtol: 1e-9, atol: 0);

            var daylightIndices = Enumerable.Range(0, expected.SpaZenith.Length)
                .Where(index => expected.SpaZenith[index] < Math.PI / 2.0)
                .ToArray();
            var spaAzimuth = daylightIndices
                .Select(index => expected.SpaAzimuth[index])
                .ToArray();
            var spencerOrCooperAzimuth = daylightIndices
                .Select(index => vectorActual[index])
                .ToArray();
            var tolerance = testCase.Name == "spencer71" ? 0.01 : 0.017;

            FixtureAssert.AssertAllClose(
                spaAzimuth, spencerOrCooperAzimuth, rtol: 1e-5, atol: tolerance);
        }
    }

    [Fact]
    public void SolarAzimuthAnalyticalMatchesPvlibBoundaryAnglesWithoutNaN()
    {
        var fixture = FixtureLoader.Load<SolarAzimuthInputs, SolarAzimuthExpected>(
            "solar_azimuth_analytical.json");
        var testCase = Assert.Single(fixture.Cases.Where(testCase =>
            testCase.Name == "test_angles_boundary"));
        var inputs = testCase.Inputs!;
        var expected = testCase.Expected!;
        var actual = SolarPosition.SolarAzimuthAnalytical(
            inputs.Latitude,
            inputs.HourAngle,
            inputs.Declination,
            inputs.Zenith);

        Assert.Equal(15, actual.Length);
        Assert.All(actual, azimuth => Assert.False(double.IsNaN(azimuth)));
        FixtureAssert.AssertAllClose(expected.Azimuth, actual, rtol: 1e-9, atol: 0);
    }

    [Fact]
    public void SolarAzimuthAnalyticalPreservesNaNAndValidatesArrayLengths()
    {
        var latitude = new[] { double.NaN, 0.0, 0.0, 0.0 };
        var hourAngle = new[] { 0.1, double.NaN, 0.1, 0.1 };
        var declination = new[] { 0.0, 0.0, double.NaN, 0.0 };
        var zenith = new[] { 1.0, 1.0, 1.0, double.NaN };
        var actual = SolarPosition.SolarAzimuthAnalytical(
            latitude, hourAngle, declination, zenith);

        Assert.All(actual, azimuth => Assert.True(double.IsNaN(azimuth)));
        Assert.Throws<ArgumentException>(() =>
            SolarPosition.SolarAzimuthAnalytical([0.0], [0.0, 0.0], [0.0], [0.0]));
        Assert.Throws<ArgumentException>(() =>
            SolarPosition.SolarAzimuthAnalytical(0.0, [0.0], [0.0, 0.0], [0.0]));
    }

    public sealed class SolarAzimuthInputs
    {
        [JsonPropertyName("latitude")]
        public double[] Latitude { get; init; } = [];

        [JsonPropertyName("hour_angle")]
        public double[] HourAngle { get; init; } = [];

        [JsonPropertyName("declination")]
        public double[] Declination { get; init; } = [];

        [JsonPropertyName("zenith")]
        public double[] Zenith { get; init; } = [];
    }

    public sealed class SolarAzimuthExpected
    {
        [JsonPropertyName("azimuth")]
        public double[] Azimuth { get; init; } = [];

        [JsonPropertyName("spa_azimuth")]
        public double[] SpaAzimuth { get; init; } = [];

        [JsonPropertyName("spa_zenith")]
        public double[] SpaZenith { get; init; } = [];
    }

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

        var events = SolarPosition.SunRiseSetTransitGeometric(
            times, inputs.Latitude, inputs.Longitude, inputs.Declination, inputs.EquationOfTime);
        Assert.All(events.Sunrise, time => Assert.NotNull(time));
        Assert.All(events.Sunset, time => Assert.NotNull(time));
        FixtureAssert.AssertTimesClose(
            expected.Sunrise, events.Sunrise.Select(time => time!.Value).ToArray(), toleranceSeconds: 1);
        FixtureAssert.AssertTimesClose(
            expected.Sunset, events.Sunset.Select(time => time!.Value).ToArray(), toleranceSeconds: 1);
        FixtureAssert.AssertTimesClose(expected.Transit, events.Transit, toleranceSeconds: 1);
        Assert.All(events.Sunrise, time => Assert.Equal(times[0].Offset, time!.Value.Offset));
        Assert.All(events.Sunset, time => Assert.Equal(times[0].Offset, time!.Value.Offset));
        Assert.All(events.Transit, time => Assert.Equal(times[0].Offset, time.Offset));

        var sunriseSpaHours = SolarPosition.TimesToHoursAfterLocalMidnight(
            expected.PublishedSpa.Sunrise);
        var sunsetSpaHours = SolarPosition.TimesToHoursAfterLocalMidnight(
            expected.PublishedSpa.Sunset);
        var transitSpaHours = SolarPosition.TimesToHoursAfterLocalMidnight(
            expected.PublishedSpa.Transit);
        FixtureAssert.AssertAllClose(
            sunriseSpaHours,
            TruncatedToMicrosecondHours(events.Sunrise.Select(time => time!.Value).ToArray()),
            rtol: 0,
            atol: 0.07910089555555544);
        FixtureAssert.AssertAllClose(
            sunsetSpaHours,
            TruncatedToMicrosecondHours(events.Sunset.Select(time => time!.Value).ToArray()),
            rtol: 0,
            atol: 0.1036246955555562);
        FixtureAssert.AssertAllClose(
            transitSpaHours,
            TruncatedToMicrosecondHours(events.Transit),
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
        var events = SolarPosition.SunRiseSetTransitGeometric(
            inputs.Times,
            inputs.Latitude,
            inputs.Longitude,
            inputs.Declination,
            inputs.EquationOfTime);

        AssertRepeatedEvent(expected.Ns.Sunrise[0], events.Sunrise);
        AssertRepeatedEvent(expected.Ns.Sunset[0], events.Sunset);
        Assert.All(events.Transit, time =>
            FixtureAssert.AssertTimesClose([expected.Ns.Transit[0]], [time], 1));
        Assert.Equal(expected.Ns.Sunrise, expected.Us.Sunrise);
        Assert.Equal(expected.Ns.Sunset, expected.Us.Sunset);
        Assert.Equal(expected.Ns.Transit, expected.Us.Transit);
    }

    [Fact]
    public void SunRiseSetTransitGeometricPreservesTransitForPolarDayAndNight()
    {
        DateTimeOffset[] times =
        [
            new(2024, 6, 20, 0, 0, 0, TimeSpan.Zero),
            new(2024, 12, 20, 0, 0, 0, TimeSpan.Zero),
            new(2024, 3, 20, 0, 0, 0, TimeSpan.Zero)
        ];

        var events = SolarPosition.SunRiseSetTransitGeometric(
            times, 70, 0, [0.4, -0.4, 0], [0, 0, 0]);

        Assert.Equal(times, events.Times);
        Assert.Null(events.Sunrise[0]);
        Assert.Null(events.Sunset[0]);
        Assert.Null(events.Sunrise[1]);
        Assert.Null(events.Sunset[1]);
        Assert.NotNull(events.Sunrise[2]);
        Assert.NotNull(events.Sunset[2]);
        Assert.Equal(times.Select(time => time.Date), events.Transit.Select(time => time.Date));
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

    private static void AssertRepeatedEvent(DateTimeOffset expected, DateTimeOffset?[] actual)
    {
        Assert.NotEmpty(actual);
        Assert.All(actual, time =>
        {
            Assert.NotNull(time);
            FixtureAssert.AssertTimesClose([expected], [time.Value], 1);
        });
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
