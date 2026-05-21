using SGPdotNET.CoordinateSystem;
using SGPdotNET.Propagation;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for GeodeticCoordinate ↔ EciCoordinate round-trip conversions.
/// </summary>
[TestClass]
public sealed class CoordinateConversionTests
{
	/// <summary>
	/// Verifies Geodetic → ECI → Geodetic round-trip preserves position.
	/// </summary>
	[TestMethod]
	public void GeodeticToEciToGeodetic_RoundTrip_PreservesPosition()
	{
		// Arrange
		var time = new DateTime(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);
		var geo = new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0);

		// Act
		var eci = geo.ToEci(time);
		var roundTrip = eci.ToGeodetic();

		// Assert
		Assert.AreEqual(geo.Latitude.Degrees, roundTrip.Latitude.Degrees, 0.001);
		Assert.AreEqual(geo.Longitude.Degrees, roundTrip.Longitude.Degrees, 0.001);
		Assert.AreEqual(geo.Altitude, roundTrip.Altitude, TestConstants.SmallDistanceToleranceKm);
	}

	/// <summary>
	/// Verifies ECI position changes correctly with time due to Earth rotation.
	/// </summary>
	[TestMethod]
	public void GeodeticToEci_PositionChangesWithTime()
	{
		// Arrange
		var geo = new GeodeticCoordinate(Angle.FromDegrees(0), Angle.FromDegrees(0), 0);
		var t1 = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
		var t2 = t1.AddHours(6);

		// Act
		var eci1 = geo.ToEci(t1);
		var eci2 = geo.ToEci(t2);

		// Assert
		var dx = Math.Abs(eci2.Position.X - eci1.Position.X);
		var dy = Math.Abs(eci2.Position.Y - eci1.Position.Y);
		Assert.IsTrue(dx + dy > 100, "ECI position should change significantly over 6 hours");
	}

	/// <summary>
	/// Verifies Geodetic at altitude 0 produces ECI radius close to Earth radius.
	/// </summary>
	[TestMethod]
	public void GeodeticToEci_SurfaceAltitude_ProducesEarthRadius()
	{
		// Arrange
		var time = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
		var geo = new GeodeticCoordinate(Angle.FromDegrees(0), Angle.FromDegrees(0), 0);

		// Act
		var eci = geo.ToEci(time);
		var radius = eci.Position.Length;

		// Assert
		Assert.AreEqual(SgpConstants.EarthRadiusKm, radius, 1.0);
	}
}