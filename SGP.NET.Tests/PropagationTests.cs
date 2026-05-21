using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Propagation;
using SGPdotNET.TLE;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for Orbit extraction: perigee, apogee, period, and recovered values for known orbits.
/// </summary>
[TestClass]
public sealed class OrbitTests
{
    private const double Tolerance = 1e-5;
    private const double DistanceToleranceKm = 1.0;

    // ISS TLE — known ~400 km LEO
    private const string IssLine1 = "1 25544U 98067A   26140.52007259  .00005164  00000-0  10084-3 0  9995";
    private const string IssLine2 = "2 25544  51.6328  77.0641 0007497  79.3410 280.8422 15.49283153567468";

    /// <summary>
    /// Verifies ISS perigee is in the expected LEO range (~414 km).
    /// </summary>
    [TestMethod]
    public void IssPerigee_IsInLeoRange()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var orbit = new Orbit(tle);

        // Act
        var perigee = orbit.Perigee;

        // Assert
        Assert.AreEqual(414.2, perigee, DistanceToleranceKm);
    }

    /// <summary>
    /// Verifies ISS apogee is in the expected LEO range (~424 km).
    /// </summary>
    [TestMethod]
    public void IssApogee_IsInLeoRange()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var orbit = new Orbit(tle);

        // Act
        var apogee = orbit.Apogee;

        // Assert
        Assert.AreEqual(424.4, apogee, DistanceToleranceKm);
    }

    /// <summary>
    /// Verifies ISS orbital period is ~92 minutes.
    /// </summary>
    [TestMethod]
    public void IssPeriod_IsApproximately92Minutes()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var orbit = new Orbit(tle);

        // Act
        var period = orbit.Period;

        // Assert
        Assert.AreEqual(92.6, period, 1.0);
    }

    /// <summary>
    /// Verifies recovered semi-major axis and mean motion are positive.
    /// </summary>
    [TestMethod]
    public void RecoveredValues_ArePositive()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var orbit = new Orbit(tle);

        // Act & Assert
        Assert.IsTrue(orbit.RecoveredSemiMajorAxis > 0);
        Assert.IsTrue(orbit.RecoveredMeanMotion > 0);
    }

    /// <summary>
    /// Verifies semi-major axis equals recovered value times Earth radius.
    /// </summary>
    [TestMethod]
    public void SemiMajorAxis_EqualsRecoveredTimesEarthRadius()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var orbit = new Orbit(tle);

        // Act
        var expected = orbit.RecoveredSemiMajorAxis * SgpConstants.EarthRadiusKm;

        // Assert
        Assert.AreEqual(expected, orbit.SemiMajorAxis, Tolerance);
    }

    /// <summary>
    /// Verifies mean motion in rad/min equals TLE rev/day converted.
    /// </summary>
    [TestMethod]
    public void MeanMotion_EqualsTleConverted()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var orbit = new Orbit(tle);

        // Act
        var expected = tle.MeanMotionRevPerDay * SgpConstants.TwoPi / SgpConstants.MinutesPerDay;

        // Assert
        Assert.AreEqual(expected, orbit.MeanMotion, Tolerance);
    }

    /// <summary>
    /// Verifies orbit properties match TLE values exactly.
    /// </summary>
    [TestMethod]
    public void OrbitProperties_MatchTleValues()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var orbit = new Orbit(tle);

        // Assert
        Assert.AreEqual(tle.MeanAnomaly, orbit.MeanAnomoly);
        Assert.AreEqual(tle.RightAscendingNode, orbit.AscendingNode);
        Assert.AreEqual(tle.ArgumentPerigee, orbit.ArgumentPerigee);
        Assert.AreEqual(tle.Eccentricity, orbit.Eccentricity, Tolerance);
        Assert.AreEqual(tle.Inclination, orbit.Inclination);
        Assert.AreEqual(tle.BStarDragTerm, orbit.BStar, Tolerance);
        Assert.AreEqual(tle.Epoch, orbit.Epoch);
    }

    /// <summary>
    /// Verifies perigee is less than apogee for non-circular orbits.
    /// </summary>
    [TestMethod]
    public void Perigee_IsLessThanApogee()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var orbit = new Orbit(tle);

        // Assert
        Assert.IsTrue(orbit.Perigee < orbit.Apogee);
    }

    /// <summary>
    /// Verifies orbit equality and hash code consistency.
    /// </summary>
    [TestMethod]
    public void EqualityAndHashCode_Consistent()
    {
        // Arrange
        var tle1 = new Tle(IssLine1, IssLine2);
        var tle2 = new Tle(IssLine1, IssLine2);
        var orbit1 = new Orbit(tle1);
        var orbit2 = new Orbit(tle2);

        // Act & Assert
        Assert.IsTrue(orbit1 == orbit2);
        Assert.AreEqual(orbit1.GetHashCode(), orbit2.GetHashCode());
    }
}

/// <summary>
/// Tests for TopocentricObservation: Doppler shift, signal delay, and relative direction.
/// </summary>
[TestClass]
public sealed class TopocentricObservationTests
{
    private const double Tolerance = TestConstants.AngleTolerance;

    /// <summary>
    /// Verifies Doppler shift sign: approaching (negative range rate) → positive shift.
    /// </summary>
    [TestMethod]
    public void GetDopplerShift_Approaching_ProducesPositiveShift()
    {
        // Arrange
        var obs = new TopocentricObservation(Angle.Zero, Angle.FromDegrees(45), 500, -2.0);

        // Act
        var shift = obs.GetDopplerShift(1e9); // 1 GHz

        // Assert
        Assert.IsTrue(shift > 0);
    }

    /// <summary>
    /// Verifies Doppler shift sign: receding (positive range rate) → negative shift.
    /// </summary>
    [TestMethod]
    public void GetDopplerShift_Receding_ProducesNegativeShift()
    {
        // Arrange
        var obs = new TopocentricObservation(Angle.Zero, Angle.FromDegrees(45), 500, 2.0);

        // Act
        var shift = obs.GetDopplerShift(1e9);

        // Assert
        Assert.IsTrue(shift < 0);
    }

    /// <summary>
    /// Verifies signal delay decreases with range (formula is c / range).
    /// </summary>
    [TestMethod]
    public void SignalDelay_DecreasesWithRange()
    {
        // Arrange
        var near = new TopocentricObservation(Angle.Zero, Angle.FromDegrees(45), 400, 0);
        var far = new TopocentricObservation(Angle.Zero, Angle.FromDegrees(45), 800, 0);

        // Act
        var delayNear = near.SignalDelay;
        var delayFar = far.SignalDelay;

        // Assert — formula is c / (Range * 1000), so larger range → smaller delay
        Assert.IsTrue(delayFar < delayNear);
    }

    /// <summary>
    /// Verifies Direction returns Approaching for negative range rate.
    /// </summary>
    [TestMethod]
    public void Direction_Approaching_ForNegativeRangeRate()
    {
        // Arrange
        var obs = new TopocentricObservation(Angle.Zero, Angle.FromDegrees(45), 500, -1.0);

        // Act
        var dir = obs.Direction;

        // Assert
        Assert.AreEqual(RelativeDirection.Approaching, dir);
    }

    /// <summary>
    /// Verifies Direction returns Receding for positive range rate.
    /// </summary>
    [TestMethod]
    public void Direction_Receding_ForPositiveRangeRate()
    {
        // Arrange
        var obs = new TopocentricObservation(Angle.Zero, Angle.FromDegrees(45), 500, 1.0);

        // Act
        var dir = obs.Direction;

        // Assert
        Assert.AreEqual(RelativeDirection.Receding, dir);
    }

    /// <summary>
    /// Verifies Direction returns Fixed for zero range rate.
    /// </summary>
    [TestMethod]
    public void Direction_Fixed_ForZeroRangeRate()
    {
        // Arrange
        var obs = new TopocentricObservation(Angle.Zero, Angle.FromDegrees(45), 500, 0);

        // Act
        var dir = obs.Direction;

        // Assert
        Assert.AreEqual(RelativeDirection.Fixed, dir);
    }

    /// <summary>
    /// Verifies copy constructor produces identical values.
    /// </summary>
    [TestMethod]
    public void CopyConstructor_ProducesIdenticalValues()
    {
        // Arrange
        var original = new TopocentricObservation(
            Angle.FromDegrees(180),
            Angle.FromDegrees(45),
            500,
            -2.0);

        // Act
        var copy = new TopocentricObservation(original);

        // Assert
        Assert.AreEqual(original.Azimuth, copy.Azimuth);
        Assert.AreEqual(original.Elevation, copy.Elevation);
        Assert.AreEqual(original.Range, copy.Range, Tolerance);
        Assert.AreEqual(original.RangeRate, copy.RangeRate, Tolerance);
    }

    /// <summary>
    /// Verifies equality and hash code consistency.
    /// </summary>
    [TestMethod]
    public void EqualityAndHashCode_Consistent()
    {
        // Arrange
        var obs1 = new TopocentricObservation(Angle.FromDegrees(180), Angle.FromDegrees(45), 500, -2.0);
        var obs2 = new TopocentricObservation(Angle.FromDegrees(180), Angle.FromDegrees(45), 500, -2.0);

        // Act & Assert
        Assert.IsTrue(obs1 == obs2);
        Assert.AreEqual(obs1.GetHashCode(), obs2.GetHashCode());
    }
}

/// <summary>
/// Tests for GroundStation: single observation angles and visibility period calculations.
/// </summary>
[TestClass]
public sealed class GroundStationTests
{
    private const string IssLine1 = "1 25544U 98067A   26140.52007259  .00005164  00000-0  10084-3 0  9995";
    private const string IssLine2 = "2 25544  51.6328  77.0641 0007497  79.3410 280.8422 15.49283153567468";

    /// <summary>
    /// Verifies single-instant observation produces valid topocentric angles.
    /// </summary>
    [TestMethod]
    public void Observe_SingleInstant_ProducesValidAngles()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var sat = new Satellite(tle);
        var station = new GroundStation(new GeodeticCoordinate(
            Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0)); // Cape Canaveral
        var time = new DateTime(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);

        // Act
        var obs = station.Observe(sat, time);

        // Assert
        Assert.IsTrue(obs.Azimuth.Degrees >= 0 && obs.Azimuth.Degrees < 360);
        Assert.IsTrue(obs.Elevation.Degrees >= -90 && obs.Elevation.Degrees <= 90);
        Assert.IsTrue(obs.Range > 0);
    }

    /// <summary>
    /// Verifies observation returns non-empty visibility list for ISS over 24 hours.
    /// </summary>
    [TestMethod]
    public void Observe_24HourWindow_ReturnsNonEmptyList()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var sat = new Satellite(tle);
        var station = new GroundStation(new GeodeticCoordinate(
            Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));
        var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);

        // Act
        var passes = station.Observe(sat, start, end, TimeSpan.FromSeconds(10));

        // Assert
        Assert.IsTrue(passes.Count > 0);
    }

    /// <summary>
    /// Verifies each visibility period has AOS before LOS.
    /// </summary>
    [TestMethod]
    public void VisibilityPeriod_AosBeforeLos()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var sat = new Satellite(tle);
        var station = new GroundStation(new GeodeticCoordinate(
            Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));
        var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);

        // Act
        var passes = station.Observe(sat, start, end, TimeSpan.FromSeconds(10));

        // Assert
        foreach (var pass in passes)
        {
            Assert.IsTrue(pass.Start < pass.End, $"AOS {pass.Start} should be before LOS {pass.End}");
        }
    }

    /// <summary>
    /// Verifies max elevation time falls between AOS and LOS.
    /// </summary>
    [TestMethod]
    public void VisibilityPeriod_MaxElTimeBetweenAosAndLos()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var sat = new Satellite(tle);
        var station = new GroundStation(new GeodeticCoordinate(
            Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));
        var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);

        // Act
        var passes = station.Observe(sat, start, end, TimeSpan.FromSeconds(10));

        // Assert
        foreach (var pass in passes)
        {
            Assert.IsTrue(pass.MaxElevationTime >= pass.Start, "Max elevation time should be >= AOS");
            Assert.IsTrue(pass.MaxElevationTime <= pass.End, "Max elevation time should be <= LOS");
        }
    }

    /// <summary>
    /// Verifies max elevation is positive for each pass.
    /// </summary>
    [TestMethod]
    public void VisibilityPeriod_MaxElevationIsPositive()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var sat = new Satellite(tle);
        var station = new GroundStation(new GeodeticCoordinate(
            Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));
        var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);

        // Act
        var passes = station.Observe(sat, start, end, TimeSpan.FromSeconds(10));

        // Assert
        foreach (var pass in passes)
        {
            Assert.IsTrue(pass.MaxElevation.Degrees > 0, "Max elevation should be positive");
        }
    }

    /// <summary>
    /// Verifies input validation: start >= end throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Observe_StartAfterEnd_ThrowsArgumentException()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var sat = new Satellite(tle);
        var station = new GroundStation(new GeodeticCoordinate(Angle.Zero, Angle.Zero, 0));
        var start = new DateTime(2026, 5, 21, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        try
        {
            _ = station.Observe(sat, start, end, TimeSpan.FromSeconds(10));
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Verifies input validation: negative deltaTime throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Observe_NegativeDeltaTime_ThrowsArgumentException()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var sat = new Satellite(tle);
        var station = new GroundStation(new GeodeticCoordinate(Angle.Zero, Angle.Zero, 0));
        var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(1);

        // Act & Assert
        try
        {
            _ = station.Observe(sat, start, end, TimeSpan.FromSeconds(-10));
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Verifies input validation: resolution out of range throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Observe_ResolutionOutOfRange_ThrowsArgumentException()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var sat = new Satellite(tle);
        var station = new GroundStation(new GeodeticCoordinate(Angle.Zero, Angle.Zero, 0));
        var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(1);

        // Act & Assert — resolution > 7
        try
        {
            _ = station.Observe(sat, start, end, TimeSpan.FromSeconds(10), resolution: 8);
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Verifies input validation: minElevation > 90° throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Observe_MinElevationOver90_ThrowsArgumentException()
    {
        // Arrange
        var tle = new Tle(IssLine1, IssLine2);
        var sat = new Satellite(tle);
        var station = new GroundStation(new GeodeticCoordinate(Angle.Zero, Angle.Zero, 0));
        var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(1);

        // Act & Assert
        try
        {
            _ = station.Observe(sat, start, end, TimeSpan.FromSeconds(10), minElevation: Angle.FromDegrees(91));
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Verifies GroundStation equality and hash code consistency.
    /// </summary>
    [TestMethod]
    public void EqualityAndHashCode_Consistent()
    {
        // Arrange
        var loc = new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0);
        var gs1 = new GroundStation(loc);
        var gs2 = new GroundStation(loc);

        // Act & Assert
        Assert.IsTrue(gs1 == gs2);
        Assert.AreEqual(gs1.GetHashCode(), gs2.GetHashCode());
    }
}
