using System;

namespace modelchain
{
    class ModelChain
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calculate Simple Day Angle!");
            string[] times = new string[4] {
                "19900101T12:30:00", "19900102T12:30:00","19900103T12:30:00",
                "19900104T12:30:00" };
            float lat = (float)37.8132664;
            float lon = (float)-122.2540443;
            pv.SolarPosition sp = new pv.SolarPosition(times, lat, lon);
        }
    }
}
