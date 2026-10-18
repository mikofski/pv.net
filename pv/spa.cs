using System;

namespace pv
{
    /// <summary>
    /// Provides numerical helpers for the NREL Solar Position Algorithm, ported from pvlib.spa.
    /// </summary>
    public static class Spa
    {
        /// <summary>
        /// Calculates the Julian day from UTC calendar components. Port of pvlib.spa.julian_day_dt,
        /// preserving its Julian/Gregorian calendar transition and astronomical year numbering.
        /// </summary>
        /// <param name="year">calendar year [n/d] (int)</param>
        /// <param name="month">calendar month [n/d] (int)</param>
        /// <param name="day">day of month [days] (int)</param>
        /// <param name="hour">UTC hour [hours] (int)</param>
        /// <param name="minute">UTC minute [minutes] (int)</param>
        /// <param name="second">UTC second [seconds] (int)</param>
        /// <param name="microsecond">fractional second [microseconds] (int)</param>
        /// <returns>Julian day [days] (double)</returns>
        public static double JulianDayDt(
            int year, int month, int day, int hour, int minute, int second, int microsecond)
        {
            double y = year;
            if (month <= 2)
            {
                y -= 1;
                month += 12;
            }

            var a = Math.Truncate(y / 100);
            var b = 2 - a + Math.Truncate(a * 0.25);
            var fractionOfDay = (microsecond / 1e6 + (second + minute * 60.0 + hour * 3600.0)) / 86400;
            var d = day + fractionOfDay;
            var jd = Math.Truncate(365.25 * (y + 4716))
                     + Math.Truncate(30.6001 * (month + 1.0)) + d - 1524.5;
            if (jd > 2299160.0)
            {
                jd += b;
            }

            return jd;
        }

        /// <summary>
        /// Calculates the Julian day from Unix time. Port of pvlib.spa.julian_day.
        /// </summary>
        /// <param name="unixtime">seconds since 1970-01-01T00:00:00Z [seconds] (double)</param>
        /// <returns>Julian day [days] (double)</returns>
        public static double JulianDay(double unixtime) => unixtime / 86400 + 2440587.5;

        /// <summary>
        /// Calculates the Julian ephemeris day. Port of pvlib.spa.julian_ephemeris_day.
        /// </summary>
        /// <param name="julianDay">Julian day [days] (double)</param>
        /// <param name="deltaT">difference between terrestrial time and UT1 [seconds] (double)</param>
        /// <returns>Julian ephemeris day [days] (double)</returns>
        public static double JulianEphemerisDay(double julianDay, double deltaT) =>
            julianDay + deltaT / 86400;

        /// <summary>
        /// Calculates Julian centuries since J2000. Port of pvlib.spa.julian_century.
        /// </summary>
        /// <param name="julianDay">Julian day [days] (double)</param>
        /// <returns>Julian century [centuries] (double)</returns>
        public static double JulianCentury(double julianDay) => (julianDay - 2451545) / 36525;

        /// <summary>
        /// Calculates Julian ephemeris centuries since J2000. Port of pvlib.spa.julian_ephemeris_century.
        /// </summary>
        /// <param name="julianEphemerisDay">Julian ephemeris day [days] (double)</param>
        /// <returns>Julian ephemeris century [centuries] (double)</returns>
        public static double JulianEphemerisCentury(double julianEphemerisDay) =>
            (julianEphemerisDay - 2451545) / 36525;

        /// <summary>
        /// Calculates Julian ephemeris millennia since J2000. Port of pvlib.spa.julian_ephemeris_millennium.
        /// </summary>
        /// <param name="julianEphemerisCentury">Julian ephemeris century [centuries] (double)</param>
        /// <returns>Julian ephemeris millennium [millennia] (double)</returns>
        public static double JulianEphemerisMillennium(double julianEphemerisCentury) =>
            julianEphemerisCentury / 10;

        /// <summary>
        /// Estimates the difference between terrestrial time and UT1 using NASA polynomials.
        /// Port of pvlib.spa.calculate_deltat. pvlib warns for years before -1999 or after 3000;
        /// this method calculates silently without throwing, but estimates are not intended for those years.
        /// </summary>
        /// <param name="year">calendar year [n/d] (int)</param>
        /// <param name="month">calendar month [n/d] (int)</param>
        /// <returns>difference between terrestrial time and UT1 [seconds] (double)</returns>
        public static double CalculateDeltaT(int year, int month)
        {
            var y = year + (month - 0.5) / 12;
            double t;
            if (year < -500 || year >= 2150)
            {
                return -20 + 32 * Math.Pow((y - 1820) / 100, 2);
            }
            if (year < 500)
            {
                t = y / 100;
                return 10583.6 - 1014.41 * t + 33.78311 * Math.Pow(t, 2)
                    - 5.952053 * Math.Pow(t, 3) - 0.1798452 * Math.Pow(t, 4)
                    + 0.022174192 * Math.Pow(t, 5) + 0.0090316521 * Math.Pow(t, 6);
            }
            if (year < 1600)
            {
                t = (y - 1000) / 100;
                return 1574.2 - 556.01 * t + 71.23472 * Math.Pow(t, 2)
                    + 0.319781 * Math.Pow(t, 3) - 0.8503463 * Math.Pow(t, 4)
                    - 0.005050998 * Math.Pow(t, 5) + 0.0083572073 * Math.Pow(t, 6);
            }
            if (year < 1700)
            {
                t = y - 1600;
                return 120 - 0.9808 * t - 0.01532 * Math.Pow(t, 2) + Math.Pow(t, 3) / 7129;
            }
            if (year < 1800)
            {
                t = y - 1700;
                return 8.83 + 0.1603 * t - 0.0059285 * Math.Pow(t, 2)
                    + 0.00013336 * Math.Pow(t, 3) - Math.Pow(t, 4) / 1174000;
            }
            if (year < 1860)
            {
                t = y - 1800;
                return 13.72 - 0.332447 * t + 0.0068612 * Math.Pow(t, 2)
                    + 0.0041116 * Math.Pow(t, 3) - 0.00037436 * Math.Pow(t, 4)
                    + 0.0000121272 * Math.Pow(t, 5) - 0.0000001699 * Math.Pow(t, 6)
                    + 0.000000000875 * Math.Pow(t, 7);
            }
            if (year < 1900)
            {
                t = y - 1860;
                return 7.62 + 0.5737 * t - 0.251754 * Math.Pow(t, 2)
                    + 0.01680668 * Math.Pow(t, 3) - 0.0004473624 * Math.Pow(t, 4)
                    + Math.Pow(t, 5) / 233174;
            }
            if (year < 1920)
            {
                t = y - 1900;
                return -2.79 + 1.494119 * t - 0.0598939 * Math.Pow(t, 2)
                    + 0.0061966 * Math.Pow(t, 3) - 0.000197 * Math.Pow(t, 4);
            }
            if (year < 1941)
            {
                t = y - 1920;
                return 21.20 + 0.84493 * t - 0.076100 * Math.Pow(t, 2) + 0.0020936 * Math.Pow(t, 3);
            }
            if (year < 1961)
            {
                t = y - 1950;
                return 29.07 + 0.407 * t - Math.Pow(t, 2) / 233 + Math.Pow(t, 3) / 2547;
            }
            if (year < 1986)
            {
                t = y - 1975;
                return 45.45 + 1.067 * t - Math.Pow(t, 2) / 260 - Math.Pow(t, 3) / 718;
            }
            if (year < 2005)
            {
                t = y - 2000;
                return 63.86 + 0.3345 * t - 0.060374 * Math.Pow(t, 2)
                    + 0.0017275 * Math.Pow(t, 3) + 0.000651814 * Math.Pow(t, 4)
                    + 0.00002373599 * Math.Pow(t, 5);
            }
            if (year < 2050)
            {
                t = y - 2000;
                return 62.92 + 0.32217 * t + 0.005589 * Math.Pow(t, 2);
            }

            return -20 + 32 * Math.Pow((y - 1820) / 100, 2) - 0.5628 * (2150 - y);
        }

        /// <summary>
        /// Estimates delta-T for aligned year and month arrays. Port of pvlib.spa.calculate_deltat,
        /// without NumPy broadcasting. pvlib warns for years before -1999 or after 3000; this method
        /// calculates silently without throwing for those years, although estimates are not intended for them.
        /// </summary>
        /// <param name="year">calendar years [n/d] (array of int)</param>
        /// <param name="month">calendar months aligned with year [n/d] (array of int)</param>
        /// <returns>differences between terrestrial time and UT1 [seconds] (array of double)</returns>
        /// <exception cref="ArgumentNullException">year or month is null.</exception>
        /// <exception cref="ArgumentException">year and month have different lengths.</exception>
        public static double[] CalculateDeltaT(int[] year, int[] month)
        {
            ArgumentNullException.ThrowIfNull(year);
            ArgumentNullException.ThrowIfNull(month);
            if (year.Length != month.Length)
            {
                throw new ArgumentException("Year and month arrays must have the same length.", nameof(month));
            }

            var deltaT = new double[year.Length];
            for (var i = 0; i < year.Length; i++)
            {
                deltaT[i] = CalculateDeltaT(year[i], month[i]);
            }

            return deltaT;
        }
    }
}
