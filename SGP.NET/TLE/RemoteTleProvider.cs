using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Policy;
using System.Threading;

namespace SGPdotNET.TLE
{
    /// <inheritdoc cref="ITleProvider" />
    /// <summary>
    ///     Provides a class to retrieve TLEs from a remote network resource
    /// </summary>
    public class RemoteTleProvider : ITleProvider
    {
        private readonly object _lock = new object();
        private readonly IEnumerable<Url> _sources;

        internal readonly TimeSpan MaxAge;
        internal readonly bool ThreeLine;

        private Dictionary<int, Tle> _cachedTles;

        internal DateTime LastRefresh = DateTime.MinValue;

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sources">The sources that should be queried</param>
        /// <param name="threeLine">True if the TLEs contain a third, preceding name line (3le format)</param>
        public RemoteTleProvider(IEnumerable<Url> sources, bool threeLine) : this(sources, threeLine,
            TimeSpan.FromDays(1))
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sources">The sources that should be queried</param>
        /// <param name="threeLine">True if the TLEs contain a third, preceding name line (3le format)</param>
        /// <param name="maxAge">The maximum time to keep TLEs cached before updating them from the remote</param>
        public RemoteTleProvider(IEnumerable<Url> sources, bool threeLine, TimeSpan maxAge)
        {
            _sources = sources;
            ThreeLine = threeLine;
            MaxAge = maxAge;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Queries the cache (updating if needed) and retrieves a two-line set for the specified satellite
        /// </summary>
        /// <param name="satelliteId">The satellite to retrieve</param>
        /// <returns>The remote TLE for the specified satellite</returns>
        public Tle GetTle(int satelliteId)
        {
            CacheRemoteTles();
            return _cachedTles.ContainsKey(satelliteId) ? _cachedTles[satelliteId] : null;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Queries the cache (updating if needed) and retrieves a two-line sets for all remote satellites
        /// </summary>
        /// <returns>The remote TLEs for the all remote satellites, as a pair of of satellite ID and TLE</returns>
        public Dictionary<int, Tle> GetTles()
        {
            CacheRemoteTles();
            return _cachedTles;
        }

        private void CacheRemoteTles()
        {
            if (DateTime.UtcNow < LastRefresh + MaxAge)
                return;

            _cachedTles = FetchNewTles();

            LastRefresh = DateTime.UtcNow;
        }

        internal virtual Dictionary<int, Tle> FetchNewTles()
        {
            var tles = new Dictionary<int, Tle>();
            using (var wc = new WebClient())
            {
                foreach (var source in _sources)
                {
                    var file = wc.DownloadString(source.Value)
                        .Replace("\r\n", "\n") // normalize line endings
                        .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries); // split into lines

                    var elementSets = Tle.ParseElements(file, true);

                    foreach (var elementSet in elementSets)
                        tles.Add((int) elementSet.NoradNumber, elementSet);
                }
            }

            return tles;
        }
    }
}