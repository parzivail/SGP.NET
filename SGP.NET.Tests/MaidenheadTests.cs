using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for Maidenhead grid square conversions from known coordinates.
/// </summary>
[TestClass]
public sealed class MaidenheadTests
{
	/// <summary>
	/// Verifies Maidenhead conversion for well-known locations at 2-pair precision.
	/// </summary>
	[TestMethod]
	[DataRow(28.5843, -80.6512, "EL98")] // Cape Canaveral
	[DataRow(51.4769, -0.0005, "IO91")] // Greenwich
	[DataRow(40.6892, -74.0445, "FN20")] // New York City
	[DataRow(-33.8566, 151.2149, "QF56")] // Sydney
	[DataRow(35.6328, 139.8808, "PM95")] // Tokyo
	public void ToMaidenhead_ProducesCorrectGridSquare(double lat, double lon, string expected)
	{
		// Arrange
		var coord = new GeodeticCoordinate(Angle.FromDegrees(lat), Angle.FromDegrees(lon), 0);

		// Act
		var result = coord.ToMaidenhead(MaidenheadPrecision.HunderedKilometers);

		// Assert
		Assert.AreEqual(expected, result);
	}

	/// <summary>
	/// Verifies both AaToXx and AaToYy standards for 5th pair.
	/// </summary>
	[TestMethod]
	public void AaToXxAndAaToYy_Standards_ProduceDifferent5thPair()
	{
		// Arrange
		var coord = new GeodeticCoordinate(Angle.FromDegrees(40.0), Angle.FromDegrees(-75.0), 0);

		// Act
		var resultXx = coord.ToMaidenhead(MaidenheadPrecision.FiveHundredMeters, MaidenheadStandard.AaToXx);
		var resultYy = coord.ToMaidenhead(MaidenheadPrecision.FiveHundredMeters, MaidenheadStandard.AaToYy);

		// Assert
		Assert.AreEqual(resultXx.Length, resultYy.Length);
		Assert.AreEqual(8, resultXx.Length);
	}

	/// <summary>
	/// Verifies precision levels produce correct string lengths.
	/// </summary>
	[TestMethod]
	[DataRow(MaidenheadPrecision.ThousandKilometers, 2)]
	[DataRow(MaidenheadPrecision.HunderedKilometers, 4)]
	[DataRow(MaidenheadPrecision.FiveKilometers, 6)]
	[DataRow(MaidenheadPrecision.FiveHundredMeters, 8)]
	[DataRow(MaidenheadPrecision.TwentyMeters, 10)]
	[DataRow(MaidenheadPrecision.TwoMeters, 12)]
	public void PrecisionLevels_ProduceCorrectStringLength(MaidenheadPrecision precision, int expectedLength)
	{
		// Arrange
		var coord = new GeodeticCoordinate(Angle.FromDegrees(40.0), Angle.FromDegrees(-75.0), 0);

		// Act
		var result = coord.ToMaidenhead(precision);

		// Assert
		Assert.AreEqual(expectedLength, result.Length);
	}

	/// <summary>
	/// Verifies edge cases: poles, antimeridian, equator/prime meridian.
	/// </summary>
	[TestMethod]
	[DataRow(90.0, 0.0, "JS00AA")] // North Pole
	[DataRow(-90.0, 0.0, "JA00AA")] // South Pole
	[DataRow(0.0, 180.0, "SJ00AA")] // Antimeridian
	[DataRow(0.0, 0.0, "JJ00AA")] // Equator/prime meridian
	public void EdgeCases_ProduceValidMaidenhead(double lat, double lon, string expected)
	{
		// Arrange
		var coord = new GeodeticCoordinate(Angle.FromDegrees(lat), Angle.FromDegrees(lon), 0);

		// Act
		var result = coord.ToMaidenhead();

		// Assert
		Assert.AreEqual(expected, result);
	}
}