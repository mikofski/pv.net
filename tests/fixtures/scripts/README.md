# Regenerating pvlib fixtures

Create and activate a virtual environment, then install the pinned pvlib version:

```sh
python3 -m venv .venv
source .venv/bin/activate
pip install pvlib==0.16.1
```

From the repository root, regenerate the Spencer (1971) equation-of-time fixture:

```sh
python tests/fixtures/scripts/equation_of_time_spencer71.py
```

The scripts write fixtures under `tests/pv.Tests/Fixtures/`:

```sh
python tests/fixtures/scripts/equation_of_time_spencer71.py
python tests/fixtures/scripts/solarposition_analytical.py
python tests/fixtures/scripts/sun_rise_set_transit_spa_polar.py
python tests/fixtures/scripts/spa_time.py
python tests/fixtures/scripts/ephemeris.py
```

`solarposition_analytical.py` covers leap-year day-of-year values and
non-hourly timestamps with a fixed UTC offset.
`sun_rise_set_transit_spa_polar.py` captures nullable sunrise/sunset events
with independently calculated transit times.
`spa_time.py` covers Julian time scales, historical calendar dates, and
delta-T polynomial boundaries using pvlib's SPA test constants.
`ephemeris.py` covers the native ephemeris position outputs, fixed-offset
timestamps, and scalar and array pressure/temperature inputs.
