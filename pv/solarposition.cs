using System;

namespace pv
{
    /// <summary>
    /// Provides stateless analytical solar-position calculations.
    /// </summary>
    public static class SolarPosition
    {
        /// <summary>
        /// Port of pvlib.solarposition._calculate_simple_day_angle.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <param name="offset">day-angle offset [days] (int, default: 1)</param>
        /// <returns>Earth orbital day angle [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] CalcSimpleDayAngleArray(double[] dayOfYear, int offset = 1)
        {
            ArgumentNullException.ThrowIfNull(dayOfYear);

            var dayAngle = new double[dayOfYear.Length];
            for (var i = 0; i < dayOfYear.Length; i++)
            {
                dayAngle[i] = (2.0 * Math.PI / 365.0) * (dayOfYear[i] - offset);
            }

            return dayAngle;
        }

        /// <summary>
        /// Port of pvlib.solarposition.equation_of_time_spencer71.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>equation of time [minutes] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] EquationOfTimeSpencer71(double[] dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear);
            var eot = new double[dayAngle.Length];
            for (var i = 0; i < dayAngle.Length; i++)
            {
                eot[i] = (1440.0 / (2.0 * Math.PI)) * (
                    0.0000075
                    + 0.001868 * Math.Cos(dayAngle[i])
                    - 0.032077 * Math.Sin(dayAngle[i])
                    - 0.014615 * Math.Cos(2.0 * dayAngle[i])
                    - 0.040849 * Math.Sin(2.0 * dayAngle[i]));
            }

            return eot;
        }

        /// <summary>
        /// Port of pvlib.solarposition.equation_of_time_pvcdrom.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>equation of time [minutes] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] EquationOfTimePvCdrom(double[] dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear, offset: 81);
            var eot = new double[dayAngle.Length];
            for (var i = 0; i < dayAngle.Length; i++)
            {
                eot[i] = 9.87 * Math.Sin(2.0 * dayAngle[i])
                         - 7.53 * Math.Cos(dayAngle[i])
                         - 1.5 * Math.Sin(dayAngle[i]);
            }

            return eot;
        }

        /// <summary>
        /// Port of pvlib.solarposition.declination_spencer71.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>solar declination [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] DeclinationSpencer71(double[] dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear);
            var declination = new double[dayAngle.Length];
            for (var i = 0; i < dayAngle.Length; i++)
            {
                var angle = dayAngle[i];
                declination[i] = 0.006918
                    - 0.399912 * Math.Cos(angle)
                    + 0.070257 * Math.Sin(angle)
                    - 0.006758 * Math.Cos(2.0 * angle)
                    + 0.000907 * Math.Sin(2.0 * angle)
                    - 0.002697 * Math.Cos(3.0 * angle)
                    + 0.00148 * Math.Sin(3.0 * angle);
            }

            return declination;
        }

        /// <summary>
        /// Port of pvlib.solarposition.declination_cooper69.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>solar declination [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] DeclinationCooper69(double[] dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear, offset: -284);

            var declination = new double[dayAngle.Length];
            for (var i = 0; i < dayAngle.Length; i++)
            {
                declination[i] = Math.PI / 180.0 * 23.45 * Math.Sin(dayAngle[i]);
            }

            return declination;
        }

        /// <summary>
        /// Port of pvlib.solarposition.hour_angle.
        /// </summary>
        /// <param name="location">site coordinates and fixed UTC offset [n/d] (Location)</param>
        /// <param name="times">timezone-aware timestamps [n/d] (array of DateTimeOffset)</param>
        /// <param name="equationOfTime">equation of time [minutes] (array of double)</param>
        /// <returns>solar hour angle [degrees] (array of double)</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="ArgumentException">A timestamp offset differs from the location offset, or array lengths differ.</exception>
        public static double[] HourAngle(
            Location location,
            DateTimeOffset[] times,
            double[] equationOfTime)
        {
            ArgumentNullException.ThrowIfNull(location);
            ArgumentNullException.ThrowIfNull(times);
            ArgumentNullException.ThrowIfNull(equationOfTime);
            if (times.Length != equationOfTime.Length)
            {
                throw new ArgumentException("Times and equationOfTime must have the same length.");
            }

            var hourAngle = new double[times.Length];
            for (var i = 0; i < times.Length; i++)
            {
                if (times[i].Offset != location.UtcOffset)
                {
                    throw new ArgumentException(
                        "Every timestamp must use the UTC offset specified by location.",
                        nameof(times));
                }

                hourAngle[i] = 15.0 * (
                    times[i].TimeOfDay.TotalHours
                    - location.UtcOffset.TotalHours
                    - 12.0)
                    + location.Longitude
                    + equationOfTime[i] / 4.0;
            }

            return hourAngle;
        }

        /// <summary>
        /// Port of pvlib.solarposition.solar_zenith_analytical.
        /// </summary>
        /// <param name="latitude">latitude, positive north of equator [radians] (double)</param>
        /// <param name="hourAngle">solar hour angle [radians] (array of double)</param>
        /// <param name="declination">solar declination [radians] (array of double)</param>
        /// <returns>solar zenith angle [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">An array argument is null.</exception>
        /// <exception cref="ArgumentException">Array lengths differ.</exception>
        public static double[] SolarZenith(
            double latitude,
            double[] hourAngle,
            double[] declination)
        {
            ArgumentNullException.ThrowIfNull(hourAngle);
            ArgumentNullException.ThrowIfNull(declination);
            if (hourAngle.Length != declination.Length)
            {
                throw new ArgumentException("hourAngle and declination must have the same length.");
            }

            var zenith = new double[hourAngle.Length];
            for (var i = 0; i < hourAngle.Length; i++)
            {
                zenith[i] = Math.Acos(
                    Math.Cos(declination[i]) * Math.Cos(latitude) * Math.Cos(hourAngle[i])
                    + Math.Sin(declination[i]) * Math.Sin(latitude));
            }

            return zenith;
        }

        /// <summary>
        /// Calculates analytical solar azimuth from spherical trigonometry (Duffie &amp; Beckman).
        /// Port of pvlib.solarposition.solar_azimuth_analytical.
        /// </summary>
        /// <param name="latitude">latitude of location [radians] (array of double)</param>
        /// <param name="hourAngle">hour angle in local solar time [radians] (array of double)</param>
        /// <param name="declination">solar declination [radians] (array of double)</param>
        /// <param name="zenith">solar zenith angle [radians] (array of double)</param>
        /// <returns>solar azimuth angle, east of north [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">An array argument is null.</exception>
        /// <exception cref="ArgumentException">Array lengths differ.</exception>
        public static double[] SolarAzimuthAnalytical(
            double[] latitude,
            double[] hourAngle,
            double[] declination,
            double[] zenith)
        {
            ArgumentNullException.ThrowIfNull(latitude);
            ArgumentNullException.ThrowIfNull(hourAngle);
            ArgumentNullException.ThrowIfNull(declination);
            ArgumentNullException.ThrowIfNull(zenith);
            if (latitude.Length != hourAngle.Length
                || latitude.Length != declination.Length
                || latitude.Length != zenith.Length)
            {
                throw new ArgumentException("All input arrays must have the same length.");
            }

            var azimuth = new double[latitude.Length];
            for (var i = 0; i < azimuth.Length; i++)
            {
                azimuth[i] = CalculateSolarAzimuthAnalytical(
                    latitude[i], hourAngle[i], declination[i], zenith[i]);
            }

            return azimuth;
        }

        /// <summary>
        /// Calculates analytical solar azimuth from spherical trigonometry (Duffie &amp; Beckman).
        /// Port of pvlib.solarposition.solar_azimuth_analytical.
        /// </summary>
        /// <param name="latitude">latitude of location [radians] (double)</param>
        /// <param name="hourAngle">hour angle in local solar time [radians] (array of double)</param>
        /// <param name="declination">solar declination [radians] (array of double)</param>
        /// <param name="zenith">solar zenith angle [radians] (array of double)</param>
        /// <returns>solar azimuth angle, east of north [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">An array argument is null.</exception>
        /// <exception cref="ArgumentException">Array lengths differ.</exception>
        public static double[] SolarAzimuthAnalytical(
            double latitude,
            double[] hourAngle,
            double[] declination,
            double[] zenith)
        {
            ArgumentNullException.ThrowIfNull(hourAngle);
            ArgumentNullException.ThrowIfNull(declination);
            ArgumentNullException.ThrowIfNull(zenith);
            if (hourAngle.Length != declination.Length
                || hourAngle.Length != zenith.Length)
            {
                throw new ArgumentException("All input arrays must have the same length.");
            }

            var azimuth = new double[hourAngle.Length];
            for (var i = 0; i < azimuth.Length; i++)
            {
                azimuth[i] = CalculateSolarAzimuthAnalytical(
                    latitude, hourAngle[i], declination[i], zenith[i]);
            }

            return azimuth;
        }

        /// <summary>
        /// Calculates analytical solar azimuth for a single set of angles.
        /// </summary>
        /// <param name="latitude">latitude of location [radians] (double)</param>
        /// <param name="hourAngle">hour angle in local solar time [radians] (double)</param>
        /// <param name="declination">solar declination [radians] (double)</param>
        /// <param name="zenith">solar zenith angle [radians] (double)</param>
        /// <returns>solar azimuth angle, east of north [radians] (double)</returns>
        private static double CalculateSolarAzimuthAnalytical(
            double latitude,
            double hourAngle,
            double declination,
            double zenith)
        {
            var numerator = Math.Cos(zenith) * Math.Sin(latitude) - Math.Sin(declination);
            var denominator = Math.Sin(zenith) * Math.Cos(latitude);
            var cosineAzimuth = numerator / denominator;

            if (Math.Abs(denominator) <= 1e-8)
            {
                cosineAzimuth = 1.0;
            }

            if (Math.Abs(cosineAzimuth - 1.0) <= 1e-8)
            {
                cosineAzimuth = 1.0;
            }

            if (Math.Abs(cosineAzimuth + 1.0) <= 1e-8)
            {
                cosineAzimuth = -1.0;
            }

            if (double.IsNaN(hourAngle))
            {
                return double.NaN;
            }

            return Math.Sign(hourAngle) * Math.Acos(cosineAzimuth) + Math.PI;
        }

        /// <summary>
        /// Port of pvlib.solarposition.sun_rise_set_transit_geometric. This method assumes a
        /// circular orbit and a point-source sun, and ignores atmospheric refraction, so errors
        /// are about 10 minutes.
        /// </summary>
        /// <param name="location">site coordinates and fixed UTC offset [n/d] (Location)</param>
        /// <param name="times">timestamps with the local UTC offset of the location [n/d] (array of DateTimeOffset)</param>
        /// <param name="declination">solar declination at times [radians] (array of double)</param>
        /// <param name="equationOfTime">equation of time [minutes] (array of double)</param>
        /// <returns>a tuple of local sunrise, sunset and transit times [n/d] (array of DateTimeOffset each)</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="ArgumentException">Input arrays have different lengths, or a timestamp offset differs from the location offset.</exception>
        public static (DateTimeOffset[] Sunrise, DateTimeOffset[] Sunset, DateTimeOffset[] Transit)
            SunRiseSetTransitGeometric(
                Location location,
                DateTimeOffset[] times,
                double[] declination,
                double[] equationOfTime)
        {
            ArgumentNullException.ThrowIfNull(location);
            ArgumentNullException.ThrowIfNull(times);
            ArgumentNullException.ThrowIfNull(declination);
            ArgumentNullException.ThrowIfNull(equationOfTime);
            if (times.Length != declination.Length || times.Length != equationOfTime.Length)
            {
                throw new ArgumentException(
                    "times, declination, and equationOfTime must have the same length.");
            }

            var latitudeRadians = location.Latitude * Math.PI / 180.0;
            var sunriseAngle = new double[times.Length];
            var sunsetAngle = new double[times.Length];
            var transitAngle = new double[times.Length];
            for (var i = 0; i < times.Length; i++)
            {
                if (times[i].Offset != location.UtcOffset)
                {
                    throw new ArgumentException(
                        "Every timestamp must use the UTC offset specified by location.",
                        nameof(times));
                }

                sunsetAngle[i] = Math.Acos(
                    -Math.Tan(declination[i]) * Math.Tan(latitudeRadians)) * 180.0 / Math.PI;
                sunriseAngle[i] = -sunsetAngle[i];
            }

            return (
                LocalTimesFromHoursSinceMidnight(
                    times, HourAngleToHours(times, sunriseAngle, location.Longitude, equationOfTime)),
                LocalTimesFromHoursSinceMidnight(
                    times, HourAngleToHours(times, sunsetAngle, location.Longitude, equationOfTime)),
                LocalTimesFromHoursSinceMidnight(
                    times, HourAngleToHours(times, transitAngle, location.Longitude, equationOfTime)));
        }

        /// <summary>
        /// Converts solar hour angles to hours after local midnight.
        /// </summary>
        /// <param name="times">timestamps with the local UTC offset of the location [n/d] (array of DateTimeOffset)</param>
        /// <param name="hourAngle">solar hour angle [degrees] (array of double)</param>
        /// <param name="longitude">longitude, positive east [degrees] (double)</param>
        /// <param name="equationOfTime">equation of time [minutes] (array of double)</param>
        /// <returns>hours after local midnight [hours] (array of double)</returns>
        /// <exception cref="ArgumentNullException">An array argument is null.</exception>
        /// <exception cref="ArgumentException">Input arrays have different lengths.</exception>
        internal static double[] HourAngleToHours(
            DateTimeOffset[] times,
            double[] hourAngle,
            double longitude,
            double[] equationOfTime)
        {
            ArgumentNullException.ThrowIfNull(times);
            ArgumentNullException.ThrowIfNull(hourAngle);
            ArgumentNullException.ThrowIfNull(equationOfTime);
            if (times.Length != hourAngle.Length || times.Length != equationOfTime.Length)
            {
                throw new ArgumentException(
                    "times, hourAngle, and equationOfTime must have the same length.");
            }

            var hours = new double[times.Length];
            for (var i = 0; i < times.Length; i++)
            {
                hours[i] = (hourAngle[i] - longitude - equationOfTime[i] / 4.0) / 15.0
                            + 12.0 + times[i].Offset.TotalHours;
            }

            return hours;
        }

        /// <summary>
        /// Converts hours after local midnight to local timestamps.
        /// </summary>
        /// <param name="times">timestamps with the local UTC offset of the location [n/d] (array of DateTimeOffset)</param>
        /// <param name="hours">hours after local midnight [hours] (array of double)</param>
        /// <returns>local timestamps [n/d] (array of DateTimeOffset)</returns>
        /// <exception cref="ArgumentNullException">An array argument is null.</exception>
        /// <exception cref="ArgumentException">Input arrays have different lengths.</exception>
        internal static DateTimeOffset[] LocalTimesFromHoursSinceMidnight(
            DateTimeOffset[] times,
            double[] hours)
        {
            ArgumentNullException.ThrowIfNull(times);
            ArgumentNullException.ThrowIfNull(hours);
            if (times.Length != hours.Length)
            {
                throw new ArgumentException("times and hours must have the same length.");
            }

            var localTimes = new DateTimeOffset[times.Length];
            for (var i = 0; i < times.Length; i++)
            {
                var localMidnight = new DateTimeOffset(
                    times[i].Year, times[i].Month, times[i].Day, 0, 0, 0, times[i].Offset);
                localTimes[i] = localMidnight.AddHours(hours[i]);
            }

            return localTimes;
        }

        /// <summary>
        /// Converts local timestamps to hours after their local calendar midnight.
        /// </summary>
        /// <param name="times">timestamps with the local UTC offset of the location [n/d] (array of DateTimeOffset)</param>
        /// <returns>hours after local midnight [hours] (array of double)</returns>
        /// <exception cref="ArgumentNullException">times is null.</exception>
        internal static double[] TimesToHoursAfterLocalMidnight(DateTimeOffset[] times)
        {
            ArgumentNullException.ThrowIfNull(times);

            var hours = new double[times.Length];
            for (var i = 0; i < times.Length; i++)
            {
                hours[i] = times[i].TimeOfDay.TotalHours;
            }

            return hours;
        }
    }
}
