using System;

namespace pv
{
    /// <summary>
    /// Contains aligned sunrise, sunset, and transit times for a batch of input timestamps.
    /// </summary>
    /// <remarks>
    /// Each event array has one entry per input timestamp. Daily events may repeat for timestamps
    /// on the same local date. Sunrise and sunset are independently nullable; their absence does
    /// not imply that transit is absent. High-level calculations return new event arrays owned by
    /// the result; callers may mutate them without affecting later calculations.
    /// </remarks>
    public class SunRiseSetTransitResult
    {
        /// <summary>
        /// Input timestamps in their supplied fixed UTC offset [n/d] (array of DateTimeOffset).
        /// </summary>
        public DateTimeOffset[] Times { get; init; } = Array.Empty<DateTimeOffset>();

        /// <summary>
        /// Sunrise times [n/d] (array of nullable DateTimeOffset); null when sunrise does not occur.
        /// </summary>
        public DateTimeOffset?[] Sunrise { get; init; } = Array.Empty<DateTimeOffset?>();

        /// <summary>
        /// Sunset times [n/d] (array of nullable DateTimeOffset); null when sunset does not occur.
        /// </summary>
        public DateTimeOffset?[] Sunset { get; init; } = Array.Empty<DateTimeOffset?>();

        /// <summary>
        /// Solar transit times [n/d] (array of DateTimeOffset).
        /// </summary>
        public DateTimeOffset[] Transit { get; init; } = Array.Empty<DateTimeOffset>();
    }
}
