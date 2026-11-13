using System.Text.Json;
using System.Text.Json.Serialization;
using pv;

namespace pv.Tests;

public class EphemerisTests
{
    private static FixtureCase<EphemerisInputs, JsonElement> LoadCase(string name)
    {
        var fixture = FixtureLoader.Load<EphemerisInputs, JsonElement>("ephemeris.json");
        Assert.Equal("0.16.1", fixture.PvlibVersion);
        return Assert.Single(fixture.Cases, testCase => testCase.Name == name);
    }

    [Fact]
    public void MatchesPvlibFixtureForAllTimestampAndParameterCases()
    {
        var fixture = FixtureLoader.Load<EphemerisInputs, JsonElement>("ephemeris.json");
        Assert.Equal("0.16.1", fixture.PvlibVersion);
        Assert.Contains(fixture.Cases, testCase => testCase.Name == "multi_season_utc");
        Assert.Contains(fixture.Cases, testCase => testCase.Name == "fractional_seconds_utc");

        foreach (var testCase in fixture.Cases)
        {
            var inputs = testCase.Inputs!;
            var actual = Calculate(inputs);

            FixtureAssert.AssertTimesClose(inputs.Times, actual.Times, toleranceSeconds: 0);
            for (var i = 0; i < inputs.Times.Length; i++)
            {
                Assert.Equal(inputs.Times[i].Offset, actual.Times[i].Offset);
                Assert.Equal(inputs.Times[i].DateTime, actual.Times[i].DateTime);
            }

            AssertColumn(testCase.Expected, "apparent_elevation", actual.ApparentElevation);
            AssertColumn(testCase.Expected, "elevation", actual.Elevation);
            AssertColumn(testCase.Expected, "azimuth", actual.Azimuth);
            AssertColumn(testCase.Expected, "apparent_zenith", actual.ApparentZenith);
            AssertColumn(testCase.Expected, "zenith", actual.Zenith);
            AssertColumn(testCase.Expected, "solar_time", actual.SolarTime!);
            Assert.Null(actual.EquationOfTime);
        }
    }

    [Fact]
    public void MatchesPvlibExpectedSolarPositionRoundedToTwoDecimals()
    {
        var testCase = LoadCase("physical_mst");
        var result = Calculate(testCase.Inputs!);

        Assert.Equal(39.87, Math.Round(result.Elevation[0], 2));
        Assert.Equal(50.11, Math.Round(result.ApparentZenith[0], 2));
        Assert.Equal(194.34, Math.Round(result.Azimuth[0], 2));
        Assert.Equal(39.89, Math.Round(result.ApparentElevation[0], 2));
    }

    [Fact]
    public void EmptyInputsAndNaNParametersArePreserved()
    {
        var location = new Location(0, 0, 0, TimeSpan.Zero);
        var empty = SolarPosition.Ephemeris(location, []);
        Assert.Empty(empty.Times);
        Assert.Empty(empty.ApparentElevation);
        Assert.Empty(empty.SolarTime!);

        var nanPressure = SolarPosition.Ephemeris(
            location,
            [new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero)],
            double.NaN);
        Assert.True(double.IsNaN(nanPressure.ApparentElevation[0]));
    }

    [Fact]
    public void ParameterArraysMustMatchTimes()
    {
        DateTimeOffset[] times = [new(2019, 1, 1, 12, 0, 0, TimeSpan.Zero)];
        var location = new Location(0, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentNullException>(() =>
            SolarPosition.Ephemeris(location, times, (double[])null!));
        Assert.Throws<ArgumentNullException>(() =>
            SolarPosition.Ephemeris(location, times, 101325, (double[])null!));
        Assert.Throws<ArgumentException>(() =>
            SolarPosition.Ephemeris(location, times, new[] { 80000.0, 90000.0 }));
        Assert.Throws<ArgumentException>(() =>
            SolarPosition.Ephemeris(location, times, 101325, new[] { 10.0, 20.0 }));
    }

    [Fact]
    public void LocationAndTimestampOffsetsAreRequired()
    {
        DateTimeOffset[] times =
        [
            new(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new(2019, 1, 1, 12, 0, 0, TimeSpan.FromHours(-5))
        ];

        Assert.Throws<ArgumentNullException>(() =>
            SolarPosition.Ephemeris(null!, times));
        Assert.Throws<ArgumentException>(() =>
            SolarPosition.Ephemeris(new Location(0, 0, 0, TimeSpan.Zero), times));
    }

    private static SolarPositionResult Calculate(EphemerisInputs inputs)
    {
        var pressureArray = inputs.Pressure.ValueKind == JsonValueKind.Array;
        var temperatureArray = inputs.Temperature.ValueKind == JsonValueKind.Array;
        var pressure = pressureArray
            ? inputs.Pressure.EnumerateArray().Select(value => value.GetDouble()).ToArray()
            : null;
        var temperature = temperatureArray
            ? inputs.Temperature.EnumerateArray().Select(value => value.GetDouble()).ToArray()
            : null;
        var pressureScalar = pressureArray ? 0 : inputs.Pressure.GetDouble();
        var temperatureScalar = temperatureArray ? 0 : inputs.Temperature.GetDouble();
        var location = new Location(
            inputs.Latitude,
            inputs.Longitude,
            0,
            inputs.Times.Length == 0 ? TimeSpan.Zero : inputs.Times[0].Offset);

        if (pressureArray && temperatureArray)
        {
            return SolarPosition.Ephemeris(
                location, inputs.Times, pressure!, temperature!);
        }

        if (pressureArray)
        {
            return SolarPosition.Ephemeris(
                location, inputs.Times, pressure!, temperatureScalar);
        }

        if (temperatureArray)
        {
            return SolarPosition.Ephemeris(
                location, inputs.Times, pressureScalar, temperature!);
        }

        return SolarPosition.Ephemeris(
            location, inputs.Times, pressureScalar, temperatureScalar);
    }

    private static void AssertColumn(JsonElement expected, string name, double[] actual)
    {
        var values = expected.GetProperty(name)
            .EnumerateArray()
            .Select(value => value.GetDouble())
            .ToArray();
        FixtureAssert.AssertAllClose(values, actual, rtol: 1e-9, atol: 0);
    }

    public sealed class EphemerisInputs
    {
        [JsonPropertyName("times")]
        public DateTimeOffset[] Times { get; init; } = [];

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("pressure")]
        public JsonElement Pressure { get; init; }

        [JsonPropertyName("temperature")]
        public JsonElement Temperature { get; init; }
    }
}
