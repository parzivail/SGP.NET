using System;

namespace SGP4_Sharp
{
    /// <summary>
    ///     Stores an Earth-centered inertial position for a particular time.
    /// </summary>
    public class Eci
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public Eci()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dt">the date to be used for this position</param>
        /// <param name="latitude">the latitude in degrees</param>
        /// <param name="longitude">the longitude in degrees</param>
        /// <param name="altitude">the altitude in kilometers</param>
        public Eci(DateTime dt, double latitude, double longitude, double altitude)
            : this(dt, new CoordGeodetic(latitude, longitude, altitude))
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dt">the date to be used for this position</param>
        /// <param name="geo">the geocentric position</param>
        public Eci(DateTime dt, CoordGeodetic geo)
        {
            var eci = geo.ToEci(dt);
            Time = dt;
            Position = eci.Position;
            Velocity = eci.Velocity;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dt">the date to be used for this position</param>
        /// <param name="position">the ECI vector position</param>
        public Eci(DateTime dt, Vector position)
        {
            Time = dt;
            Position = position;
            Velocity = new Vector();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dt">the date to be used for this position</param>
        /// <param name="position">the ECI position vector</param>
        /// <param name="velocity">the ECI velocity vector</param>
        public Eci(DateTime dt, Vector position, Vector velocity)
        {
            Time = dt;
            Position = position;
            Velocity = velocity;
        }

        public DateTime Time { get; }
        public Vector Position { get; }
        public Vector Velocity { get; }

        /// <summary>
        ///     Converts this ECI position to a geodedic one
        /// </summary>
        /// <returns></returns>
        public CoordGeodetic ToGeodetic()
        {
            var theta = Util.AcTan(Position.Y, Position.X);

            var lon = Util.WrapNegPosPi(theta - Time.ToGreenwichSiderealTime());

            var r = Math.Sqrt(Position.X * Position.X + Position.Y * Position.Y);

            const double e2 = Global.EarthFlatteningConstant * (2.0 - Global.EarthFlatteningConstant);

            var lat = Util.AcTan(Position.Z, r);
            double phi;
            double c;
            var cnt = 0;

            do
            {
                phi = lat;
                var sinphi = Math.Sin(phi);
                c = 1.0 / Math.Sqrt(1.0 - e2 * sinphi * sinphi);
                lat = Util.AcTan(Position.Z + Global.EarthRadiusKm * c * e2 * sinphi, r);
                cnt++;
            } while (Math.Abs(lat - phi) >= 1e-10 && cnt < 10);

            var alt = r / Math.Cos(lat) - Global.EarthRadiusKm * c;

            return new CoordGeodetic(lat, lon, alt, true);
        }

        /// <summary>
        ///     Get the look angle between this position and the object
        /// </summary>
        /// <param name="eci">The object to look at</param>
        /// <returns></returns>
        public CoordTopocentric GetLookAngle(Eci eci)
        {
            var geo = ToGeodetic();

            var rangeRate = eci.Velocity - Velocity;
            var range = eci.Position - Position;

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
                az += Global.KPi;

            if (az < 0.0)
                az += 2.0 * Global.KPi;

            var el = Math.Asin(topZ / range.Length);
            var rate = range.Dot(rangeRate) / range.Length;

            return new CoordTopocentric(az, el, range.Length, rate);
        }

        public override string ToString()
        {
            return $"Eci[Position={Position}, Velocity={Velocity}]";
        }

        public override int GetHashCode()
        {
            var hashCode = 818017616;
            hashCode = hashCode * -1521134295 + Time.GetHashCode();
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Velocity.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is Eci eci &&
                   Time == eci.Time &&
                   Position.Equals(eci.Position) &&
                   Velocity.Equals(eci.Velocity);
        }
    }
}