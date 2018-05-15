namespace SGP4_Sharp
{
    /// <summary>
    ///     Stores a topocentric location (azimuth, elevation, range and range rate).
    ///     Azimuth and elevation are stored in radians. Range in kilometers. Range rate in kilometers/second.
    /// </summary>
    public class CoordTopocentric
    {
        public CoordTopocentric()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="az">azimuth in radians</param>
        /// <param name="el">elevation in radians</param>
        /// <param name="rnge">range in kilometers</param>
        /// <param name="rngeRate">range rate in kilometers per second</param>
        public CoordTopocentric(double az, double el, double rnge, double rngeRate)
        {
            Azimuth = az;
            Elevation = el;
            Range = rnge;
            RangeRate = rngeRate;
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="topo">object to copy from</param>
        public CoordTopocentric(CoordTopocentric topo)
        {
            Azimuth = topo.Azimuth;
            Elevation = topo.Elevation;
            Range = topo.Range;
            RangeRate = topo.RangeRate;
        }

        /// <summary>
        ///     Azimuth in radians
        /// </summary>
        public double Azimuth { get; }

        /// <summary>
        ///     Elevation in radians
        /// </summary>
        public double Elevation { get; }

        /// <summary>
        ///     Range in kilometers
        /// </summary>
        public double Range { get; }

        /// <summary>
        ///     Range rate in kilometers/second
        /// </summary>
        public double RangeRate { get; }

        public override bool Equals(object obj)
        {
            return obj is CoordGeodetic geodetic &&
                   Equals(geodetic);
        }

        protected bool Equals(CoordTopocentric other)
        {
            return Azimuth.Equals(other.Azimuth) && Elevation.Equals(other.Elevation) && Range.Equals(other.Range) &&
                   RangeRate.Equals(other.RangeRate);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Azimuth.GetHashCode();
                hashCode = (hashCode * 397) ^ Elevation.GetHashCode();
                hashCode = (hashCode * 397) ^ Range.GetHashCode();
                hashCode = (hashCode * 397) ^ RangeRate.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"CoordTopocentric[Azimuth={Azimuth}, Elevation={Elevation}, Range={Range}, Range={RangeRate}]";
        }
    }
}