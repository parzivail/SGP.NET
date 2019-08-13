using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Observation
{
    /// <summary>
    ///     Stores a period during which a satellite is visible to a ground station
    /// </summary>
    public class SatelliteVisibilityPeriod
    {
        /// <summary>
        ///     The satellite that is being observed
        /// </summary>
        public Satellite Satellite { get; }

        /// <summary>
        ///     The start time of the observation
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        ///     The end time of the observation
        /// </summary>
        public DateTime End { get; }

        /// <summary>
        ///     The max elevation reached during observation
        /// </summary>
        public Angle MaxElevation { get; }

        /// <summary>
        ///     The position from which the satellite was observed to generate this observation
        /// </summary>
        public Coordinate ReferencePosition { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="satellite">The satellite that is being observed</param>
        /// <param name="start">The start time of the observation</param>
        /// <param name="end">The end time of the observation</param>
        /// <param name="maxElevation">The max elevation reached during observation</param>
        /// <param name="referencePosition">The position from which the satellite was observed to generate this observation</param>
        public SatelliteVisibilityPeriod(Satellite satellite, DateTime start, DateTime end, Angle maxElevation,
            Coordinate referencePosition = null)
        {
            Satellite = satellite;
            Start = start.ToStrictUtc();
            End = end.ToStrictUtc();
            MaxElevation = maxElevation;
            ReferencePosition = referencePosition;
        }

        /// <inheritdoc />
        protected bool Equals(SatelliteVisibilityPeriod other)
        {
            return Satellite.Equals(other.Satellite) && Start.Equals(other.Start) && End.Equals(other.End) &&
                   MaxElevation.Equals(other.MaxElevation) && ReferencePosition.Equals(other.ReferencePosition);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is SatelliteVisibilityPeriod per && Equals(per);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Satellite.GetHashCode();
                hashCode = (hashCode * 397) ^ Start.GetHashCode();
                hashCode = (hashCode * 397) ^ End.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxElevation.GetHashCode();
                hashCode = (hashCode * 397) ^ ReferencePosition.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public static bool operator ==(SatelliteVisibilityPeriod left, SatelliteVisibilityPeriod right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(SatelliteVisibilityPeriod left, SatelliteVisibilityPeriod right)
        {
            return !Equals(left, right);
        }
    }
}