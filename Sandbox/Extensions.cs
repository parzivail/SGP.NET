using System;
using OpenTK;
using SGPdotNET;
using Vector3 = OpenTK.Vector3;

namespace Sandbox
{
    static class Extensions
    {
        public static DateTime Round(this DateTime date, TimeSpan span)
        {
            var ticks = (date.Ticks + (span.Ticks / 2) + 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }

        public static Vector3 ToSpherical(this CoordGeodetic geo)
        {
            return new Vector3(
                (float)(Math.Cos(geo.Latitude) * Math.Cos(-geo.Longitude + Math.PI) *
                         (geo.Altitude + SgpConstants.EarthRadiusKm)),
                (float)(Math.Sin(geo.Latitude) * (geo.Altitude + SgpConstants.EarthRadiusKm)),
                (float)(Math.Cos(geo.Latitude) * Math.Sin(-geo.Longitude + Math.PI) *
                         (geo.Altitude + SgpConstants.EarthRadiusKm))
            );
        }

        public static double CalculateFootprintRadius(this CoordGeodetic geo)
        {
            return geo.CalculateFootprintRadiusRad() * SgpConstants.EarthRadiusKm;
        }

        public static double CalculateFootprintRadiusRad(this CoordGeodetic geo)
        {
            return Math.Acos(SgpConstants.EarthRadiusKm / (SgpConstants.EarthRadiusKm + geo.Altitude));
        }

        public static double DistanceToRad(this CoordGeodetic from, CoordGeodetic to)
        {
            var dist =
                Math.Sin(@from.Latitude) * Math.Sin(to.Latitude) + Math.Cos(@from.Latitude) *
                Math.Cos(to.Latitude) * Math.Cos(@from.Longitude - to.Longitude);
            dist = Math.Acos(dist);

            return dist;
        }

        public static CoordTopocentric AzimuthElevationBetween(this CoordGeodetic from, Eci to)
        {
            return from.ToEci(to.Time).GetLookAngle(to);
        }
    }
}