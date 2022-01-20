using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Observation
{
	/// <summary>
	///     Stores a period during which a satellite is visible to a ground station
	/// </summary>
	/// <param name="Satellite">The satellite that is being observed</param>
	/// <param name="Start">The start time of the observation</param>
	/// <param name="End">The end time of the observation</param>
	/// <param name="MaxElevation">The max elevation reached during observation</param>
	/// <param name="MaxElevationTime">The time at which max elevation is reached during observation</param>
	/// <param name="ReferencePosition">The position from which the satellite was observed to generate this observation</param>
	public record SatelliteVisibilityPeriod(Satellite Satellite, DateTime Start, DateTime End, Angle MaxElevation, DateTime MaxElevationTime, Coordinate ReferencePosition = null);
}