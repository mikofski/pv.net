#nullable enable
using System;

namespace pv
{
    /// <summary>
    /// Provides stateless analytical solar-position calculations.
    /// </summary>
    public static class SolarPosition
    {
        /// <summary>
        /// Port of pvlib.solarposition._calculate_simple_day_angle.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <param name="offset">day-angle offset [days] (int, default: 1)</param>
        /// <returns>Earth orbital day angle [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] CalcSimpleDayAngleArray(double[] dayOfYear, int offset = 1)
        {
            ArgumentNullException.ThrowIfNull(dayOfYear);

            var dayAngle = new double[dayOfYear.Length];
            for (var i = 0; i < dayOfYear.Length; i++)
            {
                dayAngle[i] = (2.0 * Math.PI / 365.0) * (dayOfYear[i] - offset);
            }

            return dayAngle;
        }

        /// <summary>
        /// Port of pvlib.solarposition.equation_of_time_spencer71.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>equation of time [minutes] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] EquationOfTimeSpencer71(double[] dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear);
            var eot = new double[dayAngle.Length];
            for (var i = 0; i < dayAngle.Length; i++)
            {
                eot[i] = (1440.0 / (2.0 * Math.PI)) * (
                    0.0000075
                    + 0.001868 * Math.Cos(dayAngle[i])
                    - 0.032077 * Math.Sin(dayAngle[i])
                    - 0.014615 * Math.Cos(2.0 * dayAngle[i])
                    - 0.040849 * Math.Sin(2.0 * dayAngle[i]));
            }

            return eot;
        }

        /// <summary>
        /// Port of pvlib.solarposition.equation_of_time_pvcdrom.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>equation of time [minutes] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] EquationOfTimePvCdrom(double[] dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear, offset: 81);
            var eot = new double[dayAngle.Length];
            for (var i = 0; i < dayAngle.Length; i++)
            {
                eot[i] = 9.87 * Math.Sin(2.0 * dayAngle[i])
                         - 7.53 * Math.Cos(dayAngle[i])
                         - 1.5 * Math.Sin(dayAngle[i]);
            }

            return eot;
        }

        /// <summary>
        /// Port of pvlib.solarposition.declination_spencer71.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>solar declination [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] DeclinationSpencer71(double[] dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear);
            var declination = new double[dayAngle.Length];
            for (var i = 0; i < dayAngle.Length; i++)
            {
                var angle = dayAngle[i];
                declination[i] = 0.006918
                    - 0.399912 * Math.Cos(angle)
                    + 0.070257 * Math.Sin(angle)
                    - 0.006758 * Math.Cos(2.0 * angle)
                    + 0.000907 * Math.Sin(2.0 * angle)
                    - 0.002697 * Math.Cos(3.0 * angle)
                    + 0.00148 * Math.Sin(3.0 * angle);
            }

            return declination;
        }

        /// <summary>
        /// Port of pvlib.solarposition.declination_cooper69.
        /// </summary>
        /// <param name="dayOfYear">day of year [days] (array of double)</param>
        /// <returns>solar declination [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">dayOfYear is null.</exception>
        public static double[] DeclinationCooper69(double[] dayOfYear)
        {
            var dayAngle = CalcSimpleDayAngleArray(dayOfYear, offset: -284);

            var declination = new double[dayAngle.Length];
            for (var i = 0; i < dayAngle.Length; i++)
            {
                declination[i] = Math.PI / 180.0 * 23.45 * Math.Sin(dayAngle[i]);
            }

            return declination;
        }

        /// <summary>
        /// Port of pvlib.solarposition.hour_angle.
        /// </summary>
        /// <param name="location">site coordinates and fixed UTC offset [n/d] (Location)</param>
        /// <param name="times">timezone-aware timestamps [n/d] (array of DateTimeOffset)</param>
        /// <param name="equationOfTime">equation of time [minutes] (array of double)</param>
        /// <returns>solar hour angle [degrees] (array of double)</returns>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="ArgumentException">A timestamp offset differs from the location offset, or array lengths differ.</exception>
        public static double[] HourAngle(
            Location location,
            DateTimeOffset[] times,
            double[] equationOfTime)
        {
            ArgumentNullException.ThrowIfNull(location);
            ArgumentNullException.ThrowIfNull(times);
            ArgumentNullException.ThrowIfNull(equationOfTime);
            if (times.Length != equationOfTime.Length)
            {
                throw new ArgumentException("Times and equationOfTime must have the same length.");
            }

            var hourAngle = new double[times.Length];
            for (var i = 0; i < times.Length; i++)
            {
                if (times[i].Offset != location.UtcOffset)
                {
                    throw new ArgumentException(
                        "Every timestamp must use the UTC offset specified by location.",
                        nameof(times));
                }

                hourAngle[i] = 15.0 * (
                    times[i].TimeOfDay.TotalHours
                    - location.UtcOffset.TotalHours
                    - 12.0)
                    + location.Longitude
                    + equationOfTime[i] / 4.0;
            }

            return hourAngle;
        }

        /// <summary>
        /// Port of pvlib.solarposition.solar_zenith_analytical.
        /// </summary>
        /// <param name="latitude">latitude, positive north of equator [radians] (double)</param>
        /// <param name="hourAngle">solar hour angle [radians] (array of double)</param>
        /// <param name="declination">solar declination [radians] (array of double)</param>
        /// <returns>solar zenith angle [radians] (array of double)</returns>
        /// <exception cref="ArgumentNullException">An array argument is null.</exception>
        /// <exception cref="ArgumentException">Array lengths differ.</exception>
        public static double[] SolarZenith(
            double latitude,
            double[] hourAngle,
            double[] declination)
        {
            ArgumentNullException.ThrowIfNull(hourAngle);
            ArgumentNullException.ThrowIfNull(declination);
            if (hourAngle.Length != declination.Length)
            {
                throw new ArgumentException("hourAngle and declination must have the same length.");
            }

            var zenith = new double[hourAngle.Length];
            for (var i = 0; i < hourAngle.Length; i++)
            {
                zenith[i] = Math.Acos(
                    Math.Cos(declination[i]) * Math.Cos(latitude) * Math.Cos(hourAngle[i])
                    + Math.Sin(declination[i]) * Math.Sin(latitude));
            }

            return zenith;
        }

        /// <summary>
        /// Calculates solar position with a native ephemeris calculator ported from
        /// pvlib.solarposition.ephemeris. Accuracy is not guaranteed; the method is based
        /// on informal class materials by Grover Hughes at Sandia National Laboratories (1985)
        /// and is not reliable for dates far from the present.
        /// </summary>
        /// <param name="times">timestamps; the UTC offset gives the timezone [n/d] (array of DateTimeOffset)</param>
        /// <param name="latitude">latitude, positive north [degrees] (double)</param>
        /// <param name="longitude">longitude, positive east [degrees] (double)</param>
        /// <param name="pressure">ambient pressure [Pa] (double, default: 101325.0)</param>
        /// <param name="temperature">ambient temperature [degrees C] (double, default: 12.0)</param>
        /// <returns>SolarPositionResult: ApparentElevation, Elevation, Azimuth, ApparentZenith, and Zenith [degrees] (array of double); SolarTime [hours] (array of double); EquationOfTime [minutes] (array of double), null. Result type [n/d] (SolarPositionResult).</returns>
        /// <exception cref="ArgumentNullException">times is null.</exception>
        public static SolarPositionResult Ephemeris(
            DateTimeOffset[] times,
            double latitude,
            double longitude,
            double pressure = 101325.0,
            double temperature = 12.0) =>
            EphemerisCore(times, latitude, longitude, pressure, null, temperature, null);

        /// <summary>
        /// Calculates solar position with a native ephemeris calculator ported from
        /// pvlib.solarposition.ephemeris. Accuracy is not guaranteed; the method is based
        /// on informal class materials by Grover Hughes at Sandia National Laboratories (1985)
        /// and is not reliable for dates far from the present.
        /// </summary>
        /// <param name="times">timestamps; the UTC offset gives the timezone [n/d] (array of DateTimeOffset)</param>
        /// <param name="latitude">latitude, positive north [degrees] (double)</param>
        /// <param name="longitude">longitude, positive east [degrees] (double)</param>
        /// <param name="pressure">ambient pressure [Pa] (array of double)</param>
        /// <param name="temperature">ambient temperature [degrees C] (double, default: 12.0)</param>
        /// <returns>SolarPositionResult: ApparentElevation, Elevation, Azimuth, ApparentZenith, and Zenith [degrees] (array of double); SolarTime [hours] (array of double); EquationOfTime [minutes] (array of double), null. Result type [n/d] (SolarPositionResult).</returns>
        /// <exception cref="ArgumentNullException">times or pressure is null.</exception>
        /// <exception cref="ArgumentException">pressure does not have one value per timestamp.</exception>
        public static SolarPositionResult Ephemeris(
            DateTimeOffset[] times,
            double latitude,
            double longitude,
            double[] pressure,
            double temperature = 12.0)
        {
            ArgumentNullException.ThrowIfNull(pressure);
            return EphemerisCore(times, latitude, longitude, 0.0, pressure, temperature, null);
        }

        /// <summary>
        /// Calculates solar position with a native ephemeris calculator ported from
        /// pvlib.solarposition.ephemeris. Accuracy is not guaranteed; the method is based
        /// on informal class materials by Grover Hughes at Sandia National Laboratories (1985)
        /// and is not reliable for dates far from the present.
        /// </summary>
        /// <param name="times">timestamps; the UTC offset gives the timezone [n/d] (array of DateTimeOffset)</param>
        /// <param name="latitude">latitude, positive north [degrees] (double)</param>
        /// <param name="longitude">longitude, positive east [degrees] (double)</param>
        /// <param name="pressure">ambient pressure [Pa] (double)</param>
        /// <param name="temperature">ambient temperature [degrees C] (array of double)</param>
        /// <returns>SolarPositionResult: ApparentElevation, Elevation, Azimuth, ApparentZenith, and Zenith [degrees] (array of double); SolarTime [hours] (array of double); EquationOfTime [minutes] (array of double), null. Result type [n/d] (SolarPositionResult).</returns>
        /// <exception cref="ArgumentNullException">times or temperature is null.</exception>
        /// <exception cref="ArgumentException">temperature does not have one value per timestamp.</exception>
        public static SolarPositionResult Ephemeris(
            DateTimeOffset[] times,
            double latitude,
            double longitude,
            double pressure,
            double[] temperature)
        {
            ArgumentNullException.ThrowIfNull(temperature);
            return EphemerisCore(times, latitude, longitude, pressure, null, 0.0, temperature);
        }

        /// <summary>
        /// Calculates solar position with a native ephemeris calculator ported from
        /// pvlib.solarposition.ephemeris. Accuracy is not guaranteed; the method is based
        /// on informal class materials by Grover Hughes at Sandia National Laboratories (1985)
        /// and is not reliable for dates far from the present.
        /// </summary>
        /// <param name="times">timestamps; the UTC offset gives the timezone [n/d] (array of DateTimeOffset)</param>
        /// <param name="latitude">latitude, positive north [degrees] (double)</param>
        /// <param name="longitude">longitude, positive east [degrees] (double)</param>
        /// <param name="pressure">ambient pressure [Pa] (array of double)</param>
        /// <param name="temperature">ambient temperature [degrees C] (array of double)</param>
        /// <returns>SolarPositionResult: ApparentElevation, Elevation, Azimuth, ApparentZenith, and Zenith [degrees] (array of double); SolarTime [hours] (array of double); EquationOfTime [minutes] (array of double), null. Result type [n/d] (SolarPositionResult).</returns>
        /// <exception cref="ArgumentNullException">times, pressure, or temperature is null.</exception>
        /// <exception cref="ArgumentException">pressure or temperature does not have one value per timestamp.</exception>
        public static SolarPositionResult Ephemeris(
            DateTimeOffset[] times,
            double latitude,
            double longitude,
            double[] pressure,
            double[] temperature)
        {
            ArgumentNullException.ThrowIfNull(pressure);
            ArgumentNullException.ThrowIfNull(temperature);
            return EphemerisCore(times, latitude, longitude, 0.0, pressure, 0.0, temperature);
        }

        /// <summary>
        /// Ports pvlib.solarposition.ephemeris using UTC instants and aligned optional
        /// pressure and temperature arrays.
        /// </summary>
        /// <param name="times">timestamps in their supplied offsets [n/d] (array of DateTimeOffset)</param>
        /// <param name="latitude">latitude, positive north [degrees] (double)</param>
        /// <param name="longitude">longitude, positive east [degrees] (double)</param>
        /// <param name="pressure">ambient pressure [Pa] (double)</param>
        /// <param name="pressureArray">ambient pressure [Pa] (array of double)</param>
        /// <param name="temperature">ambient temperature [degrees C] (double)</param>
        /// <param name="temperatureArray">ambient temperature [degrees C] (array of double)</param>
        /// <returns>Solar position result: ApparentElevation, Elevation, Azimuth, ApparentZenith, and Zenith [degrees]; SolarTime [hours]; EquationOfTime is null.</returns>
        /// <exception cref="ArgumentNullException">times or a supplied parameter array is null.</exception>
        /// <exception cref="ArgumentException">A supplied parameter array does not have one value per timestamp.</exception>
        private static SolarPositionResult EphemerisCore(
            DateTimeOffset[] times,
            double latitude,
            double longitude,
            double pressure,
            double[]? pressureArray,
            double temperature,
            double[]? temperatureArray)
        {
            ArgumentNullException.ThrowIfNull(times);
            if (pressureArray is not null && pressureArray.Length != times.Length)
            {
                throw new ArgumentException("pressure must have one value per timestamp.", nameof(pressureArray));
            }

            if (temperatureArray is not null && temperatureArray.Length != times.Length)
            {
                throw new ArgumentException("temperature must have one value per timestamp.", nameof(temperatureArray));
            }

            var apparentElevation = new double[times.Length];
            var elevation = new double[times.Length];
            var azimuth = new double[times.Length];
            var apparentZenith = new double[times.Length];
            var zenith = new double[times.Length];
            var solarTime = new double[times.Length];
            var latitudeRadians = latitude * Math.PI / 180.0;
            var longitudeForAlgorithm = -longitude;
            const double aberration = 20.0 / 3600.0;

            for (var i = 0; i < times.Length; i++)
            {
                var utc = times[i].UtcDateTime;
                var year = utc.Year;
                var dayOfYear = utc.DayOfYear;
                var decimalHour = utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0
                    + (utc.Ticks % TimeSpan.TicksPerMillisecond / 10)
                    / (double)TimeSpan.TicksPerHour;
                var yearFrom1900 = year - 1900.0;
                var yearBegin = 365.0 * yearFrom1900 + Math.Floor((yearFrom1900 - 1.0) / 4.0) - 0.5;
                var zeroDate = yearBegin + dayOfYear;
                var t = zeroDate / 36525.0;

                var gmst0 = 6.0 / 24.0 + 38.0 / 1440.0
                    + (45.836 + 8640184.542 * t + 0.0929 * t * t) / 86400.0;
                gmst0 = 360.0 * (gmst0 - Math.Floor(gmst0));
                var gmst = NumpyModulo(gmst0 + 360.0 * (1.0027379093 * decimalHour / 24.0), 360.0);
                var localApparentSiderealTime = NumpyModulo(360.0 + gmst - longitudeForAlgorithm, 360.0);

                var epochDate = zeroDate + decimalHour / 24.0;
                var t1 = epochDate / 36525.0;
                var obliquity = (23.452294 - 0.0130125 * t1 - 1.64e-6 * t1 * t1
                    + 5.03e-7 * t1 * t1 * t1) * Math.PI / 180.0;
                var meanLongitudePerigee = 281.22083 + 4.70684e-5 * epochDate
                    + 0.000453 * t1 * t1 + 3.0e-6 * t1 * t1 * t1;
                var meanAnomaly = NumpyModulo(358.47583 + 0.985600267 * epochDate
                    - 0.00015 * t1 * t1 - 3.0e-6 * t1 * t1 * t1, 360.0);
                var eccentricity = 0.01675104 - 4.18e-5 * t1 - 1.26e-7 * t1 * t1;
                var eccentricAnomaly = meanAnomaly;
                var previousEccentricAnomaly = 0.0;
                while (Math.Abs(eccentricAnomaly - previousEccentricAnomaly) > 0.0001)
                {
                    previousEccentricAnomaly = eccentricAnomaly;
                    eccentricAnomaly = meanAnomaly
                        + eccentricity * 180.0 / Math.PI * Math.Sin(eccentricAnomaly * Math.PI / 180.0);
                }

                var trueAnomaly = 2.0 * NumpyModulo(
                    Math.Atan2(Math.Sqrt((1.0 + eccentricity) / (1.0 - eccentricity))
                        * Math.Tan(eccentricAnomaly * Math.PI / 360.0), 1.0) * 180.0 / Math.PI,
                    360.0);
                var eclipticLongitude = NumpyModulo(meanLongitudePerigee + trueAnomaly, 360.0) - aberration;
                var eclipticLongitudeRadians = eclipticLongitude * Math.PI / 180.0;
                var declinationRadians = Math.Asin(Math.Sin(obliquity) * Math.Sin(eclipticLongitudeRadians));
                var rightAscension = Math.Atan2(
                    Math.Cos(obliquity) * Math.Sin(eclipticLongitudeRadians),
                    Math.Cos(eclipticLongitudeRadians)) * 180.0 / Math.PI;
                var hourAngle = localApparentSiderealTime - rightAscension;
                var hourAngleRadians = hourAngle * Math.PI / 180.0;
                hourAngle -= 360.0 * (Math.Abs(hourAngle) > 180.0 ? 1.0 : 0.0);

                var sunAzimuth = Math.Atan2(
                    -Math.Sin(hourAngleRadians),
                    Math.Cos(latitudeRadians) * Math.Tan(declinationRadians)
                    - Math.Sin(latitudeRadians) * Math.Cos(hourAngleRadians)) * 180.0 / Math.PI;
                if (sunAzimuth < 0.0)
                {
                    sunAzimuth += 360.0;
                }

                var sunElevation = Math.Asin(
                    Math.Cos(latitudeRadians) * Math.Cos(declinationRadians) * Math.Cos(hourAngleRadians)
                    + Math.Sin(latitudeRadians) * Math.Sin(declinationRadians)) * 180.0 / Math.PI;
                var tangentElevation = Math.Tan(sunElevation * Math.PI / 180.0);
                var refraction = 0.0;
                if (sunElevation > 5.0 && sunElevation <= 85.0)
                {
                    refraction = 58.1 / tangentElevation - 0.07 / Math.Pow(tangentElevation, 3.0)
                        + 8.6e-5 / Math.Pow(tangentElevation, 5.0);
                }
                else if (sunElevation > -0.575 && sunElevation <= 5.0)
                {
                    refraction = sunElevation * (-518.2 + sunElevation
                        * (103.4 + sunElevation * (-12.79 + sunElevation * 0.711))) + 1735.0;
                }
                else if (sunElevation > -1.0 && sunElevation <= -0.575)
                {
                    refraction = -20.774 / tangentElevation;
                }

                var localPressure = pressureArray is null ? pressure : pressureArray[i];
                var localTemperature = temperatureArray is null ? temperature : temperatureArray[i];
                refraction *= (283.0 / (273.0 + localTemperature))
                    * (localPressure / 101325.0) / 3600.0;
                var apparentSunElevation = sunElevation + refraction;

                elevation[i] = sunElevation;
                apparentElevation[i] = apparentSunElevation;
                azimuth[i] = sunAzimuth;
                zenith[i] = 90.0 - sunElevation;
                apparentZenith[i] = 90.0 - apparentSunElevation;
                solarTime[i] = (180.0 + hourAngle) / 15.0;
            }

            return new SolarPositionResult
            {
                Times = (DateTimeOffset[])times.Clone(),
                ApparentElevation = apparentElevation,
                Elevation = elevation,
                Azimuth = azimuth,
                ApparentZenith = apparentZenith,
                Zenith = zenith,
                SolarTime = solarTime
            };
        }

        /// <summary>
        /// Returns the remainder using NumPy's modulo convention.
        /// </summary>
        /// <param name="value">value to reduce [n/d] (double)</param>
        /// <param name="modulus">positive modulus [n/d] (double)</param>
        /// <returns>remainder in the range from zero up to the modulus [n/d] (double)</returns>
        private static double NumpyModulo(double value, double modulus) =>
            value - Math.Floor(value / modulus) * modulus;
    }
}
