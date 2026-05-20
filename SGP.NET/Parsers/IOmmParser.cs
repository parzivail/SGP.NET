using System.Collections.Generic;
using System.IO;

namespace SGPdotNET.Parsers;

/// <summary>
/// Interface for parsers that read OMM data from various formats.
/// </summary>
public interface IOmmParser
{
    /// <summary>
    /// Parses OMM data from a string.
    /// </summary>
    List<OmmData> Parse(string content);

    /// <summary>
    /// Parses OMM data from a text reader.
    /// </summary>
    List<OmmData> Parse(TextReader reader);

    /// <summary>
    /// Parses OMM data from a file path.
    /// </summary>
    List<OmmData> ParseFile(string path);
}