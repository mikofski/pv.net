import json
from pathlib import Path

import numpy as np
import pandas as pd
import pvlib
from pvlib import spa


def values(result):
    return np.asarray(result).tolist()


def main():
    if pvlib.__version__ != "0.16.1":
        raise RuntimeError("Fixtures require pvlib 0.16.1")

    times = pd.date_range("2003-10-17 12:30:30", periods=1, freq="D", tz="MST")
    unixtimes = np.array([timestamp.timestamp() for timestamp in times])
    lat, lon, elev = 39.742476, -105.1786, 1830.14
    pressure, temp, delta_t, atmos_refract = 820, 11, 67.0, 0.5667
    jd, jde, jc, jce, jme = 2452930.312847, 2452930.313623, 0.0379277986858, 0.037927819916852, 0.003792781991685
    l, b, r = 24.0182616917, -0.0001011219, 0.9965422974
    theta, beta = 204.0182616917, 0.0001011219
    x0, x1, x2, x3, x4 = 17185.861179, 1722.893218, 18234.075703, 18420.071012, 51.686951
    dpsi, deps = -0.00399840, 0.00166657
    epsilon0, epsilon = 84379.672625, 23.440465
    dtau, lamd = -0.005711, 204.0085519281
    v0, v, alpha, delta = 318.515579, 318.511910, 202.227408, -9.31434
    hour_angle, xi, d_alpha = 11.10590, 0.002451, -0.000369
    alpha_prime, delta_prime, hour_angle_prime = 202.22704, -9.316179, 11.10627
    elevation0, de, elevation = 39.872046, 0.016332, 39.888378
    zenith, azimuth_astronomers, azimuth = 50.11162, 14.340241, 194.340241

    nutation = np.empty(2)
    spa.longitude_obliquity_nutation(jce, x0, x1, x2, x3, x4, nutation)
    normal = spa.solar_position(unixtimes, lat, lon, elev, pressure, temp, delta_t, atmos_refract)
    sst = spa.solar_position(unixtimes, lat, lon, elev, pressure, temp, delta_t, atmos_refract, sst=True)
    esd = spa.solar_position(unixtimes, lat, lon, elev, pressure, temp, delta_t, atmos_refract, esd=True)

    expected = {
        "sumMultCosAddMult": spa.sum_mult_cos_add_mult(spa.L0, jme),
        "sumMultCosAddMultExample": spa.sum_mult_cos_add_mult(
            np.array([[1.0, 0.0, 0.0], [2.0, np.pi, 0.0]]), jme),
        "heliocentricLongitude": spa.heliocentric_longitude(jme),
        "heliocentricLatitude": spa.heliocentric_latitude(jme),
        "heliocentricRadiusVector": spa.heliocentric_radius_vector(jme),
        "geocentricLongitude": spa.geocentric_longitude(l),
        "geocentricLatitude": spa.geocentric_latitude(b),
        "meanElongation": spa.mean_elongation(jce),
        "meanAnomalySun": spa.mean_anomaly_sun(jce),
        "meanAnomalyMoon": spa.mean_anomaly_moon(jce),
        "moonArgumentLatitude": spa.moon_argument_latitude(jce),
        "moonAscendingLongitude": spa.moon_ascending_longitude(jce),
        "longitudeObliquityNutation": values(nutation),
        "meanEclipticObliquity": spa.mean_ecliptic_obliquity(jme),
        "trueEclipticObliquity": spa.true_ecliptic_obliquity(epsilon0, deps),
        "aberrationCorrection": spa.aberration_correction(r),
        "apparentSunLongitude": spa.apparent_sun_longitude(theta, dpsi, dtau),
        "meanSiderealTime": spa.mean_sidereal_time(jd, jc),
        "apparentSiderealTime": spa.apparent_sidereal_time(v0, dpsi, epsilon),
        "geocentricSunRightAscension": spa.geocentric_sun_right_ascension(lamd, epsilon, beta),
        "geocentricSunDeclination": spa.geocentric_sun_declination(lamd, epsilon, beta),
        "localHourAngle": spa.local_hour_angle(v, lon, alpha),
        "equatorialHorizontalParallax": spa.equatorial_horizontal_parallax(r),
        "uterm": spa.uterm(lat),
        "xterm": spa.xterm(spa.uterm(lat), lat, elev),
        "yterm": spa.yterm(spa.uterm(lat), lat, elev),
        "parallaxSunRightAscension": spa.parallax_sun_right_ascension(
            spa.xterm(spa.uterm(lat), lat, elev), xi, hour_angle, delta),
        "topocentricSunRightAscension": spa.topocentric_sun_right_ascension(alpha, d_alpha),
        "topocentricSunDeclination": spa.topocentric_sun_declination(
            delta, spa.xterm(spa.uterm(lat), lat, elev), spa.yterm(spa.uterm(lat), lat, elev),
            xi, d_alpha, hour_angle),
        "topocentricLocalHourAngle": spa.topocentric_local_hour_angle(hour_angle, d_alpha),
        "topocentricElevationAngleWithoutAtmosphere": spa.topocentric_elevation_angle_without_atmosphere(
            lat, delta_prime, hour_angle_prime),
        "atmosphericRefractionCorrection": spa.atmospheric_refraction_correction(
            pressure, temp, elevation0, atmos_refract),
        "topocentricElevationAngle": spa.topocentric_elevation_angle(elevation0, de),
        "topocentricZenithAngle": spa.topocentric_zenith_angle(elevation),
        "topocentricAstronomersAzimuth": spa.topocentric_astronomers_azimuth(
            hour_angle_prime, delta_prime, lat),
        "topocentricAzimuthAngle": spa.topocentric_azimuth_angle(azimuth_astronomers),
        "sunMeanLongitude": spa.sun_mean_longitude(jme),
        "equationOfTime": spa.equation_of_time(spa.sun_mean_longitude(jme), alpha, dpsi, epsilon),
    }

    cases = [{
        "name": "spa_chain",
        "inputs": {
            "unixtimes": unixtimes.tolist(), "lat": lat, "lon": lon, "elev": elev,
            "pressure": pressure, "temp": temp, "deltaT": delta_t,
            "atmosRefract": atmos_refract, "jme": jme, "jce": jce,
            "jd": jd, "jc": jc, "l": l, "b": b, "r": r, "theta": theta,
            "beta": beta, "x0": x0, "x1": x1, "x2": x2, "x3": x3, "x4": x4,
            "dpsi": dpsi, "deps": deps, "epsilon0": epsilon0, "epsilon": epsilon,
            "dtau": dtau, "lamd": lamd, "v0": v0, "v": v, "alpha": alpha,
            "delta": delta, "hourAngle": hour_angle, "xi": xi,
            "dAlpha": d_alpha, "alphaPrime": alpha_prime, "deltaPrime": delta_prime,
            "hourAnglePrime": hour_angle_prime, "elevation0": elevation0,
            "de": de, "elevation": elevation, "zenith": zenith,
            "azimuthAstronomers": azimuth_astronomers, "azimuth": azimuth,
            "sumCoefficients": [[1.0, 0.0, 0.0], [2.0, np.pi, 0.0]],
        },
        "expected": {
            **expected,
            "solarPosition": [values(row) for row in normal],
            "solarPositionSst": [values(row) for row in sst],
            "solarPositionEsd": values(esd[0]),
        },
    }]

    year_times = pd.date_range("2015-01-01 00:00", "2015-12-31 23:00", freq="h", tz="UTC")
    year_unix = np.array([timestamp.timestamp() for timestamp in year_times])
    year_delta_t = spa.calculate_deltat(year_times.year.to_numpy(), year_times.month.to_numpy())
    site = (37.8, -122.25, 100, 1013.25, 25)
    atmos_refract_2015 = 0.5667
    year_sst = spa.solar_position(year_unix, *site, year_delta_t, atmos_refract_2015, sst=True)
    year_esd = spa.solar_position(year_unix, *site, year_delta_t, atmos_refract_2015, esd=True)
    cases.extend([
        {
            "name": "hourly_2015_sst",
            "inputs": {
                "start": year_times[0].isoformat(), "count": len(year_times), "stepSeconds": 3600,
                "lat": site[0], "lon": site[1], "elev": site[2], "pressure": site[3],
                "temp": site[4], "atmosRefract": atmos_refract_2015,
            },
            "expected": {
                "apparentSiderealTime": values(year_sst[0]),
                "rightAscension": values(year_sst[1]),
                "declination": values(year_sst[2]),
            },
        },
        {
            "name": "hourly_2015_esd",
            "inputs": {
                "start": year_times[0].isoformat(), "count": len(year_times), "stepSeconds": 3600,
                "deltaT": values(year_delta_t),
            },
            "expected": {"radiusVector": values(year_esd[0])},
        },
    ])

    output = Path(__file__).resolve().parents[2] / "pv.Tests" / "Fixtures" / "spa_solar_position.json"
    with output.open("w", encoding="utf-8") as fixture:
        json.dump({"pvlib_version": pvlib.__version__, "cases": cases}, fixture, indent=2, allow_nan=False)
        fixture.write("\n")


if __name__ == "__main__":
    main()
