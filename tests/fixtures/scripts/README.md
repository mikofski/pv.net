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

The script writes `tests/pv.Tests/Fixtures/equation_of_time_spencer71.json`.
