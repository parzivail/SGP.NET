using System;

namespace SGPdotNET
{
    /// <summary>
    ///     Exception thrown by the propagator during initialization when a satellite has erroneous values
    /// </summary>
    public class SatelliteException : Exception
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="e">Message for the exception</param>
        public SatelliteException(string e) : base(e)
        {
        }
    }
}