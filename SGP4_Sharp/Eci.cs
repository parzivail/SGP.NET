using System;


namespace SGP4_Sharp
{
    /// <summary>
    /// Stores an Earth-centered inertial position for a particular time.
    /// </summary>
    public class Eci
    {
        public DateTime Time;
        public Vector Position = new Vector();
        public Vector Velocity = new Vector();

        /// <summary>
        /// Constructor
        /// </summary>
        public Eci()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dt">the date to be used for this position</param>
        /// <param name="latitude">the latitude in degrees</param>
        /// <param name="longitude">the longitude in degrees</param>
        /// <param name="altitude">the altitude in kilometers</param>
        public Eci(DateTime dt, double latitude, double longitude, double altitude)
        {
            FromGeodetic(dt, new CoordGeodetic(latitude, longitude, altitude));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dt">the date to be used for this position</param>
        /// <param name="geo">the geocentric position</param>
        public Eci(DateTime dt, CoordGeodetic geo)
        {
            FromGeodetic(dt, geo);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dt">the date to be used for this position</param>
        /// <param name="position">the ECI vector position</param>
        public Eci(DateTime dt, Vector position)
        {
            Time = dt;
            Position = position;
        }

        /// <summary>
        /// Constructor
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

        /// <summary>
        /// FromGeodetic this object with a new date and geodetic position
        /// </summary>
        /// <param name="dt">new date</param>
        /// <param name="geo">new geodetic position</param>
        public void FromGeodetic(DateTime dt, CoordGeodetic geo)
        {
            var eci = geo.ToEci(dt);
            Time = dt;
            Position = eci.Position;
            Velocity = eci.Velocity;
        }

        /// <summary>
        /// Converts this ECI position to a geodedic one
        /// </summary>
        /// <returns></returns>
        public CoordGeodetic ToGeodetic()
        {
            var theta = Util.AcTan(Position.y, Position.x);

            var lon = Util.WrapNegPosPi(theta - Time.ToGreenwichSiderealTime());

            var r = Math.Sqrt(Position.x * Position.x + Position.y * Position.y);

            const double e2 = Global.EarthFlatteningConstant * (2.0 - Global.EarthFlatteningConstant);

            var lat = Util.AcTan(Position.z, r);
            double phi;
            double c;
            var cnt = 0;

            do
            {
                phi = lat;
                var sinphi = Math.Sin(phi);
                c = 1.0 / Math.Sqrt(1.0 - e2 * sinphi * sinphi);
                lat = Util.AcTan(Position.z + Global.EarthRadiusKm * c * e2 * sinphi, r);
                cnt++;
            } while (Math.Abs(lat - phi) >= 1e-10 && cnt < 10);

            var alt = r / Math.Cos(lat) - Global.EarthRadiusKm * c;

            return new CoordGeodetic(lat, lon, alt, true);
        }

        /// <summary>
        /// Get the look angle between this position and the object
        /// </summary>
        /// <param name="eci">The object to look at</param>
        /// <returns></returns>
        public CoordTopocentric GetLookAngle(Eci eci)
        {
            var geo = ToGeodetic();

            /*
           * calculate differences
           */
            var rangeRate = eci.Velocity - Velocity;
            var range = eci.Position - Position;

            range.w = range.Magnitude();

            /*
           * Calculate Local Mean Sidereal Time for observers longitude
           */
            var theta = eci.Time.ToLocalMeanSiderealTime(geo.Longitude);

            var sinLat = Math.Sin(geo.Latitude);
            var cosLat = Math.Cos(geo.Latitude);
            var sinTheta = Math.Sin(theta);
            var cosTheta = Math.Cos(theta);

            var topS = sinLat * cosTheta * range.x
                       + sinLat * sinTheta * range.y - cosLat * range.z;
            var topE = -sinTheta * range.x
                       + cosTheta * range.y;
            var topZ = cosLat * cosTheta * range.x
                       + cosLat * sinTheta * range.y + sinLat * range.z;
            var az = Math.Atan(-topE / topS);

            if (topS > 0.0)
                az += Global.KPi;

            if (az < 0.0)
                az += 2.0 * Global.KPi;

            var el = Math.Asin(topZ / range.w);
            var rate = range.Dot(rangeRate) / range.w;

            return new CoordTopocentric(az, el, range.w, rate);
        }
    }
}
