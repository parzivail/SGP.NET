using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Propagation;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for CelestialBodies.PredictSun: distance, declination, and subsolar point accuracy.
/// </summary>
[TestClass]
public sealed class SunPositionTests
{
    /// <summary>
    /// Verifies the Sun's distance from Earth is approximately 1 AU (~149.6 million km).
    /// </summary>
    [TestMethod]
    public void SunDistance_IsApproximatelyOneAu()
    {
        // Arrange
        var time = new DateTime(2026, 6, 21, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var sun = CelestialBodies.PredictSun(time);
        var distance = sun.Position.Length;

        // Assert -- 1 AU = 149,597,870.691 km, allow ±2% for orbital eccentricity (e=0.01671)
        Assert.AreEqual(SgpConstants.KmPerAu, distance, SgpConstants.KmPerAu * 0.02);
    }

    /// <summary>
    /// Verifies the Sun's declination at the June solstice is near +23.44°.
    /// </summary>
    [TestMethod]
    public void JuneSolstice_DeclinationIsPositiveTropic()
    {
        // Arrange
        var time = new DateTime(2026, 6, 21, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var sun = CelestialBodies.PredictSun(time);
        var geo = sun.ToGeodetic();

        // Assert -- declination should be close to +23.44°
        Assert.AreEqual(23.44, geo.Latitude.Degrees, 0.5);
    }

    /// <summary>
    /// Verifies the Sun's declination at the December solstice is near -23.44°.
    /// </summary>
    [TestMethod]
    public void DecemberSolstice_DeclinationIsNegativeTropic()
    {
        // Arrange
        var time = new DateTime(2026, 12, 21, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var sun = CelestialBodies.PredictSun(time);
        var geo = sun.ToGeodetic();

        // Assert -- declination should be close to -23.44°
        Assert.AreEqual(-23.44, geo.Latitude.Degrees, 0.5);
    }

    /// <summary>
    /// Verifies the Sun's declination near the March equinox is close to 0°.
    /// </summary>
    [TestMethod]
    public void MarchEquinox_DeclinationIsNearZero()
    {
        // Arrange
        var time = new DateTime(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var sun = CelestialBodies.PredictSun(time);
        var geo = sun.ToGeodetic();

        // Assert
        Assert.AreEqual(0.0, geo.Latitude.Degrees, 0.5);
    }

    /// <summary>
    /// Verifies the Sun's declination near the September equinox is close to 0°.
    /// </summary>
    [TestMethod]
    public void SeptemberEquinox_DeclinationIsNearZero()
    {
        // Arrange
        var time = new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var sun = CelestialBodies.PredictSun(time);
        var geo = sun.ToGeodetic();

        // Assert
        Assert.AreEqual(0.0, geo.Latitude.Degrees, 0.5);
    }

    /// <summary>
    /// Verifies the subsolar longitude tracks Earth's rotation over 24 hours.
    /// </summary>
    [TestMethod]
    public void SubsolarLongitude_TracksEarthRotation()
    {
        // Arrange
        var t1 = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc);
        var t2 = t1.AddHours(12);

        // Act
        var geo1 = CelestialBodies.PredictSun(t1).ToGeodetic();
        var geo2 = CelestialBodies.PredictSun(t2).ToGeodetic();

        // Assert -- longitude should change by approximately 180° in 12 hours
        var deltaLon = Math.Abs(geo2.Longitude.Degrees - geo1.Longitude.Degrees);
        if (deltaLon > 180) deltaLon = 360 - deltaLon;
        Assert.AreEqual(180.0, deltaLon, 2.0);
    }

    /// <summary>
    /// Verifies the Sun's position is in the correct quadrant for a known date.
    /// At the June solstice, the Sun should be near RA 90° (6h), which means
    /// the ECI Y component should be positive and dominant.
    /// </summary>
    [TestMethod]
    public void JuneSolstice_PositionQuadrantIsCorrect()
    {
        // Arrange
        var time = new DateTime(2026, 6, 21, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var sun = CelestialBodies.PredictSun(time);

        // Assert -- at June solstice, Sun is near +Y in equatorial frame
        Assert.IsGreaterThan(0, sun.Position.Y);
        Assert.IsGreaterThan(Math.Abs(sun.Position.X), Math.Abs(sun.Position.Y));
        Assert.IsGreaterThan(0, sun.Position.Z); // north of equator
    }

    /// <summary>
    /// Verifies PredictSun returns consistent results for the same input time.
    /// </summary>
    [TestMethod]
    public void PredictSun_IsDeterministic()
    {
        // Arrange
        var time = new DateTime(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);

        // Act
        var sun1 = CelestialBodies.PredictSun(time);
        var sun2 = CelestialBodies.PredictSun(time);

        // Assert
        Assert.AreEqual(sun1.Position.X, sun2.Position.X, TestConstants.AngleTolerance);
        Assert.AreEqual(sun1.Position.Y, sun2.Position.Y, TestConstants.AngleTolerance);
        Assert.AreEqual(sun1.Position.Z, sun2.Position.Z, TestConstants.AngleTolerance);
    }
    
    /// <summary>
    /// Verifies the Sun can be observed from a ground station using Coordinate.Observe,
    /// producing reasonable elevation angles at solar noon vs midnight on the equator.
    /// </summary>
    [TestMethod]
    public void SunObservation_NoonVsMidnight_ElevationDifference()
    {
        // Arrange equinox, observer on equator at longitude 0°
        // Solar noon at lon=0° is approximately 12:00 UTC on the equinox
        var equinox = new DateTime(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);
        var observer = new GeodeticCoordinate(Angle.Zero, Angle.Zero, 0);

        // At equinox noon, the sun should be near zenith for an observer at lon=0°
        var noonSun = CelestialBodies.PredictSun(equinox);
        var noonObs = observer.Observe(noonSun, equinox);

        // 12 hours later, sun should be on the opposite side of Earth
        var midnightTime = equinox.AddHours(12);
        var midnightSun = CelestialBodies.PredictSun(midnightTime);
        var midnightObs = observer.Observe(midnightSun, midnightTime);

        // Assert noon elevation should be high, midnight elevation should be negative
        Assert.IsGreaterThan(60, noonObs.Elevation.Degrees, $"Noon elevation too low: {noonObs.Elevation.Degrees:F1}°");
        Assert.IsLessThan(-30, midnightObs.Elevation.Degrees, $"Midnight elevation too high: {midnightObs.Elevation.Degrees:F1}°");
    }
}
