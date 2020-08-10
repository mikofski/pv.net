from pathlib import Path
import numpy as np
import pandas as pd
from pvlib.solarposition import equation_of_time_spencer71, equation_of_time_pvcdrom
from pvlib.solarposition import declination_cooper69, declination_spencer71
from pvlib.solarposition import hour_angle
from pvlib.solarposition import solar_zenith_analytical, solar_azimuth_analytical
from pvlib.solarposition import get_solarposition

src = Path("C:/Users/mikm/source/repos/pv.net/bin/pv.dll")
assert src.exists()
import clr
# note: .dll extension is not needed
assy = clr.AddReference(str(src))
from clr import pv

lat, lon = 37.81, -122.25
tz = -8.0

dates = ["19900101T12:30:00", "19900102T12:30:00", "19900103T12:30:00", "19900104T12:30:00"]
print('Solar Position')
sp = pv.SolarPosition(dates, lat, lon, tz)
print('Day Angle, offset=1')
da = sp.CalcSimpleDayAngleArray()
eot = sp.EquationOfTimeSpencer71(da)
print('Equation of time, Spencer (1971)')
for doy in range(4):
    print(f'{doy+1:d} --> {eot[doy]:g}')

doy = np.arange(4) + 1
eot_pvcdrom = equation_of_time_pvcdrom(doy)
eot_test = equation_of_time_spencer71(doy)
assert np.allclose([_ for _ in eot], eot_test)

decl = declination_spencer71(doy)
decl_cooper = declination_cooper69(doy)

ts = pd.DatetimeIndex(dates, tz=f'Etc/GMT{int(tz):+d}')
ha = hour_angle(ts, lon, eot_test)

sp_test = get_solarposition(ts, latitude=lat, longitude=lon)
ze = solar_zenith_analytical(37.81*np.pi/180.0, ha*np.pi/180.0, decl)*180/np.pi
