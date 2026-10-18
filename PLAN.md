# PLAN: Port `pvlib.solarposition` (and `pvlib.spa`) to pv.net

This file is the plan for porting
[`pvlib.solarposition`](https://github.com/pvlib/pvlib-python/blob/main/pvlib/solarposition.py)
and [`pvlib.spa`](https://github.com/pvlib/pvlib-python/blob/main/pvlib/spa.py)
from Python to .NET. The work is split into a backlog of GitHub issues, one per
function or work package. Each issue can be done in one PR.

**Agents: read this whole file before starting an issue.** Sections 3–6
apply to *every* issue. Each issue should only add the details specific to that work item.

Reference version: **pvlib-python v0.16.1**. Use this exact version to make
fixtures, and read the Python source at that tag, e.g.
`https://github.com/pvlib/pvlib-python/blob/v0.16.1/pvlib/solarposition.py`.

---

## 1. Status of `pvlib.solarposition` in pv.net

`[x]` means the function is already in `pv/solarposition.cs`. Every other item
is either in the backlog in section 7 or explicitly out of scope in section 2.

### Public functions

- [ ] `get_solarposition` → backlog **F7** (#12)
- [ ] `spa_c` → out of scope
- [ ] pvlib `spa_python` → public `SolarPosition.Spa` (`spa`), backlog **F4** (#9)
- [ ] `sun_rise_set_transit_spa` → backlog **F6** (#11)
- [ ] `sun_rise_set_transit_ephem` → out of scope
- [ ] `pyephem` → out of scope
- [ ] `ephemeris` → backlog **F3** (#8)
- [ ] `calc_time` → out of scope
- [ ] `pyephem_earthsun_distance` → out of scope
- [ ] `nrel_earthsun_distance` → backlog **F5** (#10)
- [x] `equation_of_time_spencer71` → `SolarPosition.EquationOfTimeSpencer71`
- [x] `equation_of_time_pvcdrom` → `SolarPosition.EquationOfTimePvCdrom`
- [x] `declination_spencer71` → `SolarPosition.DeclinationSpencer71`
- [x] `declination_cooper69` → `SolarPosition.DeclinationCooper69`
- [ ] `solar_azimuth_analytical` → backlog **F1** (#6)
- [x] `solar_zenith_analytical` → `SolarPosition.SolarZenithAnalytical`
- [x] `hour_angle` → `SolarPosition.HourAngle`
- [ ] `sun_rise_set_transit_geometric` → backlog **F2** (#5)

### Private helpers

- [x] `_calculate_simple_day_angle` → `SolarPosition.CalcSimpleDayAngleArray`
- [ ] `_datetime_to_unixtime` → ported with **F4**
- [ ] `_hour_angle_to_hours`, `_local_times_from_hours_since_midnight`,
      `_times_to_hours_after_local_midnight` → ported with **F2**
- [ ] `_spa_python_import` → not needed; it only switches between pvlib's numpy and numba builds
- [ ] `_ephem_convert_to_seconds_and_microseconds`, `_ephem_to_timezone`,
      `_ephem_setup` → out of scope (PyEphem)

## 2. Out of scope

These functions are **not** ported, because they need code that is not part
of pvlib:

| pvlib function | Reason |
|---|---|
| `pyephem`, `sun_rise_set_transit_ephem`, `pyephem_earthsun_distance` | Need the PyEphem package |
| `calc_time` | Needs PyEphem and `scipy.optimize` |
| `spa_c` | Needs NREL's SPA C source, which has a restrictive license and is not shipped with pvlib. `spa` (F4, port of pvlib `spa_python`) gives the same results. |

`get_solarposition` (F7) must throw `NotSupportedException` for the methods
`pyephem` and `nrel_c`.

Named-timezone/DST handling is outside this fixed-offset workflow.

`ModelChain` is a demo/example caller, **not** a strict port of pvlib's
model chain. All ModelChain-specific architecture is deferred to later
planning/issues; it must not drive the library API in this backlog.

## 3. Design conventions (all issues)

1. **Where the code goes**
   * Ports of `pvlib.solarposition` go in `pv/solarposition.cs`, class
     `pv.SolarPosition`.
   * Ports of `pvlib.spa` go in a new file `pv/spa.cs`, class
     `public static class pv.Spa` (see section 5).
   * Ports of `pvlib.atmosphere` go in a new file `pv/atmosphere.cs`, class
     `public static class pv.Atmosphere`.
2. **Location inputs, stateless calculations.** Introduce `pv.Location` for
   shared site inputs: latitude, longitude, altitude, a fixed UTC offset, and
   an optional name.
   It holds no timestamps, calculated arrays, or last-result state.
   High-level `public static` methods on `SolarPosition` accept a `Location`,
   explicit `DateTimeOffset[]` timestamps, and per-call calculation options.
   Calculations must not depend on the existing `SolarPosition` instance
   state. Preserve the low-level helper structure with explicit numerical
   inputs; helpers do not need a `Location` when they only use numerical arrays.
   P1 specifies this API transition; do not refactor legacy methods piecemeal
   in feature issues or add deprecation machinery or compatibility shims.
3. **Naming.** Use PascalCase for the pvlib name, e.g.
   `solar_azimuth_analytical` → `SolarAzimuthAnalytical` and
   `sun_rise_set_transit_spa` → `SunRiseSetTransitSpa`. Parameters are
   camelCase versions of the pvlib names, e.g. `equation_of_time` →
   `equationOfTime` and `delta_t` → `deltaT`. The high-level API is named
   `spa` → `SolarPosition.Spa`, **not** `SpaPython`; `spa_python` is only the
   upstream source/fixture name. `ephemeris` → `SolarPosition.Ephemeris`.
4. **Units match pvlib exactly.** If pvlib takes or returns radians, so does
   the port. If pvlib uses degrees, so does the port. Do not convert units.
5. **Types**
   * A pvlib `numeric` or array value → `double[]`; a scalar `float` →
     `double`.
   * A timezone-aware `pandas.DatetimeIndex` → `DateTimeOffset[]`. For this
     workflow, use one fixed offset matching `Location` throughout the batch;
     preserve that offset in result timestamps. Use UTC instants internally
     where the algorithm requires them, without changing the local date used
     for daily events. No named timezone or DST rules are inferred.
     Timestamps may use any time step; batches need not be hourly or span a
     full year. Leap years are supported without calendar manipulation:
     preserve February 29 and subsequent dates as supplied.
     When pvlib treats naive times as UTC, pass values with offset `+00:00`,
     e.g. naive `2025-01-01T12:00:00` → `2025-01-01T12:00:00+00:00`.
   * Unix time → `double[]`, in seconds since 1970-01-01T00:00:00Z.
   * Where pvlib takes `delta_t` as a float or `None`, use
     `double? deltaT = 67.0`. Pass null to request automatic calculation via
     `Spa.CalculateDeltaT`, matching pvlib’s None behavior.
   * Where pvlib takes `how` or `numthreads` (numba options), leave them out.
     They have no meaning in .NET.
6. **Return types**
   * Individual analytical helpers (day angle, equation of time, declination,
     hour angle, zenith, azimuth) continue to return `double[]`, not result
     objects. Earth–Sun distance also returns `double[]`.
   * Only high-level position methods (`Spa`, `Ephemeris`,
     `GetSolarposition`) return `SolarPositionResult`; high-level daily-event
     methods (`SunRiseSetTransitGeometric`, `SunRiseSetTransitSpa`) return
     `SunRiseSetTransitResult`.
   * Low-level SPA scalar, array, and named-tuple outputs stay numerical;
     map them into result objects only at the high-level boundary.
7. **Shared result classes** are created **once**, in issue **P0**, so that
   parallel PRs don't conflict:
   * `pv.SolarPositionResult`
     * One batch result, not an array of batch results. Each populated output
       array has `Times.Length` elements aligned by input index.
     * `DateTimeOffset[] Times`
     * `double[] ApparentZenith`, `Zenith`, `ApparentElevation`, `Elevation`,
       `Azimuth` (all in degrees)
     * `double[] EquationOfTime` (minutes; `null` if not calculated)
     * `double[] SolarTime` (hours; `null` if not calculated)
     * `ApparentZenith`/`ApparentElevation` include atmospheric refraction;
       `Zenith`/`Elevation` do not.
   * `pv.SunRiseSetTransitResult`
     * `DateTimeOffset[] Times`, `Transit`; `DateTimeOffset?[] Sunrise`,
       `Sunset`, with one aligned event entry per input timestamp.
       Use `null` for a sunrise or sunset that does not occur, such as at
       polar extremes. Evaluate sunrise and sunset independently; their
       absence does not imply that transit is absent.
     * Repeating a valid daily event for hourly inputs is not a missing-event
       fallback. See section 8.1 for upstream behavior and required tests.
8. **Errors.** Where pvlib raises `ValueError`, throw `ArgumentException`.
   Where a method or option is not supported, throw `NotSupportedException`.
9. Preserve `NaN` inputs and outputs wherever pvlib/numpy does. Do not throw on
   `NaN` input unless the pvlib function itself raises.
10. **No new runtime dependencies** in `pv.csproj`. Use only `System.*`.
11. **Silent library.** Remove existing library console writes in P1 and add
    none in new code. Defer logging integration entirely: no logger
    abstraction, logging infrastructure, or logging dependencies in this pass.
    Intentional output from the demo caller is outside the library.

## 4. Requirements for every issue (definition of done)

### 4.1 In-code documentation (XML doc comments)

Every new public method **must** have:

* `<summary>`: one to three sentences describing what the function
  calculates, including the pvlib function it ports, e.g.
  `Port of pvlib.solarposition.solar_azimuth_analytical.`
* One `<param>` entry for **every** parameter, using the exact format below:

  ```
  <param name="NAME">DESCRIPTION [UNITS] (TYPE[, default: VALUE])</param>
  ```

  * `UNITS` is the physical unit: `radians`, `degrees`, `minutes`, `hours`,
    `seconds`, `days`, `Pa`, `m`, `degrees C`, `AU`, `percent`, `fraction`,
    etc. Use `n/d` for values without physical units, such as counts, flags,
    strings, enums, or method names.
  * `TYPE` is the .NET type in words: `double`, `int`, `string`, `bool`,
    `array of double`, `array of DateTimeOffset`, `nullable double`, etc.
  * `default: VALUE` is required if the parameter has a default value, and
    left out if it doesn't.
  * Examples:
    * `<param name="latitude">latitude, positive north of equator [radians] (array of double)</param>`
    * `<param name="pressure">ambient air pressure [Pa] (double, default: 101325.0)</param>`
    * `<param name="method">solar position algorithm name [n/d] (string, default: "nrel_numpy")</param>`
* `<returns>`: what is returned, with the same `[UNITS] (TYPE)` notation. For
  result classes or tuples, list each member with its units.
* `<exception cref="...">` for each exception the method throws on purpose.

Private helpers need at least a `<summary>` and `<param>` tags in the same
format.

### 4.2 Tests with pvlib-generated fixtures

Every new public method **must** have xUnit tests in `tests/pv.Tests/` that
compare the .NET output with a **fixture made by running the same pvlib
function**.

1. **Fixture generator script:** `tests/fixtures/scripts/<pvlib_function_name>.py`
   * It imports `pvlib` (v0.16.1) and calls the pvlib function with fixed
     inputs.
   * It writes `tests/pv.Tests/Fixtures/<pvlib_function_name>.json`.
   * The JSON must have the shape `{"pvlib_version": "...", "cases": [{"name": "...", "inputs": {...}, "expected": {...}}]}`.
   * Serialize timestamps as ISO 8601 strings including the original fixed
     UTC offset, using `Timestamp.isoformat()`; do not normalize fixtures to
     UTC. Serialize numeric `NaN` and missing timestamps (`NaT`) as JSON
     `null`, not invented dates.
   * Commit both the script and the generated JSON.
2. **Reuse pvlib's test data where possible.** Use the inputs and expected
   values in
   [`tests/test_solarposition.py`](https://github.com/pvlib/pvlib-python/blob/v0.16.1/tests/test_solarposition.py),
   [`tests/test_spa.py`](https://github.com/pvlib/pvlib-python/blob/v0.16.1/tests/test_spa.py)
   and the shared fixtures in
   [`tests/conftest.py`](https://github.com/pvlib/pvlib-python/blob/v0.16.1/tests/conftest.py)
   as the generator script's inputs. The main ones are:
   * `golden`: lat 39.742476, lon -105.1786, `America/Denver`, 1830.14 m
   * `golden_mst`: the same location in `MST` (UTC-07:00)
   * `expected_solpos`, `expected_solpos_multi`, `expected_rise_set_spa`
   * the SPA constants at the top of `test_spa.py`: `times`, `unixtimes`,
     `lat`, `lon`, `elev`, `pressure`, `temp`, `delta_t`, `atmos_refract`,
     `JD`, `JDE`, … `Phi`, `year_array`, `month_array`, `dt_actual_array`

   Each issue lists the specific pvlib tests to mirror. Where pvlib hard-codes
   a published value, such as the NREL SPA results, assert against that value
   too, with pvlib's tolerance.
3. **Tolerances**
   * Compare outputs against the fixture using relative tolerance `1e-9` for closed-form
     functions and `1e-7` for SPA functions, unless the issue specifies otherwise.
     Compare timestamps within 1 second.
   * Compared with published values: use the same decimals or `atol` as the
     pvlib test.
4. Port pvlib's **edge-case tests** too, such as NaN handling, `ValueError`
   cases and boundary angles.
   For the high-level timestamp workflow, use fixed-offset cases, including
   leap-year dates (February 29 and day 366) and non-hourly time steps.
   Adapt named-zone inputs to a fixed offset where needed, regenerating
   expected outputs with pvlib rather than relabeling published values.
   Keep leap-year timestamps unchanged; no dates need to be removed or remapped.
5. **Daily events:** include multiple hours on one local date, evening-local
   inputs whose UTC date differs, and polar day/night cases. Preserve input
   order and repeated daily events where pvlib produces them; test missing
   sunrise/sunset independently of transit. Map JSON `null` event values to
   nullable sunrise/sunset entries per sections 3 and 8.1.

### 4.3 Checks before opening the PR

* `dotnet build pv.net.sln` has no errors and no new warnings.
* `dotnet test` passes.
* Tick the item in section 1 of this file and the issue's row in section 7,
  in the same PR.
* Link the PR to its issue (`Closes #N`).

## 5. New module: `pv/spa.cs` (port of `pvlib/spa.py`)

`spa` (the port of pvlib `spa_python`), `sun_rise_set_transit_spa` and
`nrel_earthsun_distance` all call
`pvlib.spa`, the Python version of NREL's Solar Position Algorithm (Reda &
Andreas 2004, NREL/TP-560-34302). So `pvlib/spa.py` is ported first, as its
own module:

* File `pv/spa.cs`, `namespace pv`, `public static class Spa`.
* Keep the coefficient tables (`L_TABLE`, `B_TABLE`, `R_TABLE`,
  `NUTATION_ABCD_ARRAY`, `NUTATION_YTERM_ARRAY`, …) as
  `private static readonly double[,]` / `double[,,]` arrays. Copy the values
  **exactly**.
* Port every numerical function as a `public static` method with the same
  arguments, PascalCase name, and scalar `double` inputs and outputs (e.g.
  `Spa.JulianDay(double unixtime)`). The vector entry points, listed below,
  loop over arrays.
* Do **not** port `nocompile`, `solar_position_loop`, `solar_position_numba`
  or the numba / threading code. Port `solar_position_numpy` as
  `Spa.SolarPosition`, keeping the `sst` and `esd` options.
* The SPA work is split into three issues, done in this order:
  * **S1** time scales and ΔT
  * **S2** the solar position chain
  * **S3** transit/sunrise/sunset and Earth–Sun distance
* `pv.SolarPosition` uses the module like this:
  * `SolarPosition.Spa` (F4) calls `pv.Spa.SolarPosition`
  * `NrelEarthsunDistance` (F5) calls `Spa.EarthsunDistance`
  * `SunRiseSetTransitSpa` (F6) calls `Spa.TransitSunriseSunset`
  * all of them call `Spa.CalculateDeltaT` when `deltaT` is `null`
  * `Spa` remains a static numerical module; `SolarPosition` does not contain
    a SPA instance. Qualify `pv.Spa` where needed to distinguish the module
    from the high-level method.
* Tests go in `tests/pv.Tests/SpaTests.cs` and mirror the `SpaBase` class in
  pvlib's `test_spa.py`, one test per function, using that file's constants.
* S1's `CalculateDeltaT` provides `(int year, int month)` and aligned
  `(int[] year, int[] month)` overloads, without NumPy broadcasting. Repeat
  scalar inputs to match an array when needed. Unlike pvlib's warning, years
  before -1999 or after 3000 are calculated silently; the XML docs describe
  this limitation.

## 6. How agents should work through the backlog

1. Choose an **open** issue from section 7 whose **Depends on** issues are all
   closed. **P0 must be merged before any dependent issue is started.**
2. Read the issue, this file, and the pvlib v0.16.1 source and tests for the
   function.
3. Set up Python for fixtures:
   `python -m venv venv && venv/bin/pip install pvlib==0.16.1`
   (`venv/` is already git-ignored).
4. Write the fixture generator script, run it, and commit the script and the
   JSON.
5. Write the C# port with the XML docs from section 4.1.
6. Write the xUnit tests from section 4.2.
7. Run `dotnet build pv.net.sln` and `dotnet test`.
8. Tick the boxes in this file, then open a PR that closes the issue.
9. If pvlib behavior cannot be reproduced, or if a section 3 convention
   does not fit, stop and ask in the issue before making a design change. Do not invent a different API.

## 7. Backlog

Work-package IDs (`P0`, `P1`, `S1`–`S3`, `A1`, `F1`–`F7`) are stable
planning/dependency labels; the Issue column links separate GitHub issue
numbers.

| ID | Issue | Work item | Depends on | Done |
|---|---|---|---|---|
| P0 | [#1](https://github.com/mikofski/pv.net/issues/1) | Test infrastructure: xUnit project, fixture pipeline, retarget to `net8.0`, shared result classes | – | [x] |
| P1 | [#16](https://github.com/mikofski/pv.net/issues/16) | `Location` API transition, stateless calculation boundaries, silent library, result-contract reconciliation (section 8) | P0 | [x] |
| S1 | [#2](https://github.com/mikofski/pv.net/issues/2) | `Spa`: Julian day/century/millennium functions and `calculate_deltat` | P0 | [x] |
| S2 | [#3](https://github.com/mikofski/pv.net/issues/3) | `Spa`: solar position chain, from heliocentric terms to `equation_of_time` and `solar_position` | S1 | [ ] |
| S3 | [#4](https://github.com/mikofski/pv.net/issues/4) | `Spa`: `transit_sunrise_sunset` and `earthsun_distance` | S2 | [ ] |
| A1 | [#7](https://github.com/mikofski/pv.net/issues/7) | `Atmosphere`: `pres2alt` and `alt2pres` (needed by `get_solarposition`) | P0 | [ ] |
| F1 | [#6](https://github.com/mikofski/pv.net/issues/6) | `solar_azimuth_analytical` | P0 | [ ] |
| F2 | [#5](https://github.com/mikofski/pv.net/issues/5) | `sun_rise_set_transit_geometric` and its three private helpers | P1 | [ ] |
| F3 | [#8](https://github.com/mikofski/pv.net/issues/8) | `ephemeris` | P1 | [ ] |
| F4 | [#9](https://github.com/mikofski/pv.net/issues/9) | Public `spa` (`SolarPosition.Spa`, port of `spa_python`) and `_datetime_to_unixtime` | S2, P1 | [ ] |
| F5 | [#10](https://github.com/mikofski/pv.net/issues/10) | `nrel_earthsun_distance` | S3 | [ ] |
| F6 | [#11](https://github.com/mikofski/pv.net/issues/11) | `sun_rise_set_transit_spa` | S3, P1 | [ ] |
| F7 | [#12](https://github.com/mikofski/pv.net/issues/12) | `get_solarposition` | A1, F3, F4, P1 | [ ] |

### P0 – Test infrastructure

* Retarget `pv/pv.csproj` and `ModelChain/modelchain.csproj` from
  `netcoreapp3.1` (end of life) to `net8.0`.
* Add the xUnit test project `tests/pv.Tests/pv.Tests.csproj` with
  `dotnet new xunit`, targeting `net8.0`, with a project reference to
  `pv/pv.csproj`. Add it to `pv.net.sln` with `dotnet sln add`.
* Copy `Fixtures/**/*.json` to the test output directory
  (`CopyToOutputDirectory=PreserveNewest`).
* Add a fixture loader, `tests/pv.Tests/FixtureLoader.cs`, that uses
  `System.Text.Json`. It reads `double[]` with `null` → NaN, and
  `DateTimeOffset[]` from ISO 8601 strings.
* Add assertion helpers: `AssertAllClose(expected, actual, rtol, atol)`, which
  treats NaN as equal to NaN, and `AssertTimesClose(expected, actual, toleranceSeconds)`.
* Add `pv/SolarPositionResult.cs` and `pv/SunRiseSetTransitResult.cs`, as
  described in section 3, with full XML docs. Use nullable sunrise/sunset
  entries and support their JSON `null` mapping in the fixture loader.
* Add `tests/fixtures/scripts/README.md` that explains how to create the venv
  and regenerate fixtures.
* Add a GitHub Actions workflow, `.github/workflows/dotnet.yml`, that runs
  `dotnet build` and `dotnet test` on push and pull request.
* Prove that the pipeline works: write a fixture script for the
  **already ported** `equation_of_time_spencer71` (day of year 1–366) and a
  test for `SolarPosition.EquationOfTimeSpencer71`.

## 8. Existing methods and P1 transition

P1 is the follow-on coding work package, after P0 and before the high-level
feature ports that consume its contracts. Standalone numerical work (S1–S3,
A1, F1) does not wait for a façade redesign.

P1 acceptance criteria:

* Add `Location` and explicit high-level signature contracts per section 3,
  retaining stateless numerical helpers and keeping timestamps/options per call.
  **Decision:** construct `Location(latitude, longitude, altitude, utcOffset, name = null)`,
  with the optional `name` retained as site metadata;
  require finite latitude and longitude within [-90, 90] and [-180, 180]
  degrees, finite altitude in meters, and a whole-minute `TimeSpan` offset
  within ±14 hours. High-level methods take `Location` first, then
  `DateTimeOffset[] times`, then method-specific options.
* Reconcile P0's actual result classes with section 3 before high-level ports
  begin, including apparent/non-apparent fields, optional-output semantics,
  aligned array lengths, and a documented array ownership policy. Populated
  arrays align by input index; each calculation returns newly allocated output
  arrays, and callers may mutate them without changing subsequent calculations.
* Retire the unused mutable `ZenithArray`/`AzimuthArray` placeholders rather
  than keeping duplicate result state or replacing them with an array of
  batch results. Document direct caller migration; no deprecation machinery
  or compatibility shims.
* Remove all library console writes without replacing them with logging.
  Update nonconforming public XML docs. ModelChain remains an example caller;
  only mechanical call-site changes needed by the API transition belong here,
  not ModelChain-specific architecture.
* Characterize existing behavior before changing units or inputs; use
  pvlib-generated fixtures for canonical signatures and update affected callers
  together. Do not silently reinterpret an existing `double[]` argument.

Known differences to account for in that transition:

* Before P1, `SolarPosition.SolarZenith` returned **degrees**, while pvlib's
  `solar_zenith_analytical` returns **radians**. P1 renamed the method to
  `SolarPosition.SolarZenithAnalytical` and aligned its output with pvlib.
* `SolarPosition.HourAngle` returns **radians**, while pvlib's `hour_angle`
  returns **degrees**. It also uses one fixed timezone offset.
* The legacy Spencer, PVCDROM, and Cooper declination/equation-of-time methods
  took day angles instead of `dayofyear`, and used instance state.
* Their XML docs don't yet follow the `[UNITS] (TYPE, default)` format in
  section 4.1.

P1 changed the existing analytical `SolarZenith` API from degrees to radians
to match pvlib's `solar_zenith_analytical` contract and renamed it
`SolarZenithAnalytical`. This is an intentional breaking change: affected
callers and tests were updated rather than retaining the degrees contract
through a compatibility shim. P1 also changed the existing `HourAngle` API
from radians to degrees to match pvlib's `hour_angle` contract. This is an
intentional breaking change: affected callers and tests were updated rather
than retaining the radians contract through a compatibility shim.
`SolarZenithAnalytical` keeps the legacy hour-angle/declination array units
(radians), takes latitude in radians to match pvlib, and returns radians.
`HourAngle` takes a `Location`, timestamp array, and equation-of-time array;
every timestamp must use the location's fixed offset. Migrate
`EquationOfTimeSpencer71`, `EquationOfTimePvCdrom`, and `DeclinationSpencer71`
and `DeclinationCooper69` accept day-of-year arrays and calculate their day
angles internally, as pvlib does. The reusable
`CalcSimpleDayAngleArray(dayOfYear, offset = 1)` helper uses
`2π / 365 * (dayOfYear - offset)`: use offset 1 for Spencer methods and offset
81 for PVCDROM, and offset -284 for Cooper (1969). Do not pass precomputed day
angles to these analytical methods or preserve old signatures through
compatibility shims. The Cooper offset uses the equivalent
`(dayOfYear + 284) * 2π / 365` form described in
[pvlib issue #2902](https://github.com/pvlib/pvlib-python/issues/2902).
Unrelated legacy cleanup does not block the independent numerical work above.

Approved differences from pvlib v0.16.1 for this transition:

* pv.net uses one explicit fixed `TimeSpan` UTC offset and `DateTimeOffset[]`,
  not named timezone/DST behavior or pandas timestamps.
* The previous instance constructor and methods are removed without
  compatibility shims; callers migrate to static array helpers and `Location`.

### 8.1 Daily-event semantics and approved missing-event representation

In [pvlib v0.16.1 `sun_rise_set_transit_spa`](https://github.com/pvlib/pvlib-python/blob/v0.16.1/pvlib/solarposition.py),
the local calendar date of each input is used to construct the midnight-UTC
input required by the numerical SPA routine. Consequently, hourly inputs on
the same local date repeat the same sunrise, sunset, and transit when the
calculation parameters (including `deltaT`) are identical. Preserve this
per-input alignment, not a deduplicated daily table or a “next event after
this hour” interpretation. The geometric method also keeps one entry per
input but may vary if its supplied declination/equation-of-time arrays vary;
do not impose SPA's daily repetition on all methods.

This repetition does **not** define missing-event behavior.
[SPA's `transit_sunrise_sunset`](https://github.com/pvlib/pvlib-python/blob/v0.16.1/pvlib/spa.py)
uses `NaN` for sunrise/sunset when the horizon-crossing calculation has no
solution; the wrapper converts those values to pandas `NaT`. Transit can
still be present. Missing events must stay distinguishable from valid repeated
events: do not copy a previous day's time or use a default timestamp.

**Approved decision (P0/P1, F2/F6):** `SunRiseSetTransitResult.Sunrise` and
`Sunset` use `DateTimeOffset?[]`. An entry is `null` when that event does not
occur, for example when the sun never rises or never sets at polar extremes.
Keep the arrays aligned with `Times`; do not omit entries. Document this
contract in the result XML docs, map fixture JSON `null` to nullable entries,
and cover polar cases in tests. Evaluate each event independently; a missing
sunrise or sunset does not invalidate the calculated transit.

**P1 decision:** use the `Location` constructor/validation and high-level
parameter order specified above before changing signatures. All ModelChain-
specific architectural decisions remain deferred, not implicit requirements
of P1.
