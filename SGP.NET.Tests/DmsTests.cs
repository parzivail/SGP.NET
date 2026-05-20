using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for Degrees-Minutes-Seconds coordinate formatting.
/// </summary>
[TestClass]
public sealed class DmsTests
{
	/// <summary>
	/// Verifies ToDegreesMinutesSeconds for known coordinates with correct hemisphere indicators.
	/// </summary>
	[TestMethod]
	[DataRow(28.3922, -80.6077, "28°23'31.92\"\"N 80°36'27.72\"\"W")] // Cape Canaveral
	[DataRow(51.4769, -0.0005, "51°28'36.84\"\"N 0°00'1.80\"\"W")] // Greenwich
	[DataRow(-33.8688, 151.2093, "33°52'7.68\"\"S 151°12'33.48\"\"E")] // Sydney
	public void ToDegreesMinutesSeconds_ProducesCorrectFormat(double lat, double lon, string expected)
	{
		// Arrange
		var coord = new GeodeticCoordinate(Angle.FromDegrees(lat), Angle.FromDegrees(lon), 0);

		// Act
		var result = coord.ToDegreesMinutesSeconds();

		// Assert
		Assert.AreEqual(expected, result);
	}
}