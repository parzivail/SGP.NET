using System;
using SGPdotNET.Util;

namespace SGPdotNET
{
    /// <summary>
    ///     Stores a satellite observation
    /// </summary>
    public class SatelliteObservation
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
        public SatelliteObservation(Satellite satellite, DateTime start, DateTime end, Angle maxElevation,
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
    }
}