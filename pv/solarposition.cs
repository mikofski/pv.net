using System;

namespace pv
{
    public class SolarPosition
    {
        public readonly string[] Times;
        public readonly double Latitude, Longitude;
        public readonly int NDays;
        public readonly DateTime[] DateTimeArray;
        public readonly int[] DayOfYearArray;
        public double[] ZenithArray, AzimuthArray;

        /// <summary>
        /// Class that represents the solar positions for a given set of times
        /// and pair of latitude & longitude coordinates.
        /// </summary>
        /// <param name="times">an array of strings of naive local times in
        /// ISO 8601 format, seconds precision (Array of String)</param>
        /// <param name="latitude">the latitude in degrees (double)</param>
        /// <param name="longitude">the longitude in degrees (double)</param>
        public SolarPosition(string[] times, double latitude, double longitude)
        {
            Times = times;
            Latitude = latitude;
            Longitude = longitude;
            NDays = times.Length;
            DateTimeArray = TimesToDateTimes();
            DayOfYearArray = new int[NDays];
            for (var i = 0; i < NDays; i++ )
            {
                DayOfYearArray[i] = DateTimeArray[i].DayOfYear;
                Console.WriteLine(
                    $"{DateTimeArray[i]:dd/MM/yyyy HH:mm:ss} --> {DayOfYearArray[i]:n}");
            }
        }

        /// <summary>
        /// Convert array of strings of naive local times in ISO 8601 format,
        /// seconds precision, to an array of DateTime objects.
        /// </summary>
        /// <returns>Array of DateTime objects</returns>
        private DateTime[] TimesToDateTimes()
        {
            var dateTimeArray = new DateTime[NDays];
            const string fmt = "yyyyMMddTHH:mm:ss";
            for (var i = 0; i < NDays; i++)
            {
                dateTimeArray[i] = DateTime.ParseExact(Times[i], fmt,
                    System.Globalization.CultureInfo.InvariantCulture);
                Console.WriteLine($"{Times[i]} --> {dateTimeArray[i]:g}");
            }

            return dateTimeArray;
        }

        /// <summary>
        /// Calculate the day angle in radians for the Earth's orbit around the sun.
        /// For the Spencer method, offset=1; for the ASCE method, offset=0.
        /// </summary>
        /// <param name="offset">an offset in days (integer, default: 1)</param>
        /// <returns>day angles in radians (Array of double)</returns>
        public double[] CalcSimpleDayAngleArray(int offset = 1)
        {
            var dayAngle = new double[NDays];
            for (var i = 0; i < NDays; i++)
            {
                dayAngle[i] = (2.0 * Math.PI / 365.0) * (DayOfYearArray[i] - offset);
                Console.WriteLine($"{DayOfYearArray[i]:n} --> {dayAngle[i]:g}");
            }

            return dayAngle;
        }

        /// <summary>
        /// Calculate the equation of time according to Spencer (1971)
        /// </summary>
        /// <param name="dayAngle">day angle in radians (Array of double)</param>
        /// <returns>equation of time (Array of double)</returns>
        public double[] EquationOfTimeSpencer71(double[] dayAngle)
        {
            // convert from radians to minutes per day = 24[h/day] * 60[min/h] / 2 / pi
            var eot = new double[NDays];
            for (var i = 0; i < NDays; i++)
            {
                eot[i] = (1440.0 / 2 / Math.PI) * (
                             0.0000075
                             + 0.001868 * Math.Cos(dayAngle[i])
                             - 0.032077 * Math.Sin(dayAngle[i])
                             - 0.014615 * Math.Cos(2.0 * dayAngle[i])
                             - 0.040849 * Math.Sin(2.0 * dayAngle[i]));
            }

            return eot;
        }

        /// <summary>
        /// Calculate the equation of time according to PVCDROM.
        /// </summary>
        /// <param name="bday">day angle in radians relative to Vernal
        /// Equinox, typically March 22, day number 81 (Array of double)</param>
        /// <returns>equation of time (Array of double)</returns>
        public double[] EquationOfTimePvCdrom(double[] bday)
        {
            var eot = new double[NDays];
            for (var i = 0; i < NDays; i++)
            {
                // same value but about 2x faster than Spencer (1971)
                eot[i] = 9.87 * Math.Sin(2.0 * bday[i])
                         - 7.53 * Math.Cos(bday[i])
                         - 1.5 * Math.Sin(bday[i]);
            }

            return eot;
        }

        /// <summary>
        /// Solar declination from Duffie & Beckman and attributed to
        /// Spencer(1971) and Iqbal(1983).
        /// </summary>
        /// <param name="dayAngle">day angle in radians (Array of double)</param>
        /// <returns>declination in radians (Array of doubles)</returns>
        public double[] DeclinationSpencer71(double[] dayAngle)
        {
            var decl = new double[NDays];
            for (var i = 0; i < NDays; i++)
            {
                decl[i] = 0.006918
                          - 0.399912 * Math.Cos(dayAngle[i])
                          + 0.070257 * Math.Sin(dayAngle[i])
                          - 0.006758 * Math.Cos(2.0 * dayAngle[i])
                          + 0.000907 * Math.Sin(2.0 * dayAngle[i])
                          - 0.002697 * Math.Cos(3.0 * dayAngle[i])
                          + 0.00148 * Math.Sin(3.0 * dayAngle[i]);
            }

            return decl;
        }

        /// <summary>
        /// Solar declination from Duffie & Beckman and attributed to Cooper (1969).
        /// </summary>
        /// <param name="dayAngle">day angle in radians (Array of double)</param>
        /// <returns>declination in radians (Array of doubles)</returns>
        public double[] DeclinationCooper69(double[] dayAngle)
        {
            var decl = new double[NDays];
            for (var i = 0; i < NDays; i++)
            {
                decl[i] = Math.PI / 180.0 * (
                    23.45 * Math.Sin(dayAngle[i] + (2.0 * Math.PI / 365.0) * 285.0));
            }

            return decl;
        }
    }
}
