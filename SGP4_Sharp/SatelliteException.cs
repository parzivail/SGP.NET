using System;

namespace SGP4_Sharp
{
    public class SatelliteException : Exception
    {
        public SatelliteException(string e) : base(e)
        {
        }
    }
}