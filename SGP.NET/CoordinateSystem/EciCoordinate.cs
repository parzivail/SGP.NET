using System;
using SGPdotNET.Propogation;
using SGPdotNET.Util;

namespace SGPdotNET.CoordinateSystem
{
    /// <inheritdoc />
    /// <summary>
    ///     Stores an Earth-centered inertial position for a particular time
    /// </summary>
    public class EciCoordinate : Coordinate
    {
        /// <summary>
        ///     Creates a new ECI coordinate at the origin
        /// </summary>
        public EciCoordinate()
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new ECI coordinate with the specified values
        /// </summary>
        /// <param name="dt">The date to be used for this position</param>
        /// <param name="latitude">The latitude</param>
        /// <param name="longitude">The longitude</param>
        /// <param name="altitude">The altitude in kilometers</param>
        public EciCoordinate(DateTime dt, Angle latitude, Angle longitude, double altitude)
            : this(dt, new GeodeticCoordinate(latitude, longitude, altitude))
        {
        }

        /// <summary>
        ///     Creates a new ECI coordinate with the specified values
        /// </summary>
        /// <param name="dt">The date to be used for this position</param>
        /// <param name="coord">The position top copy</param>
        public EciCoordinate(DateTime dt, Coordinate coord)
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
        public EciCoordinate(DateTime dt, Vector3 position)
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
        public EciCoordinate(DateTime dt, Vector3 position, Vector3 velocity)
        {
            Time = dt;
            Position = position;
            Velocity = velocity;
        }

        /// <summary>
        ///     The time component of the coordinate
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        ///     The position component of the coordinate
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        ///     The velocity component of the coordinate
        /// </summary>
        public Vector3 Velocity { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Converts this ECI position to a geodetic one
        /// </summary>
        /// <returns>The position in a geodetic reference frame</returns>
        public override GeodeticCoordinate ToGeodetic()
        {
            var theta = MathUtil.AcTan(Position.Y, Position.X);

            var lon = MathUtil.WrapNegPosPi(theta - Time.ToGreenwichSiderealTime());

            var r = Math.Sqrt(Position.X * Position.X + Position.Y * Position.Y);

            const double e2 = SgpConstants.EarthFlatteningConstant * (2.0 - SgpConstants.EarthFlatteningConstant);

            var lat = MathUtil.AcTan(Position.Z, r);
            double phi;
            double c;
            var cnt = 0;

            do
            {
                phi = lat;
                var sinphi = Math.Sin(phi);
                c = 1.0 / Math.Sqrt(1.0 - e2 * sinphi * sinphi);
                lat = MathUtil.AcTan(Position.Z + SgpConstants.EarthRadiusKm * c * e2 * sinphi, r);
                cnt++;
            } while (Math.Abs(lat - phi) >= 1e-10 && cnt < 10);

            var alt = r / Math.Cos(lat) - SgpConstants.EarthRadiusKm * c;

            return new GeodeticCoordinate(new Angle(lat), new Angle(lon), alt);
        }

        /// <inheritdoc />
        public override EciCoordinate ToEci(DateTime dt)
        {
            if (dt == Time)
                return this;

            // Have to hand off to another coordinate system to get the new time integrated into the coordinate set
            var geo = ToGeodetic();
            return geo.ToEci(dt);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"EciCoordinate[Position={Position}, Velocity={Velocity}]";
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = 818017616;
            hashCode = hashCode * -1521134295 + Time.GetHashCode();
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Velocity.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is EciCoordinate eci &&
                   Time == eci.Time &&
                   Position.Equals(eci.Position) &&
                   Velocity.Equals(eci.Velocity);
        }
    }
}