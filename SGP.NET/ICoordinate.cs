using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGPdotNET
{
    /// <summary>
    /// Stores a generic location
    /// </summary>
    public abstract class ICoordinate
    {
        /// <summary>
        ///     Converts this position to an ECI one
        /// </summary>
        /// <param name="dt">The time for the ECI frame</param>
        /// <returns>The position in an ECI reference frame with the supplied time</returns>
        public abstract Eci ToEci(DateTime dt);

        /// <summary>
        ///     Converts this position to an ECEF one, assuming a spherical earth
        /// </summary>
        /// <returns>A spherical ECEF coordinate vector</returns>
        public abstract Vector3 ToSphericalEcef();

        /// <summary>
        ///     Converts this ECI position to a geodetic one
        /// </summary>
        /// <returns>The position in a geodetic reference frame</returns>
        public abstract CoordGeodetic ToGeodetic();

        /// <summary>
        ///     Calculates the visibility radius (km) of the satellite by which any distances from this position less than the
        ///     radius are able to see this position
        /// </summary>
        /// <returns>The visibility radius, in kilometers</returns>
        public double GetFootprint()
        {
            return GetFootprintRadians() * SgpConstants.EarthRadiusKm;
        }

        /// <summary>
        ///     Calculates the visibility radius (radians) of the satellite by which any distances from this position less than the
        ///     radius are able to see this position
        /// </summary>
        /// <returns>The visibility radius, in radians</returns>
        public double GetFootprintRadians()
        {
            var geo = ToGeodetic();
            return Math.Acos(SgpConstants.EarthRadiusKm / (SgpConstants.EarthRadiusKm + geo.Altitude));
        }

        /// <summary>
        ///     Calculates the Great Circle distance (km) to another coordinate
        /// </summary>
        /// <param name="to">The coordinate to measure against</param>
        /// <returns>The distance between the coordinates, in kilometers</returns>
        public double DistanceTo(ICoordinate to)
        {
            return DistanceToRadians(to) * SgpConstants.EarthRadiusKm;
        }

        /// <summary>
        ///     Calculates the Great Circle distance (radians) to another geodetic coordinate
        /// </summary>
        /// <param name="to">The coordinate to measure against</param>
        /// <returns>The distance between the coordinates, in radians</returns>
        public double DistanceToRadians(ICoordinate to)
        {
            var geo = ToGeodetic();
            var toGeo = to.ToGeodetic();
            var dist = Math.Sin(geo.Latitude) * Math.Sin(toGeo.Latitude) + Math.Cos(geo.Latitude) * Math.Cos(toGeo.Latitude) * Math.Cos(geo.Longitude - toGeo.Longitude);
            dist = Math.Acos(dist);

            return dist;
        }

        /// <summary>
        ///     Calculates the look angles between this coordinate and target
        /// </summary>
        /// <param name="time">The time of observation</param>
        /// <param name="to">The coordinate to observe</param>
        /// <returns>The topocentric angles between this coordinate and another</returns>
        public CoordTopocentric LookAt(ICoordinate to, DateTime? time = null)
        {
            var t = DateTime.UtcNow;
            if (time.HasValue)
                t = time.Value;

            var geo = ToGeodetic();
            var eci = to.ToEci(t);
            var self = ToEci(t);

            var rangeRate = eci.Velocity - self.Velocity;
            var range = eci.Position - self.Position;

            var theta = eci.Time.ToLocalMeanSiderealTime(geo.Longitude);

            var sinLat = Math.Sin(geo.Latitude);
            var cosLat = Math.Cos(geo.Latitude);
            var sinTheta = Math.Sin(theta);
            var cosTheta = Math.Cos(theta);

            var topS = sinLat * cosTheta * range.X
                       + sinLat * sinTheta * range.Y - cosLat * range.Z;
            var topE = -sinTheta * range.X
                       + cosTheta * range.Y;
            var topZ = cosLat * cosTheta * range.X
                       + cosLat * sinTheta * range.Y + sinLat * range.Z;
            var az = Math.Atan(-topE / topS);

            if (topS > 0.0)
                az += Math.PI;

            if (az < 0.0)
                az += 2.0 * Math.PI;

            var el = Math.Asin(topZ / range.Length);
            var rate = range.Dot(rangeRate) / range.Length;

            return new CoordTopocentric(az, el, range.Length, rate);
        }
    }
}
