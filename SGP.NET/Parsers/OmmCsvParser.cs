using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SGPdotNET.Parsers;

/// <summary>
/// Parses OMM data from CSV format.
/// </summary>
public class OmmCsvParser : OmmParserBase, IOmmParser
{
    public List<OmmData> Parse(string content)
    {
        using var reader = new StringReader(content);
        return Parse(reader);
    }

    public List<OmmData> Parse(TextReader reader)
    {
        var results = new List<OmmData>();

        var headerLine = reader.ReadLine();
        if (headerLine == null)
            return results;

        var headers = ParseCsvLine(headerLine);
        var fieldIndices = new int[headers.Length];

        for (var i = 0; i < headers.Length; i++)
        {
            var header = headers[i].Trim();
            fieldIndices[i] = KnownFields.Contains(header) ? i : -1;
        }

        string line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            var values = ParseCsvLine(line);
            var dict = new Dictionary<string, string>();

            for (var i = 0; i < fieldIndices.Length; i++)
            {
                if (fieldIndices[i] >= 0 && fieldIndices[i] < values.Length)
                {
                    dict[headers[fieldIndices[i]].Trim()] = values[fieldIndices[i]];
                }
            }

            var omm = PopulateFromDictionary(dict);
            if (IsValidForPropagation(omm))
            {
                results.Add(omm);
            }
        }

        return results;
    }

    public List<OmmData> ParseFile(string path)
    {
        using var reader = File.OpenText(path);
        return Parse(reader);
    }

    private static string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            switch (c)
            {
                case '"' when inQuotes && i + 1 < line.Length && line[i + 1] == '"':
                    current.Append('"');
                    i++;
                    break;
                case '"':
                    inQuotes = !inQuotes;
                    break;
                case ',' when !inQuotes:
                    values.Add(current.ToString());
                    current.Clear();
                    break;
                default:
                    current.Append(c);
                    break;
            }
        }

        values.Add(current.ToString());
        return values.ToArray();
    }
}