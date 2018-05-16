using System;

namespace SGPdotNET
{
    /// <summary>
    ///     Stores a geodetic location
    /// </summary>
    public class CoordGeodetic
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public CoordGeodetic()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="lat">The latitude (degrees by default)</param>
        /// <param name="lon">The longitude (degrees by default)</param>
        /// <param name="alt">The altitude in kilometers</param>
        /// <param name="isRadians">True if the provided latitude and longitude are in radians</param>
        public CoordGeodetic(double lat, double lon, double alt, bool isRadians = false)
        {
            if (isRadians)
            {
                Latitude = lat;
                Longitude = lon;
            }
            else
            {
                Latitude = Util.DegreesToRadians(lat);
                Longitude = Util.DegreesToRadians(lon);
            }

            Altitude = alt;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="geo">Object to copy from</param>
        public CoordGeodetic(CoordGeodetic geo)
        {
            Latitude = geo.Latitude;
            Longitude = geo.Longitude;
            Altitude = geo.Altitude;
        }

        /// <summary>
        ///     Latitude in radians, where -PI &lt;= latitude &lt; PI
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        ///     Longitude in radians, where -PI/2 &lt;= latitude &lt; PI/2
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        ///     Altitude in kilometers
        /// </summary>
        public double Altitude { get; }

        /// <summary>
        ///     Converts this geodetic position to an ECI one
        /// </summary>
        /// <param name="dt">The time for the ECI frame</param>
        /// <returns>The position in an ECI reference frame with the supplied time</returns>
        public Eci ToEci(DateTime dt)
        {
            var time = dt;

            const double mfactor =
                SgpConstants.TwoPi * (SgpConstants.EarthRotationPerSiderealDay / SgpConstants.SecondsPerDay);

            var theta = time.ToLocalMeanSiderealTime(Longitude);

            var c = 1.0
                    /
                    Math.Sqrt(1.0 +
                              SgpConstants.EarthFlatteningConstant * (SgpConstants.EarthFlatteningConstant - 2.0) *
                              Math.Pow(Math.Sin(Latitude), 2.0));
            var s = Math.Pow(1.0 - SgpConstants.EarthFlatteningConstant, 2.0) * c;
            var achcp = (SgpConstants.EarthRadiusKm * c + Altitude) * Math.Cos(Latitude);

            var position = new Vector3(achcp * Math.Cos(theta), achcp * Math.Sin(theta),
                (SgpConstants.EarthRadiusKm * s + Altitude) * Math.Sin(Latitude));

            var velocity = new Vector3(-mfactor * position.Y, mfactor * position.X, 0);

            return new Eci(time, position, velocity);
        }

        /// <summary>
        ///     Converts this geodetic position to an ECEF one, assuming a spherical earth
        /// </summary>
        /// <returns>A spherical ECEF coordinate vector</returns>
        public Vector3 ToSphericalEcef()
        {
            return new Vector3(
                Math.Cos(Latitude) * Math.Cos(-Longitude + Math.PI) *
                (Altitude + SgpConstants.EarthRadiusKm),
                Math.Sin(Latitude) * (Altitude + SgpConstants.EarthRadiusKm),
                Math.Cos(Latitude) * Math.Sin(-Longitude + Math.PI) *
                (Altitude + SgpConstants.EarthRadiusKm)
            );
        }

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
            return Math.Acos(SgpConstants.EarthRadiusKm / (SgpConstants.EarthRadiusKm + Altitude));
        }

        /// <summary>
        ///     Calculates the Great Circle distance (km) to another geodetic coordinate
        /// </summary>
        /// <param name="to">The coordinate to measure against</param>
        /// <returns>The distance between the coordinates, in kilometers</returns>
        public double DistanceTo(CoordGeodetic to)
        {
            return DistanceToRadians(to) * SgpConstants.EarthRadiusKm;
        }

        /// <summary>
        ///     Calculates the Great Circle distance (radians) to another geodetic coordinate
        /// </summary>
        /// <param name="to">The coordinate to measure against</param>
        /// <returns>The distance between the coordinates, in radians</returns>
        public double DistanceToRadians(CoordGeodetic to)
        {
            var dist =
                Math.Sin(Latitude) * Math.Sin(to.Latitude) + Math.Cos(Latitude) *
                Math.Cos(to.Latitude) * Math.Cos(Longitude - to.Longitude);
            dist = Math.Acos(dist);

            return dist;
        }

        /// <summary>
        ///     Calculates the look angles between this coordinate and target
        /// </summary>
        /// <param name="time">The time of observation</param>
        /// <param name="to">The coordinate to observe</param>
        /// <returns>The topocentric angles between this coordinate and another</returns>
        public CoordTopocentric LookAt(CoordGeodetic to, DateTime? time = null)
        {
            var t = DateTime.UtcNow;
            if (time.HasValue)
                t = time.Value;
            return ToEci(t).LookAt(to.ToEci(t));
        }

        public override bool Equals(object obj)
        {
            return obj is CoordGeodetic geodetic &&
                   Equals(geodetic);
        }

        protected bool Equals(CoordGeodetic other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude) &&
                   Altitude.Equals(other.Altitude);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Altitude.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"CoordGeodetic[Latitude={Latitude}, Longitude={Longitude}, Altitude={Altitude}]";
        }
    }
}