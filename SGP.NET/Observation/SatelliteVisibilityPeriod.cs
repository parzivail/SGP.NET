using System;
using System.Collections.Generic;
using SGPdotNET.Util;

namespace SGPdotNET.Observation
{
    /// <summary>
    ///     Stores a period during which a satellite is visible to a ground station
    /// </summary>
    public class SatelliteVisibilityPeriod
    {
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

        /// <inhetitdoc />
        public override bool Equals(object obj)
        {
            var period = obj as SatelliteVisibilityPeriod;
            return period != null &&
                   EqualityComparer<Satellite>.Default.Equals(Satellite, period.Satellite) &&
                   Start == period.Start &&
                   End == period.End &&
                   EqualityComparer<Angle>.Default.Equals(MaxElevation, period.MaxElevation) &&
                   EqualityComparer<Angle>.Default.Equals(StartAzimuth, period.StartAzimuth) &&
                   EqualityComparer<Angle>.Default.Equals(EndAzimuth, period.EndAzimuth);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = -1273434506;
            hashCode = hashCode * -1521134295 + EqualityComparer<Satellite>.Default.GetHashCode(Satellite);
            hashCode = hashCode * -1521134295 + Start.GetHashCode();
            hashCode = hashCode * -1521134295 + End.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Angle>.Default.GetHashCode(MaxElevation);
            hashCode = hashCode * -1521134295 + EqualityComparer<Angle>.Default.GetHashCode(StartAzimuth);
            hashCode = hashCode * -1521134295 + EqualityComparer<Angle>.Default.GetHashCode(EndAzimuth);
            return hashCode;
        }
    }
}