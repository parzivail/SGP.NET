using SGPdotNET.CoordinateSystem;
using SGPdotNET.Propagation;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for great circle distance and angle calculations between coordinates.
/// </summary>
[TestClass]
public sealed class GreatCircleTests
{
	private const double DistanceToleranceKm = 1.0;

	/// <summary>
	/// Verifies DistanceTo between known city pairs.
	/// </summary>
	[TestMethod]
	[DataRow(40.7050, -74.0152, 33.8147, -117.9211, 3923)] // NYC → LA
	[DataRow(51.5074, -0.1278, 48.8673, 2.7810, 359)] // London → Paris
	[DataRow(35.6619, 139.6976, 37.5516, 126.9879, 1154)] // Tokyo → Seoul
	public void DistanceTo_MatchesKnownDistances(double lat1, double lon1, double lat2, double lon2, double expectedKm)
	{
		// Arrange
		var a = new GeodeticCoordinate(Angle.FromDegrees(lat1), Angle.FromDegrees(lon1), 0);
		var b = new GeodeticCoordinate(Angle.FromDegrees(lat2), Angle.FromDegrees(lon2), 0);

		// Act
		var distance = a.DistanceTo(b);

		// Assert
		Assert.AreEqual(expectedKm, distance, DistanceToleranceKm);
	}

	/// <summary>
	/// Verifies symmetry: A→B equals B→A.
	/// </summary>
	[TestMethod]
	public void AngleTo_IsSymmetric()
	{
		// Arrange
		var a = new GeodeticCoordinate(Angle.FromDegrees(40.0), Angle.FromDegrees(-75.0), 0);
		var b = new GeodeticCoordinate(Angle.FromDegrees(34.0), Angle.FromDegrees(-118.0), 0);

		// Act
		var ab = a.AngleTo(b);
		var ba = b.AngleTo(a);

		// Assert
		Assert.AreEqual(ab.Radians, ba.Radians, 1e-10);
	}

	/// <summary>
	/// Verifies self-distance is near zero.
	/// </summary>
	[TestMethod]
	public void DistanceTo_Self_IsNearZero()
	{
		// Arrange
		var coord = new GeodeticCoordinate(Angle.FromDegrees(40.0), Angle.FromDegrees(-75.0), 100);

		// Act
		var distance = coord.DistanceTo(coord);

		// Assert
		Assert.AreEqual(0, distance, 1e-4);
	}

	/// <summary>
	/// Verifies pole-to-pole distance equals half Earth circumference.
	/// </summary>
	[TestMethod]
	public void AngleTo_PoleToPole_EqualsHalfCircumference()
	{
		// Arrange
		var north = new GeodeticCoordinate(Angle.FromDegrees(90), Angle.Zero, 0);
		var south = new GeodeticCoordinate(Angle.FromDegrees(-90), Angle.Zero, 0);

		// Act
		var distance = north.DistanceTo(south);
		var expected = Math.PI * SgpConstants.EarthRadiusKm;

		// Assert
		Assert.AreEqual(expected, distance, 1e-5);
	}
}