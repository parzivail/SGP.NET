using System;

namespace SGPdotNET.Parsers;

/// <summary>
/// Model for CCSDS OMM data
/// </summary>
public class OmmData
{
    /// <summary>
    /// CCSDS OMM version (e.g., "2.0")
    /// </summary>
    public string OmmVersion { get; set; }

    /// <summary>
    /// Name of the object (e.g., "ISS (ZARYA)")
    /// </summary>
    public string ObjectName { get; set; }

    /// <summary>
    /// International designator (e.g., "1998-067A")
    /// </summary>
    public string ObjectID { get; set; }

    /// <summary>
    /// NORAD catalog ID
    /// </summary>
    public uint NoradCatID { get; set; }

    /// <summary>
    /// Classification type (e.g., "U" for unclassified)
    /// </summary>
    public string ClassificationType { get; set; }

    /// <summary>
    /// Element set number
    /// </summary>
    public uint ElementSetNo { get; set; }

    /// <summary>
    /// Epoch of the mean elements (UTC)
    /// </summary>
    public DateTime Epoch { get; set; }

    /// <summary>
    /// Mean motion in revolutions per day
    /// </summary>
    public double MeanMotion { get; set; }

    /// <summary>
    /// Eccentricity (dimensionless)
    /// </summary>
    public double Eccentricity { get; set; }

    /// <summary>
    /// Inclination in degrees
    /// </summary>
    public double Inclination { get; set; }

    /// <summary>
    /// Right ascension of ascending node in degrees
    /// </summary>
    public double RAOfAscNode { get; set; }

    /// <summary>
    /// Argument of pericenter in degrees
    /// </summary>
    public double ArgOfPericenter { get; set; }

    /// <summary>
    /// Mean anomaly in degrees
    /// </summary>
    public double MeanAnomaly { get; set; }

    /// <summary>
    /// Ephemeris type (0 = SGP4)
    /// </summary>
    public int EphemerisType { get; set; }

    /// <summary>
    /// BSTAR drag term
    /// </summary>
    public double BStar { get; set; }

    /// <summary>
    /// First derivative of mean motion (rev/day^2)
    /// </summary>
    public double MeanMotionDot { get; set; }

    /// <summary>
    /// Second derivative of mean motion (rev/day^3)
    /// </summary>
    public double MeanMotionDDot { get; set; }

    /// <summary>
    /// Revolution number at epoch
    /// </summary>
    public uint RevAtEpoch { get; set; }

    /// <summary>
    /// Name of the central body (e.g., "EARTH")
    /// </summary>
    public string CenterName { get; set; }

    /// <summary>
    /// Reference frame (e.g., "TEME")
    /// </summary>
    public string RefFrame { get; set; }

    /// <summary>
    /// Time system (e.g., "UTC")
    /// </summary>
    public string TimeSystem { get; set; }

    /// <summary>
    /// Mean element theory (e.g., "SGP4")
    /// </summary>
    public string MeanElementTheory { get; set; }

    /// <summary>
    /// Creation date of the OMM file
    /// </summary>
    public string CreationDate { get; set; }

    /// <summary>
    /// Originator of the OMM data
    /// </summary>
    public string Originator { get; set; }
}