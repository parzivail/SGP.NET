using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
			var value = StripUnits(line.Substring(eqIndex + 1).Trim());

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

	/// <summary>
	/// Removes units in square brackets after a numeric value, e.g. "16.05 [rev/day]" (CCSDS 502.0-B-3, 7.7.1.1).
	/// Text values such as an object name ending in brackets are left as they are.
	/// </summary>
	private static string StripUnits(string value)
	{
		if (!value.EndsWith("]", StringComparison.Ordinal))
			return value;

		var open = value.LastIndexOf('[');
		if (open < 1 || !char.IsWhiteSpace(value[open - 1]))
			return value;

		var number = value.Substring(0, open).TrimEnd();
		return double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out _) ? number : value;
	}
}