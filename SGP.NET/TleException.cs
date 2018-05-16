using System;

namespace SGPdotNET
{
    /// <summary>
    ///     Exception thrown by the TLE parser when a TLE has erroneous values
    /// </summary>
    public class TleException : Exception
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="e">Message for the exception</param>
        public TleException(string e) : base(e)
        {
        }
    }
}