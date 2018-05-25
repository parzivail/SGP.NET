namespace SGPdotNET.Exception
{
    /// <inheritdoc />
    /// <summary>
    ///     Exception thrown by the propagator during initialization when a satellite has erroneous values
    /// </summary>
    public class SatelliteException : System.Exception
    {
        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="e">Message for the exception</param>
        public SatelliteException(string e) : base(e)
        {
        }
    }
}