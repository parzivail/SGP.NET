using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;

namespace SGPdotNET
{
    /// <inheritdoc cref="ITleProvider" />
    /// <summary>
    ///     Provides a class to retrieve TLEs from a remote network resource
    /// </summary>
    public class RemoteTleProvider : ITleProvider, IDisposable
    {
        private readonly IEnumerable<Url> _sources;
        private readonly bool _threeLine;
        private readonly WebClient _webClient;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sources">The sources that should be queried</param>
        /// <param name="threeLine">True if the TLEs contain a third, preceding name line (3le format)</param>
        public RemoteTleProvider(IEnumerable<Url> sources, bool threeLine)
        {
            _sources = sources;
            _threeLine = threeLine;
            _webClient = new WebClient();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _webClient?.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Queries the source and retrieves a two-line set for the specified satellite
        /// </summary>
        /// <param name="satelliteId">The satellite to retrieve</param>
        /// <returns>The remote TLE for the specified satellite</returns>
        public Tle GetTle(int satelliteId)
        {
            foreach (var source in _sources)
            {
                var file = _webClient.DownloadString(source.Value);
                var elementSets = file // take the file
                    .Replace("\r\n", "\n") // normalize line endings
                    .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries) // split into lines
                    .Select((value, index) => new {PairNum = index / (_threeLine ? 3 : 2), value}) // pair TLEs by index
                    .GroupBy(pair => pair.PairNum) // group TLEs by index
                    .Select(grp => grp.Select(g => g.value).ToArray()) // select groups of TLEs
                    .Select(s =>
                        new Tle((s[0].StartsWith("0 ") ? s[0].Substring(2) : s[0]).Trim(), s[1],
                            s[2])) // convert lines into TLEs
                    .ToList();
                if (elementSets.All(tle => tle.NoradNumber != satelliteId))
                    continue;

                return elementSets.First(tle => tle.NoradNumber == satelliteId);
            }

            return null;
        }
    }
}