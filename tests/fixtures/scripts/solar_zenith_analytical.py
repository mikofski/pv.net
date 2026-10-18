import json
from pathlib import Path

import numpy as np
import pandas as pd
import pvlib
from pvlib.solarposition import (
    declination_cooper69,
    declination_spencer71,
    equation_of_time_pvcdrom,
    equation_of_time_spencer71,
    hour_angle,
    solar_zenith_analytical,
)


def main():
    times = pd.date_range(
        start="1/1/2015 0:00",
        end="12/31/2015 23:00",
        freq="h",
    ).tz_localize("Etc/GMT+8")
    times_utc = times.tz_convert("UTC")
    day_of_year_utc = times_utc.dayofyear.to_numpy()
    latitude = 37.8
    longitude = -122.25
    latitude_radians = np.deg2rad(latitude)
    spa_output = pvlib.solarposition.spa_python(times, latitude, longitude, 100)
    spa_zenith = np.deg2rad(spa_output["zenith"].to_numpy())

    eot_spencer71 = equation_of_time_spencer71(day_of_year_utc)
    hour_angle_spencer71 = np.deg2rad(
        hour_angle(times, longitude, eot_spencer71)
    )
    declination_spencer71_values = declination_spencer71(day_of_year_utc)
    zenith_spencer71 = solar_zenith_analytical(
        latitude_radians, hour_angle_spencer71, declination_spencer71_values
    )

    eot_pvcdrom = equation_of_time_pvcdrom(day_of_year_utc)
    hour_angle_pvcdrom = np.deg2rad(hour_angle(times, longitude, eot_pvcdrom))
    declination_cooper69_values = declination_cooper69(day_of_year_utc)
    zenith_pvcdrom_cooper69 = solar_zenith_analytical(
        latitude_radians, hour_angle_pvcdrom, declination_cooper69_values
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
    boundary_zenith = solar_zenith_analytical(*test_angles.T)

    fixture = {
        "pvlib_version": pvlib.__version__,
        "cases": [
            {
                "name": "pvlib_test_analytical_zenith",
                "inputs": {
                    "times": [timestamp.isoformat() for timestamp in times],
                    "day_of_year_utc": day_of_year_utc.tolist(),
                    "latitude": latitude,
                    "longitude": longitude,
                    "altitude": 100.0,
                    "test_angles": test_angles.tolist(),
                },
                "expected": {
                    "spa_zenith": spa_zenith.tolist(),
                    "hour_angle_spencer71": hour_angle_spencer71.tolist(),
                    "declination_spencer71": declination_spencer71_values.tolist(),
                    "zenith_spencer71": zenith_spencer71.tolist(),
                    "hour_angle_pvcdrom": hour_angle_pvcdrom.tolist(),
                    "declination_cooper69": declination_cooper69_values.tolist(),
                    "zenith_pvcdrom_cooper69": zenith_pvcdrom_cooper69.tolist(),
                    "boundary_zenith": boundary_zenith.tolist(),
                },
            }
        ],
    }

    output_path = (
        Path(__file__).resolve().parents[3]
        / "tests"
        / "pv.Tests"
        / "Fixtures"
        / "solar_zenith_analytical.json"
    )
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump(fixture, output_file, indent=2, allow_nan=True)
        output_file.write("\n")


if __name__ == "__main__":
    main()
