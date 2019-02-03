using System.Collections.Generic;

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
        /// <returns>The TLE for the specified satellite</returns>
        Tle GetTle(int satelliteId);

        /// <summary>
        ///     Queries the source and retrieves all two-line sets
        /// </summary>
        /// <returns>All known TLEs</returns>
        Dictionary<int, Tle> GetTles();
    }
}