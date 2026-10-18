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
        public static double[] SolarZenithAnalytical(
            double latitude,
            double[] hourAngle,
            double[] declination)
        {
            ArgumentNullException.ThrowIfNull(hourAngle);

            var latitudes = new double[hourAngle.Length];
            Array.Fill(latitudes, latitude);
            return SolarZenithAnalytical(latitudes, hourAngle, declination);
        }

        /// <summary>
        /// Port of pvlib.solarposition.solar_zenith_analytical.
        /// </summary>
        /// <param name="latitude">latitude, positive north of equator [radians] (array of double)</param>
        /// <param name="hourAngle">solar hour angle [radians] (array of double)</param>
        /// <param name="declination">solar declination [radians] (array of double)</param>
        /// <returns>solar zenith angle [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">An array argument is null.</exception>
        /// <exception cref="ArgumentException">Array lengths differ.</exception>
        public static double[] SolarZenithAnalytical(
            double[] latitude,
            double[] hourAngle,
            double[] declination)
        {
            ArgumentNullException.ThrowIfNull(latitude);
            ArgumentNullException.ThrowIfNull(hourAngle);
            ArgumentNullException.ThrowIfNull(declination);
            if (latitude.Length != hourAngle.Length || hourAngle.Length != declination.Length)
            {
                throw new ArgumentException("latitude, hourAngle, and declination must have the same length.");
            }

            var zenith = new double[hourAngle.Length];
            for (var i = 0; i < hourAngle.Length; i++)
            {
                zenith[i] = Math.Acos(
                    Math.Cos(declination[i]) * Math.Cos(latitude[i]) * Math.Cos(hourAngle[i])
                    + Math.Sin(declination[i]) * Math.Sin(latitude[i]));
            }

            return zenith;
        }
    }
}
