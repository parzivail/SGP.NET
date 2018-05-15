using System;

namespace SGP4_Sharp
{
    internal class SatelliteException : Exception
    {
        public SatelliteException(string e) : base(e)
        {
        }
    }
}