namespace pv.Tests;

public static class FixtureAssert
{
    public static void AssertAllClose(double[] expected, double[] actual, double rtol, double atol)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rtol);
        ArgumentOutOfRangeException.ThrowIfNegative(atol);
        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            if (double.IsNaN(expected[i]))
            {
                Assert.True(double.IsNaN(actual[i]), $"Expected NaN at index {i}, actual: {actual[i]}.");
                continue;
            }

            if (expected[i].Equals(actual[i]))
            {
                continue;
            }

            Assert.True(double.IsFinite(expected[i]) && double.IsFinite(actual[i]),
                $"Values differ at index {i}: expected {expected[i]}, actual {actual[i]}.");
            var tolerance = atol + rtol * Math.Abs(expected[i]);
            Assert.True(Math.Abs(expected[i] - actual[i]) <= tolerance,
                $"Values differ at index {i}: expected {expected[i]}, actual {actual[i]}, tolerance {tolerance}.");
        }
    }

    public static void AssertTimesClose(
        DateTimeOffset[] expected,
        DateTimeOffset[] actual,
        double toleranceSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(toleranceSeconds);
        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            var difference = Math.Abs((expected[i] - actual[i]).TotalSeconds);
            Assert.True(difference <= toleranceSeconds,
                $"Times differ at index {i}: expected {expected[i]:O}, actual {actual[i]:O}, " +
                $"difference {difference} seconds.");
        }
    }
}
