using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SGPdotNET.Parsers;

/// <summary>
/// Parses OMM data from CCSDS NDM/XML
/// </summary>
public class OmmXmlParser : OmmParserBase, IOmmParser
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

        using var xmlReader = XmlReader.Create(reader, new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true,
            DtdProcessing = DtdProcessing.Ignore
        });

        while (xmlReader.Read())
        {
            if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "omm")
            {
                var dict = ParseOmmElement(xmlReader);
                var omm = PopulateFromDictionary(dict);
                if (IsValidForPropagation(omm))
                {
                    results.Add(omm);
                }
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

    private Dictionary<string, string> ParseOmmElement(XmlReader reader)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (reader.MoveToAttribute("version"))
        {
            dict["CCSDS_OMM_VERS"] = reader.Value;
        }

        reader.MoveToElement();

        if (!reader.Read())
            return dict;

        while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "omm")
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.LocalName)
                {
                    case "header":
                        ParseHeaderElement(reader, dict);
                        break;
                    case "body":
                        ParseBodyElement(reader, dict);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
            else if (!reader.Read())
            {
                break;
            }
        }

        return dict;
    }

    private static void ParseHeaderElement(XmlReader reader, Dictionary<string, string> dict)
    {
        if (!reader.Read())
            return;

        while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "header")
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var elementName = reader.LocalName;
                var value = reader.ReadElementContentAsString();

                if (!string.IsNullOrEmpty(value))
                {
                    dict[elementName] = value;
                }
            }
            else if (!reader.Read())
            {
                break;
            }
        }
    }

    private static void ParseBodyElement(XmlReader reader, Dictionary<string, string> dict)
    {
        if (!reader.Read())
            return;

        while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "body")
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "segment")
            {
                ParseSegmentElement(reader, dict);
            }
            else if (!reader.Read())
            {
                break;
            }
        }
    }

    private static void ParseSegmentElement(XmlReader reader, Dictionary<string, string> dict)
    {
        if (!reader.Read())
            return;

        while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "segment")
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.LocalName)
                {
                    case "metadata":
                        ParseMetadataElement(reader, dict);
                        break;
                    case "data":
                        ParseDataElement(reader, dict);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
            else if (!reader.Read())
            {
                break;
            }
        }
    }

    private static void ParseMetadataElement(XmlReader reader, Dictionary<string, string> dict)
    {
        if (!reader.Read())
            return;

        while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "metadata")
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var elementName = reader.LocalName;
                var value = reader.ReadElementContentAsString();

                if (!string.IsNullOrEmpty(value))
                {
                    dict[elementName] = value;
                }
            }
            else if (!reader.Read())
            {
                break;
            }
        }
    }

    private static void ParseDataElement(XmlReader reader, Dictionary<string, string> dict)
    {
        if (!reader.Read())
            return;

        while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "data")
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.LocalName)
                {
                    case "meanElements":
                        ParseMeanElements(reader, dict);
                        break;
                    case "tleParameters":
                        ParseTleParameters(reader, dict);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
            else if (!reader.Read())
            {
                break;
            }
        }
    }

    private static void ParseMeanElements(XmlReader reader, Dictionary<string, string> dict)
    {
        if (!reader.Read())
            return;

        while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "meanElements")
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var elementName = reader.LocalName;
                var value = reader.ReadElementContentAsString();

                if (!string.IsNullOrEmpty(value))
                {
                    dict[elementName] = value;
                }
            }
            else if (!reader.Read())
            {
                break;
            }
        }
    }

    private static void ParseTleParameters(XmlReader reader, Dictionary<string, string> dict)
    {
        if (!reader.Read())
            return;

        while (reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "tleParameters")
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var elementName = reader.LocalName;
                var value = reader.ReadElementContentAsString();

                if (!string.IsNullOrEmpty(value))
                {
                    dict[elementName] = value;
                }
            }
            else if (!reader.Read())
            {
                break;
            }
        }
    }
}