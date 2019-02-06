using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Observation
{
    /// <summary>
    ///     Stores a topocentric location (azimuth, elevation, range and range rate).
    /// </summary>
    public class TopocentricObservation
    {
        /// <summary>
        ///     Creates a new topocentirc coordinate at the origin
        /// </summary>
        public TopocentricObservation()
        {
        }

        /// <summary>
        ///     Creates a new topocentirc coordinate with the specified values
        /// </summary>
        /// <param name="azimuth">Azimuth</param>
        /// <param name="elevation">Elevation</param>
        /// <param name="range">Range in kilometers</param>
        /// <param name="rangeRate">Range rate in kilometers/second</param>
        public TopocentricObservation(Angle azimuth, Angle elevation, double range, double rangeRate)
        {
            Azimuth = azimuth;
            Elevation = elevation;
            Range = range;
            RangeRate = rangeRate;
        }

        /// <summary>
        ///     Creates a new topocentirc coordinate as a copy of the specified one
        /// </summary>
        /// <param name="topo">Object to copy from</param>
        public TopocentricObservation(TopocentricObservation topo)
        {
            Azimuth = topo.Azimuth;
            Elevation = topo.Elevation;
            Range = topo.Range;
            RangeRate = topo.RangeRate;
        }

        /// <summary>
        ///     Azimuth
        /// </summary>
        public Angle Azimuth { get; }

        /// <summary>
        ///     Elevation
        /// </summary>
        public Angle Elevation { get; }

        /// <summary>
        ///     Range in kilometers
        /// </summary>
        public double Range { get; }

        /// <summary>
        ///     Range rate in kilometers/second
        /// </summary>
        public double RangeRate { get; }

        /// <summary>
        ///     Direction relative to the observer
        /// </summary>
        public RelativeDirection Direction => RangeRate < 0 ? RelativeDirection.Approaching : (Math.Abs(RangeRate) < double.Epsilon ? RelativeDirection.Fixed : RelativeDirection.Receding);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is GeodeticCoordinate geodetic &&
                   Equals(geodetic);
        }

        /// <summary>
        ///     Checks equality between this object and another
        /// </summary>
        /// <param name="other">The other object of comparison</param>
        /// <returns>True if the two objects are equal</returns>
        protected bool Equals(TopocentricObservation other)
        {
            return Azimuth.Equals(other.Azimuth) && Elevation.Equals(other.Elevation) && Range.Equals(other.Range) &&
                   RangeRate.Equals(other.RangeRate);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"TopocentricObservation[Azimuth={Azimuth}, Elevation={Elevation}, Range={Range}km, RangeRate={RangeRate}km/s]";
        }
    }

    public enum RelativeDirection
    {
        Approaching,
        Fixed,
        Receding
    }
}