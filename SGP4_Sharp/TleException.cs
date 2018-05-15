using System;

namespace SGP4_Sharp
{
    internal class TleException : Exception
    {
        public TleException(string e) : base(e)
        {
        }
    }
}