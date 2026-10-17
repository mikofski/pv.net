using System;

namespace pv
{
    /// <summary>
    /// Contains solar position values for a sequence of times.
    /// </summary>
    public class SolarPositionResult
    {
        /// <summary>
        /// Times associated with the solar position, in n/d (array of DateTimeOffset).
        /// </summary>
        public DateTimeOffset[] Times { get; init; } = Array.Empty<DateTimeOffset>();

        /// <summary>
        /// Solar zenith angles, in degrees (array of double).
        /// </summary>
        public double[] Zenith { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Refraction-corrected solar zenith angles, in degrees (array of double).
        /// </summary>
        public double[] ApparentZenith { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar elevation angles without refraction correction, in degrees (array of double).
        /// </summary>
        public double[] Elevation { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Refraction-corrected solar elevation angles, in degrees (array of double).
        /// </summary>
        public double[] ApparentElevation { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar azimuth angles, in degrees (array of double).
        /// </summary>
        public double[] Azimuth { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar declination angles, in degrees (array of double).
        /// </summary>
        public double[] Declination { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Solar hour angles, in degrees (array of double).
        /// </summary>
        public double[] HourAngle { get; init; } = Array.Empty<double>();

        /// <summary>
        /// Equation of time, in minutes (array of double); null if not calculated.
        /// </summary>
        public double[] EquationOfTime { get; init; }

        /// <summary>
        /// Local solar time, in hours (array of double); null if not calculated.
        /// </summary>
        public double[] SolarTime { get; init; }
    }
}
