import json
from pathlib import Path
import warnings

import numpy as np
import pandas as pd
import pvlib
from pvlib import spa


def main():
    if pvlib.__version__ != "0.16.1":
        raise RuntimeError("Fixtures require pvlib 0.16.1")

    # Constants and regression inputs from pvlib v0.16.1 tests/test_spa.py.
    times = pd.date_range("2003-10-17 12:30:30", periods=1, freq="D", tz="MST")
    unixtimes = np.array([timestamp.timestamp() for timestamp in times])
    JD = 2452930.312847
    JDE = 2452930.313623
    JC = 0.0379277986858
    JCE = 0.037927819916852
    JME = 0.003792781991685
    delta_t = 67.0
    year, month = 1985, 2
    year_array = np.array([-499, 500, 1000, 1500, 1800, 1860, 1900, 1950,
                           1970, 1985, 1990, 2000, 2005, 2050, 2150])
    month_array = np.array([1, 2, 3, 4, 5, 6, 6, 7, 8, 9, 10, 11, 12, 12, 12])
    mix_year_array = np.full(10, year)
    mix_month_array = np.full(10, month)
    np.testing.assert_almost_equal(JC, spa.julian_century(JD), decimal=6)
    np.testing.assert_almost_equal(JME, spa.julian_ephemeris_millennium(JCE), decimal=10)
    cases = []

    def add(name, inputs, expected):
        cases.append({"name": name, "inputs": inputs, "expected": expected})

    timestamp = times.tz_convert("UTC")[0] + pd.Timedelta(1, unit="us")
    components = [timestamp.year, timestamp.month, timestamp.day, timestamp.hour,
                  timestamp.minute, timestamp.second, timestamp.microsecond]
    add("julian_day_dt", {"components": components}, spa.julian_day_dt(*components))
    add("julian_day", {"times": [t.isoformat() for t in times],
                       "unixtimes": unixtimes.tolist()}, spa.julian_day(unixtimes).tolist())
    add("julian_ephemeris_day", {"julianDay": JD, "deltaT": delta_t},
        spa.julian_ephemeris_day(JD, delta_t))
    add("julian_century", {"julianDay": JD}, spa.julian_century(JD))
    add("julian_ephemeris_century", {"julianEphemerisDay": JDE},
        spa.julian_ephemeris_century(JDE))
    add("julian_ephemeris_millennium", {"julianEphemerisCentury": JCE},
        spa.julian_ephemeris_millennium(JCE))
    add("calculate_deltat_scalar", {"year": year, "month": month},
        spa.calculate_deltat(year, month))
    add("calculate_deltat_array",
        {"year": year_array.tolist(), "month": month_array.tolist()},
        spa.calculate_deltat(year_array, month_array).tolist())
    add("calculate_deltat_mix_year",
        {"year": mix_year_array.tolist(), "month": mix_month_array.tolist()},
        spa.calculate_deltat(mix_year_array, month).tolist())
    add("calculate_deltat_mix_month",
        {"year": mix_year_array.tolist(), "month": mix_month_array.tolist()},
        spa.calculate_deltat(year, mix_month_array).tolist())

    issue_2207 = [
        (2000, 1, 1, 12, 0, 0), (1999, 1, 1, 0, 0, 0),
        (1987, 1, 27, 0, 0, 0), (1987, 6, 19, 12, 0, 0),
        (1988, 1, 27, 0, 0, 0), (1988, 6, 19, 12, 0, 0),
        (1900, 1, 1, 0, 0, 0), (1600, 1, 1, 0, 0, 0),
        (1600, 12, 31, 0, 0, 0), (837, 4, 10, 7, 12, 0),
        (-123, 12, 31, 0, 0, 0), (-122, 1, 1, 0, 0, 0),
        (-1000, 7, 12, 12, 0, 0), (-1000, 2, 29, 0, 0, 0),
        (-1001, 8, 17, 21, 36, 0), (-4712, 1, 1, 12, 0, 0),
    ]
    for index, values in enumerate(issue_2207):
        components = [*values, 0]
        add(f"julian_day_issue_2207_{index}", {"components": components},
            spa.julian_day_dt(*components))

    for name, components in [
        ("gregorian_before", [1582, 10, 4, 0, 0, 0, 0]),
        ("gregorian_after", [1582, 10, 15, 0, 0, 0, 0]),
        ("negative_truncation", [-4800, 1, 1, 0, 0, 0, 0]),
        ("leap_microseconds", [2024, 2, 29, 23, 59, 59, 999999]),
    ]:
        add(name, {"components": components}, spa.julian_day_dt(*components))

    boundaries = [-1999, -500, 500, 1600, 1700, 1800, 1860, 1900, 1920,
                  1941, 1961, 1986, 2005, 2050, 2150, 3000]
    years = np.array([y for boundary in boundaries
                      for y in (boundary - 1, boundary, boundary + 1)
                      for _ in range(2)])
    months = np.tile([1, 12], len(years) // 2)
    with warnings.catch_warnings():
        warnings.simplefilter("ignore", UserWarning)
        expected = spa.calculate_deltat(years, months)
    add("calculate_deltat_boundaries", {"year": years.tolist(), "month": months.tolist()},
        expected.tolist())
    add("j2000", {"julianDay": 2451545.0, "julianEphemerisDay": 2451545.0,
                  "julianEphemerisCentury": 0.0},
        [spa.julian_century(2451545.0), spa.julian_ephemeris_century(2451545.0),
         spa.julian_ephemeris_millennium(0.0)])
    add("unix_epoch", {"unixtimes": [0.0, -1.0, 0.5]},
        spa.julian_day(np.array([0.0, -1.0, 0.5])).tolist())

    output_path = Path(__file__).resolve().parents[2] / "pv.Tests" / "Fixtures" / "spa_time.json"
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump({"pvlib_version": pvlib.__version__, "cases": cases},
                  output_file, indent=2, allow_nan=False)
        output_file.write("\n")


if __name__ == "__main__":
    main()
