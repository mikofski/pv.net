using System;

namespace pv
{
    /// <summary>
    /// Contains sunrise, sunset, and transit times for a sequence of days.
    /// </summary>
    public class SunRiseSetTransitResult
    {
        /// <summary>
        /// Input times associated with the solar events, in n/d (array of DateTimeOffset).
        /// </summary>
        public DateTimeOffset[] Times { get; init; } = Array.Empty<DateTimeOffset>();

        /// <summary>
        /// Sunrise times, in n/d (array of nullable DateTimeOffset); null when sunrise does not occur.
        /// </summary>
        public DateTimeOffset?[] Sunrise { get; init; } = Array.Empty<DateTimeOffset?>();

        /// <summary>
        /// Sunset times, in n/d (array of nullable DateTimeOffset); null when sunset does not occur.
        /// </summary>
        public DateTimeOffset?[] Sunset { get; init; } = Array.Empty<DateTimeOffset?>();

        /// <summary>
        /// Solar transit times, in n/d (array of DateTimeOffset).
        /// </summary>
        public DateTimeOffset[] Transit { get; init; } = Array.Empty<DateTimeOffset>();
    }
}
