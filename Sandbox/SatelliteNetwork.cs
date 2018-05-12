using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PFX;
using SGP4_Sharp;
using STimeSpan = SGP4_Sharp.TimeSpan;
using SDateTime = SGP4_Sharp.DateTime;

namespace Sandbox
{
    class SatelliteNetwork
    {
        public CoordGeodetic GroundStation { get; }
        public List<Satellite> Satellites { get; }

        public SatelliteNetwork(CoordGeodetic groundStation)
        {
            GroundStation = groundStation;
            Satellites = new List<Satellite>();
        }

        public List<SatelliteObservation> Observe(Satellite satellite, SDateTime start = null, STimeSpan extent = null, STimeSpan granularity = null)
        {
            if (start is null)
                start = SDateTime.Now();

            if (extent is null)
                extent = new STimeSpan(1, 0, 0, 0);

            if (granularity is null)
                granularity = new STimeSpan(0, 0, 1);

            var obs = new List<SatelliteObservation>();

            var t = start;

            var state = SatelliteObservationState.NotObserving;
            SDateTime startedObserving = null;
            var startAz = 0d;
            var maxEl = 0d;

            while (t < start + extent)
            {
                var posEci = satellite.Predict(t);
                var pos = posEci.ToGeodetic();

                if (IsVisible(pos))
                {
                    if (state == SatelliteObservationState.NotObserving)
                    {
                        var azEl = GroundStation.AzimuthElevationBetween(posEci);

                        if (azEl.Elevation > maxEl)
                            maxEl = azEl.Elevation;

                        startAz = azEl.Azimuth;
                        startedObserving = t;
                    }

                    state = SatelliteObservationState.Observing;
                }
                else
                {
                    if (state == SatelliteObservationState.Observing)
                    {
                        var azEl = GroundStation.AzimuthElevationBetween(posEci);

                        obs.Add(new SatelliteObservation(startedObserving, t, maxEl, startAz, azEl.Azimuth));
                        maxEl = 0;
                    }

                    state = SatelliteObservationState.NotObserving;
                }

                t += granularity;
            }

            return obs;
        }

        public bool IsVisible(CoordGeodetic pos)
        {
            var footprint = pos.CalculateFootprintRadiusRad();
            return GroundStation.DistanceToRad(pos) < footprint;
        }
    }

    internal enum SatelliteObservationState
    {
        NotObserving,
        Observing
    }
}
