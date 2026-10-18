import json
from pathlib import Path

import numpy as np
import pandas as pd
import pvlib
from pvlib import solarposition


def write_fixture(name, cases):
    output_path = (
        Path(__file__).resolve().parents[3]
        / "tests"
        / "pv.Tests"
        / "Fixtures"
        / f"{name}.json"
    )
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump(
            {"pvlib_version": pvlib.__version__, "cases": cases},
            output_file,
            indent=2,
            allow_nan=False,
        )
        output_file.write("\n")


def nullable_values(values):
    return [None if np.isnan(value) else float(value) for value in values]


def main():
    day_cases = []
    edge_days = np.array([0, 1, 59.5, 60, 81, 172.25, 266, 355, 365, 366, np.nan])
    for name, days, offset in [
        ("leap_year", np.arange(1, 367), 1),
        ("fractional_days_and_nan", edge_days, 1),
        ("zero_offset", edge_days, 0),
        ("pvcdrom_offset", edge_days, 81),
        ("cooper_offset", edge_days, -284),
        ("empty", np.array([]), 1),
    ]:
        expected = {
            "day_angle": solarposition._calculate_simple_day_angle(days, offset),
            "equation_of_time_spencer71": solarposition.equation_of_time_spencer71(days),
            "equation_of_time_pvcdrom": solarposition.equation_of_time_pvcdrom(days),
            "declination_spencer71": solarposition.declination_spencer71(days),
            "declination_cooper69": solarposition.declination_cooper69(days),
        }
        day_cases.append(
            {
                "name": name,
                "inputs": {"day_of_year": nullable_values(days), "offset": offset},
                "expected": {
                    key: nullable_values(values) for key, values in expected.items()
                },
            }
        )
    write_fixture("solarposition_day_helpers", day_cases)

    hour_cases = []
    for name, offset, longitude in [
        ("utc", "+00:00", 0.0),
        ("positive_fractional_offset", "+05:30", 72.6),
        ("negative_fractional_offset", "-03:30", -52.7),
        ("east_date_line", "+14:00", 180.0),
        ("west_date_line", "-12:00", -180.0),
    ]:
        times = pd.DatetimeIndex(
            [
                f"2024-02-29T00:00:00{offset}",
                f"2024-02-29T08:17:23.125000{offset}",
                f"2024-02-29T12:00:00{offset}",
                f"2024-02-29T23:59:59.999000{offset}",
                f"2024-03-01T00:00:00{offset}",
            ]
        )
        equation_of_time = np.array([-14.0, 0.0, 16.0, 4.5, np.nan])
        hour_cases.append(
            {
                "name": name,
                "inputs": {
                    "times": [time.isoformat() for time in times],
                    "longitude": longitude,
                    "equation_of_time": nullable_values(equation_of_time),
                },
                "expected": nullable_values(
                    solarposition.hour_angle(times, longitude, equation_of_time)
                ),
            }
        )
    write_fixture("hour_angle", hour_cases)


if __name__ == "__main__":
    main()
