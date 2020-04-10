using System;

namespace modelchain
{
    class ModelChain
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calculate Solar Position");
            string[] times;
            var lat = 37.81;
            var lon = -122.25;
            var tz = -8.0;
            var nargs = args.Length;
            if (nargs > 3)
            {
                times = new string[nargs - 3];
                try
                {
                    lat = Convert.ToDouble(args[0]);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine($"Latitude: {args[0]} must be double");
                    return;
                }
                try
                {
                    lon = Convert.ToDouble(args[1]);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine($"Longitude: {args[1]} must be double");
                    return;
                }
                try
                {
                    tz = Convert.ToDouble(args[2]);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine($"TimeZone: {args[2]} must be double");
                    return;
                }
                for (var i = 0; i < nargs - 3; i++)
                {
                    times[i] = args[3 + i];
                }
            }
            else
            {

                times = new[]
                {
                    "19900101T12:30:00", "19900102T12:30:00",
                    "19900103T12:30:00", "19900104T12:30:00"
                };
            }
            var sp = new pv.SolarPosition(times, lat, lon, tz);
            Console.WriteLine("Day Angle (radians)");
            var dayAngle = sp.CalcSimpleDayAngleArray();

            Console.WriteLine("Equation of Time, Spencer (1971)");
            var eot = sp.EquationOfTimeSpencer71(dayAngle);
            for (var i = 0; i < sp.NDays; i++)
            {
                Console.WriteLine($"{sp.DayOfYearArray[i]:n} --> {eot[i]:g}");
            }

            Console.WriteLine("day angle relative to vernal equinox (radians)");
            var bday = sp.CalcSimpleDayAngleArray(offset: 81);
            Console.WriteLine("Equation of Time, PVCDROM");
            var eotPvCdrom = sp.EquationOfTimePvCdrom(bday);
            for (var i = 0; i < sp.NDays; i++)
            {
                Console.WriteLine($"{sp.DayOfYearArray[i]:n} --> {eotPvCdrom[i]:g}");
            }

            Console.WriteLine("Declination, Spencer 1971");
            var declinationSpencer71 = sp.DeclinationSpencer71(dayAngle);
            for (var i = 0; i < sp.NDays; i++)
            {
                Console.WriteLine($"{sp.DayOfYearArray[i]:n} --> {declinationSpencer71[i]:g}");
            }

            Console.WriteLine("Declination, Cooper 1969");
            var declinationCooper69 = sp.DeclinationCooper69(dayAngle);
            for (var i = 0; i < sp.NDays; i++)
            {
                Console.WriteLine($"{sp.DayOfYearArray[i]:n} --> {declinationCooper69[i]:g}");
            }

            Console.WriteLine("Hour Angle");
            var hourAngle = sp.HourAngle(eot);
            for (var i = 0; i < sp.NDays; i++)
            {
                Console.WriteLine($"{sp.DateTimeArray[i]:MM/dd/yyyy HH:mm:ss} --> {hourAngle[i]:g}");
            }

            Console.WriteLine("Zenith");
            var ze = sp.SolarZenith(hourAngle, declinationSpencer71);
            for (var i = 0; i < sp.NDays; i++)
            {
                Console.WriteLine($"{sp.DateTimeArray[i]:MM/dd/yyyy HH:mm:ss} --> {ze[i]:g}");
            }
        }
    }
}
