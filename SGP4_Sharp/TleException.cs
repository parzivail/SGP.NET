using System;

namespace SGP4_Sharp
{
    public class TleException : Exception
    {
        public TleException(string e) : base(e)
        {
        }
    }
}