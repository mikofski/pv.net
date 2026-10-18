using System.Text.Json;
using pv;

namespace pv.Tests;

public class SpaSolarPositionTests
{
    private static FixtureCase<JsonElement, JsonElement> LoadCase(string name)
    {
        var fixture = FixtureLoader.Load<JsonElement, JsonElement>("spa_solar_position.json");
        Assert.Equal("0.16.1", fixture.PvlibVersion);
        return Assert.Single(fixture.Cases, testCase => testCase.Name == name);
    }

    private static double[] Doubles(JsonElement values) =>
        values.EnumerateArray().Select(value => value.GetDouble()).ToArray();

    private static void AssertScalar(JsonElement expected, double actual) =>
        FixtureAssert.AssertAllClose([expected.GetDouble()], [actual], rtol: 1e-7, atol: 0);

    private static void AssertPublished(double expected, double actual, int decimals) =>
        FixtureAssert.AssertAllClose([expected], [actual], rtol: 0, atol: 1.5 * Math.Pow(10, -decimals));

    [Fact]
    public void ScalarSolarPositionChainMatchesPvlibFixtureAndPublishedValues()
    {
        var testCase = LoadCase("spa_chain");
        var inputs = testCase.Inputs;
        var expected = testCase.Expected;
        var lat = inputs.GetProperty("lat").GetDouble();
        var lon = inputs.GetProperty("lon").GetDouble();
        var elev = inputs.GetProperty("elev").GetDouble();
        var pressure = inputs.GetProperty("pressure").GetDouble();
        var temp = inputs.GetProperty("temp").GetDouble();
        var jme = inputs.GetProperty("jme").GetDouble();
        var jce = inputs.GetProperty("jce").GetDouble();
        var l = inputs.GetProperty("l").GetDouble();
        var b = inputs.GetProperty("b").GetDouble();
        var r = inputs.GetProperty("r").GetDouble();
        var theta = inputs.GetProperty("theta").GetDouble();
        var beta = inputs.GetProperty("beta").GetDouble();
        var epsilon = inputs.GetProperty("epsilon").GetDouble();
        var dpsi = inputs.GetProperty("dpsi").GetDouble();
        var delta = inputs.GetProperty("delta").GetDouble();
        var xi = inputs.GetProperty("xi").GetDouble();
        var hourAngle = inputs.GetProperty("hourAngle").GetDouble();
        var deltaAlpha = inputs.GetProperty("dAlpha").GetDouble();
        var deltaPrime = inputs.GetProperty("deltaPrime").GetDouble();
        var hourAnglePrime = inputs.GetProperty("hourAnglePrime").GetDouble();
        var elevation0 = inputs.GetProperty("elevation0").GetDouble();
        var de = inputs.GetProperty("de").GetDouble();
        var elevation = inputs.GetProperty("elevation").GetDouble();
        var azimuthAstronomers = inputs.GetProperty("azimuthAstronomers").GetDouble();
        var actual = new Dictionary<string, double>
        {
            ["heliocentricLongitude"] = Spa.HeliocentricLongitude(jme),
            ["heliocentricLatitude"] = Spa.HeliocentricLatitude(jme),
            ["heliocentricRadiusVector"] = Spa.HeliocentricRadiusVector(jme),
            ["geocentricLongitude"] = Spa.GeocentricLongitude(l),
            ["geocentricLatitude"] = Spa.GeocentricLatitude(b),
            ["meanElongation"] = Spa.MeanElongation(jce),
            ["meanAnomalySun"] = Spa.MeanAnomalySun(jce),
            ["meanAnomalyMoon"] = Spa.MeanAnomalyMoon(jce),
            ["moonArgumentLatitude"] = Spa.MoonArgumentLatitude(jce),
            ["moonAscendingLongitude"] = Spa.MoonAscendingLongitude(jce),
            ["meanEclipticObliquity"] = Spa.MeanEclipticObliquity(jme),
            ["trueEclipticObliquity"] = Spa.TrueEclipticObliquity(
                inputs.GetProperty("epsilon0").GetDouble(), inputs.GetProperty("deps").GetDouble()),
            ["aberrationCorrection"] = Spa.AberrationCorrection(r),
            ["apparentSunLongitude"] = Spa.ApparentSunLongitude(
                theta, dpsi, inputs.GetProperty("dtau").GetDouble()),
            ["meanSiderealTime"] = Spa.MeanSiderealTime(
                inputs.GetProperty("jd").GetDouble(), inputs.GetProperty("jc").GetDouble()),
            ["apparentSiderealTime"] = Spa.ApparentSiderealTime(
                inputs.GetProperty("v0").GetDouble(), dpsi, epsilon),
            ["geocentricSunRightAscension"] = Spa.GeocentricSunRightAscension(
                inputs.GetProperty("lamd").GetDouble(), epsilon, beta),
            ["geocentricSunDeclination"] = Spa.GeocentricSunDeclination(
                inputs.GetProperty("lamd").GetDouble(), epsilon, beta),
            ["localHourAngle"] = Spa.LocalHourAngle(
                inputs.GetProperty("v").GetDouble(), lon, inputs.GetProperty("alpha").GetDouble()),
            ["equatorialHorizontalParallax"] = Spa.EquatorialHorizontalParallax(r),
            ["uterm"] = Spa.Uterm(lat),
            ["xterm"] = Spa.Xterm(Spa.Uterm(lat), lat, elev),
            ["yterm"] = Spa.Yterm(Spa.Uterm(lat), lat, elev),
            ["parallaxSunRightAscension"] = Spa.ParallaxSunRightAscension(
                Spa.Xterm(Spa.Uterm(lat), lat, elev), xi, hourAngle, delta),
            ["topocentricSunRightAscension"] = Spa.TopocentricSunRightAscension(
                inputs.GetProperty("alpha").GetDouble(), deltaAlpha),
            ["topocentricSunDeclination"] = Spa.TopocentricSunDeclination(
                delta, Spa.Xterm(Spa.Uterm(lat), lat, elev), Spa.Yterm(Spa.Uterm(lat), lat, elev),
                xi, deltaAlpha, hourAngle),
            ["topocentricLocalHourAngle"] = Spa.TopocentricLocalHourAngle(hourAngle, deltaAlpha),
            ["topocentricElevationAngleWithoutAtmosphere"] = Spa.TopocentricElevationAngleWithoutAtmosphere(
                lat, deltaPrime, hourAnglePrime),
            ["atmosphericRefractionCorrection"] = Spa.AtmosphericRefractionCorrection(
                pressure, temp, elevation0, inputs.GetProperty("atmosRefract").GetDouble()),
            ["topocentricElevationAngle"] = Spa.TopocentricElevationAngle(elevation0, de),
            ["topocentricZenithAngle"] = Spa.TopocentricZenithAngle(elevation),
            ["topocentricAstronomersAzimuth"] = Spa.TopocentricAstronomersAzimuth(
                hourAnglePrime, deltaPrime, lat),
            ["topocentricAzimuthAngle"] = Spa.TopocentricAzimuthAngle(azimuthAstronomers),
            ["sunMeanLongitude"] = Spa.SunMeanLongitude(jme),
            ["equationOfTime"] = Spa.EquationOfTime(
                Spa.SunMeanLongitude(jme), inputs.GetProperty("alpha").GetDouble(), dpsi, epsilon),
        };

        foreach (var (name, value) in actual)
        {
            AssertScalar(expected.GetProperty(name), value);
        }

        var nutation = Spa.LongitudeObliquityNutation(
            jce,
            inputs.GetProperty("x0").GetDouble(),
            inputs.GetProperty("x1").GetDouble(),
            inputs.GetProperty("x2").GetDouble(),
            inputs.GetProperty("x3").GetDouble(),
            inputs.GetProperty("x4").GetDouble());
        FixtureAssert.AssertAllClose(
            Doubles(expected.GetProperty("longitudeObliquityNutation")),
            [nutation.LongitudeNutation, nutation.ObliquityNutation], rtol: 1e-7, atol: 0);

        var coefficientRows = inputs.GetProperty("sumCoefficients").EnumerateArray()
            .Select(Doubles).ToArray();
        var coefficients = new double[coefficientRows.Length, 3];
        for (var row = 0; row < coefficientRows.Length; row++)
        {
            for (var column = 0; column < 3; column++)
            {
                coefficients[row, column] = coefficientRows[row][column];
            }
        }
        AssertScalar(expected.GetProperty("sumMultCosAddMultExample"),
            Spa.SumMultCosAddMult(coefficients, jme));

        AssertPublished(24.0182616917, Spa.HeliocentricLongitude(jme), 6);
        AssertPublished(-0.0001011219, Spa.HeliocentricLatitude(jme), 6);
        AssertPublished(0.9965422974, Spa.HeliocentricRadiusVector(jme), 6);
        AssertPublished(204.0182616917, Spa.GeocentricLongitude(l), 6);
        AssertPublished(0.0001011219, Spa.GeocentricLatitude(b), 6);
        AssertPublished(-0.00399840, nutation.LongitudeNutation, 6);
        AssertPublished(0.00166657, nutation.ObliquityNutation, 6);
        AssertPublished(14.64, actual["equationOfTime"], 2);
    }

    [Fact]
    public void SolarPositionReturnsPvlibNormalAndModeResults()
    {
        var testCase = LoadCase("spa_chain");
        var inputs = testCase.Inputs;
        var expected = testCase.Expected;
        var unixtimes = Doubles(inputs.GetProperty("unixtimes"));
        var lat = inputs.GetProperty("lat").GetDouble();
        var lon = inputs.GetProperty("lon").GetDouble();
        var elev = inputs.GetProperty("elev").GetDouble();
        var pressure = inputs.GetProperty("pressure").GetDouble();
        var temp = inputs.GetProperty("temp").GetDouble();
        var deltaT = inputs.GetProperty("deltaT").GetDouble();
        var atmosRefract = inputs.GetProperty("atmosRefract").GetDouble();

        var normal = Spa.SolarPosition(unixtimes, lat, lon, elev, pressure, temp, deltaT, atmosRefract);
        var expectedNormal = expected.GetProperty("solarPosition");
        FixtureAssert.AssertAllClose(Doubles(expectedNormal[0]), normal.ApparentZenith, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(Doubles(expectedNormal[1]), normal.Zenith, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(Doubles(expectedNormal[2]), normal.ApparentElevation, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(Doubles(expectedNormal[3]), normal.Elevation, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(Doubles(expectedNormal[4]), normal.Azimuth, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(Doubles(expectedNormal[5]), normal.EquationOfTime, rtol: 1e-7, atol: 0);
        AssertPublished(50.11162, normal.ApparentZenith[0], 5);
        AssertPublished(50.127954, normal.Zenith[0], 5);
        AssertPublished(39.888378, normal.ApparentElevation[0], 5);
        AssertPublished(39.872046, normal.Elevation[0], 5);
        AssertPublished(194.340241, normal.Azimuth[0], 5);

        var sst = Spa.SolarPositionSst(unixtimes, deltaT);
        var expectedSst = expected.GetProperty("solarPositionSst");
        FixtureAssert.AssertAllClose(Doubles(expectedSst[0]), sst.ApparentSiderealTime, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(Doubles(expectedSst[1]), sst.RightAscension, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(Doubles(expectedSst[2]), sst.Declination, rtol: 1e-7, atol: 0);

        var esd = Spa.SolarPositionEsd(unixtimes, deltaT);
        FixtureAssert.AssertAllClose(Doubles(expected.GetProperty("solarPositionEsd")), esd, rtol: 1e-7, atol: 0);

        var arrayDeltaT = Spa.SolarPosition(unixtimes, lat, lon, elev, pressure, temp, [deltaT], atmosRefract);
        Assert.Throws<ArgumentException>(() =>
            Spa.SolarPosition(unixtimes, lat, lon, elev, pressure, temp, Array.Empty<double>(), atmosRefract));
        Assert.Empty(Spa.SolarPosition([], lat, lon, elev, pressure, temp, Array.Empty<double>(), atmosRefract).Zenith);
        Assert.Equal(normal.Zenith, arrayDeltaT.Zenith);
    }

    [Fact]
    public void SolarPositionSstAndEsdMatchHourly2015Fixture()
    {
        var sstCase = LoadCase("hourly_2015_sst");
        var esdCase = LoadCase("hourly_2015_esd");
        var start = DateTimeOffset.Parse(sstCase.Inputs.GetProperty("start").GetString()!);
        var count = sstCase.Inputs.GetProperty("count").GetInt32();
        var step = sstCase.Inputs.GetProperty("stepSeconds").GetInt32();
        var unixtimes = Enumerable.Range(0, count)
            .Select(i => (double)(start.ToUnixTimeSeconds() + i * step)).ToArray();
        var deltaT = Enumerable.Range(0, count)
            .Select(i => Spa.CalculateDeltaT(2015, start.AddSeconds(i * step).Month)).ToArray();

        var sst = Spa.SolarPositionSst(unixtimes, deltaT);
        FixtureAssert.AssertAllClose(
            Doubles(sstCase.Expected.GetProperty("apparentSiderealTime")), sst.ApparentSiderealTime, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(
            Doubles(sstCase.Expected.GetProperty("rightAscension")), sst.RightAscension, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(
            Doubles(sstCase.Expected.GetProperty("declination")), sst.Declination, rtol: 1e-7, atol: 0);
        FixtureAssert.AssertAllClose(
            Doubles(esdCase.Expected.GetProperty("radiusVector")),
            Spa.SolarPositionEsd(unixtimes, Doubles(esdCase.Inputs.GetProperty("deltaT"))), rtol: 1e-7, atol: 0);
        Assert.Equal(8760, sst.Declination.Length);
    }

    [Fact]
    public void SolarPositionModesPreserveNanAndValidateNullInputs()
    {
        var result = Spa.SolarPosition([double.NaN], 0, 0, 0, 1013.25, 25, 67, 0.5667);
        Assert.True(double.IsNaN(result.ApparentZenith[0]));
        Assert.True(double.IsNaN(Spa.HeliocentricLongitude(double.NaN)));
        Assert.Throws<ArgumentNullException>(() =>
            Spa.SolarPosition(null!, 0, 0, 0, 1013.25, 25, 67, 0.5667));
        Assert.Throws<ArgumentNullException>(() => Spa.SolarPositionSst(null!, 67));
        Assert.Throws<ArgumentNullException>(() => Spa.SolarPositionEsd(null!, 67));
    }
}
