using System;

namespace SGP4_Sharp
{
    /// <summary>
    /// Stores a geodetic location (latitude, longitude, altitude).
    /// </summary>
    public class CoordGeodetic
    {
        /// <summary>
        /// Latitude in radians, where -PI &lt;= latitude &lt; PI
        /// </summary>
        public double Latitude;
        /// <summary>
        /// Longitude in radians, where -PI/2 &lt;= latitude &lt; PI/2
        /// </summary>
        public double Longitude;
        /// <summary>
        /// Altitude in kilometers
        /// </summary>
        public double Altitude;

        public CoordGeodetic()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lat">the latitude (degrees by default)</param>
        /// <param name="lon">the longitude (degrees by default)</param>
        /// <param name="alt">the altitude in kilometers</param>
        /// <param name="isRadians">whether the latitude and longitude are in radians</param>
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
        /// Copy constructor
        /// </summary>
        /// <param name="geo">object to copy from</param>
        public CoordGeodetic(CoordGeodetic geo)
        {
            Latitude = geo.Latitude;
            Longitude = geo.Longitude;
            Altitude = geo.Altitude;
        }

        /// <summary>
        /// Converts this geodedic position to a ECI one
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public Eci ToEci(DateTime dt)
        {
            var time = dt;

            const double mfactor = Global.KTwopi * (Global.EarthRotationPerSiderealDay / Global.KSecondsPerDay);
            
            var theta = time.ToLocalMeanSiderealTime(Longitude);
            
            var c = 1.0
                    / Math.Sqrt(1.0 + Global.EarthFlatteningConstant * (Global.EarthFlatteningConstant - 2.0) * Math.Pow(Math.Sin(Latitude), 2.0));
            var s = Math.Pow(1.0 - Global.EarthFlatteningConstant, 2.0) * c;
            var achcp = (Global.EarthRadiusKm * c + Altitude) * Math.Cos(Latitude);

            var position = new Vector
            {
                x = achcp * Math.Cos(theta),
                y = achcp * Math.Sin(theta),
                z = (Global.EarthRadiusKm * s + Altitude) * Math.Sin(Latitude)
            };
            position.w = position.Magnitude();

            var velocity = new Vector
            {
                x = -mfactor * position.y,
                y = mfactor * position.x,
                z = 0.0
            };
            velocity.w = velocity.Magnitude();

            return new Eci
            {
                Time = time,
                Position = position,
                Velocity = velocity
            };
        }

        public override bool Equals(object obj)
        {
            return obj is CoordGeodetic geodetic &&
                   Equals(geodetic);
        }

        protected bool Equals(CoordGeodetic other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude) && Altitude.Equals(other.Altitude);
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
            return $"CoordGeodetic{{Latitude={Latitude}, Longitude={Longitude}, Altitude={Altitude}}}";
        }
    }
}