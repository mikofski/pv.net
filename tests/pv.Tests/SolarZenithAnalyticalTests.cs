using System.Text.Json.Serialization;
using pv;

namespace pv.Tests;

public class SolarZenithAnalyticalTests
{
    [Fact]
    public void MatchesPvlibTestAnalyticalZenith()
    {
        var fixture = FixtureLoader.Load<ZenithInputs, ZenithExpected>("solar_zenith_analytical.json");
        var testCase = Assert.Single(fixture.Cases);
        var inputs = testCase.Inputs!;
        var expected = testCase.Expected!;
        Assert.Equal(8760, inputs.Times.Length);
        Assert.Equal(TimeSpan.FromHours(-8), inputs.Times[0].Offset);
        Assert.Equal(new DateTimeOffset(2015, 12, 31, 23, 0, 0, TimeSpan.FromHours(-8)), inputs.Times[^1]);

        var dayOfYearUtc = inputs.Times.Select(time => (double)time.UtcDateTime.DayOfYear).ToArray();
        var location = new Location(
            inputs.Latitude,
            inputs.Longitude,
            inputs.Altitude,
            inputs.Times[0].Offset);
        var latitudeRadians = inputs.Latitude * Math.PI / 180.0;

        var equationOfTimeSpencer71 = SolarPosition.EquationOfTimeSpencer71(dayOfYearUtc);
        var hourAngleSpencer71 = ToRadians(SolarPosition.HourAngle(location, inputs.Times, equationOfTimeSpencer71));
        var declinationSpencer71 = SolarPosition.DeclinationSpencer71(dayOfYearUtc);
        var zenithSpencer71 = SolarPosition.SolarZenithAnalytical(
            latitudeRadians,
            hourAngleSpencer71,
            declinationSpencer71);
        FixtureAssert.AssertAllClose(expected.HourAngleSpencer71, hourAngleSpencer71, rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.DeclinationSpencer71, declinationSpencer71, rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.ZenithSpencer71, zenithSpencer71, rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.SpaZenith, zenithSpencer71, rtol: 1e-5, atol: 0.015);

        var equationOfTimePvcdrom = SolarPosition.EquationOfTimePvCdrom(dayOfYearUtc);
        var hourAnglePvcdrom = ToRadians(SolarPosition.HourAngle(location, inputs.Times, equationOfTimePvcdrom));
        var declinationCooper69 = SolarPosition.DeclinationCooper69(dayOfYearUtc);
        var zenithPvcdromCooper69 = SolarPosition.SolarZenithAnalytical(
            latitudeRadians,
            hourAnglePvcdrom,
            declinationCooper69);
        FixtureAssert.AssertAllClose(expected.HourAnglePvcdrom, hourAnglePvcdrom, rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.DeclinationCooper69, declinationCooper69, rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.ZenithPvcdromCooper69, zenithPvcdromCooper69, rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(expected.SpaZenith, zenithPvcdromCooper69, rtol: 1e-5, atol: 0.025);
    }

    [Fact]
    public void ArrayLatitudeOverloadMatchesPvlibBoundaryAngles()
    {
        var fixture = FixtureLoader.Load<ZenithInputs, ZenithExpected>("solar_zenith_analytical.json");
        var testCase = Assert.Single(fixture.Cases);
        var inputs = testCase.Inputs!;
        var expected = testCase.Expected!;
        var actual = SolarPosition.SolarZenithAnalytical(
            inputs.TestAngles.Select(angle => angle[0]).ToArray(),
            inputs.TestAngles.Select(angle => angle[1]).ToArray(),
            inputs.TestAngles.Select(angle => angle[2]).ToArray());

        Assert.DoesNotContain(actual, double.IsNaN);
        FixtureAssert.AssertAllClose(expected.BoundaryZenith, actual, rtol: 1e-9, atol: 0);
    }

    [Fact]
    public void RejectsMismatchedAnalyticalZenithArrayLengths()
    {
        Assert.Throws<ArgumentException>(() =>
            SolarPosition.SolarZenithAnalytical([0.0], [0.0, 0.0], [0.0, 0.0]));
    }

    private static double[] ToRadians(double[] degrees) =>
        Array.ConvertAll(degrees, angle => angle * Math.PI / 180.0);

    public sealed class ZenithInputs
    {
        [JsonPropertyName("times")]
        public DateTimeOffset[] Times { get; init; } = [];

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("altitude")]
        public double Altitude { get; init; }

        [JsonPropertyName("test_angles")]
        public double[][] TestAngles { get; init; } = [];
    }

    public sealed class ZenithExpected
    {
        [JsonPropertyName("spa_zenith")]
        public double[] SpaZenith { get; init; } = [];

        [JsonPropertyName("hour_angle_spencer71")]
        public double[] HourAngleSpencer71 { get; init; } = [];

        [JsonPropertyName("declination_spencer71")]
        public double[] DeclinationSpencer71 { get; init; } = [];

        [JsonPropertyName("zenith_spencer71")]
        public double[] ZenithSpencer71 { get; init; } = [];

        [JsonPropertyName("hour_angle_pvcdrom")]
        public double[] HourAnglePvcdrom { get; init; } = [];

        [JsonPropertyName("declination_cooper69")]
        public double[] DeclinationCooper69 { get; init; } = [];

        [JsonPropertyName("zenith_pvcdrom_cooper69")]
        public double[] ZenithPvcdromCooper69 { get; init; } = [];

        [JsonPropertyName("boundary_zenith")]
        public double[] BoundaryZenith { get; init; } = [];
    }
}
