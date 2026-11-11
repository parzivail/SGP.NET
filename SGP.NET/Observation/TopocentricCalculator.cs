using System;
using SGPdotNET.Propagation;
using SGPdotNET.Util;

namespace SGPdotNET.Observation;

/// <summary>
///     Computes azimuth, elevation, range, and range-rate directly from
///     geodetic observer parameters and an ECI target state
/// </summary>
public static class TopocentricCalculator
{
	/// <summary>
	///     Computes topocentric angles for a target given observer geodetic parameters and ECI target state.
	/// </summary>
	/// <param name="observerLatRad">Observer geodetic latitude in radians.</param>
	/// <param name="observerLonRad">Observer geodetic longitude in radians.</param>
	/// <param name="observerAltKm">Observer altitude in km.</param>
	/// <param name="targetEciPos">Target position in ECI frame (km).</param>
	/// <param name="targetEciVel">Target velocity in ECI frame (km/s).</param>
	/// <param name="gstRad">Greenwich sidereal time at observation epoch (radians).</param>
	/// <param name="azimuth">Output: azimuth in radians [0, 2π).</param>
	/// <param name="elevation">Output: elevation in radians [-π/2, π/2].</param>
	/// <param name="rangeKm">Output: slant range in km.</param>
	/// <param name="rangeRateKmPerSec">Output: range rate relative to the rotating observer in km/s.</param>
	public static void ComputeTopocentric(
		double observerLatRad,
		double observerLonRad,
		double observerAltKm,
		Vector3 targetEciPos,
		Vector3 targetEciVel,
		double gstRad,
		out double azimuth,
		out double elevation,
		out double rangeKm,
		out double rangeRateKmPerSec
	)
	{
		var localSidereal = gstRad + observerLonRad;
		var sinLat = Math.Sin(observerLatRad);
		var cosLat = Math.Cos(observerLatRad);
		var sinTheta = Math.Sin(localSidereal);
		var cosTheta = Math.Cos(localSidereal);

		var c = 1.0 / Math.Sqrt(1.0 + SgpConstants.EarthFlatteningConstant *
			(SgpConstants.EarthFlatteningConstant - 2.0) * sinLat * sinLat);
		var s = (1.0 - SgpConstants.EarthFlatteningConstant) *
				(1.0 - SgpConstants.EarthFlatteningConstant) * c;
		var achcp = (SgpConstants.EarthRadiusKm * c + observerAltKm) * cosLat;

		var obsX = achcp * cosTheta;
		var obsY = achcp * sinTheta;
		var obsZ = (SgpConstants.EarthRadiusKm * s + observerAltKm) * sinLat;

		var rangeX = targetEciPos.X - obsX;
		var rangeY = targetEciPos.Y - obsY;
		var rangeZ = targetEciPos.Z - obsZ;

		var topS = sinLat * cosTheta * rangeX
			+ sinLat * sinTheta * rangeY - cosLat * rangeZ;
		var topE = -sinTheta * rangeX
				   + cosTheta * rangeY;
		var topZ = cosLat * cosTheta * rangeX
				   + cosLat * sinTheta * rangeY + sinLat * rangeZ;

		rangeKm = Math.Sqrt(topS * topS + topE * topE + topZ * topZ);

		azimuth = Math.Atan(-topE / topS);
		if (topS > 0.0)
			azimuth += Math.PI;
		if (azimuth < 0.0)
			azimuth += 2.0 * Math.PI;

		elevation = Math.Asin(topZ / rangeKm);

		// A fixed station still moves in ECI as Earth rotates, so range rate uses relative velocity.
		const double earthRotationRate = SgpConstants.TwoPi * (SgpConstants.EarthRotationPerSiderealDay / SgpConstants.SecondsPerDay);
		var observerVelX = -earthRotationRate * obsY;
		var observerVelY = earthRotationRate * obsX;

		var velX = targetEciVel.X - observerVelX;
		var velY = targetEciVel.Y - observerVelY;
		var velZ = targetEciVel.Z;

		var topSdot = sinLat * cosTheta * velX
			+ sinLat * sinTheta * velY - cosLat * velZ;
		var topEdot = -sinTheta * velX
					  + cosTheta * velY;
		var topZdot = cosLat * cosTheta * velX
					  + cosLat * sinTheta * velY + sinLat * velZ;

		rangeRateKmPerSec = (topS * topSdot + topE * topEdot + topZ * topZdot) / rangeKm;
	}

	/// <summary>
	///     Computes topocentric angles for a target given observer geodetic parameters and ECI target position (zero velocity).
	/// </summary>
	public static void ComputeTopocentricStatic(
		double observerLatRad,
		double observerLonRad,
		double observerAltKm,
		Vector3 targetEciPos,
		double gstRad,
		out double azimuth,
		out double elevation,
		out double rangeKm
	)
	{
		ComputeTopocentric(
			observerLatRad,
			observerLonRad,
			observerAltKm,
			targetEciPos,
			new Vector3(),
			gstRad,
			out azimuth,
			out elevation,
			out rangeKm,
			out _
		);
	}
}
