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
        /// <param name="dayOfYear">day of year [days] (double)</param>
        /// <param name="offset">day-angle offset [days] (int, default: 1)</param>
        /// <returns>Earth orbital day angle [radians] (double)</returns>
        public static double CalcSimpleDayAngleArray(double dayOfYear, int offset = 1)
        {
            return (2.0 * Math.PI / 365.0) * (dayOfYear - offset);
        }

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
                dayAngle[i] = CalcSimpleDayAngleArray(dayOfYear[i], offset);
            }

            return dayAngle;
        }

        /// <summary>
        /// Port of pvlib.solarposition.equation_of_time_spencer71.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (double)</param>
        /// <returns>equation of time [minutes] (double)</returns>
        public static double EquationOfTimeSpencer71(double dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear);
            return (1440.0 / (2.0 * Math.PI)) * (
                0.0000075
                + 0.001868 * Math.Cos(dayAngle)
                - 0.032077 * Math.Sin(dayAngle)
                - 0.014615 * Math.Cos(2.0 * dayAngle)
                - 0.040849 * Math.Sin(2.0 * dayAngle));
        }

        /// <summary>
        /// Port of pvlib.solarposition.equation_of_time_spencer71.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>equation of time [minutes] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] EquationOfTimeSpencer71(double[] dayOfYear)
        {
            ArgumentNullException.ThrowIfNull(dayOfYear);
            var eot = new double[dayOfYear.Length];
            for (var i = 0; i < dayOfYear.Length; i++)
            {
                eot[i] = EquationOfTimeSpencer71(dayOfYear[i]);
            }

            return eot;
        }

        /// <summary>
        /// Port of pvlib.solarposition.equation_of_time_pvcdrom.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (double)</param>
        /// <returns>equation of time [minutes] (double)</returns>
        public static double EquationOfTimePvCdrom(double dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear, offset: 81);
            return 9.87 * Math.Sin(2.0 * dayAngle)
                   - 7.53 * Math.Cos(dayAngle)
                   - 1.5 * Math.Sin(dayAngle);
        }

        /// <summary>
        /// Port of pvlib.solarposition.equation_of_time_pvcdrom.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>equation of time [minutes] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] EquationOfTimePvCdrom(double[] dayOfYear)
        {
            ArgumentNullException.ThrowIfNull(dayOfYear);
            var eot = new double[dayOfYear.Length];
            for (var i = 0; i < dayOfYear.Length; i++)
            {
                eot[i] = EquationOfTimePvCdrom(dayOfYear[i]);
            }

            return eot;
        }

        /// <summary>
        /// Port of pvlib.solarposition.declination_spencer71.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (double)</param>
        /// <returns>solar declination [radians] (double)</returns>
        public static double DeclinationSpencer71(double dayOfYear)
        {
            var angle = CalcSimpleDayAngleArray(dayOfYear);
            return 0.006918
                - 0.399912 * Math.Cos(angle)
                + 0.070257 * Math.Sin(angle)
                - 0.006758 * Math.Cos(2.0 * angle)
                + 0.000907 * Math.Sin(2.0 * angle)
                - 0.002697 * Math.Cos(3.0 * angle)
                + 0.00148 * Math.Sin(3.0 * angle);
        }

        /// <summary>
        /// Port of pvlib.solarposition.declination_spencer71.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>solar declination [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] DeclinationSpencer71(double[] dayOfYear)
        {
            ArgumentNullException.ThrowIfNull(dayOfYear);
            var declination = new double[dayOfYear.Length];
            for (var i = 0; i < dayOfYear.Length; i++)
            {
                declination[i] = DeclinationSpencer71(dayOfYear[i]);
            }

            return declination;
        }

        /// <summary>
        /// Port of pvlib.solarposition.declination_cooper69.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (double)</param>
        /// <returns>solar declination [radians] (double)</returns>
        public static double DeclinationCooper69(double dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear, offset: -284);
            return Math.PI / 180.0 * 23.45 * Math.Sin(dayAngle);
        }

        /// <summary>
        /// Port of pvlib.solarposition.declination_cooper69.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>solar declination [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] DeclinationCooper69(double[] dayOfYear)
        {
            ArgumentNullException.ThrowIfNull(dayOfYear);
            var declination = new double[dayOfYear.Length];
            for (var i = 0; i < dayOfYear.Length; i++)
            {
                declination[i] = DeclinationCooper69(dayOfYear[i]);
            }

            return declination;
        }

        /// <summary>
        /// Port of pvlib.solarposition.hour_angle.
        /// </summary>
        /// <param name="location">site coordinates and fixed UTC offset [n/d] (Location)</param>
        /// <param name="time">timezone-aware timestamp [n/d] (DateTimeOffset)</param>
        /// <param name="equationOfTime">equation of time [minutes] (double)</param>
        /// <returns>solar hour angle [degrees] (double)</returns>
        /// <exception cref="ArgumentNullException">location is null.</exception>
        /// <exception cref="ArgumentException">The timestamp offset differs from the location offset.</exception>
        public static double HourAngle(Location location, DateTimeOffset time, double equationOfTime)
        {
            ArgumentNullException.ThrowIfNull(location);
            if (time.Offset != location.UtcOffset)
            {
                throw new ArgumentException(
                    "The timestamp must use the UTC offset specified by location.",
                    nameof(time));
            }

            return 15.0 * (time.TimeOfDay.TotalHours - location.UtcOffset.TotalHours - 12.0)
                + location.Longitude
                + equationOfTime / 4.0;
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

                hourAngle[i] = HourAngle(location, times[i], equationOfTime[i]);
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
    }
}
