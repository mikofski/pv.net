import json
from pathlib import Path

import pandas as pd
import pvlib
from pvlib.solarposition import sun_rise_set_transit_spa


def timestamp_or_none(value):
    if pd.isna(value):
        return None
    return value.round("s").isoformat()


def main():
    cases = []
    for name, timestamp in [
        ("polar_day", "2024-06-21T12:00:00+00:00"),
        ("polar_night", "2024-12-21T12:00:00+00:00"),
    ]:
        times = pd.DatetimeIndex([timestamp])
        events = sun_rise_set_transit_spa(times, latitude=90, longitude=0, delta_t=67.0)
        event = events.iloc[0]
        cases.append(
            {
                "name": name,
                "inputs": {"timestamp": timestamp, "latitude": 90.0, "longitude": 0.0},
                "expected": {
                    "sunrise": timestamp_or_none(event["sunrise"]),
                    "sunset": timestamp_or_none(event["sunset"]),
                    "transit": timestamp_or_none(event["transit"]),
                },
            }
        )

    fixture = {"pvlib_version": pvlib.__version__, "cases": cases}
    output_path = (
        Path(__file__).resolve().parents[3]
        / "tests"
        / "pv.Tests"
        / "Fixtures"
        / "sun_rise_set_transit_spa_polar.json"
    )
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump(fixture, output_file, indent=2)
        output_file.write("\n")


if __name__ == "__main__":
    main()
