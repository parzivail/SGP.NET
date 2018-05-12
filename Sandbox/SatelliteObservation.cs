using SGP4_Sharp;

namespace Sandbox
{
    internal class SatelliteObservation
    {
        public DateTime Start { get; }
        public DateTime End { get; }
        public double MaxEl { get; }
        public double StartAz { get; }
        public double EndAz { get; }

        public SatelliteObservation(DateTime start, DateTime end, double maxEl, double startAz, double endAz)
        {
            Start = start;
            End = end;
            MaxEl = maxEl;
            StartAz = startAz;
            EndAz = endAz;
        }
    }
}