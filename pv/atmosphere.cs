using System;

namespace pv
{
    /// <summary>
    /// Provides atmospheric pressure and altitude conversions ported from pvlib.atmosphere.
    /// </summary>
    public static class Atmosphere
    {
        /// <summary>
        /// Calculates altitude from pressure; port of pvlib.atmosphere.pres2alt.
        /// Assumes base pressure 101325 Pa, temperature 288.15 K at zero altitude,
        /// g = 9.80665 m/s², lapse rate -6.5e-3 K/m, R = 287.053 J/(kg K), and 0% relative humidity.
        /// </summary>
        /// <param name="pressure">atmospheric pressure [Pa] (double)</param>
        /// <returns>altitude above sea level [m] (double)</returns>
        public static double Pres2Alt(double pressure) =>
            44331.5 - 4946.62 * Math.Pow(pressure, 0.190263);

        /// <summary>
        /// Calculates altitudes from pressures; port of pvlib.atmosphere.pres2alt.
        /// Assumes base pressure 101325 Pa, temperature 288.15 K at zero altitude,
        /// g = 9.80665 m/s², lapse rate -6.5e-3 K/m, R = 287.053 J/(kg K), and 0% relative humidity.
        /// </summary>
        /// <param name="pressure">atmospheric pressure [Pa] (array of double)</param>
        /// <returns>altitude above sea level [m] (array of double)</returns>
        /// <exception cref="ArgumentNullException">pressure is null.</exception>
        public static double[] Pres2Alt(double[] pressure) =>
            Array.ConvertAll(pressure, Pres2Alt);

        /// <summary>
        /// Calculates pressure from altitude; port of pvlib.atmosphere.alt2pres.
        /// Assumes base pressure 101325 Pa, temperature 288.15 K at zero altitude,
        /// g = 9.80665 m/s², lapse rate -6.5e-3 K/m, R = 287.053 J/(kg K), and 0% relative humidity.
        /// </summary>
        /// <param name="altitude">altitude above sea level [m] (double)</param>
        /// <returns>atmospheric pressure [Pa] (double)</returns>
        public static double Alt2Pres(double altitude) =>
            100 * Math.Pow((44331.514 - altitude) / 11880.516, 1 / 0.1902632);

        /// <summary>
        /// Calculates pressures from altitudes; port of pvlib.atmosphere.alt2pres.
        /// Assumes base pressure 101325 Pa, temperature 288.15 K at zero altitude,
        /// g = 9.80665 m/s², lapse rate -6.5e-3 K/m, R = 287.053 J/(kg K), and 0% relative humidity.
        /// </summary>
        /// <param name="altitude">altitude above sea level [m] (array of double)</param>
        /// <returns>atmospheric pressure [Pa] (array of double)</returns>
        /// <exception cref="ArgumentNullException">altitude is null.</exception>
        public static double[] Alt2Pres(double[] altitude) =>
            Array.ConvertAll(altitude, Alt2Pres);
    }
}
