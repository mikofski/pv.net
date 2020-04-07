using System;

namespace pv.net
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calculate Simple Day Angle!");
            string[] times = new string[4] { "19900101T12:30:00", "19900102T12:30:00", "19900103T12:30:00", "19900104T12:30:00" };
            SolarPosition sp = new pv.net.SolarPosition(times, (float)32.1, (float)-123.4);
        }
    }
}
