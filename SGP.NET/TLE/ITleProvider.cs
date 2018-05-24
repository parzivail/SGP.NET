namespace SGPdotNET.TLE
{
    /// <summary>
    ///     Provides a class to retrieve TLEs from a resource
    /// </summary>
    public interface ITleProvider
    {
        /// <summary>
        ///     Queries the source and retrieves a two-line set for the specified satellite
        /// </summary>
        /// <param name="satelliteId">The satellite to retrieve</param>
        /// <returns>The remote TLE for the specified satellite</returns>
        Tle GetTle(int satelliteId);
    }
}