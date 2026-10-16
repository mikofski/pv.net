namespace pv.Tests;

public class FixtureLoaderTests
{
    [Fact]
    public void LoadsNullAsNaNAndOffsetStringsAsDateTimeOffset()
    {
        var fixture = FixtureLoader.Load<DateInput, double[]>("fixture_loader.json");
        var testCase = Assert.Single(fixture.Cases);

        Assert.Equal("test", fixture.PvlibVersion);
        Assert.Equal(new DateTimeOffset(2021, 6, 1, 12, 30, 0, TimeSpan.FromHours(5.5)),
            testCase.Inputs!.Timestamp);
        Assert.Equal(2.0, testCase.Expected![0]);
        Assert.True(double.IsNaN(testCase.Expected[1]));
    }

    public sealed class DateInput
    {
        public DateTimeOffset Timestamp { get; init; }
    }
}
