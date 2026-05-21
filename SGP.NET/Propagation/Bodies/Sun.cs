using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Propagation.Bodies;

public static class Sun
{
	/// <summary>
	///     Calculates the Sun's position in Earth-Centered Inertial (ECI) coordinates
	///     using the Astronomical Almanac approximate algorithm.
	/// </summary>
	/// <param name="time">The time of observation (UTC).</param>
	/// <returns>
	///     An EciCoordinate representing the Sun's position. The Position vector is in kilometers
	///     relative to Earth's center. Velocity is zero (not computed by this algorithm).
	/// </returns>
	/// <remarks>
	///     <para>
	///         This implementation follows the "Approximate position" equations from the
	///         Astronomical Almanac, as documented at
	///         https://en.wikipedia.org/wiki/Position_of_the_Sun
	///     </para>
	///     <para>
	///         Accuracy is approximately 0.01° (36 arcseconds) for dates between 1950 and 2050.
	///     </para>
	///     <para>
	///         The returned ECI coordinate can be converted to geodetic coordinates via
	///         <c>eci.ToGeodetic()</c> to obtain the subsolar point (latitude/longitude where
	///         the Sun is directly overhead), or used with <c>GroundStation.Observe()</c> for
	///         sunrise/sunset and solar track calculations.
	///     </para>
	/// </remarks>
	public static EciCoordinate Predict(DateTime time)
	{
		var n = time.ToJulian() - 2451545.0;

		var l = MathUtil.Wrap360(280.460 + 0.9856474 * n);
		var gDeg = MathUtil.Wrap360(357.528 + 0.9856003 * n);
		var g = MathUtil.DegreesToRadians(gDeg);

		var lambdaDeg = l + 1.915 * Math.Sin(g) + 0.020 * Math.Sin(2.0 * g);
		var lambda = MathUtil.DegreesToRadians(lambdaDeg);

		var epsilonDeg = 23.439 - 0.0000004 * n;
		var epsilon = MathUtil.DegreesToRadians(epsilonDeg);

		var r = 1.00014 - 0.01671 * Math.Cos(g) - 0.00014 * Math.Cos(2.0 * g);

		var cosLambda = Math.Cos(lambda);
		var sinLambda = Math.Sin(lambda);
		var cosEpsilon = Math.Cos(epsilon);
		var sinEpsilon = Math.Sin(epsilon);

		var xAu = r * cosLambda;
		var yAu = r * cosEpsilon * sinLambda;
		var zAu = r * sinEpsilon * sinLambda;

		var xKm = xAu * SgpConstants.KmPerAu;
		var yKm = yAu * SgpConstants.KmPerAu;
		var zKm = zAu * SgpConstants.KmPerAu;

		return new EciCoordinate(time, new Vector3(xKm, yKm, zKm));
	}
}