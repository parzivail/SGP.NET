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
	/// TLE for ISS.
	/// </summary>
	public const string IssLine1 = "1 25544U 98067A   26140.52007259  .00005164  00000-0  10084-3 0  9995";
	
	/// <summary>
	/// TLE for ISS.
	/// </summary>
	public const string IssLine2 = "2 25544  51.6328  77.0641 0007497  79.3410 280.8422 15.49283153567468";
}