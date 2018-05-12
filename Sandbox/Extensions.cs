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
                (float)(Math.Cos(geo.latitude) * Math.Cos(-geo.longitude + Math.PI) *
                         (geo.altitude + Global.kXKMPER)),
                (float)(Math.Sin(geo.latitude) * (geo.altitude + Global.kXKMPER)),
                (float)(Math.Cos(geo.latitude) * Math.Sin(-geo.longitude + Math.PI) *
                         (geo.altitude + Global.kXKMPER))
            );
        }

        public static double CalculateFootprintRadius(this CoordGeodetic geo)
        {
            return geo.CalculateFootprintRadiusRad() * Global.kXKMPER;
        }

        public static double CalculateFootprintRadiusRad(this CoordGeodetic geo)
        {
            return Math.Acos(Global.kXKMPER / (Global.kXKMPER + geo.altitude));
        }

        public static double DistanceToRad(this CoordGeodetic from, CoordGeodetic to)
        {
            var dist =
                Math.Sin(from.latitude) * Math.Sin(to.latitude) + Math.Cos(from.latitude) *
                Math.Cos(to.latitude) * Math.Cos(from.longitude - to.longitude);
            dist = Math.Acos(dist);

            return dist;
        }

        public static AzimuthElevation AzimuthElevationBetween(this CoordGeodetic from, Eci to)
        {
            throw new NotImplementedException();
        }
    }
}