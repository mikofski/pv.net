using System;

namespace pv
{
    /// <summary>
    /// Provides numerical helpers for the NREL Solar Position Algorithm, ported from pvlib.spa.
    /// </summary>
    public static class Spa
    {
        private static readonly double[,] L0 = {
            { 175347046, 0, 0 }, { 3341656, 4.6692568, 6283.07585 }, { 34894, 4.6261, 12566.1517 }, { 3497, 2.7441, 5753.3849 }, { 3418, 2.8289, 3.5231 }, { 3136, 3.6277, 77713.7715 }, { 2676, 4.4181, 7860.4194 }, { 2343, 6.1352, 3930.2097 }, { 1324, 0.7425, 11506.7698 }, { 1273, 2.0371, 529.691 }, { 1199, 1.1096, 1577.3435 }, { 990, 5.233, 5884.927 }, { 902, 2.045, 26.298 }, { 857, 3.508, 398.149 }, { 780, 1.179, 5223.694 }, { 753, 2.533, 5507.553 }, { 505, 4.583, 18849.228 }, { 492, 4.205, 775.523 }, { 357, 2.92, 0.067 }, { 317, 5.849, 11790.629 }, { 284, 1.899, 796.298 }, { 271, 0.315, 10977.079 }, { 243, 0.345, 5486.778 }, { 206, 4.806, 2544.314 }, { 205, 1.869, 5573.143 }, { 202, 2.458, 6069.777 }, { 156, 0.833, 213.299 }, { 132, 3.411, 2942.463 }, { 126, 1.083, 20.775 }, { 115, 0.645, 0.98 }, { 103, 0.636, 4694.003 }, { 102, 0.976, 15720.839 }, { 102, 4.267, 7.114 }, { 99, 6.21, 2146.17 }, { 98, 0.68, 155.42 }, { 86, 5.98, 161000.69 }, { 85, 1.3, 6275.96 }, { 85, 3.67, 71430.7 }, { 80, 1.81, 17260.15 }, { 79, 3.04, 12036.46 }, { 75, 1.76, 5088.63 }, { 74, 3.5, 3154.69 }, { 74, 4.68, 801.82 }, { 70, 0.83, 9437.76 }, { 62, 3.98, 8827.39 }, { 61, 1.82, 7084.9 }, { 57, 2.78, 6286.6 }, { 56, 4.39, 14143.5 }, { 56, 3.47, 6279.55 }, { 52, 0.19, 12139.55 }, { 52, 1.33, 1748.02 }, { 51, 0.28, 5856.48 }, { 49, 0.49, 1194.45 }, { 41, 5.37, 8429.24 }, { 41, 2.4, 19651.05 }, { 39, 6.17, 10447.39 }, { 37, 6.04, 10213.29 }, { 37, 2.57, 1059.38 }, { 36, 1.71, 2352.87 }, { 36, 1.78, 6812.77 }, { 33, 0.59, 17789.85 }, { 30, 0.44, 83996.85 }, { 30, 2.74, 1349.87 }, { 25, 3.16, 4690.48 }
        };
        private static readonly double[,] L1 = {
            { 628331966747, 0, 0 }, { 206059, 2.678235, 6283.07585 }, { 4303, 2.6351, 12566.1517 }, { 425, 1.59, 3.523 }, { 119, 5.796, 26.298 }, { 109, 2.966, 1577.344 }, { 93, 2.59, 18849.23 }, { 72, 1.14, 529.69 }, { 68, 1.87, 398.15 }, { 67, 4.41, 5507.55 }, { 59, 2.89, 5223.69 }, { 56, 2.17, 155.42 }, { 45, 0.4, 796.3 }, { 36, 0.47, 775.52 }, { 29, 2.65, 7.11 }, { 21, 5.34, 0.98 }, { 19, 1.85, 5486.78 }, { 19, 4.97, 213.3 }, { 17, 2.99, 6275.96 }, { 16, 0.03, 2544.31 }, { 16, 1.43, 2146.17 }, { 15, 1.21, 10977.08 }, { 12, 2.83, 1748.02 }, { 12, 3.26, 5088.63 }, { 12, 5.27, 1194.45 }, { 12, 2.08, 4694 }, { 11, 0.77, 553.57 }, { 10, 1.3, 6286.6 }, { 10, 4.24, 1349.87 }, { 9, 2.7, 242.73 }, { 9, 5.64, 951.72 }, { 8, 5.3, 2352.87 }, { 6, 2.65, 9437.76 }, { 6, 4.67, 4690.48 }
        };
        private static readonly double[,] L2 = {
            { 52919, 0, 0 }, { 8720, 1.0721, 6283.0758 }, { 309, 0.867, 12566.152 }, { 27, 0.05, 3.52 }, { 16, 5.19, 26.3 }, { 16, 3.68, 155.42 }, { 10, 0.76, 18849.23 }, { 9, 2.06, 77713.77 }, { 7, 0.83, 775.52 }, { 5, 4.66, 1577.34 }, { 4, 1.03, 7.11 }, { 4, 3.44, 5573.14 }, { 3, 5.14, 796.3 }, { 3, 6.05, 5507.55 }, { 3, 1.19, 242.73 }, { 3, 6.12, 529.69 }, { 3, 0.31, 398.15 }, { 3, 2.28, 553.57 }, { 2, 4.38, 5223.69 }, { 2, 3.75, 0.98 }
        };
        private static readonly double[,] L3 = {
            { 289, 5.844, 6283.076 }, { 35, 0, 0 }, { 17, 5.49, 12566.15 }, { 3, 5.2, 155.42 }, { 1, 4.72, 3.52 }, { 1, 5.3, 18849.23 }, { 1, 5.97, 242.73 }
        };
        private static readonly double[,] L4 = {
            { 114, 3.142, 0 }, { 8, 4.13, 6283.08 }, { 1, 3.84, 12566.15 }
        };
        private static readonly double[,] L5 = {
            { 1, 3.14, 0 }
        };
        private static readonly double[,] B0 = {
            { 280, 3.199, 84334.662 }, { 102, 5.422, 5507.553 }, { 80, 3.88, 5223.69 }, { 44, 3.7, 2352.87 }, { 32, 4, 1577.34 }
        };
        private static readonly double[,] B1 = {
            { 9, 3.9, 5507.55 }, { 6, 1.73, 5223.69 }
        };
        private static readonly double[,] R0 = {
            { 100013989, 0, 0 }, { 1670700, 3.0984635, 6283.07585 }, { 13956, 3.05525, 12566.1517 }, { 3084, 5.1985, 77713.7715 }, { 1628, 1.1739, 5753.3849 }, { 1576, 2.8469, 7860.4194 }, { 925, 5.453, 11506.77 }, { 542, 4.564, 3930.21 }, { 472, 3.661, 5884.927 }, { 346, 0.964, 5507.553 }, { 329, 5.9, 5223.694 }, { 307, 0.299, 5573.143 }, { 243, 4.273, 11790.629 }, { 212, 5.847, 1577.344 }, { 186, 5.022, 10977.079 }, { 175, 3.012, 18849.228 }, { 110, 5.055, 5486.778 }, { 98, 0.89, 6069.78 }, { 86, 5.69, 15720.84 }, { 86, 1.27, 161000.69 }, { 65, 0.27, 17260.15 }, { 63, 0.92, 529.69 }, { 57, 2.01, 83996.85 }, { 56, 5.24, 71430.7 }, { 49, 3.25, 2544.31 }, { 47, 2.58, 775.52 }, { 45, 5.54, 9437.76 }, { 43, 6.01, 6275.96 }, { 39, 5.36, 4694 }, { 38, 2.39, 8827.39 }, { 37, 0.83, 19651.05 }, { 37, 4.9, 12139.55 }, { 36, 1.67, 12036.46 }, { 35, 1.84, 2942.46 }, { 33, 0.24, 7084.9 }, { 32, 0.18, 5088.63 }, { 32, 1.78, 398.15 }, { 28, 1.21, 6286.6 }, { 28, 1.9, 6279.55 }, { 26, 4.59, 10447.39 }
        };
        private static readonly double[,] R1 = {
            { 103019, 1.10749, 6283.07585 }, { 1721, 1.0644, 12566.1517 }, { 702, 3.142, 0 }, { 32, 1.02, 18849.23 }, { 31, 2.84, 5507.55 }, { 25, 1.32, 5223.69 }, { 18, 1.42, 1577.34 }, { 10, 5.91, 10977.08 }, { 9, 1.42, 6275.96 }, { 9, 0.27, 5486.78 }
        };
        private static readonly double[,] R2 = {
            { 4359, 5.7846, 6283.0758 }, { 124, 5.579, 12566.152 }, { 12, 3.14, 0 }, { 9, 3.63, 77713.77 }, { 6, 1.87, 5573.14 }, { 3, 5.47, 18849.23 }
        };
        private static readonly double[,] R3 = {
            { 145, 4.273, 6283.076 }, { 7, 3.92, 12566.15 }
        };
        private static readonly double[,] R4 = {
            { 4, 2.56, 6283.08 }
        };
        private static readonly double[,] NUTATION_ABCD_ARRAY = {
            { -171996, -174.2, 92025, 8.9 }, { -13187, -1.6, 5736, -3.1 }, { -2274, -0.2, 977, -0.5 }, { 2062, 0.2, -895, 0.5 }, { 1426, -3.4, 54, -0.1 }, { 712, 0.1, -7, 0 }, { -517, 1.2, 224, -0.6 }, { -386, -0.4, 200, 0 }, { -301, 0, 129, -0.1 }, { 217, -0.5, -95, 0.3 }, { -158, 0, 0, 0 }, { 129, 0.1, -70, 0 }, { 123, 0, -53, 0 }, { 63, 0, 0, 0 }, { 63, 0.1, -33, 0 }, { -59, 0, 26, 0 }, { -58, -0.1, 32, 0 }, { -51, 0, 27, 0 }, { 48, 0, 0, 0 }, { 46, 0, -24, 0 }, { -38, 0, 16, 0 }, { -31, 0, 13, 0 }, { 29, 0, 0, 0 }, { 29, 0, -12, 0 }, { 26, 0, 0, 0 }, { -22, 0, 0, 0 }, { 21, 0, -10, 0 }, { 17, -0.1, 0, 0 }, { 16, 0, -8, 0 }, { -16, 0.1, 7, 0 }, { -15, 0, 9, 0 }, { -13, 0, 7, 0 }, { -12, 0, 6, 0 }, { 11, 0, 0, 0 }, { -10, 0, 5, 0 }, { -8, 0, 3, 0 }, { 7, 0, -3, 0 }, { -7, 0, 0, 0 }, { -7, 0, 3, 0 }, { -7, 0, 3, 0 }, { 6, 0, 0, 0 }, { 6, 0, -3, 0 }, { 6, 0, -3, 0 }, { -6, 0, 3, 0 }, { -6, 0, 3, 0 }, { 5, 0, 0, 0 }, { -5, 0, 3, 0 }, { -5, 0, 3, 0 }, { -5, 0, 3, 0 }, { 4, 0, 0, 0 }, { 4, 0, 0, 0 }, { 4, 0, 0, 0 }, { -4, 0, 0, 0 }, { -4, 0, 0, 0 }, { -4, 0, 0, 0 }, { 3, 0, 0, 0 }, { -3, 0, 0, 0 }, { -3, 0, 0, 0 }, { -3, 0, 0, 0 }, { -3, 0, 0, 0 }, { -3, 0, 0, 0 }, { -3, 0, 0, 0 }, { -3, 0, 0, 0 }
        };
        private static readonly double[,] NUTATION_YTERM_ARRAY = {
            { 0, 0, 0, 0, 1 }, { -2, 0, 0, 2, 2 }, { 0, 0, 0, 2, 2 }, { 0, 0, 0, 0, 2 }, { 0, 1, 0, 0, 0 }, { 0, 0, 1, 0, 0 }, { -2, 1, 0, 2, 2 }, { 0, 0, 0, 2, 1 }, { 0, 0, 1, 2, 2 }, { -2, -1, 0, 2, 2 }, { -2, 0, 1, 0, 0 }, { -2, 0, 0, 2, 1 }, { 0, 0, -1, 2, 2 }, { 2, 0, 0, 0, 0 }, { 0, 0, 1, 0, 1 }, { 2, 0, -1, 2, 2 }, { 0, 0, -1, 0, 1 }, { 0, 0, 1, 2, 1 }, { -2, 0, 2, 0, 0 }, { 0, 0, -2, 2, 1 }, { 2, 0, 0, 2, 2 }, { 0, 0, 2, 2, 2 }, { 0, 0, 2, 0, 0 }, { -2, 0, 1, 2, 2 }, { 0, 0, 0, 2, 0 }, { -2, 0, 0, 2, 0 }, { 0, 0, -1, 2, 1 }, { 0, 2, 0, 0, 0 }, { 2, 0, -1, 0, 1 }, { -2, 2, 0, 2, 2 }, { 0, 1, 0, 0, 1 }, { -2, 0, 1, 0, 1 }, { 0, -1, 0, 0, 1 }, { 0, 0, 2, -2, 0 }, { 2, 0, -1, 2, 1 }, { 2, 0, 1, 2, 2 }, { 0, 1, 0, 2, 2 }, { -2, 1, 1, 0, 0 }, { 0, -1, 0, 2, 2 }, { 2, 0, 0, 2, 1 }, { 2, 0, 1, 0, 0 }, { -2, 0, 2, 2, 2 }, { -2, 0, 1, 2, 1 }, { 2, 0, -2, 0, 1 }, { 2, 0, 0, 0, 1 }, { 0, -1, 1, 0, 0 }, { -2, -1, 0, 2, 1 }, { -2, 0, 0, 0, 1 }, { 0, 0, 2, 2, 1 }, { -2, 0, 2, 0, 1 }, { -2, 1, 0, 2, 1 }, { 0, 0, 1, -2, 0 }, { -1, 0, 1, 0, 0 }, { -2, 1, 0, 0, 0 }, { 1, 0, 0, 0, 0 }, { 0, 0, 1, 2, 0 }, { 0, 0, -2, 2, 2 }, { -1, -1, 1, 0, 0 }, { 0, 1, 1, 0, 0 }, { 0, -1, 1, 2, 2 }, { 2, -1, -1, 2, 2 }, { 0, 0, 3, 2, 2 }, { 2, -1, 0, 2, 2 }
        };

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
