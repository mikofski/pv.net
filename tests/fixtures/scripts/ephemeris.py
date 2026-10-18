import json
from pathlib import Path

import pandas as pd
import pvlib
from pvlib.solarposition import ephemeris


def main():
    if pvlib.__version__ != "0.16.1":
        raise RuntimeError("Fixtures require pvlib 0.16.1")

    cases = []

    def add_case(name, times, latitude, longitude, pressure, temperature):
        pressure_input = (
            pd.Series(pressure, index=times)
            if isinstance(pressure, list)
            else pressure
        )
        temperature_input = (
            pd.Series(temperature, index=times)
            if isinstance(temperature, list)
            else temperature
        )
        result = ephemeris(
            times, latitude, longitude, pressure_input, temperature_input
        )
        serialized_times = [
            timestamp.isoformat()
            if timestamp.tzinfo is not None
            else timestamp.tz_localize("UTC").isoformat()
            for timestamp in times
        ]
        cases.append(
            {
                "name": name,
                "inputs": {
                    "times": serialized_times,
                    "latitude": latitude,
                    "longitude": longitude,
                    "pressure": pressure,
                    "temperature": temperature,
                },
                "expected": {
                    column: result[column].tolist()
                    for column in (
                        "apparent_elevation",
                        "elevation",
                        "azimuth",
                        "apparent_zenith",
                        "zenith",
                        "solar_time",
                    )
                },
            }
        )

    latitude = 39.742476
    longitude = -105.1786
    add_case(
        "physical_mst",
        pd.date_range("2003-10-17 12:30:30", periods=1, freq="D", tz="MST"),
        latitude,
        longitude,
        82000,
        11,
    )
    add_case(
        "physical_dst",
        pd.date_range(
            "2003-10-17 13:30:30",
            periods=1,
            freq="D",
            tz="America/Denver",
        ),
        latitude,
        longitude,
        82000,
        11,
    )
    add_case(
        "physical_no_tz",
        pd.date_range("2003-10-17 19:30:30", periods=1, freq="D"),
        latitude,
        longitude,
        82000,
        11,
    )

    add_case(
        "hourly_utc",
        pd.date_range("2019-01-01", periods=24, freq="h", tz="UTC"),
        0,
        0,
        101325,
        12,
    )
    add_case(
        "hourly_us_eastern",
        pd.date_range("2019-01-01", periods=24, freq="h", tz="US/Eastern"),
        0,
        0,
        101325,
        12,
    )

    array_times = pd.date_range(
        "2019-03-20 05:30", periods=4, freq="2h", tz="UTC"
    )
    add_case("pressure_array", array_times, 0, 0, [80000, 85000, 90000, 95000], 11)
    add_case(
        "temperature_array",
        array_times,
        0,
        0,
        82000,
        [0, 5, 15, 25],
    )
    add_case(
        "pressure_temperature_arrays",
        array_times,
        0,
        0,
        [80000, 85000, 90000, 95000],
        [0, 5, 15, 25],
    )

    output_path = (
        Path(__file__).resolve().parents[2]
        / "pv.Tests"
        / "Fixtures"
        / "ephemeris.json"
    )
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump(
            {"pvlib_version": pvlib.__version__, "cases": cases},
            output_file,
            indent=2,
            allow_nan=False,
        )
        output_file.write("\n")


if __name__ == "__main__":
    main()
