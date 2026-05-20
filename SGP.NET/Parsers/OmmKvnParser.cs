using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SGPdotNET.Parsers;

/// <summary>
/// Parses OMM data from KVN format. Blocks are separated by blank lines.
/// </summary>
public class OmmKvnParser : OmmParserBase, IOmmParser
{
    /// <inheritdoc />
    public List<OmmData> Parse(string content)
    {
        using var reader = new StringReader(content);
        return Parse(reader);
    }

    /// <inheritdoc />
    public List<OmmData> Parse(TextReader reader)
    {
        var results = new List<OmmData>();
        var currentDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();

            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            if (!line.Contains("="))
                continue;

            var eqIndex = line.IndexOf('=');
            var key = line.Substring(0, eqIndex).Trim();
            var value = line.Substring(eqIndex + 1).Trim();

            if (string.Equals(key, "CCSDS_OMM_VERS", StringComparison.OrdinalIgnoreCase) && currentDict.Count > 0)
            {
                var omm = PopulateFromDictionary(currentDict);
                if (IsValidForPropagation(omm))
                {
                    results.Add(omm);
                }
                currentDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(key))
            {
                currentDict[key] = value;
            }
        }

        if (currentDict.Count > 0)
        {
            var omm = PopulateFromDictionary(currentDict);
            if (IsValidForPropagation(omm))
            {
                results.Add(omm);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public List<OmmData> ParseFile(string path)
    {
        using var reader = File.OpenText(path);
        return Parse(reader);
    }
}