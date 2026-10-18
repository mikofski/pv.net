using System;
using System.Globalization;
using pv;

namespace modelchain
{
    class ModelChain
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calculate Solar Position");
            DateTimeOffset[] times;
            var lat = 37.81;
            var lon = -122.25;
            var tz = -8.0;
            var nargs = args.Length;
            if (nargs > 3)
            {
                times = new DateTimeOffset[nargs - 3];
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
                    var localTime = DateTime.ParseExact(
                        args[3 + i],
                        "yyyy-MM-dd'T'HH:mm:ss",
                        CultureInfo.InvariantCulture);
                    times[i] = new DateTimeOffset(localTime, TimeSpan.FromHours(tz));
                }
            }
            else
            {

                times = new[]
                {
                    new DateTimeOffset(1990, 1, 1, 12, 30, 0, TimeSpan.FromHours(tz)),
                    new DateTimeOffset(1990, 1, 2, 12, 30, 0, TimeSpan.FromHours(tz)),
                    new DateTimeOffset(1990, 1, 3, 12, 30, 0, TimeSpan.FromHours(tz)),
                    new DateTimeOffset(1990, 1, 4, 12, 30, 0, TimeSpan.FromHours(tz))
                };
            }
            var location = new Location(lat, lon, altitude: 0, TimeSpan.FromHours(tz));
            var dayOfYear = Array.ConvertAll(times, time => (double)time.DayOfYear);
            Console.WriteLine("Day Angle (radians)");
            var dayAngle = SolarPosition.CalcSimpleDayAngleArray(dayOfYear);

            Console.WriteLine("Equation of Time, Spencer (1971)");
            var eot = SolarPosition.EquationOfTimeSpencer71(dayOfYear);
            for (var i = 0; i < times.Length; i++)
            {
                Console.WriteLine($"{dayOfYear[i]:g}[days] --> {eot[i]:f5}[min]");
            }

            Console.WriteLine("day angle relative to vernal equinox (radians)");
            Console.WriteLine("Equation of Time, PVCDROM");
            var eotPvCdrom = SolarPosition.EquationOfTimePvCdrom(dayOfYear);
            for (var i = 0; i < times.Length; i++)
            {
                Console.WriteLine($"{dayOfYear[i]:g}[days] --> {eotPvCdrom[i]:f5}[min]");
            }

            Console.WriteLine("Declination, Spencer 1971");
            var declinationSpencer71 = SolarPosition.DeclinationSpencer71(dayOfYear);
            for (var i = 0; i < times.Length; i++)
            {
                Console.WriteLine($"{dayOfYear[i]:g}[days] --> {declinationSpencer71[i]:f5}[rad]");
            }

            Console.WriteLine("Declination, Cooper 1969");
            var declinationCooper69 = SolarPosition.DeclinationCooper69(dayAngle);
            for (var i = 0; i < times.Length; i++)
            {
                Console.WriteLine($"{dayOfYear[i]:g}[days] --> {declinationCooper69[i]:f5}[rad]");
            }

            Console.WriteLine("Hour Angle");
            var hourAngle = SolarPosition.HourAngle(location, times, eot);
            for (var i = 0; i < times.Length; i++)
            {
                Console.WriteLine($"{times[i]:g} --> {hourAngle[i]:f5}[deg]");
            }

            Console.WriteLine("Zenith");
            var hourAngleRadians = Array.ConvertAll(hourAngle, angle => angle * Math.PI / 180.0);
            var ze = SolarPosition.SolarZenith(lat * Math.PI / 180.0, hourAngleRadians, declinationSpencer71);
            for (var i = 0; i < times.Length; i++)
            {
                Console.WriteLine($"{times[i]:g} --> {ze[i]:f5}[rad]");
            }
        }
    }
}
