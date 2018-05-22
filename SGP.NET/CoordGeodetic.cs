using System;

namespace SGPdotNET
{
    /// <summary>
    ///     Stores a geodetic location
    /// </summary>
    public class CoordGeodetic : Coordinate
    {
        /// <summary>
        ///     Creates a new geodetic coordinate at the origin
        /// </summary>
        public CoordGeodetic()
        {
        }

        /// <summary>
        ///     Creates a new geodetic coordinate with the specified values
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
        ///     Creates a new geodetic coordinate as a copy of the specified one
        /// </summary>
        /// <param name="coord">Object to copy from</param>
        public CoordGeodetic(Coordinate coord)
        {
            var geo = coord.ToGeodetic();
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
        public override CoordEci ToEci(DateTime dt)
        {
            var time = dt;

            const double mfactor =
                SgpConstants.TwoPi * (SgpConstants.EarthRotationPerSiderealDay / SgpConstants.SecondsPerDay);

            var theta = time.ToLocalMeanSiderealTime(Longitude);

            var c = 1.0 /
                    Math.Sqrt(1.0 +
                              SgpConstants.EarthFlatteningConstant * (SgpConstants.EarthFlatteningConstant - 2.0) *
                              Math.Pow(Math.Sin(Latitude), 2.0));
            var s = Math.Pow(1.0 - SgpConstants.EarthFlatteningConstant, 2.0) * c;
            var achcp = (SgpConstants.EarthRadiusKm * c + Altitude) * Math.Cos(Latitude);

            var position = new Vector3(achcp * Math.Cos(theta), achcp * Math.Sin(theta),
                (SgpConstants.EarthRadiusKm * s + Altitude) * Math.Sin(Latitude));
            var velocity = new Vector3(-mfactor * position.Y, mfactor * position.X, 0);

            return new CoordEci(time, position, velocity);
        }

        public override CoordGeodetic ToGeodetic()
        {
            return this;
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