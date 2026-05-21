using System;
using System.IO;
using System.Linq;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Parsers;
using SGPdotNET.Propagation;
using SGPdotNET.TLE;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for GroundStation.Observe and Coordinate.Observe numerical accuracy,
/// consistency, and edge cases. These serve as regression guards for refactoring.
/// </summary>
[TestClass]
public sealed class ObservationAccuracyTests
{
	private static readonly DateTime TestTime = new(2026, 5, 20, 22, 30, 0, DateTimeKind.Utc);

	private static string ExampleDir => Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "example_inputs");

	/// <summary>
	/// Verifies GroundStation.Observe and Coordinate.Observe produce identical results
	/// for the same satellite and time.
	/// </summary>
	[TestMethod]
	public void GroundStationObserve_MatchesCoordinateObserve()
	{
		// Arrange
		var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
		var sat = new Satellite(tle);
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));

		// Act
		var gsObs = station.Observe(sat, TestTime);
		var satEci = sat.Predict(TestTime);
		var coordObs = station.Location.Observe(satEci, TestTime);

		// Assert
		Assert.AreEqual(gsObs.Azimuth.Degrees, coordObs.Azimuth.Degrees, TestConstants.ObservationAngleToleranceDeg, "Azimuth mismatch");
		Assert.AreEqual(gsObs.Elevation.Degrees, coordObs.Elevation.Degrees, TestConstants.ObservationAngleToleranceDeg, "Elevation mismatch");
		Assert.AreEqual(gsObs.Range, coordObs.Range, TestConstants.ObservationRangeToleranceKm, "Range mismatch");
	}

	/// <summary>
	/// Verifies Observe is deterministic: same inputs produce identical outputs.
	/// </summary>
	[TestMethod]
	public void Observe_IsDeterministic()
	{
		// Arrange
		var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
		var sat = new Satellite(tle);
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));

		// Act
		var obs1 = station.Observe(sat, TestTime);
		var obs2 = station.Observe(sat, TestTime);

		// Assert
		Assert.AreEqual(obs1.Azimuth.Degrees, obs2.Azimuth.Degrees, TestConstants.AngleTolerance);
		Assert.AreEqual(obs1.Elevation.Degrees, obs2.Elevation.Degrees, TestConstants.AngleTolerance);
		Assert.AreEqual(obs1.Range, obs2.Range, TestConstants.SmallDistanceToleranceKm);
	}

	/// <summary>
	/// Verifies range matches the direct ECI distance between ground station and satellite.
	/// </summary>
	[TestMethod]
	public void Range_MatchesEciDistance()
	{
		// Arrange
		var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
		var sat = new Satellite(tle);
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));

		// Act
		var obs = station.Observe(sat, TestTime);
		var satEci = sat.Predict(TestTime);
		var stationEci = station.Location.ToEci(TestTime);
		var directDistance = (satEci.Position - stationEci.Position).Length;

		// Assert
		Assert.AreEqual(directDistance, obs.Range, TestConstants.ObservationRangeToleranceKm, "Range should match ECI distance");
	}

	/// <summary>
	/// Verifies elevation sign matches whether satellite is above or below the observer's horizon plane.
	/// When the satellite's ECI position is further from Earth's center than the observer,
	/// and in roughly the same direction, elevation should be positive.
	/// </summary>
	[TestMethod]
	public void Elevation_SignMatchesRelativePosition()
	{
		// Arrange
		var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
		var sat = new Satellite(tle);
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));

		// Act
		var obs = station.Observe(sat, TestTime);
		var satEci = sat.Predict(TestTime);
		var stationEci = station.Location.ToEci(TestTime);

		// Dot product of station→sat vector with station radial direction
		var rangeVec = satEci.Position - stationEci.Position;
		var radialDot = rangeVec.Dot(stationEci.Position);

		// Assert — if satellite is outward from station, elevation should be positive
		if (radialDot > 0)
			Assert.IsGreaterThan(0, obs.Elevation.Degrees, "Satellite is outward from station, elevation should be positive");
	}

	/// <summary>
	/// Verifies visibility periods are consistent across different time step sizes.
	/// Coarser steps should find the same number of passes (±1) as finer steps.
	/// </summary>
	[TestMethod]
	public void VisibilityPeriodCount_ConsistentAcrossStepSizes()
	{
		// Arrange
		var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
		var sat = new Satellite(tle);
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));
		var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
		var end = start.AddDays(1);

		// Act
		var fine = station.Observe(sat, start, end, TimeSpan.FromSeconds(5));
		var coarse = station.Observe(sat, start, end, TimeSpan.FromSeconds(30));

		// Assert — coarse should find same or fewer passes, never more
		Assert.IsLessThanOrEqualTo(fine.Count + 1,
			coarse.Count, $"Coarse ({coarse.Count}) found more passes than fine ({fine.Count}) + 1");
		Assert.IsGreaterThanOrEqualTo(fine.Count - 1,
			coarse.Count, $"Coarse ({coarse.Count}) found significantly fewer passes than fine ({fine.Count}) - 1");
	}

	/// <summary>
	/// Verifies higher minElevation produces fewer or equal visibility periods.
	/// </summary>
	[TestMethod]
	public void HigherMinElevation_FewerOrEqualPasses()
	{
		// Arrange
		var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
		var sat = new Satellite(tle);
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));
		var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
		var end = start.AddDays(1);

		// Act
		var low = station.Observe(sat, start, end, TimeSpan.FromSeconds(10), Angle.Zero);
		var high = station.Observe(sat, start, end, TimeSpan.FromSeconds(10), Angle.FromDegrees(30));

		// Assert
		Assert.IsLessThanOrEqualTo(low.Count,
			high.Count, $"High elevation filter ({high.Count}) should produce <= passes than low ({low.Count})");
	}

	/// <summary>
	/// Verifies each visibility period's max elevation meets the minElevation threshold.
	/// </summary>
	[TestMethod]
	public void VisibilityPeriod_MaxElMeetsMinThreshold()
	{
		// Arrange
		var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
		var sat = new Satellite(tle);
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));
		var start = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);
		var end = start.AddDays(1);
		var minEl = Angle.FromDegrees(10);

		// Act
		var passes = station.Observe(sat, start, end, TimeSpan.FromSeconds(10), minEl);

		// Assert
		foreach (var pass in passes)
		{
			Assert.IsGreaterThanOrEqualTo(minEl.Degrees - TestConstants.BigDistanceToleranceKm,
				pass.MaxElevation.Degrees, $"Max elevation {pass.MaxElevation.Degrees:F1}° below threshold {minEl.Degrees}°");
		}
	}

	/// <summary>
	/// Verifies IsVisible is consistent with Observe elevation at the same time.
	/// </summary>
	[TestMethod]
	public void IsVisible_ConsistentWithObserveElevation()
	{
		// Arrange
		var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
		var sat = new Satellite(tle);
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));
		var minEl = Angle.FromDegrees(5);

		// Act
		var satEci = sat.Predict(TestTime);
		var obs = station.Observe(sat, TestTime);
		var isVisible = station.IsVisible(satEci, minEl, TestTime);

		// Assert
		Assert.AreEqual(obs.Elevation.Degrees >= minEl.Degrees, isVisible,
			"IsVisible should match Observe elevation check");
	}

	/// <summary>
	/// Verifies Observe with an EciCoordinate target (not Satellite-predicted) works correctly.
	/// This exercises the direct Coordinate.Observe path.
	/// </summary>
	[TestMethod]
	public void CoordinateObserve_WithEciTarget_ProducesValidResult()
	{
		// Arrange
		var station = new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0);
		var targetEci = new EciCoordinate(TestTime, new Vector3(6000, 1000, 1000));

		// Act
		var obs = station.Observe(targetEci, TestTime);

		// Assert
		Assert.IsTrue(obs.Azimuth.Degrees is >= 0 and < 360);
		Assert.IsTrue(obs.Elevation.Degrees is >= -90 and <= 90);
		Assert.IsGreaterThan(0, obs.Range);
	}

	/// <summary>
	/// Verifies Observe with a GeodeticCoordinate target produces the same result
	/// as converting the target to ECI first.
	/// </summary>
	[TestMethod]
	public void CoordinateObserve_GeodeticTarget_MatchesEciTarget()
	{
		// Arrange
		var station = new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0);
		var targetGeo = new GeodeticCoordinate(Angle.FromDegrees(35.0), Angle.FromDegrees(-75.0), 400);

		// Act
		var obsFromGeo = station.Observe(targetGeo, TestTime);
		var obsFromEci = station.Observe(targetGeo.ToEci(TestTime), TestTime);

		// Assert
		Assert.AreEqual(obsFromGeo.Azimuth.Degrees, obsFromEci.Azimuth.Degrees, TestConstants.ObservationAngleToleranceDeg);
		Assert.AreEqual(obsFromGeo.Elevation.Degrees, obsFromEci.Elevation.Degrees, TestConstants.ObservationAngleToleranceDeg);
		Assert.AreEqual(obsFromGeo.Range, obsFromEci.Range, TestConstants.ObservationRangeToleranceKm);
	}

	/// <summary>
	/// Verifies that a satellite directly overhead produces elevation near 90°.
	/// Uses a synthetic EciCoordinate at the observer's zenith.
	/// </summary>
	[TestMethod]
	public void Observe_OverheadTarget_ElevationNear90()
	{
		// Arrange
		var station = new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0);
		var stationEci = station.ToEci(TestTime);
		var overhead = new Vector3(
			stationEci.Position.X * 1.06,
			stationEci.Position.Y * 1.06,
			stationEci.Position.Z * 1.06
		);
		var target = new EciCoordinate(TestTime, overhead);

		// Act
		var obs = station.Observe(target, TestTime);

		// Assert — elevation should be very close to 90°
		Assert.IsGreaterThan(89.0,
			obs.Elevation.Degrees, $"Overhead target elevation {obs.Elevation.Degrees:F2}° should be > 89°");
	}

	/// <summary>
	/// Verifies that a target at the observer's antipode produces elevation near -90°.
	/// </summary>
	[TestMethod]
	public void Observe_AntipodeTarget_ElevationNearMinus90()
	{
		// Arrange
		var station = new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0);
		var stationEci = station.ToEci(TestTime);
		var antipode = new Vector3(
			-stationEci.Position.X * 1.06,
			-stationEci.Position.Y * 1.06,
			-stationEci.Position.Z * 1.06);
		var target = new EciCoordinate(TestTime, antipode);

		// Act
		var obs = station.Observe(target, TestTime);

		// Assert — elevation should be very close to -90°
		Assert.IsLessThan(-89.0,
			obs.Elevation.Degrees, $"Antipode target elevation {obs.Elevation.Degrees:F2}° should be < -89°");
	}

	/// <summary>
	/// Verifies that a target perpendicular to the observer's radial direction
	/// (i.e., on the geometric horizon plane) produces elevation near 0°.
	/// </summary>
	[TestMethod]
	public void Observe_HorizonPlaneTarget_ElevationNear0()
	{
		// Arrange
		var station = new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0);
		var stationEci = station.ToEci(TestTime);
		var radial = stationEci.Position;
		var rLen = radial.Length;

		// Create a vector perpendicular to radial by crossing with an arbitrary non-parallel vector
		var arbitrary = Math.Abs(radial.Z / rLen) < 0.9
			? new Vector3(0, 0, 1)
			: new Vector3(1, 0, 0);
		var crossX = radial.Y * arbitrary.Z - radial.Z * arbitrary.Y;
		var crossY = radial.Z * arbitrary.X - radial.X * arbitrary.Z;
		var crossZ = radial.X * arbitrary.Y - radial.Y * arbitrary.X;
		var perp = new Vector3(crossX, crossY, crossZ);
		var perpLen = perp.Length;

		// Place target at the horizon distance (tangent to Earth's surface from observer)
		var horizonDist = Math.Sqrt((rLen + 0.1) * (rLen + 0.1) - rLen * rLen);
		var horizonPoint = new Vector3(
			stationEci.Position.X + perp.X / perpLen * horizonDist,
			stationEci.Position.Y + perp.Y / perpLen * horizonDist,
			stationEci.Position.Z + perp.Z / perpLen * horizonDist
		);
		var target = new EciCoordinate(TestTime, horizonPoint);

		// Act
		var obs = station.Observe(target, TestTime);

		// Assert — elevation should be near 0°
		Assert.IsLessThan(TestConstants.MaxAngleChangePer10Seconds,
			Math.Abs(obs.Elevation.Degrees), $"Horizon plane target elevation {obs.Elevation.Degrees:F2}° should be near 0°");
	}

	/// <summary>
	/// Verifies cross-format OMM satellites produce consistent observation angles
	/// when they share the same orbital elements.
	/// </summary>
	[TestMethod]
	public void Observe_OmmFormats_ProduceConsistentAngles()
	{
		// Arrange
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));

		var csvData = new OmmCsvParser().ParseFile(Path.Combine(ExampleDir, "csv_resource.csv"));
		var kvnData = new OmmKvnParser().ParseFile(Path.Combine(ExampleDir, "kvn_visual.txt"));

		// TERRA (25994) is in both CSV and KVN
		var csvOmm = csvData.First(x => x.NoradCatID == 25994);
		var kvnOmm = kvnData.First(x => x.NoradCatID == 25994);

		// Act
		var csvSat = new Satellite(csvOmm);
		var kvnSat = new Satellite(kvnOmm);

		var csvObs = station.Observe(csvSat, TestTime);
		var kvnObs = station.Observe(kvnSat, TestTime);

		// Assert
		Assert.AreEqual(csvObs.Azimuth.Degrees, kvnObs.Azimuth.Degrees, TestConstants.CrossFormatAngleToleranceDeg, "Azimuth mismatch CSV vs KVN");
		Assert.AreEqual(csvObs.Elevation.Degrees, kvnObs.Elevation.Degrees, TestConstants.CrossFormatAngleToleranceDeg, "Elevation mismatch CSV vs KVN");
		Assert.AreEqual(csvObs.Range, kvnObs.Range, TestConstants.CrossFormatRangeToleranceKm, "Range mismatch CSV vs KVN");
	}

	/// <summary>
	/// Verifies that changing the observation time by a small amount produces
	/// smoothly changing angles (no discontinuities).
	/// </summary>
	[TestMethod]
	public void Observe_SmallTimeChange_SmoothAngleChange()
	{
		// Arrange
		var tle = new Tle(TestConstants.IssLine1, TestConstants.IssLine2);
		var sat = new Satellite(tle);
		var station = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(28.3922), Angle.FromDegrees(-80.6077), 0));

		// Act
		var t1 = TestTime;
		var t2 = t1.AddSeconds(10);

		var obs1 = station.Observe(sat, t1);
		var obs2 = station.Observe(sat, t2);

		// Assert — angles should change by less than 1° in 10 seconds for ISS
		var dAz = Math.Abs(obs2.Azimuth.Degrees - obs1.Azimuth.Degrees);
		if (dAz > 180) dAz = 360 - dAz;
		var dEl = Math.Abs(obs2.Elevation.Degrees - obs1.Elevation.Degrees);

		Assert.IsLessThan(TestConstants.MaxAngleChangePer10Seconds, dAz, $"Azimuth changed {dAz:F1}° in 10s, expected < {TestConstants.MaxAngleChangePer10Seconds}°");
		Assert.IsLessThan(TestConstants.MaxAngleChangePer10Seconds, dEl, $"Elevation changed {dEl:F1}° in 10s, expected < {TestConstants.MaxAngleChangePer10Seconds}°");
	}
}