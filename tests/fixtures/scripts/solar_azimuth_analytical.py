import json
from pathlib import Path

import numpy as np
import pandas as pd
import pvlib
from pvlib import solarposition


def as_json_array(values):
    return [
        None if np.isnan(value) else float(value)
        for value in np.asarray(values).reshape(-1)
    ]


def main():
    times = pd.date_range(
        start="1/1/2015 0:00",
        end="12/31/2015 23:00",
        freq="h",
    ).tz_localize("Etc/GMT+8")
    times_utc = times.tz_convert("UTC")
    latitude = 37.8
    longitude = -122.25
    latitude_radians = np.radians(latitude)
    spa = solarposition.spa_python(times, latitude, longitude, 100)

    cases = []
    for name, equation_of_time, declination in [
        (
            "spencer71",
            solarposition.equation_of_time_spencer71(times_utc.dayofyear),
            solarposition.declination_spencer71(times_utc.dayofyear),
        ),
        (
            "pvcdrom_cooper69",
            solarposition.equation_of_time_pvcdrom(times_utc.dayofyear),
            solarposition.declination_cooper69(times_utc.dayofyear),
        ),
    ]:
        hour_angle = np.radians(
            solarposition.hour_angle(times, longitude, equation_of_time)
        )
        zenith = solarposition.solar_zenith_analytical(
            latitude_radians, hour_angle, declination
        )
        azimuth = solarposition.solar_azimuth_analytical(
            latitude_radians, hour_angle, declination, zenith
        )
        cases.append(
            {
                "name": name,
                "inputs": {
                    "latitude": as_json_array(
                        np.full(hour_angle.shape, latitude_radians)
                    ),
                    "hour_angle": as_json_array(hour_angle),
                    "declination": as_json_array(declination),
                    "zenith": as_json_array(zenith),
                },
                "expected": {
                    "azimuth": as_json_array(azimuth),
                    "spa_azimuth": as_json_array(
                        np.radians(spa["azimuth"].to_numpy())
                    ),
                    "spa_zenith": as_json_array(
                        np.radians(spa["zenith"].to_numpy())
                    ),
                },
            }
        )

    test_angles = np.radians(
        np.array(
            [
                [0.0, -180.0, -20.0],
                [0.0, 0.0, -5.0],
                [0.0, 0.0, 0.0],
                [0.0, 0.0, 15.0],
                [0.0, 180.0, 20.0],
                [30.0, 0.0, -20.0],
                [30.0, 0.0, -5.0],
                [30.0, 0.0, 0.0],
                [30.0, 180.0, 5.0],
                [30.0, 0.0, 10.0],
                [-30.0, 0.0, -20.0],
                [-30.0, 0.0, -15.0],
                [-30.0, 0.0, 0.0],
                [-30.0, -180.0, 5.0],
                [-30.0, 180.0, 10.0],
            ]
        )
    )
    boundary_latitude, boundary_hour_angle, boundary_declination = test_angles.T
    boundary_zenith = solarposition.solar_zenith_analytical(
        boundary_latitude, boundary_hour_angle, boundary_declination
    )
    boundary_azimuth = solarposition.solar_azimuth_analytical(
        boundary_latitude,
        boundary_hour_angle,
        boundary_declination,
        boundary_zenith,
    )
    cases.append(
        {
            "name": "test_angles_boundary",
            "inputs": {
                "latitude": as_json_array(boundary_latitude),
                "hour_angle": as_json_array(boundary_hour_angle),
                "declination": as_json_array(boundary_declination),
                "zenith": as_json_array(boundary_zenith),
            },
            "expected": {
                "azimuth": as_json_array(boundary_azimuth),
                "zenith": as_json_array(boundary_zenith),
            },
        }
    )

    fixture = {"pvlib_version": pvlib.__version__, "cases": cases}
    output_path = (
        Path(__file__).resolve().parents[2]
        / "pv.Tests"
        / "Fixtures"
        / "solar_azimuth_analytical.json"
    )
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump(fixture, output_file, indent=2, allow_nan=False)
        output_file.write("\n")


if __name__ == "__main__":
    main()
