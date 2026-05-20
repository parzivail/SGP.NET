using System;
using System.Collections.Generic;
using System.Globalization;

namespace SGPdotNET.Parsers;

/// <summary>
/// Base class providing shared parsing utilities for all OMM format parsers.
/// </summary>
public abstract class OmmParserBase
{
    /// <summary>
    /// Canonical OMM field names used across all formats.
    /// </summary>
    protected static readonly HashSet<string> KnownFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "ARG_OF_PERICENTER",
        "BSTAR", 
        "CCSDS_OMM_VERS", 
        "CENTER_NAME",
        "CLASSIFICATION_TYPE",
        "CREATION_DATE", 
        "ECCENTRICITY",
        "ELEMENT_SET_NO",
        "EPHEMERIS_TYPE", 
        "EPOCH", 
        "INCLINATION", 
        "MEAN_ANOMALY",
        "MEAN_ELEMENT_THEORY",
        "MEAN_MOTION",
        "MEAN_MOTION_DDOT",
        "MEAN_MOTION_DOT", 
        "NORAD_CAT_ID",
        "OBJECT_ID", 
        "OBJECT_NAME", 
        "ORIGINATOR",
        "RA_OF_ASC_NODE",
        "REF_FRAME", 
        "REV_AT_EPOCH", 
        "TIME_SYSTEM"
    };

    /// <summary>
    /// Populates an OmmData instance from a dictionary of key-value pairs.
    /// </summary>
    protected static OmmData PopulateFromDictionary(Dictionary<string, string> dict)
    {
        var omm = new OmmData();

        foreach (var kvp in dict)
        {
            var key = kvp.Key.Trim();
            var value = kvp.Value.Trim();

            if (string.IsNullOrEmpty(value))
                continue;

            switch (key)
            {
                case "CCSDS_OMM_VERS":
                    omm.OmmVersion = value;
                    break;
                case "CREATION_DATE":
                    omm.CreationDate = value;
                    break;
                case "ORIGINATOR":
                    omm.Originator = value;
                    break;
                case "OBJECT_NAME":
                    omm.ObjectName = value;
                    break;
                case "OBJECT_ID":
                    omm.ObjectID = value;
                    break;
                case "CENTER_NAME":
                    omm.CenterName = value;
                    break;
                case "REF_FRAME":
                    omm.RefFrame = value;
                    break;
                case "TIME_SYSTEM":
                    omm.TimeSystem = value;
                    break;
                case "MEAN_ELEMENT_THEORY":
                    omm.MeanElementTheory = value;
                    break;
                case "EPOCH":
                    omm.Epoch = ParseDateTime(value);
                    break;
                case "MEAN_MOTION":
                    omm.MeanMotion = ParseDouble(value);
                    break;
                case "ECCENTRICITY":
                    omm.Eccentricity = ParseDouble(value);
                    break;
                case "INCLINATION":
                    omm.Inclination = ParseDouble(value);
                    break;
                case "RA_OF_ASC_NODE":
                    omm.RAOfAscNode = ParseDouble(value);
                    break;
                case "ARG_OF_PERICENTER":
                    omm.ArgOfPericenter = ParseDouble(value);
                    break;
                case "MEAN_ANOMALY":
                    omm.MeanAnomaly = ParseDouble(value);
                    break;
                case "EPHEMERIS_TYPE":
                    omm.EphemerisType = ParseInt(value);
                    break;
                case "CLASSIFICATION_TYPE":
                    omm.ClassificationType = value;
                    break;
                case "NORAD_CAT_ID":
                    omm.NoradCatID = ParseUInt(value);
                    break;
                case "ELEMENT_SET_NO":
                    omm.ElementSetNo = ParseUInt(value);
                    break;
                case "REV_AT_EPOCH":
                    omm.RevAtEpoch = ParseUInt(value);
                    break;
                case "BSTAR":
                    omm.BStar = ParseDouble(value);
                    break;
                case "MEAN_MOTION_DOT":
                    omm.MeanMotionDot = ParseDouble(value);
                    break;
                case "MEAN_MOTION_DDOT":
                    omm.MeanMotionDDot = ParseDouble(value);
                    break;
            }
        }

        return omm;
    }

    /// <summary>
    /// Parses a double value, handling leading dots (e.g., ".5" -> 0.5) and scientific notation.
    /// </summary>
    protected static double ParseDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0.0;

        value = value.Trim();

        if (value.StartsWith("."))
            value = "0" + value;
        else if (value.StartsWith("-."))
            value = "-0" + value.Substring(1);

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new FormatException($"Cannot parse double value: '{value}'");
    }

    /// <summary>
    /// Parses an unsigned integer value.
    /// </summary>
    protected static uint ParseUInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        if (uint.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new FormatException($"Cannot parse unsigned integer value: '{value}'");
    }

    /// <summary>
    /// Parses a signed integer value.
    /// </summary>
    protected static int ParseInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        throw new FormatException($"Cannot parse integer value: '{value}'");
    }

    /// <summary>
    /// Parses an ISO 8601 date-time string to UTC DateTime.
    /// </summary>
    protected static DateTime ParseDateTime(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.MinValue;

        if (DateTime.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var result))
            return result;

        throw new FormatException($"Cannot parse date-time value: '{value}'");
    }

    /// <summary>
    /// Checks if an OmmData instance has the minimum required fields for SGP4 propagation.
    /// </summary>
    protected static bool IsValidForPropagation(OmmData omm)
    {
        return omm.Epoch != DateTime.MinValue &&
               omm.NoradCatID != 0 &&
               omm.MeanMotion != 0.0;
    }
}