using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using PFX;
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
            var s = to.Position();
            var o = new Eci(to.GetDateTime(), from).Position();

            var longitude = from.longitude;
            var latitude = from.latitude;

            var rx = s.x - o.x;
            var ry = s.y - o.y;
            var rz = s.z - o.z;

            var theta = to.GetDateTime().ToLocalMeanSiderealTime(longitude) % (Math.PI * 2);

            var topS = Math.Sin(latitude) * Math.Cos(theta) * rx +
            Math.Sin(latitude) * Math.Sin(theta) * ry -
                Math.Cos(latitude) * rz;

            var topE = -Math.Sin(theta) * rx +
                Math.Cos(theta) * ry;

            var topZ = Math.Cos(latitude) * Math.Cos(theta) * rx +
                Math.Cos(latitude) * Math.Sin(theta) * ry +
                Math.Sin(latitude) * rz;

            var rangeSat = Math.Sqrt(topS * topS + topE * topE + topZ * topZ);
            var el = Math.Asin(topZ / rangeSat);
            var az = Math.Atan2(-topE, topS) + Math.PI;

            return new AzimuthElevation(az, el, rangeSat);
        }
    }
}