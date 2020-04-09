using System;

namespace modelchain
{
    class ModelChain
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calculate Solar Position");
            string[] times = new string[4] {
                "19900101T12:30:00", "19900102T12:30:00","19900103T12:30:00",
                "19900104T12:30:00" };
            double lat = 37.8132664;
            double lon = -122.2540443;
            pv.SolarPosition sp = new pv.SolarPosition(times, lat, lon);
            int[] dayofyear = new int[4] { 1, 2, 3, 4 };
            double[] eot = sp.EquationOfTimeSpencer71(dayofyear);
            Console.WriteLine("Equation of Time, Spencer (1971)");
            for (var i = 0; i < sp.NDays; i++)
            {
                Console.WriteLine($"{dayofyear[i]:n} --> {eot[i]:g}");
            }
        }
    }
}
