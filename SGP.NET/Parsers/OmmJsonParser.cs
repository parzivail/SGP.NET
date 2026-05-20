using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SGPdotNET.Parsers;

/// <summary>
/// Parses OMM data from JSON format
/// </summary>
public class OmmJsonParser : OmmParserBase, IOmmParser
{
    public List<OmmData> Parse(string content)
    {
        return Parse(JsonDocument.Parse(content));
    }

    public List<OmmData> Parse(TextReader reader)
    {
        using var doc = JsonDocument.Parse(reader.ReadToEnd());
        return Parse(doc);
    }

    public List<OmmData> ParseFile(string path)
    {
        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);
        return Parse(doc);
    }

    private List<OmmData> Parse(JsonDocument doc)
    {
        var results = new List<OmmData>();

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected JSON array at root");

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
                continue;

            var dict = new Dictionary<string, string>();

            foreach (var property in element.EnumerateObject())
            {
                dict[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString(),
                    JsonValueKind.Number => property.Value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "",
                    _ => property.Value.GetRawText()
                };
            }

            var omm = PopulateFromDictionary(dict);
            if (IsValidForPropagation(omm))
            {
                results.Add(omm);
            }
        }

        return results;
    }
}