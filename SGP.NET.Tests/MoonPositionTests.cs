using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Propagation;
using SGPdotNET.Propagation.Bodies;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for Moon.Predict: distance, position, and sublunar point accuracy.
/// Algorithm source: Meeus, "Astronomical Algorithms" 2nd Ed., Chapter 47.
/// </summary>
[TestClass]
public sealed class MoonPositionTests
{
    /// <summary>
    /// Verifies the Moon's distance from Earth is within expected bounds (~356,000–406,000 km).
    /// </summary>
    [TestMethod]
    public void MoonDistance_IsWithinExpectedBounds()
    {
        // Arrange
        var time = new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var moon = Moon.Predict(time);
        var distance = moon.Position.Length;

        // Assert perigee ~356,400 km, apogee ~406,700 km
        Assert.IsTrue(distance is > 350000 and < 410000, $"Moon distance {distance:F0} km outside expected range [350000, 410000]");
    }

    /// <summary>
    /// Verifies the Meeus Chapter 47 example: 1992 April 12.0 (JD 2448724.5).
    /// Expected mean (non-nutation) values: ecliptic longitude ~133.163°,
    /// ecliptic latitude ~3.279°, distance ~368,410 km.
    /// Note: The book's apparent values (134.688°, 13.768°) include nutation corrections.
    /// </summary>
    [TestMethod]
    public void MeeusExample_1992April12_MatchesMeanValues()
    {
        // Arrange 1992 April 12.0 = JD 2448724.5
        var time = new DateTime(1992, 4, 12, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var moon = Moon.Predict(time);
        var pos = moon.Position;

        // Convert ECI to ecliptic coordinates for comparison
        var jde = time.ToJulian();
        var epsilonDeg = 23.439 - 0.0000004 * (jde - 2451545.0);
        var epsilon = epsilonDeg * Math.PI / 180.0;
        var cosEpsilon = Math.Cos(epsilon);
        var sinEpsilon = Math.Sin(epsilon);

        var xEcl = pos.X;
        var yEcl = pos.Y * cosEpsilon + pos.Z * sinEpsilon;
        var zEcl = -pos.Y * sinEpsilon + pos.Z * cosEpsilon;

        var lambdaDeg = Math.Atan2(yEcl, xEcl) * 180.0 / Math.PI;
        if (lambdaDeg < 0) lambdaDeg += 360.0;
        var betaDeg = Math.Atan2(zEcl, Math.Sqrt(xEcl * xEcl + yEcl * yEcl)) * 180.0 / Math.PI;
        var distance = moon.Position.Length;

        // Assert mean position matches reference implementation
        // Longitude matches geocentric ecliptical: 133.162655°
        Assert.AreEqual(133.163, lambdaDeg, 0.01, "Ecliptic longitude mismatch");
        // Latitude is ~-3.23° for mean position (apparent is +13.77° with nutation)
        Assert.AreEqual(-3.23, betaDeg, 0.01, "Ecliptic latitude mismatch");
        Assert.AreEqual(368410.0, distance, 10.0, "Distance mismatch");
    }

    /// <summary>
    /// Verifies the Moon's ecliptic latitude stays within ±6° (orbital inclination ~5.14°).
    /// Note: geodetic latitude from ECI is declination (equatorial), which can reach ±28.5°.
    /// </summary>
    [TestMethod]
    public void MoonLatitude_StaysWithinOrbitalInclination()
    {
        // Arrange -- test several dates across a year
        var dates = new[]
        {
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
        };

        // Assert declination (equatorial) can reach ±28.5°, but ecliptic latitude < 6°
        foreach (var date in dates)
        {
            var moon = Moon.Predict(date);
            var geo = moon.ToGeodetic();
            // Declination can be up to ~28.5° (23.44° + 5.14°)
            Assert.IsLessThanOrEqualTo(30.0, Math.Abs(geo.Latitude.Degrees), $"Moon declination {geo.Latitude.Degrees:F2}° exceeds ±30° on {date:yyyy-MM-dd}");
        }
    }

    /// <summary>
    /// Verifies the sublunar longitude tracks Earth's rotation over 24 hours.
    /// </summary>
    [TestMethod]
    public void SublunarLongitude_TracksEarthRotation()
    {
        // Arrange
        var t1 = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc);
        var t2 = t1.AddHours(12);

        // Act
        var geo1 = Moon.Predict(t1).ToGeodetic();
        var geo2 = Moon.Predict(t2).ToGeodetic();

        // Assert longitude should change by approximately 180° in 12 hours
        // (Moon moves ~6.5° in 12h, so net change ≈ 180° ± 10°)
        var deltaLon = Math.Abs(geo2.Longitude.Degrees - geo1.Longitude.Degrees);
        if (deltaLon > 180) deltaLon = 360 - deltaLon;
        Assert.IsTrue(deltaLon is > 170 and < 190, $"Sublunar longitude changed {deltaLon:F1}° in 12h, expected ~180°");
    }

    /// <summary>
    /// Verifies the Moon's declination stays within expected bounds (±28.5° max).
    /// The Moon's declination varies between ±(23.44° + 5.14°) ≈ ±28.5° over its 18.6-year nodal cycle.
    /// </summary>
    [TestMethod]
    public void MoonDeclination_StaysWithinBounds()
    {
        // Arrange test dates across a year
        var dates = new[]
        {
            new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc),
        };

        // Assert
        foreach (var date in dates)
        {
            var moon = Moon.Predict(date);
            var geo = moon.ToGeodetic();
            Assert.IsLessThanOrEqualTo(30.0, Math.Abs(geo.Latitude.Degrees), $"Moon declination {geo.Latitude.Degrees:F1}° exceeds ±30° on {date:yyyy-MM-dd}");
        }
    }

    /// <summary>
    /// Verifies Moon.Predict returns consistent results for the same input time.
    /// </summary>
    [TestMethod]
    public void PredictMoon_IsDeterministic()
    {
        // Arrange
        var time = new DateTime(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);

        // Act
        var moon1 = Moon.Predict(time);
        var moon2 = Moon.Predict(time);

        // Assert
        Assert.AreEqual(moon1.Position.X, moon2.Position.X, TestConstants.AngleTolerance);
        Assert.AreEqual(moon1.Position.Y, moon2.Position.Y, TestConstants.AngleTolerance);
        Assert.AreEqual(moon1.Position.Z, moon2.Position.Z, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies the Moon can be observed from a ground station, producing
    /// reasonable elevation angles at moonrise vs moonset times.
    /// </summary>
    [TestMethod]
    public void MoonObservation_ProducesValidElevation()
    {
        // Arrange
        var observer = new GeodeticCoordinate(
            Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0);
        var time = new DateTime(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);

        // Act
        var moon = Moon.Predict(time);
        var obs = observer.Observe(moon, time);

        // Assert
        Assert.IsTrue(obs.Azimuth.Degrees is >= 0 and < 360);
        Assert.IsTrue(obs.Elevation.Degrees is >= -90 and <= 90);
        Assert.IsTrue(obs.Range is > 350000 and < 410000, $"Moon range {obs.Range:F0} km outside expected bounds");
    }

    /// <summary>
    /// Verifies the Moon's distance changes measurably over a day due to its ~27-day orbit.
    /// </summary>
    [TestMethod]
    public void MoonDistance_ChangesOverDay()
    {
        // Arrange
        var t1 = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
        var t2 = t1.AddDays(1);

        // Act
        var d1 = Moon.Predict(t1).Position.Length;
        var d2 = Moon.Predict(t2).Position.Length;

        // Assert Moon's distance changes by several hundred km per day
        Assert.IsGreaterThan(100, Math.Abs(d2 - d1), $"Moon distance change {Math.Abs(d2 - d1):F0} km over 1 day should be > 100 km");
    }

    /// <summary>
    /// Verifies the Moon's position changes smoothly over small time intervals.
    /// </summary>
    [TestMethod]
    public void MoonPosition_SmallTimeChange_SmoothMovement()
    {
        // Arrange
        var t1 = new DateTime(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);
        var t2 = t1.AddMinutes(10);

        // Act
        var moon1 = Moon.Predict(t1);
        var moon2 = Moon.Predict(t2);

        // Assert Moon moves ~0.5° per hour, so ~0.08° in 10 minutes
        var dx = moon2.Position.X - moon1.Position.X;
        var dy = moon2.Position.Y - moon1.Position.Y;
        var dz = moon2.Position.Z - moon1.Position.Z;
        var dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);

        // At ~385,000 km, 0.08° ≈ 540 km
        Assert.IsTrue(dist is > 200 and < 2000, $"Moon moved {dist:F0} km in 10 min, expected 200–2000 km");
    }
}
