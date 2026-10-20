using System.Text.Json.Serialization;
using pv;

namespace pv.Tests;

public sealed class SolarPositionTests
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
}
