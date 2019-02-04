using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SGPdotNET.TLE
{
    /// <inheritdoc cref="ITleProvider" />
    /// <summary>
    ///     Provides a class to retrieve TLEs from a local resource
    /// </summary>
    public class LocalTleProvider : ITleProvider
    {
        private Dictionary<int, Tle> _tles;

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sourceFilename">The source that should be loaded</param>
        /// <param name="threeLine">True if the TLEs contain a third, preceding name line (3le format)</param>
        public LocalTleProvider(string sourceFilename, bool threeLine)
        {
            LoadTles(sourceFilename, threeLine);
        }

        /// <inheritdoc />
        public Tle GetTle(int satelliteId)
        {
            return !_tles.ContainsKey(satelliteId) ? null : _tles[satelliteId];
        }

        /// <inheritdoc />
        public Dictionary<int, Tle> GetTles()
        {
            return _tles;
        }

        private void LoadTles(string sourceFilename, bool threeLine)
        {
            using (var sr = new StreamReader(sourceFilename))
            {
                var restOfFile = sr.ReadToEnd()
                    .Replace("\r\n", "\n") // normalize line endings
                    .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries); // split into lines

                var elementSets = Tle.ParseElements(restOfFile, threeLine);

                _tles = elementSets.ToDictionary(elementSet => (int) elementSet.NoradNumber);
            }
        }
    }
}