using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGPdotNET.Exception;
using SGPdotNET.TLE;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for TLE parsing edge cases: year wrapping, leading-dot fields, negative BStar, validation.
/// </summary>
[TestClass]
public sealed class TleParsingTests
{
    /// <summary>
    /// Verifies year wrapping: two-digit year 56 → 2056, 57 → 1957.
    /// </summary>
    [TestMethod]
    public void Epoch_YearWrapping_56Becomes2056()
    {
        // Arrange
        var line1 = "1 25544U 98067A   56140.52007259  .00005164  00000-0  10084-3 0  9995";

        // Act
        var tle = new Tle(line1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual(2056, tle.Epoch.Year);
    }

    [TestMethod]
    public void Epoch_YearWrapping_57Becomes1957()
    {
        // Arrange
        var line1 = "1 25544U 98067A   57140.52007259  .00005164  00000-0  10084-3 0  9995";

        // Act
        var tle = new Tle(line1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual(1957, tle.Epoch.Year);
    }

    /// <summary>
    /// Verifies leading-dot eccentricity in line 2 (e.g., "0007497" → 0.0007497).
    /// </summary>
    [TestMethod]
    public void Eccentricity_LeadingDot_ParsedCorrectly()
    {
        // Arrange & Act
        var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual(0.0007497, tle.Eccentricity, 1e-10);
    }

    /// <summary>
    /// Verifies negative BStar drag term parsed correctly.
    /// </summary>
    [TestMethod]
    public void BStar_Negative_ParsedCorrectly()
    {
        // Arrange — BStar field "-12345-4" means -0.12345e-4 = -1.2345e-5
        var line1 = "1 25544U 98067A   26140.52007259  .00005164  00000-0 -12345-4 0  9995";

        // Act
        var tle = new Tle(line1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual(-1.2345e-5, tle.BStarDragTerm, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies zero-padded inclination, RAAN, arg of perigee, mean anomaly parsed correctly.
    /// </summary>
    [TestMethod]
    public void Angles_ZeroPadded_ParsedCorrectly()
    {
        // Arrange & Act
        var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual(51.6328, tle.Inclination.Degrees, TestConstants.AngleTolerance);
        Assert.AreEqual(77.0641, tle.RightAscendingNode.Degrees, TestConstants.AngleTolerance);
        Assert.AreEqual(79.3410, tle.ArgumentPerigee.Degrees, TestConstants.AngleTolerance);
        Assert.AreEqual(280.8422, tle.MeanAnomaly.Degrees, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies mean motion parsed correctly from line 2.
    /// </summary>
    [TestMethod]
    public void MeanMotion_ParsedCorrectly()
    {
        // Arrange & Act
        var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual(15.49283153, tle.MeanMotionRevPerDay, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies NORAD catalog number parsed correctly from both lines.
    /// </summary>
    [TestMethod]
    public void NoradNumber_ParsedCorrectly()
    {
        // Arrange & Act
        var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual(25544u, tle.NoradNumber);
    }

    /// <summary>
    /// Verifies international designator parsed correctly (includes trailing spaces).
    /// </summary>
    [TestMethod]
    public void IntDesignator_ParsedCorrectly()
    {
        // Arrange & Act
        var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual("98067A  ", tle.IntDesignator);
    }

    /// <summary>
    /// Verifies epoch day-of-year parsed correctly.
    /// </summary>
    [TestMethod]
    public void Epoch_DayOfYear_ParsedCorrectly()
    {
        // Arrange & Act
        var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert day 140.52007259 of 2026
        Assert.AreEqual(2026, tle.Epoch.Year);
        Assert.AreEqual(5, tle.Epoch.Month);
        Assert.AreEqual(20, tle.Epoch.Day);
    }

    /// <summary>
    /// Verifies name line with "0 " prefix is passed through as-is.
    /// </summary>
    [TestMethod]
    public void Name_WithZeroPrefix_PassedThrough()
    {
        // Arrange & Act
        var tle = new Tle("0 ISS (ZARYA)", TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual("0 ISS (ZARYA)", tle.Name);
    }

    /// <summary>
    /// Verifies name line without "0 " prefix is used as-is.
    /// </summary>
    [TestMethod]
    public void Name_WithoutZeroPrefix_UsedAsIs()
    {
        // Arrange & Act
        var tle = new Tle("ISS (ZARYA)", TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual("ISS (ZARYA)", tle.Name);
    }

    /// <summary>
    /// Verifies default name is null when no name provided.
    /// </summary>
    [TestMethod]
    public void Name_Default_IsNull()
    {
        // Arrange & Act
        var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert
        Assert.IsNull(tle.Name);
    }

    /// <summary>
    /// Verifies satellite number mismatch throws TleException.
    /// </summary>
    [TestMethod]
    public void SatelliteNumberMismatch_ThrowsTleException()
    {
        // Arrange
        
        // Invalid NORAD number
        const string line2 = "2 99999  51.6328  77.0641 0007497  79.3410 280.8422 15.49283153567468";

        // Act & Assert
        try
        {
            _ = new Tle(TestConstants.IssLine1, line2);
            Assert.Fail("Expected TleException");
        }
        catch (TleException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Verifies invalid line length throws TleException.
    /// </summary>
    [TestMethod]
    public void InvalidLineLength_ThrowsTleException()
    {
        // Arrange
        const string shortLine = "1 25544U 98067A";

        // Act & Assert
        try
        {
            _ = new Tle(shortLine, TestConstants.IssLine2);
            Assert.Fail("Expected TleException");
        }
        catch (TleException)
        {
            // Expected
        }

        try
        {
            _ = new Tle(TestConstants.IssLine1, shortLine);
            Assert.Fail("Expected TleException");
        }
        catch (TleException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Verifies invalid line beginning character throws TleException.
    /// </summary>
    [TestMethod]
    public void InvalidLineBeginning_ThrowsTleException()
    {
        // Arrange
        const string badLine1 = "X 25544U 98067A   26140.52007259  .00005164  00000-0  10084-3 0  9995";
        const string badLine2 = "X 25544  51.6328  77.0641 0007497  79.3410 280.8422 15.49283153567468";

        // Act & Assert
        try
        {
            _ = new Tle(badLine1, TestConstants.IssLine2);
            Assert.Fail("Expected TleException");
        }
        catch (TleException)
        {
            // Expected
        }

        try
        {
            _ = new Tle(TestConstants.IssLine1, badLine2);
            Assert.Fail("Expected TleException");
        }
        catch (TleException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Verifies copy constructor produces identical values.
    /// </summary>
    [TestMethod]
    public void CopyConstructor_ProducesIdenticalValues()
    {
        // Arrange
        var original = new Tle("ISS (ZARYA)", TestConstants.IssLine1, TestConstants.IssLine2);

        // Act
        var copy = new Tle(original);

        // Assert
        Assert.AreEqual(original.Name, copy.Name);
        Assert.AreEqual(original.Line1, copy.Line1);
        Assert.AreEqual(original.Line2, copy.Line2);
        Assert.AreEqual(original.NoradNumber, copy.NoradNumber);
        Assert.AreEqual(original.IntDesignator, copy.IntDesignator);
        Assert.AreEqual(original.Epoch, copy.Epoch);
        Assert.AreEqual(original.MeanMotionDtOver2, copy.MeanMotionDtOver2, TestConstants.AngleTolerance);
        Assert.AreEqual(original.MeanMotionDdtOver6, copy.MeanMotionDdtOver6, TestConstants.AngleTolerance);
        Assert.AreEqual(original.BStarDragTerm, copy.BStarDragTerm, TestConstants.AngleTolerance);
        Assert.AreEqual(original.Inclination, copy.Inclination);
        Assert.AreEqual(original.RightAscendingNode, copy.RightAscendingNode);
        Assert.AreEqual(original.Eccentricity, copy.Eccentricity, 1e-10);
        Assert.AreEqual(original.ArgumentPerigee, copy.ArgumentPerigee);
        Assert.AreEqual(original.MeanAnomaly, copy.MeanAnomaly);
        Assert.AreEqual(original.MeanMotionRevPerDay, copy.MeanMotionRevPerDay, TestConstants.AngleTolerance);
        Assert.AreEqual(original.OrbitNumber, copy.OrbitNumber);
    }

    /// <summary>
    /// Verifies ParseElements with 3-line format.
    /// </summary>
    [TestMethod]
    public void ParseElements_ThreeLine_ParsesCorrectly()
    {
        // Arrange
        var lines = new[]
        {
            "0 ISS (ZARYA)",
            TestConstants.IssLine1,
            TestConstants.IssLine2
        };

        // Act
        var tles = Tle.ParseElements(lines, true);

        // Assert
        Assert.HasCount(1, tles);
        Assert.AreEqual("ISS (ZARYA)", tles[0].Name);
        Assert.AreEqual(25544u, tles[0].NoradNumber);
    }

    /// <summary>
    /// Verifies ParseElements with 2-line format.
    /// </summary>
    [TestMethod]
    public void ParseElements_TwoLine_ParsesCorrectly()
    {
        // Arrange
        var lines = new[]
        {
            TestConstants.IssLine1,
            TestConstants.IssLine2
        };

        // Act
        var tles = Tle.ParseElements(lines, false);

        // Assert
        Assert.HasCount(1, tles);
        Assert.IsNull(tles[0].Name);
        Assert.AreEqual(25544u, tles[0].NoradNumber);
    }

    /// <summary>
    /// Verifies orbit number at epoch parsed correctly.
    /// </summary>
    [TestMethod]
    public void OrbitNumber_ParsedCorrectly()
    {
        // Arrange & Act
        var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert
        Assert.AreEqual(56746u, tle.OrbitNumber);
    }

    /// <summary>
    /// Verifies mean motion first derivative parsed correctly (TLE field is already dt/2).
    /// </summary>
    [TestMethod]
    public void MeanMotionDt2_ParsedCorrectly()
    {
        // Arrange & Act
        var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Assert the TLE field value IS MeanMotionDt/2
        Assert.AreEqual(0.00005164, tle.MeanMotionDtOver2, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies equality and hash code consistency for TLE objects.
    /// </summary>
    [TestMethod]
    public void EqualityAndHashCode_Consistent()
    {
        // Arrange
        var tle1 = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
        var tle2 = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);

        // Act & Assert
        Assert.IsTrue(tle1 == tle2);
        Assert.AreEqual(tle1.GetHashCode(), tle2.GetHashCode());
    }
}
