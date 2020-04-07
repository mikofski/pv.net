using System;

namespace pv.net
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            args = new string[4] { "19900101T12:30:00", "19900102T12:30:00", "19900103T12:30:00", "19900104T12:30:00" };
            int nargs = args.Length;
            SolarPosition sp = new pv.net.SolarPosition(args, (float)32.1, (float)-123.4);
        }
    }
}
