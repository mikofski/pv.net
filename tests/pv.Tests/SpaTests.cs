using System.Text.Json;
using pv;

namespace pv.Tests;

public class SpaTests
{
    private const double JD = 2452930.312847;
    private const double JDE = 2452930.313623;
    private const double JC = 0.0379277986858;
    private const double JCE = 0.037927819916852;
    private const double JME = 0.003792781991685;
    private const double DeltaT = 67.0;
    private const double DtActual = 54.413442486;

    private static FixtureCase<JsonElement, JsonElement> LoadCase(string name)
    {
        var fixture = FixtureLoader.Load<JsonElement, JsonElement>("spa_time.json");
        Assert.Equal("0.16.1", fixture.PvlibVersion);
        return Assert.Single(fixture.Cases, testCase => testCase.Name == name);
    }

    private static int[] Integers(JsonElement values) =>
        values.EnumerateArray().Select(value => value.GetInt32()).ToArray();

    private static double[] Doubles(JsonElement values) =>
        values.EnumerateArray().Select(value => value.GetDouble()).ToArray();

    private static void AssertFixture(JsonElement expected, double actual) =>
        FixtureAssert.AssertAllClose([expected.GetDouble()], [actual], rtol: 1e-7, atol: 0);

    private static void AssertAlmostEqual(double expected, double actual, int decimals = 7) =>
        FixtureAssert.AssertAllClose([expected], [actual], rtol: 0, atol: 1.5 * Math.Pow(10, -decimals));

    [Fact]
    public void JulianDayDt()
    {
        var testCase = LoadCase("julian_day_dt");
        var c = Integers(testCase.Inputs.GetProperty("components"));
        Assert.Equal(new[] { 2003, 10, 17, 19, 30, 30, 1 }, c);
        var actual = Spa.JulianDayDt(c[0], c[1], c[2], c[3], c[4], c[5], c[6]);

        AssertAlmostEqual(JD + 1e-6 / 86400, actual, 6);
        AssertFixture(testCase.Expected, actual);
    }

    [Theory]
    [InlineData("julian_day")]
    [InlineData("unix_epoch")]
    public void JulianDay(string name)
    {
        var testCase = LoadCase(name);
        var unixtimes = Doubles(testCase.Inputs.GetProperty("unixtimes"));
        var actual = Array.ConvertAll(unixtimes, Spa.JulianDay);

        FixtureAssert.AssertAllClose(Doubles(testCase.Expected), actual, rtol: 1e-7, atol: 0);
        if (name == "julian_day")
        {
            AssertAlmostEqual(JD, actual[0], 6);
            var time = DateTimeOffset.Parse(testCase.Inputs.GetProperty("times")[0].GetString()!);
            Assert.Equal(TimeSpan.FromHours(-7), time.Offset);
            Assert.Equal((double)time.ToUnixTimeSeconds(), unixtimes[0]);
        }
        else
        {
            Assert.Equal(2440587.5, actual[0]);
        }
    }

    [Fact]
    public void JulianEphemerisDay()
    {
        var testCase = LoadCase("julian_ephemeris_day");
        var actual = Spa.JulianEphemerisDay(
            testCase.Inputs.GetProperty("julianDay").GetDouble(),
            testCase.Inputs.GetProperty("deltaT").GetDouble());

        AssertAlmostEqual(JDE, actual, 5);
        AssertFixture(testCase.Expected, actual);
    }

    [Fact]
    public void JulianCentury()
    {
        var testCase = LoadCase("julian_century");
        var actual = Spa.JulianCentury(testCase.Inputs.GetProperty("julianDay").GetDouble());

        AssertAlmostEqual(JC, actual, 6);
        AssertFixture(testCase.Expected, actual);
    }

    [Fact]
    public void JulianEphemerisCentury()
    {
        var testCase = LoadCase("julian_ephemeris_century");
        var actual = Spa.JulianEphemerisCentury(
            testCase.Inputs.GetProperty("julianEphemerisDay").GetDouble());

        AssertAlmostEqual(JCE, actual, 10);
        AssertFixture(testCase.Expected, actual);
    }

    [Fact]
    public void JulianEphemerisMillennium()
    {
        var testCase = LoadCase("julian_ephemeris_millennium");
        var actual = Spa.JulianEphemerisMillennium(
            testCase.Inputs.GetProperty("julianEphemerisCentury").GetDouble());

        AssertAlmostEqual(JME, actual, 10);
        AssertFixture(testCase.Expected, actual);
    }

    [Fact]
    public void CalculateDeltaTScalar()
    {
        var testCase = LoadCase("calculate_deltat_scalar");
        var actual = Spa.CalculateDeltaT(
            testCase.Inputs.GetProperty("year").GetInt32(),
            testCase.Inputs.GetProperty("month").GetInt32());

        AssertAlmostEqual(DtActual, actual);
        AssertFixture(testCase.Expected, actual);
    }

    [Theory]
    [InlineData("calculate_deltat_array")]
    [InlineData("calculate_deltat_mix_year")]
    [InlineData("calculate_deltat_mix_month")]
    [InlineData("calculate_deltat_boundaries")]
    public void CalculateDeltaTArray(string name)
    {
        var testCase = LoadCase(name);
        var years = Integers(testCase.Inputs.GetProperty("year"));
        var months = Integers(testCase.Inputs.GetProperty("month"));
        var actual = Spa.CalculateDeltaT(years, months);

        FixtureAssert.AssertAllClose(Doubles(testCase.Expected), actual, rtol: 1e-7, atol: 0);
        for (var i = 0; i < years.Length; i++)
        {
            Assert.Equal(Spa.CalculateDeltaT(years[i], months[i]), actual[i]);
        }
        Assert.Equal(years, Integers(testCase.Inputs.GetProperty("year")));
        Assert.Equal(months, Integers(testCase.Inputs.GetProperty("month")));
        Assert.NotSame(actual, Spa.CalculateDeltaT(years, months));

        if (name == "calculate_deltat_array")
        {
            double[] published = [
                1.7184831e4, 5.7088051e3, 1.5730419e3, 1.9801820e2, 1.3596506e1,
                7.8316585, -2.1171894, 2.9289261e1, 4.0824887e1, 5.4724581e1,
                5.7426651e1, 6.4108015e1, 6.5038015e1, 9.4952955e1, 3.3050693e2
            ];
            FixtureAssert.AssertAllClose(published, actual, rtol: 0, atol: 1.5e-3);
        }
        else if (name is "calculate_deltat_mix_year" or "calculate_deltat_mix_month")
        {
            Assert.Equal(10, actual.Length);
            Assert.All(actual, value => AssertAlmostEqual(DtActual, value));
        }
    }

    [Theory]
    [InlineData(2000, 1, 1, 12, 0, 0, 2451545.0)]
    [InlineData(1999, 1, 1, 0, 0, 0, 2451179.5)]
    [InlineData(1987, 1, 27, 0, 0, 0, 2446822.5)]
    [InlineData(1987, 6, 19, 12, 0, 0, 2446966.0)]
    [InlineData(1988, 1, 27, 0, 0, 0, 2447187.5)]
    [InlineData(1988, 6, 19, 12, 0, 0, 2447332.0)]
    [InlineData(1900, 1, 1, 0, 0, 0, 2415020.5)]
    [InlineData(1600, 1, 1, 0, 0, 0, 2305447.5)]
    [InlineData(1600, 12, 31, 0, 0, 0, 2305812.5)]
    [InlineData(837, 4, 10, 7, 12, 0, 2026871.8)]
    [InlineData(-123, 12, 31, 0, 0, 0, 1676496.5)]
    [InlineData(-122, 1, 1, 0, 0, 0, 1676497.5)]
    [InlineData(-1000, 7, 12, 12, 0, 0, 1356001.0)]
    [InlineData(-1000, 2, 29, 0, 0, 0, 1355866.5)]
    [InlineData(-1001, 8, 17, 21, 36, 0, 1355671.4)]
    [InlineData(-4712, 1, 1, 12, 0, 0, 0.0)]
    public void JulianDayIssue2077(
        int year, int month, int day, int hour, int minute, int second, double expected)
    {
        Assert.Equal(expected, Spa.JulianDayDt(year, month, day, hour, minute, second, 0));
    }

    [Fact]
    public void JulianDayDtMatchesHistoricalAndLeapYearFixtures()
    {
        var fixture = FixtureLoader.Load<JsonElement, JsonElement>("spa_time.json");
        var cases = fixture.Cases.Where(testCase => testCase.Inputs.TryGetProperty("components", out _)).ToArray();
        Assert.Equal(21, cases.Length);
        foreach (var testCase in cases)
        {
            var c = Integers(testCase.Inputs.GetProperty("components"));
            var actual = Spa.JulianDayDt(c[0], c[1], c[2], c[3], c[4], c[5], c[6]);
            AssertFixture(testCase.Expected, actual);
            Assert.Equal(testCase.Expected.GetDouble(), actual);
        }
    }

    [Fact]
    public void J2000TimeScalesAreZero()
    {
        var testCase = LoadCase("j2000");
        double[] actual = [
            Spa.JulianCentury(testCase.Inputs.GetProperty("julianDay").GetDouble()),
            Spa.JulianEphemerisCentury(testCase.Inputs.GetProperty("julianEphemerisDay").GetDouble()),
            Spa.JulianEphemerisMillennium(testCase.Inputs.GetProperty("julianEphemerisCentury").GetDouble())
        ];
        FixtureAssert.AssertAllClose(Doubles(testCase.Expected), actual, rtol: 1e-7, atol: 0);
        Assert.All(actual, value => Assert.Equal(0.0, value));
    }

    [Fact]
    public void TimeScalesPreserveNaN()
    {
        Assert.True(double.IsNaN(Spa.JulianDay(double.NaN)));
        Assert.True(double.IsNaN(Spa.JulianEphemerisDay(double.NaN, DeltaT)));
        Assert.True(double.IsNaN(Spa.JulianEphemerisDay(JD, double.NaN)));
        Assert.True(double.IsNaN(Spa.JulianCentury(double.NaN)));
        Assert.True(double.IsNaN(Spa.JulianEphemerisCentury(double.NaN)));
        Assert.True(double.IsNaN(Spa.JulianEphemerisMillennium(double.NaN)));
    }

    [Fact]
    public void CalculateDeltaTValidatesArraysAndAcceptsEmptyInputs()
    {
        Assert.Throws<ArgumentNullException>(() => Spa.CalculateDeltaT(null!, [1]));
        Assert.Throws<ArgumentNullException>(() => Spa.CalculateDeltaT([2000], null!));
        Assert.Throws<ArgumentException>(() => Spa.CalculateDeltaT([2000, 2001], [1]));
        Assert.Throws<ArgumentException>(() => Spa.CalculateDeltaT([2000], [1, 2]));
        Assert.Empty(Spa.CalculateDeltaT([], []));
    }
}
