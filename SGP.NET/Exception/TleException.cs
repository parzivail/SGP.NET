namespace SGPdotNET.Exception
{
    /// <summary>
    ///     Exception thrown by the TLE parser when a TLE has invalid values
    /// </summary>
    public class TleException : System.Exception
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