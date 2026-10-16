import json
from pathlib import Path

import numpy as np
import pvlib
from pvlib.solarposition import equation_of_time_spencer71


def main():
    days_of_year = list(range(1, 366))
    expected = equation_of_time_spencer71(np.asarray(days_of_year))
    fixture = {
        "pvlib_version": pvlib.__version__,
        "cases": [
            {
                "name": "equation_of_time_spencer71_2015",
                "inputs": {"day_of_year": days_of_year},
                "expected": [float(value) for value in expected],
            }
        ],
    }
    output_path = (
        Path(__file__).resolve().parents[3]
        / "tests"
        / "pv.Tests"
        / "Fixtures"
        / "equation_of_time_spencer71.json"
    )
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump(fixture, output_file, indent=2, allow_nan=True)
        output_file.write("\n")


if __name__ == "__main__":
    main()
