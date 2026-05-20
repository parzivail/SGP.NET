using System;
using System.IO;
using System.Linq;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Parsers;
using SGPdotNET.TLE;

namespace SGPdotNET.Tests;

/// <summary>
/// Verifies that OMM parsers produce propagation results consistent with TLE baselines.
/// </summary>
[TestClass]
public sealed class OmmPropagationTests
{
    private static readonly DateTime TestTime = new(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);
    private const double PositionToleranceKm = 1e-3;

    private static string ExampleDir => Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "example_inputs");

    private static EciCoordinate GetTleBaseline(uint noradId, string tleFile)
    {
        var lines = File.ReadAllLines(Path.Combine(ExampleDir, tleFile));
        var tles = Tle.ParseElements(lines, true);
        var tle = tles.First(t => t.NoradNumber == noradId);
        return new Satellite(tle).Predict(TestTime);
    }

    private static OmmData FindOmmByNorad(List<OmmData> data, uint noradId)
    {
        return data.First(x => x.NoradCatID == noradId);
    }

    private static void AssertPositionsMatch(EciCoordinate expected, EciCoordinate actual, string label)
    {
        var dx = Math.Abs(actual.Position.X - expected.Position.X);
        var dy = Math.Abs(actual.Position.Y - expected.Position.Y);
        var dz = Math.Abs(actual.Position.Z - expected.Position.Z);

        Assert.IsLessThan(PositionToleranceKm, dx, $"{label} X delta {dx:E4} km exceeds tolerance");
        Assert.IsLessThan(PositionToleranceKm, dy, $"{label} Y delta {dy:E4} km exceeds tolerance");
        Assert.IsLessThan(PositionToleranceKm, dz, $"{label} Z delta {dz:E4} km exceeds tolerance");
    }

    /// <summary>
    /// Verifies the TLE baseline for ISS (NORAD 25544) produces known position values.
    /// </summary>
    [TestMethod]
    public void IssTleBaseline_ProducesExpectedPosition()
    {
        // Arrange
        var baseline = GetTleBaseline(25544, "iss.txt");

        // Act
        var x = baseline.Position.X;
        var y = baseline.Position.Y;
        var z = baseline.Position.Z;

        // Assert
        Assert.AreEqual(-2435.710411, x, 1e-3);
        Assert.AreEqual(-6278.309905, y, 1e-3);
        Assert.AreEqual(918.205704, z, 1e-3);
    }

    /// <summary>
    /// Verifies ISS propagated from OMM XML matches the TLE baseline.
    /// </summary>
    [TestMethod]
    public void IssXml_PropagatesToSamePositionAsTle()
    {
        // Arrange
        var baseline = GetTleBaseline(25544, "iss.txt");
        var xmlData = new OmmXmlParser().ParseFile(Path.Combine(ExampleDir, "xml_stations.xml"));
        var issOmm = FindOmmByNorad(xmlData, 25544);

        // Act
        var sat = new Satellite(issOmm);
        var pos = sat.Predict(TestTime);

        // Assert
        AssertPositionsMatch(baseline, pos, "ISS XML vs TLE");
    }

    /// <summary>
    /// Verifies ISS propagated from OMM KVN matches the TLE baseline.
    /// </summary>
    [TestMethod]
    public void IssKvn_PropagatesToSamePositionAsTle()
    {
        // Arrange
        var baseline = GetTleBaseline(25544, "iss.txt");
        var kvnData = new OmmKvnParser().ParseFile(Path.Combine(ExampleDir, "kvn_visual.txt"));
        var issOmm = FindOmmByNorad(kvnData, 25544);

        // Act
        var sat = new Satellite(issOmm);
        var pos = sat.Predict(TestTime);

        // Assert
        AssertPositionsMatch(baseline, pos, "ISS KVN vs TLE");
    }

    /// <summary>
    /// Verifies satellites present in both OMM CSV and KVN propagate to identical positions.
    /// </summary>
    [TestMethod]
    public void CsvAndKvn_OverlapSatellites_ProduceSamePosition()
    {
        // Arrange
        var csvData = new OmmCsvParser().ParseFile(Path.Combine(ExampleDir, "csv_resource.csv"));
        var kvnData = new OmmKvnParser().ParseFile(Path.Combine(ExampleDir, "kvn_visual.txt"));
        var overlapIds = new[]
        {
            25994u, // TERRA
            27424u, // AQUA
            29228u, // RESURS DK-1
            31598u, // SKYMED 1
            39766u, // ALOS-2
            41038u, // YAOGAN-29
        };

        foreach (var id in overlapIds)
        {
            var csvOmm = FindOmmByNorad(csvData, id);
            var kvnOmm = FindOmmByNorad(kvnData, id);

            // Act
            var csvPos = new Satellite(csvOmm).Predict(TestTime);
            var kvnPos = new Satellite(kvnOmm).Predict(TestTime);

            // Assert
            AssertPositionsMatch(csvPos, kvnPos, $"NORAD {id} CSV vs KVN");
        }
    }

    /// <summary>
    /// Verifies satellites present in both OMM KVN and XML propagate to identical positions.
    /// </summary>
    [TestMethod]
    public void KvnAndXml_OverlapSatellites_ProduceSamePosition()
    {
        // Arrange
        var kvnData = new OmmKvnParser().ParseFile(Path.Combine(ExampleDir, "kvn_visual.txt"));
        var xmlData = new OmmXmlParser().ParseFile(Path.Combine(ExampleDir, "xml_stations.xml"));
        var overlapIds = new[]
        {
            25544u, // ISS (ZARYA)
            48274u, // CSS (TIANHE)
            66174u, // HTV-X1
            66515u, // SZ-21 MODULE
        };

        foreach (var id in overlapIds)
        {
            var kvnOmm = FindOmmByNorad(kvnData, id);
            var xmlOmm = FindOmmByNorad(xmlData, id);

            // Act
            var kvnPos = new Satellite(kvnOmm).Predict(TestTime);
            var xmlPos = new Satellite(xmlOmm).Predict(TestTime);

            // Assert
            AssertPositionsMatch(kvnPos, xmlPos, $"NORAD {id} KVN vs XML");
        }
    }
}
