using System;
using System.IO;
using System.Linq;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Parsers;
using SGPdotNET.TLE;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Integration tests: full pipeline from OMM files through Satellite to propagation and observation.
/// </summary>
[TestClass]
public sealed class IntegrationTests
{
    private static readonly DateTime TestTime = new(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);

    private static string ExampleDir => Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "example_inputs");

    /// <summary>
    /// Verifies Satellite from OmmData produces same position as from Tle for identical elements.
    /// </summary>
    [TestMethod]
    public void SatelliteFromOmmData_MatchesTlePosition()
    {
        // Arrange
        var xmlPath = Path.Combine(ExampleDir, "xml_stations.xml");
        var xmlData = new OmmXmlParser().ParseFile(xmlPath);
        var issOmm = xmlData.First(x => x.NoradCatID == 25544);

        var issTle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Act
        var satOmm = new Satellite(issOmm);
        var satTle = new Satellite(issTle);

        var posOmm = satOmm.Predict(TestTime);
        var posTle = satTle.Predict(TestTime);

        // Assert
        AssertPositionsMatch(posTle, posOmm, "ISS OMM vs TLE");
    }

    /// <summary>
    /// Verifies Satellite.LoadFromOmm* static methods return correct count.
    /// </summary>
    [TestMethod]
    public void LoadFromOmmStaticMethods_ReturnCorrectCount()
    {
        // Act
        var csvSats = Satellite.LoadFromOmmCsv(Path.Combine(ExampleDir, "csv_resource.csv"));
        var jsonSats = Satellite.LoadFromOmmJson(Path.Combine(ExampleDir, "json_analyst.json"));
        var kvnSats = Satellite.LoadFromOmmKvn(Path.Combine(ExampleDir, "kvn_visual.txt"));
        var xmlSats = Satellite.LoadFromOmmXml(Path.Combine(ExampleDir, "xml_stations.xml"));

        // Assert
        Assert.HasCount(167, csvSats);
        Assert.HasCount(597, jsonSats);
        Assert.HasCount(148, kvnSats);
        Assert.HasCount(27, xmlSats);
    }

    /// <summary>
    /// Verifies full pipeline: OMM file → Satellite → GroundStation.Observe → visibility periods.
    /// </summary>
    [TestMethod]
    public void FullPipeline_OmmFileToVisibilityPeriods()
    {
        // Arrange
        var xmlPath = Path.Combine(ExampleDir, "xml_stations.xml");
        var sats = Satellite.LoadFromOmmXml(xmlPath);
        var iss = sats.First(x => x.Name.Contains("ISS"));
        var station = new GroundStation(new GeodeticCoordinate(
            Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));
        var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);

        // Act
        var passes = station.Observe(iss, start, end, TimeSpan.FromSeconds(10));

        // Assert
        Assert.IsNotEmpty(passes);
        foreach (var pass in passes)
        {
            Assert.IsTrue(pass.Start < pass.End);
            Assert.IsGreaterThan(0, pass.MaxElevation.Degrees);
        }
    }

    /// <summary>
    /// Verifies coordinate conversion chain: Geodetic → ECI → Geodetic preserves position.
    /// </summary>
    [TestMethod]
    public void GeodeticToEciToGeodetic_PreservesPosition()
    {
        // Arrange
        var time = new DateTime(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);
        var geo = new GeodeticCoordinate(
            Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0);

        // Act
        var eci = geo.ToEci(time);
        var roundTrip = eci.ToGeodetic();

        // Assert
        Assert.AreEqual(geo.Latitude.Degrees, roundTrip.Latitude.Degrees, TestConstants.AngleTolerance);
        Assert.AreEqual(geo.Longitude.Degrees, roundTrip.Longitude.Degrees, TestConstants.AngleTolerance);
        Assert.AreEqual(geo.Altitude, roundTrip.Altitude, 1e-10);
    }

    private static void AssertPositionsMatch(EciCoordinate expected, EciCoordinate actual, string label)
    {
        var dx = Math.Abs(actual.Position.X - expected.Position.X);
        var dy = Math.Abs(actual.Position.Y - expected.Position.Y);
        var dz = Math.Abs(actual.Position.Z - expected.Position.Z);

        Assert.IsLessThan(TestConstants.SmallDistanceToleranceKm, dx, $"{label} X delta {dx:E4} km exceeds tolerance");
        Assert.IsLessThan(TestConstants.SmallDistanceToleranceKm, dy, $"{label} Y delta {dy:E4} km exceeds tolerance");
        Assert.IsLessThan(TestConstants.SmallDistanceToleranceKm, dz, $"{label} Z delta {dz:E4} km exceeds tolerance");
    }
}
