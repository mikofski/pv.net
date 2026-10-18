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

    [Fact]
    public void LoadsPolarEventsAsNullAndPreservesTransit()
    {
        var fixture = FixtureLoader.Load<EventInput, EventOutput>("sun_rise_set_transit_spa_polar.json");
        Assert.Equal(2, fixture.Cases.Count);

        foreach (var testCase in fixture.Cases)
        {
            Assert.Null(Assert.Single(testCase.Expected!.Sunrise));
            Assert.Null(Assert.Single(testCase.Expected.Sunset));
            var transit = Assert.Single(testCase.Expected.Transit);
            Assert.Equal(testCase.Inputs!.Timestamp.Date, transit.Date);
        }
    }

    public sealed class DateInput
    {
        public DateTimeOffset Timestamp { get; init; }
    }

    public sealed class EventInput
    {
        public DateTimeOffset Timestamp { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }

    public sealed class EventOutput
    {
        public DateTimeOffset?[] Sunrise { get; init; } = [];
        public DateTimeOffset?[] Sunset { get; init; } = [];
        public DateTimeOffset[] Transit { get; init; } = [];
    }
}
