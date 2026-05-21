using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using SGPdotNET.Parsers;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for OMM CSV parser edge cases: leading dots, negative values, scientific notation, empty lines.
/// </summary>
[TestClass]
public sealed class OmmCsvParserTests
{
    /// <summary>
    /// Verifies leading-dot eccentricity parsed correctly (e.g., ".00418613").
    /// </summary>
    [TestMethod]
    public void LeadingDotEccentricity_ParsedCorrectly()
    {
        // Arrange
        const string csv = "OBJECT_NAME,EPOCH,MEAN_MOTION,ECCENTRICITY,INCLINATION,RA_OF_ASC_NODE,ARG_OF_PERICENTER,MEAN_ANOMALY,EPHEMERIS_TYPE,CLASSIFICATION_TYPE,NORAD_CAT_ID,ELEMENT_SET_NO,REV_AT_EPOCH,BSTAR,MEAN_MOTION_DOT,MEAN_MOTION_DDOT\n"
                           + "TEST,2026-05-20T08:56:33.249984,14.46106951,.00418613,24.9710,19.3104,203.4028,317.7975,0,U,22490,999,75708,.36140333E-4,.319E-5,0";

        // Act
        var data = new OmmCsvParser().Parse(csv);

        // Assert
        Assert.HasCount(1, data);
        Assert.AreEqual(0.00418613, data[0].Eccentricity, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies negative BStar parsed correctly (e.g., "-.39E-6").
    /// </summary>
    [TestMethod]
    public void NegativeBStar_ParsedCorrectly()
    {
        // Arrange
        const string csv = "OBJECT_NAME,EPOCH,MEAN_MOTION,ECCENTRICITY,INCLINATION,RA_OF_ASC_NODE,ARG_OF_PERICENTER,MEAN_ANOMALY,EPHEMERIS_TYPE,CLASSIFICATION_TYPE,NORAD_CAT_ID,ELEMENT_SET_NO,REV_AT_EPOCH,BSTAR,MEAN_MOTION_DOT,MEAN_MOTION_DDOT\n"
                           + "TEST,2026-05-20T08:56:33.249984,14.46106951,.00418613,24.9710,19.3104,203.4028,317.7975,0,U,22490,999,75708,-.39E-6,.319E-5,0";

        // Act
        var data = new OmmCsvParser().Parse(csv);

        // Assert
        Assert.AreEqual(-3.9e-7, data[0].BStar, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies scientific notation mean motion parsed correctly.
    /// </summary>
    [TestMethod]
    public void ScientificNotationMeanMotion_ParsedCorrectly()
    {
        // Arrange
        const string csv = "OBJECT_NAME,EPOCH,MEAN_MOTION,ECCENTRICITY,INCLINATION,RA_OF_ASC_NODE,ARG_OF_PERICENTER,MEAN_ANOMALY,EPHEMERIS_TYPE,CLASSIFICATION_TYPE,NORAD_CAT_ID,ELEMENT_SET_NO,REV_AT_EPOCH,BSTAR,MEAN_MOTION_DOT,MEAN_MOTION_DDOT\n"
                           + "TEST,2026-05-20T08:56:33.249984,1.446106951E1,.00418613,24.9710,19.3104,203.4028,317.7975,0,U,22490,999,75708,.36140333E-4,.319E-5,0";

        // Act
        var data = new OmmCsvParser().Parse(csv);

        // Assert
        Assert.AreEqual(14.46106951, data[0].MeanMotion, 1e-8);
    }

    /// <summary>
    /// Verifies empty lines are skipped.
    /// </summary>
    [TestMethod]
    public void EmptyLines_Skipped()
    {
        // Arrange
        const string csv = "OBJECT_NAME,EPOCH,MEAN_MOTION,ECCENTRICITY,INCLINATION,RA_OF_ASC_NODE,ARG_OF_PERICENTER,MEAN_ANOMALY,EPHEMERIS_TYPE,CLASSIFICATION_TYPE,NORAD_CAT_ID,ELEMENT_SET_NO,REV_AT_EPOCH,BSTAR,MEAN_MOTION_DOT,MEAN_MOTION_DDOT\n"
                           + "\n"
                           + "TEST,2026-05-20T08:56:33.249984,14.46106951,.00418613,24.9710,19.3104,203.4028,317.7975,0,U,22490,999,75708,.36140333E-4,.319E-5,0\n"
                           + "\n";

        // Act
        var data = new OmmCsvParser().Parse(csv);

        // Assert
        Assert.HasCount(1, data);
    }

    /// <summary>
    /// Verifies header-only file returns empty list.
    /// </summary>
    [TestMethod]
    public void HeaderOnly_ReturnsEmptyList()
    {
        // Arrange
        const string csv = "OBJECT_NAME,EPOCH,MEAN_MOTION,ECCENTRICITY,INCLINATION,RA_OF_ASC_NODE,ARG_OF_PERICENTER,MEAN_ANOMALY,EPHEMERIS_TYPE,CLASSIFICATION_TYPE,NORAD_CAT_ID,ELEMENT_SET_NO,REV_AT_EPOCH,BSTAR,MEAN_MOTION_DOT,MEAN_MOTION_DDOT\n";

        // Act
        var data = new OmmCsvParser().Parse(csv);

        // Assert
        Assert.IsEmpty(data);
    }
}

/// <summary>
/// Tests for OMM JSON parser edge cases: null fields, missing fields, non-array root, mixed types.
/// </summary>
[TestClass]
public sealed class OmmJsonParserTests
{
    /// <summary>
    /// Verifies null fields are handled gracefully.
    /// </summary>
    [TestMethod]
    public void NullFields_HandledGracefully()
    {
        // Arrange
        const string json = "[{\"OBJECT_NAME\":\"TEST\",\"EPOCH\":\"2026-05-20T08:56:33.249984\",\"MEAN_MOTION\":14.46,\"ECCENTRICITY\":0.004,\"INCLINATION\":25.0,\"RA_OF_ASC_NODE\":19.3,\"ARG_OF_PERICENTER\":203.4,\"MEAN_ANOMALY\":317.8,\"EPHEMERIS_TYPE\":0,\"CLASSIFICATION_TYPE\":\"U\",\"NORAD_CAT_ID\":22490,\"ELEMENT_SET_NO\":999,\"REV_AT_EPOCH\":75708,\"BSTAR\":0.000036,\"MEAN_MOTION_DOT\":0.000003,\"MEAN_MOTION_DDOT\":null}]";

        // Act
        var data = new OmmJsonParser().Parse(json);

        // Assert
        Assert.HasCount(1, data);
        Assert.AreEqual(22490u, data[0].NoradCatID);
    }

    /// <summary>
    /// Verifies missing optional fields use defaults.
    /// </summary>
    [TestMethod]
    public void MissingOptionalFields_UseDefaults()
    {
        // Arrange
        const string json = "[{\"OBJECT_NAME\":\"TEST\",\"EPOCH\":\"2026-05-20T08:56:33.249984\",\"MEAN_MOTION\":14.46,\"ECCENTRICITY\":0.004,\"INCLINATION\":25.0,\"RA_OF_ASC_NODE\":19.3,\"ARG_OF_PERICENTER\":203.4,\"MEAN_ANOMALY\":317.8,\"EPHEMERIS_TYPE\":0,\"CLASSIFICATION_TYPE\":\"U\",\"NORAD_CAT_ID\":22490,\"ELEMENT_SET_NO\":999,\"REV_AT_EPOCH\":75708,\"BSTAR\":0.000036,\"MEAN_MOTION_DOT\":0.000003}]";

        // Act
        var data = new OmmJsonParser().Parse(json);

        // Assert
        Assert.HasCount(1, data);
        Assert.AreEqual(0.0, data[0].MeanMotionDDot); // missing field defaults to 0
    }

    /// <summary>
    /// Verifies non-array root throws JsonException.
    /// </summary>
    [TestMethod]
    public void NonArrayRoot_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"OBJECT_NAME\":\"TEST\"}";

        // Act & Assert
        try
        {
            _ = new OmmJsonParser().Parse(json);
            Assert.Fail("Expected JsonException");
        }
        catch (JsonException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Verifies mixed value types (string, number, boolean, null) handled.
    /// </summary>
    [TestMethod]
    public void MixedValueTypes_Handled()
    {
        // Arrange
        const string json = "[{\"OBJECT_NAME\":\"TEST\",\"EPOCH\":\"2026-05-20T08:56:33.249984\",\"MEAN_MOTION\":14.46,\"ECCENTRICITY\":0.004,\"INCLINATION\":25.0,\"RA_OF_ASC_NODE\":19.3,\"ARG_OF_PERICENTER\":203.4,\"MEAN_ANOMALY\":317.8,\"EPHEMERIS_TYPE\":0,\"CLASSIFICATION_TYPE\":\"U\",\"NORAD_CAT_ID\":22490,\"ELEMENT_SET_NO\":999,\"REV_AT_EPOCH\":75708,\"BSTAR\":0.000036,\"MEAN_MOTION_DOT\":0.000003,\"MEAN_MOTION_DDOT\":0,\"unknown_bool\":true,\"unknown_null\":null}]";

        // Act
        var data = new OmmJsonParser().Parse(json);

        // Assert
        Assert.HasCount(1, data);
        Assert.AreEqual(22490u, data[0].NoradCatID);
    }

    /// <summary>
    /// Verifies empty array returns empty list.
    /// </summary>
    [TestMethod]
    public void EmptyArray_ReturnsEmptyList()
    {
        // Arrange
        const string json = "[]";

        // Act
        var data = new OmmJsonParser().Parse(json);

        // Assert
        Assert.IsEmpty(data);
    }
}

/// <summary>
/// Tests for OMM KVN parser edge cases: multi-section records, trailing blank lines, CCSDS_OMM_VERS boundaries.
/// </summary>
[TestClass]
public sealed class OmmKvnParserTests
{
    /// <summary>
    /// Verifies multi-section records (header/metadata/data separated by blank lines) parsed as one satellite.
    /// </summary>
    [TestMethod]
    public void MultiSectionRecord_ParsedAsOneSatellite()
    {
        // Arrange
        const string kvn = "CCSDS_OMM_VERS = 2.0\n"
                  + "\n"
                  + "OBJECT_NAME    = TEST\n"
                  + "OBJECT_ID      = 2026-001A\n"
                  + "CENTER_NAME    = EARTH\n"
                  + "REF_FRAME      = TEME\n"
                  + "TIME_SYSTEM    = UTC\n"
                  + "MEAN_ELEMENT_THEORY = SGP4\n"
                  + "\n"
                  + "EPOCH          = 2026-05-20T08:56:33.249984\n"
                  + "MEAN_MOTION    = 14.46106951\n"
                  + "ECCENTRICITY   = .00418613\n"
                  + "INCLINATION    = 24.9710\n"
                  + "RA_OF_ASC_NODE = 19.3104\n"
                  + "ARG_OF_PERICENTER = 203.4028\n"
                  + "MEAN_ANOMALY   = 317.7975\n"
                  + "\n"
                  + "EPHEMERIS_TYPE = 0\n"
                  + "CLASSIFICATION_TYPE = U\n"
                  + "NORAD_CAT_ID   = 22490\n"
                  + "ELEMENT_SET_NO = 999\n"
                  + "REV_AT_EPOCH   = 75708\n"
                  + "BSTAR          = .36140333E-4\n"
                  + "MEAN_MOTION_DOT = .319E-5\n"
                  + "MEAN_MOTION_DDOT = 0\n";

        // Act
        var data = new OmmKvnParser().Parse(kvn);

        // Assert
        Assert.HasCount(1, data);
        Assert.AreEqual("TEST", data[0].ObjectName);
        Assert.AreEqual(22490u, data[0].NoradCatID);
    }

    /// <summary>
    /// Verifies trailing blank lines don't produce extra records.
    /// </summary>
    [TestMethod]
    public void TrailingBlankLines_NoExtraRecords()
    {
        // Arrange
        const string kvn = "CCSDS_OMM_VERS = 2.0\n"
                  + "\n"
                  + "OBJECT_NAME    = TEST\n"
                  + "EPOCH          = 2026-05-20T08:56:33.249984\n"
                  + "MEAN_MOTION    = 14.46\n"
                  + "ECCENTRICITY   = .004\n"
                  + "INCLINATION    = 25.0\n"
                  + "RA_OF_ASC_NODE = 19.3\n"
                  + "ARG_OF_PERICENTER = 203.4\n"
                  + "MEAN_ANOMALY   = 317.8\n"
                  + "\n"
                  + "EPHEMERIS_TYPE = 0\n"
                  + "CLASSIFICATION_TYPE = U\n"
                  + "NORAD_CAT_ID   = 22490\n"
                  + "ELEMENT_SET_NO = 999\n"
                  + "REV_AT_EPOCH   = 75708\n"
                  + "BSTAR          = .36E-4\n"
                  + "MEAN_MOTION_DOT = .32E-5\n"
                  + "MEAN_MOTION_DDOT = 0\n"
                  + "\n"
                  + "\n";

        // Act
        var data = new OmmKvnParser().Parse(kvn);

        // Assert
        Assert.HasCount(1, data);
    }

    /// <summary>
    /// Verifies CCSDS_OMM_VERS boundary correctly splits records.
    /// </summary>
    [TestMethod]
    public void CcsdsOmmVersBoundary_SplitsRecords()
    {
        // Arrange
        const string kvn = "CCSDS_OMM_VERS = 2.0\n"
                  + "\n"
                  + "OBJECT_NAME    = SAT1\n"
                  + "EPOCH          = 2026-05-20T08:56:33.249984\n"
                  + "MEAN_MOTION    = 14.46\n"
                  + "ECCENTRICITY   = .004\n"
                  + "INCLINATION    = 25.0\n"
                  + "RA_OF_ASC_NODE = 19.3\n"
                  + "ARG_OF_PERICENTER = 203.4\n"
                  + "MEAN_ANOMALY   = 317.8\n"
                  + "\n"
                  + "EPHEMERIS_TYPE = 0\n"
                  + "CLASSIFICATION_TYPE = U\n"
                  + "NORAD_CAT_ID   = 22490\n"
                  + "ELEMENT_SET_NO = 999\n"
                  + "REV_AT_EPOCH   = 75708\n"
                  + "BSTAR          = .36E-4\n"
                  + "MEAN_MOTION_DOT = .32E-5\n"
                  + "MEAN_MOTION_DDOT = 0\n"
                  + "\n"
                  + "CCSDS_OMM_VERS = 2.0\n"
                  + "\n"
                  + "OBJECT_NAME    = SAT2\n"
                  + "EPOCH          = 2026-05-20T09:00:00.000000\n"
                  + "MEAN_MOTION    = 15.0\n"
                  + "ECCENTRICITY   = .001\n"
                  + "INCLINATION    = 51.6\n"
                  + "RA_OF_ASC_NODE = 77.0\n"
                  + "ARG_OF_PERICENTER = 79.3\n"
                  + "MEAN_ANOMALY   = 280.8\n"
                  + "\n"
                  + "EPHEMERIS_TYPE = 0\n"
                  + "CLASSIFICATION_TYPE = U\n"
                  + "NORAD_CAT_ID   = 25544\n"
                  + "ELEMENT_SET_NO = 999\n"
                  + "REV_AT_EPOCH   = 56746\n"
                  + "BSTAR          = .10E-3\n"
                  + "MEAN_MOTION_DOT = .52E-4\n"
                  + "MEAN_MOTION_DDOT = 0\n";

        // Act
        var data = new OmmKvnParser().Parse(kvn);

        // Assert
        Assert.HasCount(2, data);
        Assert.AreEqual("SAT1", data[0].ObjectName);
        Assert.AreEqual("SAT2", data[1].ObjectName);
    }

    /// <summary>
    /// Verifies empty value lines are ignored.
    /// </summary>
    [TestMethod]
    public void EmptyValueLines_Ignored()
    {
        // Arrange
        const string kvn = "CCSDS_OMM_VERS = 2.0\n"
                  + "CREATION_DATE  = \n"
                  + "ORIGINATOR     = \n"
                  + "\n"
                  + "OBJECT_NAME    = TEST\n"
                  + "EPOCH          = 2026-05-20T08:56:33.249984\n"
                  + "MEAN_MOTION    = 14.46\n"
                  + "ECCENTRICITY   = .004\n"
                  + "INCLINATION    = 25.0\n"
                  + "RA_OF_ASC_NODE = 19.3\n"
                  + "ARG_OF_PERICENTER = 203.4\n"
                  + "MEAN_ANOMALY   = 317.8\n"
                  + "\n"
                  + "EPHEMERIS_TYPE = 0\n"
                  + "CLASSIFICATION_TYPE = U\n"
                  + "NORAD_CAT_ID   = 22490\n"
                  + "ELEMENT_SET_NO = 999\n"
                  + "REV_AT_EPOCH   = 75708\n"
                  + "BSTAR          = .36E-4\n"
                  + "MEAN_MOTION_DOT = .32E-5\n"
                  + "MEAN_MOTION_DDOT = 0\n";

        // Act
        var data = new OmmKvnParser().Parse(kvn);

        // Assert
        Assert.HasCount(1, data);
        Assert.IsNull(data[0].CreationDate);
    }
}

/// <summary>
/// Tests for OMM XML parser edge cases: empty elements, nested structure, version attribute.
/// </summary>
[TestClass]
public sealed class OmmXmlParserTests
{
    /// <summary>
    /// Verifies empty element values are ignored.
    /// </summary>
    [TestMethod]
    public void EmptyElementValues_Ignored()
    {
        // Arrange
        const string xml = "<?xml version=\"1.0\"?><ndm><omm id=\"CCSDS_OMM_VERS\" version=\"2.0\"><header><CREATION_DATE/><ORIGINATOR/></header><body><segment><metadata><OBJECT_NAME>TEST</OBJECT_NAME><OBJECT_ID>2026-001A</OBJECT_ID><CENTER_NAME>EARTH</CENTER_NAME><REF_FRAME>TEME</REF_FRAME><TIME_SYSTEM>UTC</TIME_SYSTEM><MEAN_ELEMENT_THEORY>SGP4</MEAN_ELEMENT_THEORY></metadata><data><meanElements><EPOCH>2026-05-20T08:56:33.249984</EPOCH><MEAN_MOTION>14.46</MEAN_MOTION><ECCENTRICITY>.004</ECCENTRICITY><INCLINATION>25.0</INCLINATION><RA_OF_ASC_NODE>19.3</RA_OF_ASC_NODE><ARG_OF_PERICENTER>203.4</ARG_OF_PERICENTER><MEAN_ANOMALY>317.8</MEAN_ANOMALY></meanElements><tleParameters><EPHEMERIS_TYPE>0</EPHEMERIS_TYPE><CLASSIFICATION_TYPE>U</CLASSIFICATION_TYPE><NORAD_CAT_ID>22490</NORAD_CAT_ID><ELEMENT_SET_NO>999</ELEMENT_SET_NO><REV_AT_EPOCH>75708</REV_AT_EPOCH><BSTAR>.36E-4</BSTAR><MEAN_MOTION_DOT>.32E-5</MEAN_MOTION_DOT><MEAN_MOTION_DDOT>0</MEAN_MOTION_DDOT></tleParameters></data></segment></body></omm></ndm>";

        // Act
        var data = new OmmXmlParser().Parse(xml);

        // Assert
        Assert.HasCount(1, data);
        Assert.IsNull(data[0].CreationDate);
    }

    /// <summary>
    /// Verifies nested ndm > omm > body > segment > metadata + data structure parsed correctly.
    /// </summary>
    [TestMethod]
    public void NestedStructure_ParsedCorrectly()
    {
        // Arrange
        const string xml = "<?xml version=\"1.0\"?><ndm><omm id=\"CCSDS_OMM_VERS\" version=\"2.0\"><header><CREATION_DATE/><ORIGINATOR/></header><body><segment><metadata><OBJECT_NAME>TEST</OBJECT_NAME><OBJECT_ID>2026-001A</OBJECT_ID><CENTER_NAME>EARTH</CENTER_NAME><REF_FRAME>TEME</REF_FRAME><TIME_SYSTEM>UTC</TIME_SYSTEM><MEAN_ELEMENT_THEORY>SGP4</MEAN_ELEMENT_THEORY></metadata><data><meanElements><EPOCH>2026-05-20T08:56:33.249984</EPOCH><MEAN_MOTION>14.46</MEAN_MOTION><ECCENTRICITY>.004</ECCENTRICITY><INCLINATION>25.0</INCLINATION><RA_OF_ASC_NODE>19.3</RA_OF_ASC_NODE><ARG_OF_PERICENTER>203.4</ARG_OF_PERICENTER><MEAN_ANOMALY>317.8</MEAN_ANOMALY></meanElements><tleParameters><EPHEMERIS_TYPE>0</EPHEMERIS_TYPE><CLASSIFICATION_TYPE>U</CLASSIFICATION_TYPE><NORAD_CAT_ID>22490</NORAD_CAT_ID><ELEMENT_SET_NO>999</ELEMENT_SET_NO><REV_AT_EPOCH>75708</REV_AT_EPOCH><BSTAR>.36E-4</BSTAR><MEAN_MOTION_DOT>.32E-5</MEAN_MOTION_DOT><MEAN_MOTION_DDOT>0</MEAN_MOTION_DDOT></tleParameters></data></segment></body></omm></ndm>";

        // Act
        var data = new OmmXmlParser().Parse(xml);

        // Assert
        Assert.HasCount(1, data);
        Assert.AreEqual("TEST", data[0].ObjectName);
        Assert.AreEqual("2026-001A", data[0].ObjectID);
        Assert.AreEqual("EARTH", data[0].CenterName);
        Assert.AreEqual("TEME", data[0].RefFrame);
        Assert.AreEqual("SGP4", data[0].MeanElementTheory);
    }

    /// <summary>
    /// Verifies version attribute on omm element captured as OmmVersion.
    /// </summary>
    [TestMethod]
    public void VersionAttribute_CapturedAsOmmVersion()
    {
        // Arrange
        const string xml = "<?xml version=\"1.0\"?><ndm><omm id=\"CCSDS_OMM_VERS\" version=\"2.0\"><header><CREATION_DATE/><ORIGINATOR/></header><body><segment><metadata><OBJECT_NAME>TEST</OBJECT_NAME><OBJECT_ID>2026-001A</OBJECT_ID><CENTER_NAME>EARTH</CENTER_NAME><REF_FRAME>TEME</REF_FRAME><TIME_SYSTEM>UTC</TIME_SYSTEM><MEAN_ELEMENT_THEORY>SGP4</MEAN_ELEMENT_THEORY></metadata><data><meanElements><EPOCH>2026-05-20T08:56:33.249984</EPOCH><MEAN_MOTION>14.46</MEAN_MOTION><ECCENTRICITY>.004</ECCENTRICITY><INCLINATION>25.0</INCLINATION><RA_OF_ASC_NODE>19.3</RA_OF_ASC_NODE><ARG_OF_PERICENTER>203.4</ARG_OF_PERICENTER><MEAN_ANOMALY>317.8</MEAN_ANOMALY></meanElements><tleParameters><EPHEMERIS_TYPE>0</EPHEMERIS_TYPE><CLASSIFICATION_TYPE>U</CLASSIFICATION_TYPE><NORAD_CAT_ID>22490</NORAD_CAT_ID><ELEMENT_SET_NO>999</ELEMENT_SET_NO><REV_AT_EPOCH>75708</REV_AT_EPOCH><BSTAR>.36E-4</BSTAR><MEAN_MOTION_DOT>.32E-5</MEAN_MOTION_DOT><MEAN_MOTION_DDOT>0</MEAN_MOTION_DDOT></tleParameters></data></segment></body></omm></ndm>";

        // Act
        var data = new OmmXmlParser().Parse(xml);

        // Assert
        Assert.AreEqual("2.0", data[0].OmmVersion);
    }

    /// <summary>
    /// Verifies multiple omm elements in one document parsed.
    /// </summary>
    [TestMethod]
    public void MultipleOmmElements_Parsed()
    {
        // Arrange
        const string xml = "<?xml version=\"1.0\"?><ndm>"
                  + "<omm id=\"CCSDS_OMM_VERS\" version=\"2.0\"><header><CREATION_DATE/><ORIGINATOR/></header><body><segment><metadata><OBJECT_NAME>SAT1</OBJECT_NAME><OBJECT_ID>2026-001A</OBJECT_ID><CENTER_NAME>EARTH</CENTER_NAME><REF_FRAME>TEME</REF_FRAME><TIME_SYSTEM>UTC</TIME_SYSTEM><MEAN_ELEMENT_THEORY>SGP4</MEAN_ELEMENT_THEORY></metadata><data><meanElements><EPOCH>2026-05-20T08:56:33.249984</EPOCH><MEAN_MOTION>14.46</MEAN_MOTION><ECCENTRICITY>.004</ECCENTRICITY><INCLINATION>25.0</INCLINATION><RA_OF_ASC_NODE>19.3</RA_OF_ASC_NODE><ARG_OF_PERICENTER>203.4</ARG_OF_PERICENTER><MEAN_ANOMALY>317.8</MEAN_ANOMALY></meanElements><tleParameters><EPHEMERIS_TYPE>0</EPHEMERIS_TYPE><CLASSIFICATION_TYPE>U</CLASSIFICATION_TYPE><NORAD_CAT_ID>22490</NORAD_CAT_ID><ELEMENT_SET_NO>999</ELEMENT_SET_NO><REV_AT_EPOCH>75708</REV_AT_EPOCH><BSTAR>.36E-4</BSTAR><MEAN_MOTION_DOT>.32E-5</MEAN_MOTION_DOT><MEAN_MOTION_DDOT>0</MEAN_MOTION_DDOT></tleParameters></data></segment></body></omm>"
                  + "<omm id=\"CCSDS_OMM_VERS\" version=\"2.0\"><header><CREATION_DATE/><ORIGINATOR/></header><body><segment><metadata><OBJECT_NAME>SAT2</OBJECT_NAME><OBJECT_ID>2026-002A</OBJECT_ID><CENTER_NAME>EARTH</CENTER_NAME><REF_FRAME>TEME</REF_FRAME><TIME_SYSTEM>UTC</TIME_SYSTEM><MEAN_ELEMENT_THEORY>SGP4</MEAN_ELEMENT_THEORY></metadata><data><meanElements><EPOCH>2026-05-20T09:00:00.000000</EPOCH><MEAN_MOTION>15.0</MEAN_MOTION><ECCENTRICITY>.001</ECCENTRICITY><INCLINATION>51.6</INCLINATION><RA_OF_ASC_NODE>77.0</RA_OF_ASC_NODE><ARG_OF_PERICENTER>79.3</ARG_OF_PERICENTER><MEAN_ANOMALY>280.8</MEAN_ANOMALY></meanElements><tleParameters><EPHEMERIS_TYPE>0</EPHEMERIS_TYPE><CLASSIFICATION_TYPE>U</CLASSIFICATION_TYPE><NORAD_CAT_ID>25544</NORAD_CAT_ID><ELEMENT_SET_NO>999</ELEMENT_SET_NO><REV_AT_EPOCH>56746</REV_AT_EPOCH><BSTAR>.10E-3</BSTAR><MEAN_MOTION_DOT>.52E-4</MEAN_MOTION_DOT><MEAN_MOTION_DDOT>0</MEAN_MOTION_DDOT></tleParameters></data></segment></body></omm>"
                  + "</ndm>";

        // Act
        var data = new OmmXmlParser().Parse(xml);

        // Assert
        Assert.HasCount(2, data);
        Assert.AreEqual("SAT1", data[0].ObjectName);
        Assert.AreEqual("SAT2", data[1].ObjectName);
    }

    /// <summary>
    /// Verifies empty document returns empty list.
    /// </summary>
    [TestMethod]
    public void EmptyDocument_ReturnsEmptyList()
    {
        // Arrange
        const string xml = "<?xml version=\"1.0\"?><ndm></ndm>";

        // Act
        var data = new OmmXmlParser().Parse(xml);

        // Assert
        Assert.IsEmpty(data);
    }
}
