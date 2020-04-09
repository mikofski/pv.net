import clr
from pathlib import Path

src = Path("C:/Users/mikm/source/repos/pv.net/bin/solarposition.dll")
assert src.exists()

# note: .dll extension is not needed
assy = clr.AddReference(str(src))

#import pv
from clr import pv

dates = ["19900101T12:30:00", "19900102T12:30:00", "19900103T12:30:00", "19900104T12:30:00"]
sp = pv.SolarPosition(dates, 32.1, -121.0)
eot = sp.EquationOfTimeSpencer71([1,2,3,4])
print('Equation of time, Spencer (1971)')
for doy in range(4):
    print(f'{doy+1:d} --> {eot[doy]:g}')
