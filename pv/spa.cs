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

        /// <summary>Sum coefficient amplitudes multiplied by cosines. Port of pvlib.spa.sum_mult_cos_add_mult.</summary>
        /// <param name="arr">SPA coefficient rows [n/d] (two-dimensional array of double)</param>
        /// <param name="x">Julian ephemeris millennium [millennia] (double)</param>
        /// <returns>Coefficient sum [n/d] (double)</returns>
        public static double SumMultCosAddMult(double[,] arr, double x)
        {
            double sum = 0;
            for (var row = 0; row < arr.GetLength(0); row++)
            {
                sum += arr[row, 0] * Math.Cos(arr[row, 1] + arr[row, 2] * x);
            }
            return sum;
        }

        /// <summary>Calculates heliocentric longitude. Port of pvlib.spa.heliocentric_longitude.</summary>
        /// <param name="julianEphemerisMillennium">Julian ephemeris millennium [millennia] (double)</param>
        /// <returns>Heliocentric longitude [degrees] (double)</returns>
        public static double HeliocentricLongitude(double julianEphemerisMillennium)
        {
            var x = julianEphemerisMillennium;
            var radians = (SumMultCosAddMult(L0, x) + SumMultCosAddMult(L1, x) * x
                + SumMultCosAddMult(L2, x) * Math.Pow(x, 2) + SumMultCosAddMult(L3, x) * Math.Pow(x, 3)
                + SumMultCosAddMult(L4, x) * Math.Pow(x, 4) + SumMultCosAddMult(L5, x) * Math.Pow(x, 5)) / 1e8;
            return Mod(radians * 180 / Math.PI, 360);
        }

        /// <summary>Calculates heliocentric latitude. Port of pvlib.spa.heliocentric_latitude.</summary>
        /// <param name="julianEphemerisMillennium">Julian ephemeris millennium [millennia] (double)</param>
        /// <returns>Heliocentric latitude [degrees] (double)</returns>
        public static double HeliocentricLatitude(double julianEphemerisMillennium)
        {
            var x = julianEphemerisMillennium;
            return (SumMultCosAddMult(B0, x) + SumMultCosAddMult(B1, x) * x) / 1e8 * 180 / Math.PI;
        }

        /// <summary>Calculates the heliocentric radius vector. Port of pvlib.spa.heliocentric_radius_vector.</summary>
        /// <param name="julianEphemerisMillennium">Julian ephemeris millennium [millennia] (double)</param>
        /// <returns>Heliocentric radius vector [AU] (double)</returns>
        public static double HeliocentricRadiusVector(double julianEphemerisMillennium)
        {
            var x = julianEphemerisMillennium;
            return (SumMultCosAddMult(R0, x) + SumMultCosAddMult(R1, x) * x
                + SumMultCosAddMult(R2, x) * Math.Pow(x, 2) + SumMultCosAddMult(R3, x) * Math.Pow(x, 3)
                + SumMultCosAddMult(R4, x) * Math.Pow(x, 4)) / 1e8;
        }

        /// <summary>Calculates geocentric longitude. Port of pvlib.spa.geocentric_longitude.</summary>
        /// <param name="heliocentricLongitude">Heliocentric longitude [degrees] (double)</param>
        /// <returns>Geocentric longitude [degrees] (double)</returns>
        public static double GeocentricLongitude(double heliocentricLongitude) => Mod(heliocentricLongitude + 180, 360);

        /// <summary>Calculates geocentric latitude. Port of pvlib.spa.geocentric_latitude.</summary>
        /// <param name="heliocentricLatitude">Heliocentric latitude [degrees] (double)</param>
        /// <returns>Geocentric latitude [degrees] (double)</returns>
        public static double GeocentricLatitude(double heliocentricLatitude) => -heliocentricLatitude;

        /// <summary>Calculates the mean elongation of the Moon from the Sun. Port of pvlib.spa.mean_elongation.</summary>
        /// <param name="julianEphemerisCentury">Julian ephemeris century [centuries] (double)</param>
        /// <returns>Mean elongation [degrees] (double)</returns>
        public static double MeanElongation(double julianEphemerisCentury)
        {
            var x = julianEphemerisCentury;
            return 297.85036 + 445267.111480 * x - 0.0019142 * Math.Pow(x, 2) + Math.Pow(x, 3) / 189474;
        }

        /// <summary>Calculates the mean anomaly of the Sun. Port of pvlib.spa.mean_anomaly_sun.</summary>
        /// <param name="julianEphemerisCentury">Julian ephemeris century [centuries] (double)</param>
        /// <returns>Mean anomaly of the Sun [degrees] (double)</returns>
        public static double MeanAnomalySun(double julianEphemerisCentury)
        {
            var x = julianEphemerisCentury;
            return 357.52772 + 35999.050340 * x - 0.0001603 * Math.Pow(x, 2) - Math.Pow(x, 3) / 300000;
        }

        /// <summary>Calculates the mean anomaly of the Moon. Port of pvlib.spa.mean_anomaly_moon.</summary>
        /// <param name="julianEphemerisCentury">Julian ephemeris century [centuries] (double)</param>
        /// <returns>Mean anomaly of the Moon [degrees] (double)</returns>
        public static double MeanAnomalyMoon(double julianEphemerisCentury)
        {
            var x = julianEphemerisCentury;
            return 134.96298 + 477198.867398 * x + 0.0086972 * Math.Pow(x, 2) + Math.Pow(x, 3) / 56250;
        }

        /// <summary>Calculates the Moon's argument of latitude. Port of pvlib.spa.moon_argument_latitude.</summary>
        /// <param name="julianEphemerisCentury">Julian ephemeris century [centuries] (double)</param>
        /// <returns>Moon's argument of latitude [degrees] (double)</returns>
        public static double MoonArgumentLatitude(double julianEphemerisCentury)
        {
            var x = julianEphemerisCentury;
            return 93.27191 + 483202.017538 * x - 0.0036825 * Math.Pow(x, 2) + Math.Pow(x, 3) / 327270;
        }

        /// <summary>Calculates the Moon's ascending longitude. Port of pvlib.spa.moon_ascending_longitude.</summary>
        /// <param name="julianEphemerisCentury">Julian ephemeris century [centuries] (double)</param>
        /// <returns>Moon's ascending longitude [degrees] (double)</returns>
        public static double MoonAscendingLongitude(double julianEphemerisCentury)
        {
            var x = julianEphemerisCentury;
            return 125.04452 - 1934.136261 * x + 0.0020708 * Math.Pow(x, 2) + Math.Pow(x, 3) / 450000;
        }

        /// <summary>Calculates longitude and obliquity nutation. Port of pvlib.spa.longitude_obliquity_nutation.</summary>
        /// <param name="julianEphemerisCentury">Julian ephemeris century [centuries] (double)</param>
        /// <param name="meanElongation">Mean elongation [degrees] (double)</param>
        /// <param name="meanAnomalySun">Mean anomaly of the Sun [degrees] (double)</param>
        /// <param name="meanAnomalyMoon">Mean anomaly of the Moon [degrees] (double)</param>
        /// <param name="moonArgumentLatitude">Moon's argument of latitude [degrees] (double)</param>
        /// <param name="moonAscendingLongitude">Moon's ascending longitude [degrees] (double)</param>
        /// <returns>LongitudeNutation [degrees] and ObliquityNutation [degrees] (named tuple of double)</returns>
        public static (double LongitudeNutation, double ObliquityNutation) LongitudeObliquityNutation(
            double julianEphemerisCentury, double meanElongation, double meanAnomalySun,
            double meanAnomalyMoon, double moonArgumentLatitude, double moonAscendingLongitude)
        {
            double longitude = 0;
            double obliquity = 0;
            for (var row = 0; row < NUTATION_YTERM_ARRAY.GetLength(0); row++)
            {
                var args = NUTATION_YTERM_ARRAY[row, 0] * meanElongation
                    + NUTATION_YTERM_ARRAY[row, 1] * meanAnomalySun
                    + NUTATION_YTERM_ARRAY[row, 2] * meanAnomalyMoon
                    + NUTATION_YTERM_ARRAY[row, 3] * moonArgumentLatitude
                    + NUTATION_YTERM_ARRAY[row, 4] * moonAscendingLongitude;
                var radians = args * Math.PI / 180;
                longitude += (NUTATION_ABCD_ARRAY[row, 0] + NUTATION_ABCD_ARRAY[row, 1] * julianEphemerisCentury) * Math.Sin(radians);
                obliquity += (NUTATION_ABCD_ARRAY[row, 2] + NUTATION_ABCD_ARRAY[row, 3] * julianEphemerisCentury) * Math.Cos(radians);
            }
            return (longitude / 36000000, obliquity / 36000000);
        }

        /// <summary>Calculates mean ecliptic obliquity. Port of pvlib.spa.mean_ecliptic_obliquity.</summary>
        /// <param name="julianEphemerisMillennium">Julian ephemeris millennium [millennia] (double)</param>
        /// <returns>Mean ecliptic obliquity [arcseconds] (double)</returns>
        public static double MeanEclipticObliquity(double julianEphemerisMillennium)
        {
            var u = julianEphemerisMillennium / 10;
            return 84381.448 - 4680.93 * u - 1.55 * Math.Pow(u, 2) + 1999.25 * Math.Pow(u, 3)
                - 51.38 * Math.Pow(u, 4) - 249.67 * Math.Pow(u, 5) - 39.05 * Math.Pow(u, 6)
                + 7.12 * Math.Pow(u, 7) + 27.87 * Math.Pow(u, 8) + 5.79 * Math.Pow(u, 9)
                + 2.45 * Math.Pow(u, 10);
        }

        /// <summary>Calculates true ecliptic obliquity. Port of pvlib.spa.true_ecliptic_obliquity.</summary>
        /// <param name="meanEclipticObliquity">Mean ecliptic obliquity [arcseconds] (double)</param>
        /// <param name="obliquityNutation">Obliquity nutation [degrees] (double)</param>
        /// <returns>True ecliptic obliquity [degrees] (double)</returns>
        public static double TrueEclipticObliquity(double meanEclipticObliquity, double obliquityNutation) =>
            meanEclipticObliquity / 3600 + obliquityNutation;

        /// <summary>Calculates the aberration correction. Port of pvlib.spa.aberration_correction.</summary>
        /// <param name="earthRadiusVector">Earth-Sun radius vector [AU] (double)</param>
        /// <returns>Aberration correction [degrees] (double)</returns>
        public static double AberrationCorrection(double earthRadiusVector) => -20.4898 / (3600 * earthRadiusVector);

        /// <summary>Calculates apparent solar longitude. Port of pvlib.spa.apparent_sun_longitude.</summary>
        /// <param name="geocentricLongitude">Geocentric longitude [degrees] (double)</param>
        /// <param name="longitudeNutation">Longitude nutation [degrees] (double)</param>
        /// <param name="aberrationCorrection">Aberration correction [degrees] (double)</param>
        /// <returns>Apparent solar longitude [degrees] (double)</returns>
        public static double ApparentSunLongitude(double geocentricLongitude, double longitudeNutation, double aberrationCorrection) =>
            geocentricLongitude + longitudeNutation + aberrationCorrection;

        /// <summary>Calculates mean sidereal time. Port of pvlib.spa.mean_sidereal_time.</summary>
        /// <param name="julianDay">Julian day [days] (double)</param>
        /// <param name="julianCentury">Julian century [centuries] (double)</param>
        /// <returns>Mean sidereal time [degrees] (double)</returns>
        public static double MeanSiderealTime(double julianDay, double julianCentury)
        {
            var t = julianCentury;
            return Mod(280.46061837 + 360.98564736629 * (julianDay - 2451545)
                + 0.000387933 * Math.Pow(t, 2) - Math.Pow(t, 3) / 38710000, 360);
        }

        /// <summary>Calculates apparent sidereal time. Port of pvlib.spa.apparent_sidereal_time.</summary>
        /// <param name="meanSiderealTime">Mean sidereal time [degrees] (double)</param>
        /// <param name="longitudeNutation">Longitude nutation [degrees] (double)</param>
        /// <param name="trueEclipticObliquity">True ecliptic obliquity [degrees] (double)</param>
        /// <returns>Apparent sidereal time [degrees] (double)</returns>
        public static double ApparentSiderealTime(double meanSiderealTime, double longitudeNutation, double trueEclipticObliquity) =>
            meanSiderealTime + longitudeNutation * Math.Cos(trueEclipticObliquity * Math.PI / 180);

        /// <summary>Calculates geocentric solar right ascension. Port of pvlib.spa.geocentric_sun_right_ascension.</summary>
        /// <param name="apparentSunLongitude">Apparent solar longitude [degrees] (double)</param>
        /// <param name="trueEclipticObliquity">True ecliptic obliquity [degrees] (double)</param>
        /// <param name="geocentricLatitude">Geocentric latitude [degrees] (double)</param>
        /// <returns>Geocentric solar right ascension [degrees] (double)</returns>
        public static double GeocentricSunRightAscension(double apparentSunLongitude, double trueEclipticObliquity, double geocentricLatitude)
        {
            var eps = trueEclipticObliquity * Math.PI / 180;
            var lam = apparentSunLongitude * Math.PI / 180;
            var beta = geocentricLatitude * Math.PI / 180;
            var numerator = Math.Sin(lam) * Math.Cos(eps) - Math.Tan(beta) * Math.Sin(eps);
            return Mod(Math.Atan2(numerator, Math.Cos(lam)) * 180 / Math.PI, 360);
        }

        /// <summary>Calculates geocentric solar declination. Port of pvlib.spa.geocentric_sun_declination.</summary>
        /// <param name="apparentSunLongitude">Apparent solar longitude [degrees] (double)</param>
        /// <param name="trueEclipticObliquity">True ecliptic obliquity [degrees] (double)</param>
        /// <param name="geocentricLatitude">Geocentric latitude [degrees] (double)</param>
        /// <returns>Geocentric solar declination [degrees] (double)</returns>
        public static double GeocentricSunDeclination(double apparentSunLongitude, double trueEclipticObliquity, double geocentricLatitude)
        {
            var beta = geocentricLatitude * Math.PI / 180;
            var eps = trueEclipticObliquity * Math.PI / 180;
            return Math.Asin(Math.Sin(beta) * Math.Cos(eps) + Math.Cos(beta) * Math.Sin(eps)
                * Math.Sin(apparentSunLongitude * Math.PI / 180)) * 180 / Math.PI;
        }

        /// <summary>Calculates local hour angle measured westward from south. Port of pvlib.spa.local_hour_angle.</summary>
        /// <param name="apparentSiderealTime">Apparent sidereal time [degrees] (double)</param>
        /// <param name="observerLongitude">Observer longitude, positive east [degrees] (double)</param>
        /// <param name="sunRightAscension">Solar right ascension [degrees] (double)</param>
        /// <returns>Local hour angle [degrees] (double)</returns>
        public static double LocalHourAngle(double apparentSiderealTime, double observerLongitude, double sunRightAscension) =>
            Mod(apparentSiderealTime + observerLongitude - sunRightAscension, 360);

        /// <summary>Calculates equatorial horizontal parallax. Port of pvlib.spa.equatorial_horizontal_parallax.</summary>
        /// <param name="earthRadiusVector">Earth-Sun radius vector [AU] (double)</param>
        /// <returns>Equatorial horizontal parallax [degrees] (double)</returns>
        public static double EquatorialHorizontalParallax(double earthRadiusVector) => 8.794 / (3600 * earthRadiusVector);

        /// <summary>Calculates the observer's geocentric latitude parameter. Port of pvlib.spa.uterm.</summary>
        /// <param name="observerLatitude">Observer latitude [degrees] (double)</param>
        /// <returns>Observer geocentric latitude parameter [radians] (double)</returns>
        public static double Uterm(double observerLatitude) =>
            Math.Atan(0.99664719 * Math.Tan(observerLatitude * Math.PI / 180));

        /// <summary>Calculates the observer's x coordinate factor. Port of pvlib.spa.xterm.</summary>
        /// <param name="u">Observer geocentric latitude parameter [radians] (double)</param>
        /// <param name="observerLatitude">Observer latitude [degrees] (double)</param>
        /// <param name="observerElevation">Observer elevation [m] (double)</param>
        /// <returns>Observer x coordinate factor [n/d] (double)</returns>
        public static double Xterm(double u, double observerLatitude, double observerElevation) =>
            Math.Cos(u) + observerElevation / 6378140 * Math.Cos(observerLatitude * Math.PI / 180);

        /// <summary>Calculates the observer's y coordinate factor. Port of pvlib.spa.yterm.</summary>
        /// <param name="u">Observer geocentric latitude parameter [radians] (double)</param>
        /// <param name="observerLatitude">Observer latitude [degrees] (double)</param>
        /// <param name="observerElevation">Observer elevation [m] (double)</param>
        /// <returns>Observer y coordinate factor [n/d] (double)</returns>
        public static double Yterm(double u, double observerLatitude, double observerElevation) =>
            0.99664719 * Math.Sin(u) + observerElevation / 6378140 * Math.Sin(observerLatitude * Math.PI / 180);

        /// <summary>Calculates solar right-ascension parallax. Port of pvlib.spa.parallax_sun_right_ascension.</summary>
        /// <param name="xterm">Observer x coordinate factor [n/d] (double)</param>
        /// <param name="equatorialHorizontalParallax">Equatorial horizontal parallax [degrees] (double)</param>
        /// <param name="localHourAngle">Local hour angle [degrees] (double)</param>
        /// <param name="geocentricSunDeclination">Geocentric solar declination [degrees] (double)</param>
        /// <returns>Solar right-ascension parallax [degrees] (double)</returns>
        public static double ParallaxSunRightAscension(double xterm, double equatorialHorizontalParallax, double localHourAngle, double geocentricSunDeclination)
        {
            var xi = equatorialHorizontalParallax * Math.PI / 180;
            var hourAngle = localHourAngle * Math.PI / 180;
            var numerator = -xterm * Math.Sin(xi) * Math.Sin(hourAngle);
            var denominator = Math.Cos(geocentricSunDeclination * Math.PI / 180)
                - xterm * Math.Sin(xi) * Math.Cos(hourAngle);
            return Math.Atan2(numerator, denominator) * 180 / Math.PI;
        }

        /// <summary>Calculates topocentric solar right ascension. Port of pvlib.spa.topocentric_sun_right_ascension.</summary>
        /// <param name="geocentricSunRightAscension">Geocentric solar right ascension [degrees] (double)</param>
        /// <param name="parallaxSunRightAscension">Solar right-ascension parallax [degrees] (double)</param>
        /// <returns>Topocentric solar right ascension [degrees] (double)</returns>
        public static double TopocentricSunRightAscension(double geocentricSunRightAscension, double parallaxSunRightAscension) =>
            geocentricSunRightAscension + parallaxSunRightAscension;

        /// <summary>Calculates topocentric solar declination. Port of pvlib.spa.topocentric_sun_declination.</summary>
        /// <param name="geocentricSunDeclination">Geocentric solar declination [degrees] (double)</param>
        /// <param name="xterm">Observer x coordinate factor [n/d] (double)</param>
        /// <param name="yterm">Observer y coordinate factor [n/d] (double)</param>
        /// <param name="equatorialHorizontalParallax">Equatorial horizontal parallax [degrees] (double)</param>
        /// <param name="parallaxSunRightAscension">Solar right-ascension parallax [degrees] (double)</param>
        /// <param name="localHourAngle">Local hour angle [degrees] (double)</param>
        /// <returns>Topocentric solar declination [degrees] (double)</returns>
        public static double TopocentricSunDeclination(double geocentricSunDeclination, double xterm, double yterm,
            double equatorialHorizontalParallax, double parallaxSunRightAscension, double localHourAngle)
        {
            var delta = geocentricSunDeclination * Math.PI / 180;
            var xi = equatorialHorizontalParallax * Math.PI / 180;
            var num = (Math.Sin(delta) - yterm * Math.Sin(xi))
                * Math.Cos(parallaxSunRightAscension * Math.PI / 180);
            var den = Math.Cos(delta) - xterm * Math.Sin(xi) * Math.Cos(localHourAngle * Math.PI / 180);
            return Math.Atan2(num, den) * 180 / Math.PI;
        }

        /// <summary>Calculates topocentric local hour angle. Port of pvlib.spa.topocentric_local_hour_angle.</summary>
        /// <param name="localHourAngle">Local hour angle [degrees] (double)</param>
        /// <param name="parallaxSunRightAscension">Solar right-ascension parallax [degrees] (double)</param>
        /// <returns>Topocentric local hour angle [degrees] (double)</returns>
        public static double TopocentricLocalHourAngle(double localHourAngle, double parallaxSunRightAscension) =>
            localHourAngle - parallaxSunRightAscension;

        /// <summary>Calculates topocentric elevation without atmospheric refraction. Port of pvlib.spa.topocentric_elevation_angle_without_atmosphere.</summary>
        /// <param name="observerLatitude">Observer latitude [degrees] (double)</param>
        /// <param name="topocentricSunDeclination">Topocentric solar declination [degrees] (double)</param>
        /// <param name="topocentricLocalHourAngle">Topocentric local hour angle [degrees] (double)</param>
        /// <returns>Topocentric elevation angle without atmosphere [degrees] (double)</returns>
        public static double TopocentricElevationAngleWithoutAtmosphere(
            double observerLatitude, double topocentricSunDeclination, double topocentricLocalHourAngle)
        {
            var lat = observerLatitude * Math.PI / 180;
            var delta = topocentricSunDeclination * Math.PI / 180;
            var hourAngle = topocentricLocalHourAngle * Math.PI / 180;
            return Math.Asin(Math.Sin(lat) * Math.Sin(delta) + Math.Cos(lat) * Math.Cos(delta) * Math.Cos(hourAngle))
                * 180 / Math.PI;
        }

        /// <summary>Calculates atmospheric refraction correction. Port of pvlib.spa.atmospheric_refraction_correction.</summary>
        /// <param name="localPressure">Local air pressure [mbar] (double)</param>
        /// <param name="localTemp">Local air temperature [degrees C] (double)</param>
        /// <param name="topocentricElevationAngleWithoutAtmosphere">Topocentric elevation without atmosphere [degrees] (double)</param>
        /// <param name="atmosRefract">Atmospheric refraction at sunrise and sunset [degrees] (double)</param>
        /// <returns>Atmospheric refraction correction [degrees] (double)</returns>
        public static double AtmosphericRefractionCorrection(
            double localPressure, double localTemp, double topocentricElevationAngleWithoutAtmosphere, double atmosRefract)
        {
            var elevation = topocentricElevationAngleWithoutAtmosphere;
            var enabled = elevation >= -(0.26667 + atmosRefract);
            var correction = (localPressure / 1010 * 283 / (273 + localTemp)) * 1.02
                / (60 * Math.Tan((elevation + 10.3 / (elevation + 5.11)) * Math.PI / 180));
            return enabled ? correction : 0;
        }

        /// <summary>Calculates apparent topocentric elevation. Port of pvlib.spa.topocentric_elevation_angle.</summary>
        /// <param name="topocentricElevationAngleWithoutAtmosphere">Topocentric elevation without atmosphere [degrees] (double)</param>
        /// <param name="atmosphericRefractionCorrection">Atmospheric refraction correction [degrees] (double)</param>
        /// <returns>Topocentric elevation angle [degrees] (double)</returns>
        public static double TopocentricElevationAngle(double topocentricElevationAngleWithoutAtmosphere, double atmosphericRefractionCorrection) =>
            topocentricElevationAngleWithoutAtmosphere + atmosphericRefractionCorrection;

        /// <summary>Calculates topocentric zenith angle. Port of pvlib.spa.topocentric_zenith_angle.</summary>
        /// <param name="topocentricElevationAngle">Topocentric elevation angle [degrees] (double)</param>
        /// <returns>Topocentric zenith angle [degrees] (double)</returns>
        public static double TopocentricZenithAngle(double topocentricElevationAngle) => 90 - topocentricElevationAngle;

        /// <summary>Calculates topocentric astronomical azimuth. Port of pvlib.spa.topocentric_astronomers_azimuth.</summary>
        /// <param name="topocentricLocalHourAngle">Topocentric local hour angle [degrees] (double)</param>
        /// <param name="topocentricSunDeclination">Topocentric solar declination [degrees] (double)</param>
        /// <param name="observerLatitude">Observer latitude [degrees] (double)</param>
        /// <returns>Topocentric astronomical azimuth [degrees] (double)</returns>
        public static double TopocentricAstronomersAzimuth(
            double topocentricLocalHourAngle, double topocentricSunDeclination, double observerLatitude)
        {
            var hourAngle = topocentricLocalHourAngle * Math.PI / 180;
            var lat = observerLatitude * Math.PI / 180;
            var numerator = Math.Sin(hourAngle);
            var denominator = Math.Cos(hourAngle) * Math.Sin(lat)
                - Math.Tan(topocentricSunDeclination * Math.PI / 180) * Math.Cos(lat);
            return Mod(Math.Atan2(numerator, denominator) * 180 / Math.PI, 360);
        }

        /// <summary>Calculates topocentric azimuth angle. Port of pvlib.spa.topocentric_azimuth_angle.</summary>
        /// <param name="topocentricAstronomersAzimuth">Topocentric astronomical azimuth [degrees] (double)</param>
        /// <returns>Topocentric azimuth angle [degrees] (double)</returns>
        public static double TopocentricAzimuthAngle(double topocentricAstronomersAzimuth) =>
            Mod(topocentricAstronomersAzimuth + 180, 360);

        /// <summary>Calculates the Sun's mean longitude. Port of pvlib.spa.sun_mean_longitude.</summary>
        /// <param name="julianEphemerisMillennium">Julian ephemeris millennium [millennia] (double)</param>
        /// <returns>Sun's mean longitude [degrees] (double)</returns>
        public static double SunMeanLongitude(double julianEphemerisMillennium)
        {
            var x = julianEphemerisMillennium;
            return 280.4664567 + 360007.6982779 * x + 0.03032028 * Math.Pow(x, 2)
                + Math.Pow(x, 3) / 49931 - Math.Pow(x, 4) / 15300 - Math.Pow(x, 5) / 2000000;
        }

        /// <summary>Calculates the equation of time. Port of pvlib.spa.equation_of_time.</summary>
        /// <param name="sunMeanLongitude">Sun's mean longitude [degrees] (double)</param>
        /// <param name="geocentricSunRightAscension">Geocentric solar right ascension [degrees] (double)</param>
        /// <param name="longitudeNutation">Longitude nutation [degrees] (double)</param>
        /// <param name="trueEclipticObliquity">True ecliptic obliquity [degrees] (double)</param>
        /// <returns>Equation of time [minutes] (double)</returns>
        public static double EquationOfTime(
            double sunMeanLongitude, double geocentricSunRightAscension, double longitudeNutation, double trueEclipticObliquity)
        {
            var e = Mod(sunMeanLongitude - 0.0057183 - geocentricSunRightAscension
                + longitudeNutation * Math.Cos(trueEclipticObliquity * Math.PI / 180), 360) * 4;
            if (e > 20)
            {
                e -= 1440;
            }
            else if (e < -20)
            {
                e += 1440;
            }
            return e;
        }

        /// <summary>Calculates the complete SPA chain for one timestamp and returns intermediate and final values.</summary>
        /// <param name="unixtime">Seconds since 1970-01-01T00:00:00Z [seconds] (double)</param>
        /// <param name="latitude">Observer latitude [degrees] (double)</param>
        /// <param name="longitude">Observer longitude, positive east [degrees] (double)</param>
        /// <param name="elevation">Observer elevation [m] (double)</param>
        /// <param name="pressure">Local air pressure [mbar] (double)</param>
        /// <param name="temperature">Local air temperature [degrees C] (double)</param>
        /// <param name="deltaT">Difference between terrestrial time and UT1 [seconds] (double)</param>
        /// <param name="atmosRefract">Atmospheric refraction at sunrise and sunset [degrees] (double)</param>
        /// <returns>Apparent zenith, zenith, apparent elevation, elevation, azimuth, equation of time, sidereal time, right ascension, declination, and radius vector in their native units (named tuple)</returns>
        private static (double ApparentZenith, double Zenith, double ApparentElevation, double Elevation,
            double Azimuth, double EquationOfTime, double SiderealTime, double RightAscension,
            double Declination, double RadiusVector) CalculateSolarPosition(
            double unixtime, double latitude, double longitude, double elevation, double pressure,
            double temperature, double deltaT, double atmosRefract)
        {
            var jd = JulianDay(unixtime);
            var jde = JulianEphemerisDay(jd, deltaT);
            var jc = JulianCentury(jd);
            var jce = JulianEphemerisCentury(jde);
            var jme = JulianEphemerisMillennium(jce);
            var radius = HeliocentricRadiusVector(jme);
            var heliocentricLongitude = HeliocentricLongitude(jme);
            var heliocentricLatitude = HeliocentricLatitude(jme);
            var theta = GeocentricLongitude(heliocentricLongitude);
            var beta = GeocentricLatitude(heliocentricLatitude);
            var x0 = MeanElongation(jce);
            var x1 = MeanAnomalySun(jce);
            var x2 = MeanAnomalyMoon(jce);
            var x3 = MoonArgumentLatitude(jce);
            var x4 = MoonAscendingLongitude(jce);
            var nutation = LongitudeObliquityNutation(jce, x0, x1, x2, x3, x4);
            var epsilon0 = MeanEclipticObliquity(jme);
            var epsilon = TrueEclipticObliquity(epsilon0, nutation.ObliquityNutation);
            var tau = AberrationCorrection(radius);
            var apparentLongitude = ApparentSunLongitude(theta, nutation.LongitudeNutation, tau);
            var meanSidereal = MeanSiderealTime(jd, jc);
            var sidereal = ApparentSiderealTime(meanSidereal, nutation.LongitudeNutation, epsilon);
            var alpha = GeocentricSunRightAscension(apparentLongitude, epsilon, beta);
            var delta = GeocentricSunDeclination(apparentLongitude, epsilon, beta);
            var eot = EquationOfTime(SunMeanLongitude(jme), alpha, nutation.LongitudeNutation, epsilon);
            var hourAngle = LocalHourAngle(sidereal, longitude, alpha);
            var parallax = EquatorialHorizontalParallax(radius);
            var u = Uterm(latitude);
            var x = Xterm(u, latitude, elevation);
            var y = Yterm(u, latitude, elevation);
            var deltaAlpha = ParallaxSunRightAscension(x, parallax, hourAngle, delta);
            var deltaPrime = TopocentricSunDeclination(delta, x, y, parallax, deltaAlpha, hourAngle);
            var hourAnglePrime = TopocentricLocalHourAngle(hourAngle, deltaAlpha);
            var elevation0 = TopocentricElevationAngleWithoutAtmosphere(latitude, deltaPrime, hourAnglePrime);
            var refraction = AtmosphericRefractionCorrection(pressure, temperature, elevation0, atmosRefract);
            var elevationApparent = TopocentricElevationAngle(elevation0, refraction);
            var zenithApparent = TopocentricZenithAngle(elevationApparent);
            var zenith = TopocentricZenithAngle(elevation0);
            var gamma = TopocentricAstronomersAzimuth(hourAnglePrime, deltaPrime, latitude);
            var azimuth = TopocentricAzimuthAngle(gamma);
            return (zenithApparent, zenith, elevationApparent, elevation0, azimuth, eot, sidereal, alpha, delta, radius);
        }

        /// <summary>Calculates SPA solar position for each Unix timestamp. Mode-specific outputs use SolarPositionSst or SolarPositionEsd because their tuple shapes differ from the normal six-array result.</summary>
        /// <param name="unixtime">Seconds since 1970-01-01T00:00:00Z [seconds] (array of double)</param>
        /// <param name="lat">Observer latitude [degrees] (double)</param>
        /// <param name="lon">Observer longitude, positive east [degrees] (double)</param>
        /// <param name="elev">Observer elevation [m] (double)</param>
        /// <param name="pressure">Local air pressure [mbar] (double)</param>
        /// <param name="temp">Local air temperature [degrees C] (double)</param>
        /// <param name="deltaT">Difference between terrestrial time and UT1 [seconds] (array of double)</param>
        /// <param name="atmosRefract">Atmospheric refraction at sunrise and sunset [degrees] (double)</param>
        /// <returns>ApparentZenith and Zenith [degrees], ApparentElevation and Elevation [degrees], Azimuth [degrees], and EquationOfTime [minutes] (named tuple of arrays of double)</returns>
        /// <exception cref="ArgumentNullException">unixtime or deltaT is null.</exception>
        /// <exception cref="ArgumentException">unixtime and deltaT have different lengths.</exception>
        public static (double[] ApparentZenith, double[] Zenith, double[] ApparentElevation, double[] Elevation,
            double[] Azimuth, double[] EquationOfTime) SolarPosition(
            double[] unixtime, double lat, double lon, double elev, double pressure, double temp,
            double[] deltaT, double atmosRefract)
        {
            ValidateSolarPositionInputs(unixtime, deltaT);
            var apparentZenith = new double[unixtime.Length];
            var zenith = new double[unixtime.Length];
            var apparentElevation = new double[unixtime.Length];
            var elevation = new double[unixtime.Length];
            var azimuth = new double[unixtime.Length];
            var equationOfTime = new double[unixtime.Length];
            for (var i = 0; i < unixtime.Length; i++)
            {
                var result = CalculateSolarPosition(unixtime[i], lat, lon, elev, pressure, temp, deltaT[i], atmosRefract);
                apparentZenith[i] = result.ApparentZenith;
                zenith[i] = result.Zenith;
                apparentElevation[i] = result.ApparentElevation;
                elevation[i] = result.Elevation;
                azimuth[i] = result.Azimuth;
                equationOfTime[i] = result.EquationOfTime;
            }
            return (apparentZenith, zenith, apparentElevation, elevation, azimuth, equationOfTime);
        }

        /// <summary>Calculates SPA solar position with one delta-T value broadcast across all timestamps.</summary>
        /// <param name="unixtime">Seconds since 1970-01-01T00:00:00Z [seconds] (array of double)</param>
        /// <param name="lat">Observer latitude [degrees] (double)</param>
        /// <param name="lon">Observer longitude, positive east [degrees] (double)</param>
        /// <param name="elev">Observer elevation [m] (double)</param>
        /// <param name="pressure">Local air pressure [mbar] (double)</param>
        /// <param name="temp">Local air temperature [degrees C] (double)</param>
        /// <param name="deltaT">Difference between terrestrial time and UT1 [seconds] (double)</param>
        /// <param name="atmosRefract">Atmospheric refraction at sunrise and sunset [degrees] (double)</param>
        /// <returns>ApparentZenith and Zenith [degrees], ApparentElevation and Elevation [degrees], Azimuth [degrees], and EquationOfTime [minutes] (named tuple of arrays of double)</returns>
        /// <exception cref="ArgumentNullException">unixtime is null.</exception>
        public static (double[] ApparentZenith, double[] Zenith, double[] ApparentElevation, double[] Elevation,
            double[] Azimuth, double[] EquationOfTime) SolarPosition(
            double[] unixtime, double lat, double lon, double elev, double pressure, double temp,
            double deltaT, double atmosRefract) =>
            SolarPosition(unixtime, lat, lon, elev, pressure, temp, RepeatDeltaT(unixtime, deltaT), atmosRefract);

        /// <summary>Calculates SPA intermediate values for transit, sunrise, and sunset calculations (pvlib's sst mode).</summary>
        /// <param name="unixtime">Seconds since 1970-01-01T00:00:00Z [seconds] (array of double)</param>
        /// <param name="deltaT">Difference between terrestrial time and UT1 [seconds] (array of double)</param>
        /// <returns>ApparentSiderealTime [degrees], RightAscension [degrees], and Declination [degrees] (named tuple of arrays of double)</returns>
        /// <exception cref="ArgumentNullException">unixtime or deltaT is null.</exception>
        /// <exception cref="ArgumentException">unixtime and deltaT have different lengths.</exception>
        public static (double[] ApparentSiderealTime, double[] RightAscension, double[] Declination) SolarPositionSst(
            double[] unixtime, double[] deltaT)
        {
            ValidateSolarPositionInputs(unixtime, deltaT);
            var sidereal = new double[unixtime.Length];
            var rightAscension = new double[unixtime.Length];
            var declination = new double[unixtime.Length];
            for (var i = 0; i < unixtime.Length; i++)
            {
                var result = CalculateSolarPosition(unixtime[i], 0, 0, 0, 0, 0, deltaT[i], 0);
                sidereal[i] = result.SiderealTime;
                rightAscension[i] = result.RightAscension;
                declination[i] = result.Declination;
            }
            return (sidereal, rightAscension, declination);
        }

        /// <summary>Calculates SPA intermediate values with one delta-T value broadcast across timestamps.</summary>
        /// <param name="unixtime">Seconds since 1970-01-01T00:00:00Z [seconds] (array of double)</param>
        /// <param name="deltaT">Difference between terrestrial time and UT1 [seconds] (double)</param>
        /// <returns>ApparentSiderealTime [degrees], RightAscension [degrees], and Declination [degrees] (named tuple of arrays of double)</returns>
        /// <exception cref="ArgumentNullException">unixtime is null.</exception>
        public static (double[] ApparentSiderealTime, double[] RightAscension, double[] Declination) SolarPositionSst(
            double[] unixtime, double deltaT) =>
            SolarPositionSst(unixtime, RepeatDeltaT(unixtime, deltaT));

        /// <summary>Calculates Earth-Sun radius vector for each Unix timestamp (pvlib's esd mode).</summary>
        /// <param name="unixtime">Seconds since 1970-01-01T00:00:00Z [seconds] (array of double)</param>
        /// <param name="deltaT">Difference between terrestrial time and UT1 [seconds] (array of double)</param>
        /// <returns>Earth-Sun radius vector [AU] (array of double)</returns>
        /// <exception cref="ArgumentNullException">unixtime or deltaT is null.</exception>
        /// <exception cref="ArgumentException">unixtime and deltaT have different lengths.</exception>
        public static double[] SolarPositionEsd(double[] unixtime, double[] deltaT)
        {
            ValidateSolarPositionInputs(unixtime, deltaT);
            var radius = new double[unixtime.Length];
            for (var i = 0; i < unixtime.Length; i++)
            {
                var jme = JulianEphemerisMillennium(JulianEphemerisCentury(
                    JulianEphemerisDay(JulianDay(unixtime[i]), deltaT[i])));
                radius[i] = HeliocentricRadiusVector(jme);
            }
            return radius;
        }

        /// <summary>Calculates Earth-Sun radius vector with one delta-T value broadcast across timestamps.</summary>
        /// <param name="unixtime">Seconds since 1970-01-01T00:00:00Z [seconds] (array of double)</param>
        /// <param name="deltaT">Difference between terrestrial time and UT1 [seconds] (double)</param>
        /// <returns>Earth-Sun radius vector [AU] (array of double)</returns>
        /// <exception cref="ArgumentNullException">unixtime is null.</exception>
        public static double[] SolarPositionEsd(double[] unixtime, double deltaT) =>
            SolarPositionEsd(unixtime, RepeatDeltaT(unixtime, deltaT));

        /// <summary>Returns an array containing delta-T for every timestamp.</summary>
        /// <param name="unixtime">Seconds since 1970-01-01T00:00:00Z [seconds] (array of double)</param>
        /// <param name="deltaT">Difference between terrestrial time and UT1 [seconds] (double)</param>
        /// <returns>Repeated delta-T values [seconds] (array of double)</returns>
        /// <exception cref="ArgumentNullException">unixtime is null.</exception>
        private static double[] RepeatDeltaT(double[] unixtime, double deltaT)
        {
            ArgumentNullException.ThrowIfNull(unixtime);
            var values = new double[unixtime.Length];
            Array.Fill(values, deltaT);
            return values;
        }

        /// <summary>Validates aligned vector inputs to the SPA position entry points.</summary>
        /// <param name="unixtime">Seconds since 1970-01-01T00:00:00Z [seconds] (array of double)</param>
        /// <param name="deltaT">Difference between terrestrial time and UT1 [seconds] (array of double)</param>
        /// <exception cref="ArgumentNullException">unixtime or deltaT is null.</exception>
        /// <exception cref="ArgumentException">unixtime and deltaT have different lengths.</exception>
        private static void ValidateSolarPositionInputs(double[] unixtime, double[] deltaT)
        {
            ArgumentNullException.ThrowIfNull(unixtime);
            ArgumentNullException.ThrowIfNull(deltaT);
            if (unixtime.Length != deltaT.Length)
            {
                throw new ArgumentException("Unix time and delta-T arrays must have the same length.", nameof(deltaT));
            }
        }

        /// <summary>Returns a non-negative remainder, matching NumPy's modulo for positive divisors.</summary>
        /// <param name="value">Value to reduce [n/d] (double)</param>
        /// <param name="divisor">Positive modulo divisor [n/d] (double)</param>
        /// <returns>Modulo remainder [n/d] (double)</returns>
        private static double Mod(double value, double divisor) => ((value % divisor) + divisor) % divisor;

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
