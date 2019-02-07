using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace SGPdotNET.TLE
{
    /// <inheritdoc cref="RemoteTleProvider" />
    /// <summary>
    ///     Provides a class to retrieve TLEs from a remote network resource
    /// </summary>
    public class CachingRemoteTleProvider : RemoteTleProvider
    {
        private readonly string _localFilename;

        /// <inheritdoc />
        /// <summary>
        ///     Constructor, defaulting to max-age of 24 hours
        /// </summary>
        /// <param name="threeLine">True if the TLEs contain a third, preceding name line (3le format)</param>
        /// <param name="localFilename">The file in which the TLEs will be locally cached</param>
        /// <param name="sources">The sources that should be queried</param>
        public CachingRemoteTleProvider(bool threeLine, string localFilename, params Url[] sources)
            : this(threeLine, TimeSpan.FromDays(1),
                localFilename, sources)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="threeLine">True if the TLEs contain a third, preceding name line (3le format)</param>
        /// <param name="maxAge">The maximum time to keep TLEs cached before updating them from the remote</param>
        /// <param name="localFilename">The file in which the TLEs will be locally cached</param>
        /// <param name="sources">The sources that should be queried</param>
        public CachingRemoteTleProvider(bool threeLine, TimeSpan maxAge, string localFilename,
            params Url[] sources) : base(threeLine, maxAge, sources)
        {
            _localFilename = localFilename;
        }

        internal override Dictionary<int, Tle> FetchNewTles()
        {
            if (File.Exists(_localFilename))
                using (var sr = new StreamReader(_localFilename))
                {
                    var dateLine = sr.ReadLine();

                    if (DateTime.TryParse(dateLine, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal,
                            out var date) && DateTime.UtcNow - date < MaxAge)
                    {
                        LastRefresh = date;
                        var restOfFile = sr.ReadToEnd()
                            .Replace("\r\n", "\n") // normalize line endings
                            .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries); // split into lines

                        var elementSets = Tle.ParseElements(restOfFile, ThreeLine);

                        return elementSets.ToDictionary(elementSet => (int) elementSet.NoradNumber);
                    }
                }

            var tles = base.FetchNewTles();
            WriteOutNewTles(tles);

            return tles;
        }

        private void WriteOutNewTles(Dictionary<int, Tle> tles)
        {
            var sb = new StringBuilder();
            sb.AppendLine(DateTime.UtcNow.ToString("u"));
            foreach (var tle in tles)
            {
                sb.AppendLine(tle.Value.Name);
                sb.AppendLine(tle.Value.Line1);
                sb.AppendLine(tle.Value.Line2);
            }

            File.WriteAllText(_localFilename, sb.ToString(), Encoding.UTF8);
        }
    }
}