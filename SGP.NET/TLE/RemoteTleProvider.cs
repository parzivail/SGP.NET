using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly TimeSpan _maxAge;
        private readonly IEnumerable<Url> _sources;
        private readonly bool _threeLine;

        private List<Tle> _cachedTles;
        private DateTime _lastRefresh = DateTime.MinValue;

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
            _threeLine = threeLine;
            _maxAge = maxAge;
            CacheRemoteTles(false);
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
            return _cachedTles.FirstOrDefault(tle => tle.NoradNumber == satelliteId);
        }

        /// <summary>
        ///     Queries the cache (updating if needed) and retrieves a two-line sets for all remote satellites
        /// </summary>
        /// <returns>The remote TLEs for the all remote satellites</returns>
        public List<Tle> GetTles()
        {
            CacheRemoteTles();
            return _cachedTles;
        }

        private void CacheRemoteTles(bool async = true)
        {
            if (DateTime.Now < _lastRefresh + _maxAge)
                return;

            _cachedTles = new List<Tle>();
            if (async)
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    GetTlesFromRemote();
                }).Start();
            else
                GetTlesFromRemote();

            _lastRefresh = DateTime.Now;
        }

        private void GetTlesFromRemote()
        {
            using (var wc = new WebClient())
            {
                foreach (var source in _sources)
                {
                    var file = wc.DownloadString(source.Value);
                    var elementSets = file // take the file
                        .Replace("\r\n", "\n") // normalize line endings
                        .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries) // split into lines
                        .Select((value, index) =>
                            new {PairNum = index / (_threeLine ? 3 : 2), value}) // pair TLEs by index
                        .GroupBy(pair => pair.PairNum) // group TLEs by index
                        .Select(grp => grp.Select(g => g.value).ToArray()) // select groups of TLEs
                        .Select(s =>
                            new Tle((s[0].StartsWith("0 ") ? s[0].Substring(2) : s[0]).Trim(), s[1],
                                s[2])) // convert lines into TLEs
                        .ToList();

                    _cachedTles.AddRange(elementSets);
                }
            }
        }
    }
}