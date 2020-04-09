using System;

namespace pv
{
    public class SolarPosition
    {
        public readonly double Latitude, Longitude;
        public readonly string[] Times;
        public readonly int NDays;
        public double[] ZenithArray, AzimuthArray;
        public double[] DayAngleArray;

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
            NDays = times.Length;
            Latitude = latitude;
            Longitude = longitude;
            DateTime[] dateTimeArray = TimesToDateTimes();
            int[] dayofyear = new int[NDays];
            for (var i = 0; i < NDays; i++ )
            {
                dayofyear[i] = dateTimeArray[i].DayOfYear;
                Console.WriteLine($"{dateTimeArray[i]:dd/MM/yyyy HH:mm:ss} --> {dayofyear[i]:n}");
            }
            DayAngleArray = CalcSimpleDayAngleArray(dayofyear);
        }

        /// <summary>
        /// Calculate the day angle for the Earth's orbit around the sun.
        /// </summary>
        /// <param name="dayofyear">the day of the year (integer)</param>
        /// <param name="offset">an offset in days (integer, default: 1)</param>
        /// <returns>day angles (Array of double)</returns>
        public double[] CalcSimpleDayAngleArray(int[] dayofyear, int offset = 1)
        {
            double[] dayAngle = new double[NDays];
            for (var i = 0; i < NDays; i++)
            {
                dayAngle[i] = (2.0 * Math.PI / 365.0) * (dayofyear[i] - offset);
                Console.WriteLine($"{dayofyear[i]:n} --> {dayAngle[i]:g}");
            }

            return dayAngle;
        }

        /// <summary>
        /// Convert array of strings of naive local times in ISO 8601 format,
        /// seconds precision, to an array of DateTime objects.
        /// </summary>
        /// <returns>Array of DateTime objects</returns>
        public DateTime[] TimesToDateTimes()
        {
            DateTime[] dateTimeArray = new DateTime[NDays];
            for (var i = 0; i < NDays; i++)
            {
                dateTimeArray[i] = DateTime.ParseExact(Times[i], "yyyyMMddTHH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture);
                Console.WriteLine($"{Times[i]} --> {dateTimeArray[i]:g}");
            }

            return dateTimeArray;
        }

        /// <summary>
        /// Calculate the equation of time according to Spencer (1971)
        /// </summary>
        /// <param name="dayofyear">the day of the year (integer)</param>
        /// <returns>equation of time (double)</returns>
        public double[] EquationOfTimeSpencer71(int[] dayofyear)
        {
            double[] dayAngle = CalcSimpleDayAngleArray(dayofyear);
            // convert from radians to minutes per day = 24[h/day] * 60[min/h] / 2 / pi
            double[] eot = new double[NDays];
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
    }
}
