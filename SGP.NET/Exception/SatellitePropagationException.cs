namespace SGPdotNET.Exception
{
    /// <inheritdoc />
    /// <summary>
    ///     Exception thrown by the propagator when a satellite has erroneous values
    /// </summary>
    public class SatellitePropagationException : System.Exception
    {
        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="e">Message for the exception</param>
        public SatellitePropagationException(string e) : base(e)
        {
        }
    }
}