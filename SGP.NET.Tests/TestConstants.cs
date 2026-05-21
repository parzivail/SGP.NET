namespace SGPdotNET.Tests;

/// <summary>
/// Constants used in tests.
/// </summary>
public class TestConstants
{
	/// <summary>
	/// Tolerance for angles, both degrees and radians.
	/// </summary>
	public const double AngleTolerance = 1e-10;
	
	/// <summary>
	/// Tolerances for small distances in km.
	/// </summary>
	public const double SmallDistanceToleranceKm = 1e-4;
	
	/// <summary>
	/// Tolerances for large distances in km.
	/// </summary>
	public const double BigDistanceToleranceKm = 1e-1;
	
	/// <summary>
	/// Tolerance for observation angle comparisons (azimuth, elevation) in degrees.
	/// </summary>
	public const double ObservationAngleToleranceDeg = 1e-8;
	
	/// <summary>
	/// Tolerance for observation range comparisons in km.
	/// </summary>
	public const double ObservationRangeToleranceKm = 1e-5;
	
	/// <summary>
	/// Tolerance for cross-format OMM observation angle comparisons in degrees.
	/// </summary>
	public const double CrossFormatAngleToleranceDeg = 0.01;
	
	/// <summary>
	/// Tolerance for cross-format OMM observation range comparisons in km.
	/// </summary>
	public const double CrossFormatRangeToleranceKm = 0.1;
	
	/// <summary>
	/// Maximum expected angle change (degrees) for a satellite in 10 seconds.
	/// </summary>
	public const double MaxAngleChangePer10Seconds = 5.0;
	
	/// <summary>
	/// TLE for ISS.
	/// </summary>
	public const string IssLine1 = "1 25544U 98067A   26140.52007259  .00005164  00000-0  10084-3 0  9995";
	
	/// <summary>
	/// TLE for ISS.
	/// </summary>
	public const string IssLine2 = "2 25544  51.6328  77.0641 0007497  79.3410 280.8422 15.49283153567468";
}