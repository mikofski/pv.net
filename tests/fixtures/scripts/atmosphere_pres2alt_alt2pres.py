import json
from pathlib import Path

import numpy as np
import pvlib
from pvlib.atmosphere import alt2pres, pres2alt


def main():
    if pvlib.__version__ != "0.16.1":
        raise RuntimeError("Fixtures require pvlib 0.16.1")

    cases = []

    def add(name, parameter, values, function):
        values = np.array(values, dtype=float)
        with np.errstate(invalid="ignore"):
            expected = function(values)
        cases.append({
            "name": name,
            "inputs": {parameter: [None if np.isnan(v) else v for v in values.tolist()]},
            "expected": [None if np.isnan(v) else v for v in expected.tolist()],
        })

    # Inputs from pvlib v0.16.1 tests/test_atmosphere.py.
    add("pres2alt_upstream", "pressure", [10000, 90000, 101325], pres2alt)
    add("alt2pres_upstream", "altitude", [-100, 0, 1000, 8000], alt2pres)
    # Inputs needed by get_solarposition tests.
    add("pres2alt_solarposition", "pressure", [82000, 101325], pres2alt)
    add("alt2pres_solarposition", "altitude", [0, 1830.14, 2000], alt2pres)
    add("pres2alt_edges", "pressure", [np.nan, 0, -1], pres2alt)
    add("alt2pres_edges", "altitude", [np.nan, 44331.514, 45000], alt2pres)

    output_path = (
        Path(__file__).resolve().parents[2]
        / "pv.Tests" / "Fixtures" / "atmosphere_pres2alt_alt2pres.json"
    )
    with output_path.open("w", encoding="utf-8") as output_file:
        json.dump({"pvlib_version": pvlib.__version__, "cases": cases},
                  output_file, indent=2, allow_nan=False)
        output_file.write("\n")


if __name__ == "__main__":
    main()
