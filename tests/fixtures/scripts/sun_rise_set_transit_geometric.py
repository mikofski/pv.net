import json
from pathlib import Path

import numpy as np
import pandas as pd
import pvlib
from pvlib.solarposition import (
    _hour_angle_to_hours,
    _local_times_from_hours_since_midnight,
    _times_to_hours_after_local_midnight,
    declination_spencer71,
    equation_of_time_spencer71,
    sun_rise_set_transit_geometric,
)


def timestamps(values):
    return [value.isoformat() for value in values]


def events_as_json(events):
    return {
        "sunrise": timestamps(events[0]),
        "sunset": timestamps(events[1]),
        "transit": timestamps(events[2]),
    }


def main():
    cases = []
    times = pd.DatetimeIndex(["2015-01-02", "2015-08-02"]).tz_localize("MST")
    times_utc = times.tz_convert("UTC")
    day_of_year = times_utc.dayofyear
    latitude = 39.742476
    longitude = -105.1786
    equation_of_time = equation_of_time_spencer71(day_of_year)
    declination = declination_spencer71(day_of_year)
    events = sun_rise_set_transit_geometric(
        times, latitude, longitude, declination, equation_of_time
    )
    published_spa = {
        "sunrise": timestamps(
            pd.DatetimeIndex(
                ["2015-01-02 07:21:55", "2015-08-02 05:00:27"]
            ).tz_localize("MST")
        ),
        "sunset": timestamps(
            pd.DatetimeIndex(
                ["2015-01-02 16:47:43", "2015-08-02 19:13:58"]
            ).tz_localize("MST")
        ),
        "transit": timestamps(
            pd.DatetimeIndex(
                ["2015-01-02 12:04:45", "2015-08-02 12:06:58"]
            ).tz_localize("MST")
        ),
    }

    helper_times = pd.DatetimeIndex(
        ["2015-01-02 12:30:00", "2015-08-02 03:45:00"]
    ).tz_localize("MST")
    helper_hour_angle = np.array([-90.0, 45.0])
    helper_equation_of_time = np.array([12.0, -6.0])
    helper_hours = _hour_angle_to_hours(
        helper_times, helper_hour_angle, longitude, helper_equation_of_time
    )
    cases.append(
        {
            "name": "golden_mst",
            "inputs": {
                "times": timestamps(times),
                "latitude": latitude,
                "longitude": longitude,
                "day_of_year_utc": day_of_year.tolist(),
                "declination": declination.tolist(),
                "equation_of_time": equation_of_time.tolist(),
                "helper_times": timestamps(helper_times),
                "helper_hour_angle": helper_hour_angle.tolist(),
                "helper_equation_of_time": helper_equation_of_time.tolist(),
            },
            "expected": {
                **events_as_json(events),
                "published_spa": published_spa,
                "times_to_hours_after_local_midnight": _times_to_hours_after_local_midnight(
                    helper_times
                ).tolist(),
                "hour_angle_to_hours": helper_hours.tolist(),
                "local_times_from_hours_since_midnight": timestamps(
                    _local_times_from_hours_since_midnight(helper_times, helper_hours)
                ),
            },
        }
    )

    for timezone in ["UTC", "US/Eastern"]:
        times_ns = pd.date_range(
            start="2019-01-01", freq="h", periods=24, tz=timezone, unit="ns"
        )
        times_us = pd.date_range(
            start="2019-01-01", freq="h", periods=24, tz=timezone, unit="us"
        )
        zeroes = np.zeros(len(times_ns))
        events_ns = sun_rise_set_transit_geometric(
            times_ns, 40, -80, zeroes, zeroes
        )
        events_us = sun_rise_set_transit_geometric(
            times_us, 40, -80, zeroes, zeroes
        )
        cases.append(
            {
                "name": f"microsecond_index_{timezone.lower().replace('/', '_')}",
                "inputs": {
                    "times": timestamps(times_ns),
                    "latitude": 40.0,
                    "longitude": -80.0,
                    "declination": zeroes.tolist(),
                    "equation_of_time": zeroes.tolist(),
                },
                "expected": {
                    "ns": events_as_json(tuple([values[:1] for values in events_ns])),
                    "us": events_as_json(tuple([values[:1] for values in events_us])),
                },
            }
        )

    fixture = {"pvlib_version": pvlib.__version__, "cases": cases}
    output_path = (
        Path(__file__).resolve().parents[3]
        / "tests"
        / "pv.Tests"
        / "Fixtures"
        / "sun_rise_set_transit_geometric.json"
    )
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump(fixture, output_file, indent=2)
        output_file.write("\n")


if __name__ == "__main__":
    main()
