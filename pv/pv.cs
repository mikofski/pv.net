using System;
using System.Linq;

namespace pv
{
    public class SolarPosition
    {
        public float Latitude, Longitude;
        public string[] Times;
        public int NDays;
        public float[] ZenithArray, AzimuthArray;
        public float[] DayAngleArray;

        public SolarPosition(string[] times, float latitude, float longitude)
        {
            this.Times = times;
            this.NDays = times.Length;
            this.Latitude = latitude;
            this.Longitude = longitude;
            DateTime[] theseDateTimes = TimesToDateTimes();
            // int[] dayofyear = new int[NDays];
            int[] dayofyear = theseDateTimes.Select(dt => dt.DayOfYear).ToArray();
            for (var i = 0; i < NDays; i++ )
            {
                // dayofyear[i] = theseDateTimes[i].DayOfYear;
                Console.WriteLine($"{theseDateTimes[i]:dd/MM/yyyy HH:mm:ss} --> {dayofyear[i]:n}");
            }
            DayAngleArray = CalcSimpleDayAngleArray(dayofyear);
        }

        public float[] CalcSimpleDayAngleArray(int[] dayofyear, int offset = 1)
        {
            // float[] dayAngle = new float[NDays];
            float[] dayAngle = dayofyear.Select(doy => (float)(2.0 * Math.PI / 365.0) * (doy - offset)).ToArray();
            for (var i = 0; i < NDays; i++)
            {
                // dayAngle[i] = (float) (2.0 * Math.PI / 365.0) * (dayofyear[i] - offset);
                Console.WriteLine($"{dayofyear[i]:n} --> {dayAngle[i]:g}");
            }
            return dayAngle;
        }

        public DateTime[] TimesToDateTimes()
        {
            //DateTime[] theseDateTimes = new DateTime[NDays];
            DateTime[] theseDateTimes = Times.Select(time => DateTime.ParseExact(time, "yyyyMMddTHH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            for (var i = 0; i < NDays; i++)
            {
                // theseDateTimes[i] = DateTime.ParseExact(Times[i], "yyyyMMddTHH:mm:ss",
                //     System.Globalization.CultureInfo.InvariantCulture);
                Console.WriteLine($"{Times[i]} --> {theseDateTimes[i]:g}");
            }

            return theseDateTimes;
        }

    }
}
