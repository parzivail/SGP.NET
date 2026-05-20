using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for satellite footprint, boundary, and line-of-sight calculations.
/// </summary>
[TestClass]
public sealed class FootprintTests
{
	/// <summary>
	/// Verifies GetFootprintAngle for a surface observer is zero.
	/// </summary>
	[TestMethod]
	public void GetFootprintAngle_SurfaceObserver_IsZero()
	{
		// Arrange
		var coord = new GeodeticCoordinate(Angle.FromDegrees(40.0), Angle.FromDegrees(-75.0), 0);

		// Act
		var angle = coord.GetFootprintAngle();

		// Assert
		Assert.AreEqual(0, angle.Radians, 1e-10);
	}

	/// <summary>
	/// Verifies CanSee returns true for observer within footprint, false outside.
	/// </summary>
	[TestMethod]
	public void CanSee_DistinguishesVisibleFromInvisible()
	{
		// Arrange
		var satellite = new GeodeticCoordinate(Angle.FromDegrees(40.0), Angle.FromDegrees(-75.0), 400);
		var nearby = new GeodeticCoordinate(Angle.FromDegrees(40.0), Angle.FromDegrees(-74.0), 0);
		var farAway = new GeodeticCoordinate(Angle.FromDegrees(-40.0), Angle.FromDegrees(75.0), 0);

		// Act & Assert
		Assert.IsTrue(nearby.CanSee(satellite));
		Assert.IsFalse(farAway.CanSee(satellite));
	}
}