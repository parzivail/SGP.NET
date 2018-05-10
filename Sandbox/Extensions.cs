using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SGP4_Sharp;

namespace Sandbox
{
    static class Extensions
    {
        public static Vector3 ToSpherical(this CoordGeodetic geo)
        {
            return new Vector3(
                (float) (Math.Cos(geo.latitude) * Math.Cos(-geo.longitude + Math.PI) *
                         (geo.altitude + Global.kXKMPER)),
                (float) (Math.Sin(geo.latitude) * (geo.altitude + Global.kXKMPER)),
                (float) (Math.Cos(geo.latitude) * Math.Sin(-geo.longitude + Math.PI) *
                         (geo.altitude + Global.kXKMPER))
            );
        }

        public static double CalculateFootprintDiameter(this CoordGeodetic geo)
        {
            return 2 * Global.kXKMPER * Math.Acos(Global.kXKMPER / (Global.kXKMPER + geo.altitude));
        }
    }
}
