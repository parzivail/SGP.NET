using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PFX;
using SGP4_Sharp;

namespace Sandbox
{
    class GroundStation
    {
        public CoordGeodetic Location { get; }
        public List<Satellite> TrackedSatellites { get; }

        public GroundStation(CoordGeodetic location)
        {
            Location = location;
            TrackedSatellites = new List<Satellite>();
        }

        public List<SatelliteObservation> Observe(Satellite satellite, DateTime? start = null, TimeSpan? extent = null, TimeSpan? granularity = null)
        {
            if (start is null)
                start = DateTime.UtcNow.Round(TimeSpan.FromSeconds(1));

            if (extent is null)
                extent = new TimeSpan(1, 0, 0, 0);

            if (granularity is null)
                granularity = new TimeSpan(0, 0, 1);

            var obs = new List<SatelliteObservation>();

            var t = start - granularity;
            var state = SatelliteObservationState.Init;

            DateTime? startedObserving = null;
            var startAz = 0d;
            var maxEl = 0d;

            while (t < start + extent)
            {
                t += granularity;

                var posEci = satellite.Predict(t.GetValueOrDefault());

                if (IsVisible(posEci))
                {
                    if (state == SatelliteObservationState.Init)
                        continue;

                    var azEl = Location.AzimuthElevationBetween(posEci);

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
                        var azEl = Location.AzimuthElevationBetween(posEci);
                        obs.Add(new SatelliteObservation(satellite, startedObserving.GetValueOrDefault(), t.GetValueOrDefault(), maxEl, startAz, azEl.Azimuth));
                    }

                    maxEl = 0;
                    state = SatelliteObservationState.NotObserving;
                }
            }

            return obs;
        }

        public bool IsVisible(Eci pos, double minElevation = 5)
        {
            var pGeo = pos.ToGeodetic();
            var footprint = pGeo.CalculateFootprintRadiusRad();

            if (Location.DistanceToRad(pGeo) > footprint) return false;

            if (Math.Abs(minElevation) < double.Epsilon)
                return true;

            var aer = Location.AzimuthElevationBetween(pos);
            return aer.Elevation / Math.PI * 180 >= minElevation;

        }
    }

    internal enum SatelliteObservationState
    {
        Init,
        NotObserving,
        Observing
    }
}
