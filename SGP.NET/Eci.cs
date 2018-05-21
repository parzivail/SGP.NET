using System;

namespace SGPdotNET
{
    /// <summary>
    ///     Stores an Earth-centered inertial position for a particular time
    /// </summary>
    public class Eci : Coordinate
    {
        /// <summary>
        ///     Creates a new ECI coordinate at the origin
        /// </summary>
        public Eci()
        {
        }

        /// <summary>
        ///     Creates a new ECI coordinate with the specified values
        /// </summary>
        /// <param name="dt">The date to be used for this position</param>
        /// <param name="latitude">The latitude in degrees</param>
        /// <param name="longitude">The longitude in degrees</param>
        /// <param name="altitude">The altitude in kilometers</param>
        public Eci(DateTime dt, double latitude, double longitude, double altitude)
            : this(dt, new CoordGeodetic(latitude, longitude, altitude))
        {
        }

        /// <summary>
        ///     Creates a new ECI coordinate with the specified values
        /// </summary>
        /// <param name="dt">The date to be used for this position</param>
        /// <param name="coord">The position top copy</param>
        public Eci(DateTime dt, Coordinate coord)
        {
            var eci = coord.ToEci(dt);
            Time = dt;
            Position = eci.Position;
            Velocity = eci.Velocity;
        }

        /// <summary>
        ///     Creates a new ECI coordinate with the specified values
        /// </summary>
        /// <param name="dt">The date to be used for this position</param>
        /// <param name="position">The ECI vector position</param>
        public Eci(DateTime dt, Vector3 position)
        {
            Time = dt;
            Position = position;
            Velocity = new Vector3();
        }

        /// <summary>
        ///     Creates a new ECI coordinate with the specified values
        /// </summary>
        /// <param name="dt">The date to be used for this position</param>
        /// <param name="position">The ECI position vector</param>
        /// <param name="velocity">The ECI velocity vector</param>
        public Eci(DateTime dt, Vector3 position, Vector3 velocity)
        {
            Time = dt;
            Position = position;
            Velocity = velocity;
        }

        public DateTime Time { get; }
        public Vector3 Position { get; }
        public Vector3 Velocity { get; }

        /// <summary>
        ///     Converts this ECI position to a geodetic one
        /// </summary>
        /// <returns>The position in a geodetic reference frame</returns>
        public override CoordGeodetic ToGeodetic()
        {
            var theta = Util.AcTan(Position.Y, Position.X);

            var lon = Util.WrapNegPosPi(theta - Time.ToGreenwichSiderealTime());

            var r = Math.Sqrt(Position.X * Position.X + Position.Y * Position.Y);

            const double e2 = SgpConstants.EarthFlatteningConstant * (2.0 - SgpConstants.EarthFlatteningConstant);

            var lat = Util.AcTan(Position.Z, r);
            double phi;
            double c;
            var cnt = 0;

            do
            {
                phi = lat;
                var sinphi = Math.Sin(phi);
                c = 1.0 / Math.Sqrt(1.0 - e2 * sinphi * sinphi);
                lat = Util.AcTan(Position.Z + SgpConstants.EarthRadiusKm * c * e2 * sinphi, r);
                cnt++;
            } while (Math.Abs(lat - phi) >= 1e-10 && cnt < 10);

            var alt = r / Math.Cos(lat) - SgpConstants.EarthRadiusKm * c;

            return new CoordGeodetic(lat, lon, alt, true);
        }

        /// <summary>
        ///     Converts this ECI position to an ECEF one, assuming a spherical earth
        /// </summary>
        /// <returns>A spherical ECEF coordinate vector</returns>
        public override Vector3 ToSphericalEcef()
        {
            return ToGeodetic().ToSphericalEcef();
        }

        public override Eci ToEci(DateTime dt)
        {
            return this;
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