using pv;

namespace pv.Tests;

public class AtmosphereTests
{
    private static FixtureCase<AtmosphereInputs, double[]> LoadCase(string name)
    {
        var fixture = FixtureLoader.Load<AtmosphereInputs, double[]>("atmosphere_pres2alt_alt2pres.json");
        Assert.Equal("0.16.1", fixture.PvlibVersion);
        return Assert.Single(fixture.Cases, testCase => testCase.Name == name);
    }

    [Fact]
    public void Pres2AltMatchesUpstreamTest()
    {
        var actual = Atmosphere.Pres2Alt([10000, 90000, 101325]);

        FixtureAssert.AssertAllClose([15797.638, 988.637, 0.124], actual, rtol: 1e-7, atol: 0.001);
    }

    [Fact]
    public void Alt2PresMatchesUpstreamTest()
    {
        var actual = Atmosphere.Alt2Pres([-100, 0, 1000, 8000]);

        FixtureAssert.AssertAllClose(
            [102532.073, 101324.999, 89874.750, 35600.496], actual, rtol: 1e-7, atol: 0.001);
    }

    [Theory]
    [InlineData("pres2alt_upstream")]
    [InlineData("pres2alt_solarposition")]
    [InlineData("pres2alt_edges")]
    public void Pres2AltScalarAndArrayMatchFixture(string name)
    {
        var testCase = LoadCase(name);
        var pressure = testCase.Inputs!.Pressure;
        var original = (double[])pressure.Clone();
        var actual = Atmosphere.Pres2Alt(pressure);

        FixtureAssert.AssertAllClose(testCase.Expected!, actual, rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(
            testCase.Expected!, Array.ConvertAll(pressure, Atmosphere.Pres2Alt), rtol: 1e-9, atol: 0);
        Assert.NotSame(pressure, actual);
        Assert.Equal(original, pressure);
    }

    [Theory]
    [InlineData("alt2pres_upstream")]
    [InlineData("alt2pres_solarposition")]
    [InlineData("alt2pres_edges")]
    public void Alt2PresScalarAndArrayMatchFixture(string name)
    {
        var testCase = LoadCase(name);
        var altitude = testCase.Inputs!.Altitude;
        var original = (double[])altitude.Clone();
        var actual = Atmosphere.Alt2Pres(altitude);

        FixtureAssert.AssertAllClose(testCase.Expected!, actual, rtol: 1e-9, atol: 0);
        FixtureAssert.AssertAllClose(
            testCase.Expected!, Array.ConvertAll(altitude, Atmosphere.Alt2Pres), rtol: 1e-9, atol: 0);
        Assert.NotSame(altitude, actual);
        Assert.Equal(original, altitude);
    }

    [Fact]
    public void EmptyArraysReturnEmptyArrays()
    {
        Assert.Empty(Atmosphere.Pres2Alt(Array.Empty<double>()));
        Assert.Empty(Atmosphere.Alt2Pres(Array.Empty<double>()));
    }

    [Fact]
    public void NullArraysThrow()
    {
        Assert.Throws<ArgumentNullException>(() => Atmosphere.Pres2Alt((double[])null!));
        Assert.Throws<ArgumentNullException>(() => Atmosphere.Alt2Pres((double[])null!));
    }

    public sealed class AtmosphereInputs
    {
        public double[] Pressure { get; init; } = [];
        public double[] Altitude { get; init; } = [];
    }
}
