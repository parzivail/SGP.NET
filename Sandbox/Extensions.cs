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
        public static System.DateTime Round(this System.DateTime date, System.TimeSpan span)
        {
            var ticks = (date.Ticks + (span.Ticks / 2) + 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }

        public static Vector3 ToSpherical(this CoordGeodetic geo)
        {
            return new Vector3(
                (float)(Math.Cos(geo.Latitude) * Math.Cos(-geo.Longitude + Math.PI) *
                         (geo.Altitude + Global.EarthRadiusKm)),
                (float)(Math.Sin(geo.Latitude) * (geo.Altitude + Global.EarthRadiusKm)),
                (float)(Math.Cos(geo.Latitude) * Math.Sin(-geo.Longitude + Math.PI) *
                         (geo.Altitude + Global.EarthRadiusKm))
            );
        }

        public static double CalculateFootprintRadius(this CoordGeodetic geo)
        {
            return geo.CalculateFootprintRadiusRad() * Global.EarthRadiusKm;
        }

        public static double CalculateFootprintRadiusRad(this CoordGeodetic geo)
        {
            return Math.Acos(Global.EarthRadiusKm / (Global.EarthRadiusKm + geo.Altitude));
        }

        public static double DistanceToRad(this CoordGeodetic from, CoordGeodetic to)
        {
            var dist =
                Math.Sin(from.Latitude) * Math.Sin(to.Latitude) + Math.Cos(from.Latitude) *
                Math.Cos(to.Latitude) * Math.Cos(from.Longitude - to.Longitude);
            dist = Math.Acos(dist);

            return dist;
        }

        public static CoordTopocentric AzimuthElevationBetween(this CoordGeodetic from, Eci to)
        {
            return from.ToEci(to.Time).GetLookAngle(to);
        }
    }
}