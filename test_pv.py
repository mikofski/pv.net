import clr
from pathlib import Path

src = Path("C:/Users/mikm/source/repos/pv.net/bin/pv.dll")
assert src.exists()

# note: .dll extension is not needed
assy = clr.AddReference(str(src))

#import pv
from clr import pv

dates = ["19900101T12:30:00", "19900102T12:30:00", "19900103T12:30:00", "19900104T12:30:00"]
pv.SolarPosition(dates, 32.1, -121.0)
