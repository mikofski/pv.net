#nullable enable
using System;

namespace pv
{
    /// <summary>
    /// Immutable site inputs shared by solar-position calculations.
    /// </summary>
    public sealed class Location
    {
        /// <summary>
        /// Latitude, positive north of the equator [degrees].
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Longitude, positive east of Greenwich [degrees].
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Site altitude above sea level [m].
        /// </summary>
        public double Altitude { get; }

        /// <summary>
        /// Fixed offset from UTC [n/d].
        /// </summary>
        public TimeSpan UtcOffset { get; }

        /// <summary>
        /// Optional site name [n/d].
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Creates a site location with a fixed UTC offset.
        /// </summary>
        /// <param name="latitude">latitude, positive north of the equator [degrees] (double)</param>
        /// <param name="longitude">longitude, positive east of Greenwich [degrees] (double)</param>
        /// <param name="altitude">site altitude above sea level [m] (double)</param>
        /// <param name="utcOffset">fixed offset from UTC [n/d] (TimeSpan)</param>
        /// <param name="name">optional location name [n/d] (string, default: null)</param>
        /// <exception cref="ArgumentOutOfRangeException">A coordinate, altitude, or UTC offset is outside its valid range.</exception>
        public Location(
            double latitude,
            double longitude,
            double altitude,
            TimeSpan utcOffset,
            string? name = null)
        {
            if (!double.IsFinite(latitude) || latitude < -90.0 || latitude > 90.0)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be finite and between -90 and 90 degrees.");
            }

            if (!double.IsFinite(longitude) || longitude < -180.0 || longitude > 180.0)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be finite and between -180 and 180 degrees.");
            }

            if (!double.IsFinite(altitude))
            {
                throw new ArgumentOutOfRangeException(nameof(altitude), "Altitude must be finite.");
            }

            if (utcOffset < TimeSpan.FromHours(-14)
                || utcOffset > TimeSpan.FromHours(14)
                || utcOffset.Ticks % TimeSpan.TicksPerMinute != 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(utcOffset),
                    "UTC offset must be a whole number of minutes between -14 and +14 hours.");
            }

            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            UtcOffset = utcOffset;
            Name = name;
        }
    }
}
