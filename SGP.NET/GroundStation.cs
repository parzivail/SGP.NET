using System;
using System.Collections.Generic;

namespace SGPdotNET
{
    /// <summary>
    ///     A representation of a ground station that can observe satellites
    /// </summary>
    public class GroundStation
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="location">The location of the ground station</param>
        public GroundStation(CoordGeodetic location)
        {
            Location = location;
        }

        /// <summary>
        ///     The location of the ground station
        /// </summary>
        public CoordGeodetic Location { get; }

        /// <summary>
        ///     Creates a list of all of the predicted observations within the specified time period, such that an AOS for the
        ///     satellite from this ground station is seen at or after the start parameter
        /// </summary>
        /// <param name="satellite">The satellite to observe</param>
        /// <param name="start">The time to start observing</param>
        /// <param name="end">The time to end observing</param>
        /// <param name="deltaTime">The time step for the prediction simulation</param>
        /// <returns>A list of observations where an AOS is seen at or after the start parameter</returns>
        public List<SatelliteObservation> Observe(Satellite satellite, DateTime start, DateTime end, TimeSpan deltaTime)
        {
            start = start.Round(deltaTime);

            var obs = new List<SatelliteObservation>();

            var t = start - deltaTime;
            var state = SatelliteObservationState.Init;

            var startedObserving = start;
            var startAz = 0d;
            var maxEl = 0d;

            while (t <= end || state == SatelliteObservationState.Observing)
            {
                t += deltaTime;

                var eciLocation = Location.ToEci(t);
                var posEci = satellite.Predict(t);

                if (IsVisible(posEci))
                {
                    if (state == SatelliteObservationState.Init)
                        continue;

                    var azEl = eciLocation.LookAt(posEci);

                    if (azEl.Elevation > maxEl)
                        maxEl = azEl.Elevation;

                    if (state == SatelliteObservationState.NotObserving)
                    {
                        startAz = azEl.Azimuth;
                        startedObserving = t;
                    }

                    state = SatelliteObservationState.Observing;
                }
                else
                {
                    if (state == SatelliteObservationState.Observing)
                    {
                        var azEl = eciLocation.LookAt(posEci);
                        obs.Add(new SatelliteObservation(satellite, startedObserving, t, maxEl,
                            startAz, azEl.Azimuth));
                    }

                    maxEl = 0;
                    state = SatelliteObservationState.NotObserving;
                }
            }

            return obs;
        }

        /// <summary>
        ///     Tests whether or not a satellite is above a specified elevation, defaulting to 0 degrees
        /// </summary>
        /// <param name="pos">The position to check</param>
        /// <param name="minElevation">The minimum elevation required to be "visible"</param>
        /// <returns>True if the satellite is above the specified elevation, false otherwise</returns>
        public bool IsVisible(Eci pos, double minElevation = 0)
        {
            var pGeo = pos.ToGeodetic();
            var footprint = pGeo.GetFootprintRadians();
            var eciLocation = Location.ToEci(pos.Time);

            if (Location.DistanceToRadians(pGeo) > footprint) return false;

            if (Math.Abs(minElevation) < double.Epsilon)
                return true;

            var aer = eciLocation.LookAt(pos);
            return aer.Elevation / Math.PI * 180 >= minElevation;
        }

        internal enum SatelliteObservationState
        {
            Init,
            NotObserving,
            Observing
        }
    }
}