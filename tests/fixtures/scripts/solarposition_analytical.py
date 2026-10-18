import json
from pathlib import Path

import numpy as np
import pandas as pd
import pvlib
from pvlib.solarposition import (
    _calculate_simple_day_angle,
    declination_cooper69,
    declination_spencer71,
    equation_of_time_pvcdrom,
    equation_of_time_spencer71,
    hour_angle,
    solar_zenith_analytical,
)


def main():
    day_of_year = np.arange(1, 367)
    times = pd.DatetimeIndex(
        [
            "2024-02-29T08:17:00+05:30",
            "2024-02-29T11:43:00+05:30",
            "2024-03-01T17:05:00+05:30",
        ]
    )
    latitude = 23.4
    longitude = 72.6
    eot = equation_of_time_spencer71(day_of_year)[[59, 59, 60]]
    hour_angles = hour_angle(times, longitude, eot)
    declinations = declination_spencer71(np.array([60, 60, 61]))
    zenith = solar_zenith_analytical(
        np.radians(latitude), np.radians(hour_angles), declinations
    )
    expected = {
        "day_angle": _calculate_simple_day_angle(day_of_year),
        "equation_of_time_spencer71": equation_of_time_spencer71(day_of_year),
        "equation_of_time_pvcdrom": equation_of_time_pvcdrom(day_of_year),
        "declination_spencer71": declination_spencer71(day_of_year),
        "declination_cooper69": declination_cooper69(day_of_year),
        "hour_angle": hour_angles,
        "solar_zenith": zenith,
    }

    fixture = {
        "pvlib_version": pvlib.__version__,
        "cases": [
            {
                "name": "leap_year_and_non_hourly_fixed_offset",
                "inputs": {
                    "day_of_year": day_of_year.tolist(),
                    "times": [timestamp.isoformat() for timestamp in times],
                    "latitude": latitude,
                    "longitude": longitude,
                    "altitude": 12.0,
                    "equation_of_time_for_times": eot.tolist(),
                },
                "expected": {name: values.tolist() for name, values in expected.items()},
            }
        ],
    }
    output_path = (
        Path(__file__).resolve().parents[3]
        / "tests"
        / "pv.Tests"
        / "Fixtures"
        / "solarposition_analytical.json"
    )
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump(fixture, output_file, indent=2, allow_nan=True)
        output_file.write("\n")


if __name__ == "__main__":
    main()
