using System;
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
        ///     The azimuth at the start of the observation
        /// </summary>
        public Angle StartAzimuth { get; }

        /// <summary>
        ///     The azimuth at the end of the observation
        /// </summary>
        public Angle EndAzimuth { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="satellite">The satellite that is being observed</param>
        /// <param name="start">The start time of the observation</param>
        /// <param name="end">The end time of the observation</param>
        /// <param name="maxElevation">The max elevation reached during observation</param>
        /// <param name="startAzimuth">The azimuth at the start of the observation</param>
        /// <param name="endAzimuth">The azimuth at the end of the observation</param>
        public SatelliteVisibilityPeriod(Satellite satellite, DateTime start, DateTime end, Angle maxElevation,
            Angle startAzimuth, Angle endAzimuth)
        {
            Satellite = satellite;
            Start = start;
            End = end;
            MaxElevation = maxElevation;
            StartAzimuth = startAzimuth;
            EndAzimuth = endAzimuth;
        }

        /// <inheritdoc />
        protected bool Equals(SatelliteVisibilityPeriod other)
        {
            return Satellite.Equals(other.Satellite) && Start.Equals(other.Start) && End.Equals(other.End) &&
                   MaxElevation.Equals(other.MaxElevation) && StartAzimuth.Equals(other.StartAzimuth) &&
                   EndAzimuth.Equals(other.EndAzimuth);
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
                hashCode = (hashCode * 397) ^ StartAzimuth.GetHashCode();
                hashCode = (hashCode * 397) ^ EndAzimuth.GetHashCode();
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