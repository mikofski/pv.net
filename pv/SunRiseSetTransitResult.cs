using System;

namespace pv
{
    /// <summary>
    /// Contains sunrise, sunset, and transit times for a sequence of days.
    /// </summary>
    public class SunRiseSetTransitResult
    {
        /// <summary>
        /// Sunrise times, in n/d (array of DateTimeOffset).
        /// </summary>
        public DateTimeOffset[] Sunrise { get; init; } = Array.Empty<DateTimeOffset>();

        /// <summary>
        /// Sunset times, in n/d (array of DateTimeOffset).
        /// </summary>
        public DateTimeOffset[] Sunset { get; init; } = Array.Empty<DateTimeOffset>();

        /// <summary>
        /// Solar transit times, in n/d (array of DateTimeOffset).
        /// </summary>
        public DateTimeOffset[] Transit { get; init; } = Array.Empty<DateTimeOffset>();
    }
}
