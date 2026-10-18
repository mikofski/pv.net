#nullable enable
using System;

namespace pv
{
    /// <summary>
    /// Contains aligned solar position values for one batch of times.
    /// </summary>
    /// <remarks>
    /// Every populated output array aligns by index with <see cref="Times"/> and has the same length.
    /// High-level calculations return a new result and newly allocated output arrays; callers may mutate
    /// those arrays without affecting later calculations. Optional outputs are null when not requested.
    /// </remarks>
    public class SolarPositionResult
    {
        /// <summary>
        /// Input timestamps in their supplied fixed UTC offset [n/d] (array of DateTimeOffset).
        /// </summary>
        public DateTimeOffset[] Times { get; init; } = Array.Empty<DateTimeOffset>();

        /// <summary>
        /// Solar zenith without atmospheric refraction [degrees] (array of double).
        /// </summary>
        public double[] Zenith { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar zenith including atmospheric refraction [degrees] (array of double).
        /// </summary>
        public double[] ApparentZenith { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar elevation without atmospheric refraction [degrees] (array of double).
        /// </summary>
        public double[] Elevation { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar elevation including atmospheric refraction [degrees] (array of double).
        /// </summary>
        public double[] ApparentElevation { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar azimuth [degrees] (array of double).
        /// </summary>
        public double[] Azimuth { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar declination [degrees] (array of double).
        /// </summary>
        public double[] Declination { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar hour angle [degrees] (array of double).
        /// </summary>
        public double[] HourAngle { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Equation of time [minutes] (array of double); null if not calculated.
        /// </summary>
        public double[]? EquationOfTime { get; init; }

        /// <summary>
        /// Local solar time [hours] (array of double); null if not calculated.
        /// </summary>
        public double[]? SolarTime { get; init; }
    }
}
