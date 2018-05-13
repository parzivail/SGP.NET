using SGP4_Sharp;

namespace Sandbox
{
    internal class SatelliteObservation
    {
        public Satellite Satellite { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public double MaxEl { get; }
        public double StartAz { get; }
        public double EndAz { get; }

        public SatelliteObservation(Satellite satellite, DateTime start, DateTime end, double maxEl, double startAz, double endAz)
        {
            Satellite = satellite;
            Start = start;
            End = end;
            MaxEl = maxEl;
            StartAz = startAz;
            EndAz = endAz;
        }
    }
}