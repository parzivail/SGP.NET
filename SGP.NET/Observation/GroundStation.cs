using System;
using System.Collections.Generic;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Observation;

/// <summary>
///     A representation of a ground station that can observe satellites
/// </summary>
public class GroundStation
{
	/// <summary>
	///     Holds the time of an elevation crossing point, the maximum elevation observed
	///     during the search, and the time at which that maximum occurred.
	/// </summary>
	private readonly struct CrossingPointInfo(DateTime crossingPointTime, DateTime maxElevationTime, Angle maxElevation)
	{
		public DateTime CrossingPointTime { get; } = crossingPointTime;
		public DateTime MaxElevationTime { get; } = maxElevationTime;
		public Angle MaxElevation { get; } = maxElevation;
	}
	
	/// <summary>
	///     The location of the ground station
	/// </summary>
	public Coordinate Location { get; }

	/// <summary>
	///     Precomputed observer latitude in radians for fast topocentric calculations.
	/// </summary>
	public double ObserverLatRad { get; }

	/// <summary>
	///     Precomputed observer longitude in radians for fast topocentric calculations.
	/// </summary>
	public double ObserverLonRad { get; }

	/// <summary>
	///     Precomputed observer altitude in km for fast topocentric calculations.
	/// </summary>
	public double ObserverAltKm { get; }

	/// <summary>
	///     Creates a new ground station at the specified location
	/// </summary>
	/// <param name="location">The location of the ground station. Cannot be null</param>
	public GroundStation(Coordinate location)
	{
		Location = location ?? throw new ArgumentNullException(nameof(location));
		var geo = location.ToGeodetic();
		ObserverLatRad = geo.Latitude.Radians;
		ObserverLonRad = geo.Longitude.Radians;
		ObserverAltKm = geo.Altitude;
	}

	/// <summary>
	///     Creates a list of all the predicted observations within the specified time period for this GroundStation.
	/// </summary>
	/// <param name="satellite">The satellite to observe</param>
	/// <param name="start">The time to start observing</param>
	/// <param name="end">The time to end observing</param>
	/// <param name="deltaTime">The time step for the prediction simulation</param>
	/// <param name="minElevation">The minimum elevation. Default is Angle.Zero.</param>
	/// <param name="clipToStartTime">Whether to clip the start time of the first satellite visibility period to start, if applicable. Default is true</param>
	/// <param name="clipToEndTime">Whether to clip the end time of the last satellite visibility period to end, if applicable. Default is false</param>
	/// <param name="resolution">The number of second decimal places to calculate for the start and end times. Cannot be greater than 7 (i.e. greater than tick resolution). Default is 3.</param>
	/// <returns>A list of observations where an AOS is seen at or after the start parameter</returns>
	/// <exception cref="ArgumentException">Thrown if start is greater than or equal to end, deltaTime is non-positive, resolution is not in range 0-7, or minElevation is greater than 90°</exception>
	public List<SatelliteVisibilityPeriod> Observe(
		Satellite satellite,
		DateTime start, DateTime end,
		TimeSpan deltaTime,
		Angle minElevation = default,
		bool clipToStartTime = true,
		bool clipToEndTime = false,
		int resolution = 3
	)
	{
		if (deltaTime.TotalSeconds <= 0)
			throw new ArgumentException("deltaTime must be positive", nameof(deltaTime));

		start = start.ToStrictUtc();
		end = end.ToStrictUtc();
		if (start >= end)
			throw new ArgumentException("start time must be less than end time", nameof(start));

		if (deltaTime <= TimeSpan.Zero)
			throw new ArgumentException("deltaTime must be greater than zero", nameof(deltaTime));

		if (resolution < 0)
			throw new ArgumentException("resolution must be non-negative", nameof(resolution));

		if (resolution > 7)
			throw new ArgumentException("resolution must be no more than 7 decimal places (no more than tick resolution)", nameof(resolution));

		if (minElevation.Degrees > 90)
			throw new ArgumentException("minElevation cannot be greater than 90°", nameof(minElevation));

		start = start.Round(deltaTime);
		var clippedEnd = clipToEndTime ? (DateTime?)end : null;

		var obs = new List<SatelliteVisibilityPeriod>();

		var t = start;

		do
		{
			// find the AOS Time of the next pass
			var aosCrossingPoint = FindNextBelowToAboveCrossingPoint(satellite, t, end, deltaTime, minElevation, resolution);
			if (!aosCrossingPoint.HasValue)
				// we're done if no crossing point was found
				break;

			var aosTime = aosCrossingPoint.Value;
			t = aosTime + deltaTime;
				
			// find the LOS time and max elevation for the next pass
			DateTime losTime;
			DateTime maxElTime;
			if (clippedEnd.HasValue && t > clippedEnd.Value)
			{
				losTime = clippedEnd.Value;
				maxElTime = clippedEnd.Value;
			}
			else
			{
				var tu = FindNextAboveToBelowCrossingPoint(satellite, t, deltaTime, minElevation, resolution, clippedEnd);
				losTime = tu.CrossingPointTime;
				maxElTime = tu.MaxElevationTime;
			}

			if (maxElTime == DateTime.MinValue)
			{
				t = losTime + deltaTime;
				continue;
			}

			var before = maxElTime - deltaTime;

			if (clipToStartTime)
			{
				// ensure before is clipped for max elevation search 
				before = start > before ? start : before;
			}

			var after = maxElTime + deltaTime;
			if (clipToEndTime)
			{
				// ensure after is clipped for max elevation search
				after = end < after ? end : after;
			}

			// add the visibility period for the pass
			var (maxEl, maxElTime2) = FindMaxElevation(satellite, before, maxElTime, after, resolution);
			maxElTime = maxElTime2;
			
			obs.Add(new SatelliteVisibilityPeriod(satellite, aosTime, losTime, maxEl, maxElTime, Location));

			t = losTime + deltaTime;
		} while (t <= end);

		if (!clipToStartTime && obs.Count > 0 && obs[0].Start <= start)
		{
			var first = obs[0];
			var tu = FindNextAboveToBelowCrossingPoint(satellite, first.Start, deltaTime.Negate(), minElevation, resolution);
			var maxElTime = first.MaxElevation > tu.MaxElevation ? first.MaxElevationTime : tu.MaxElevationTime;
			var (maxEl, nextMaxElTime) = FindMaxElevation(satellite, maxElTime - deltaTime, maxElTime, maxElTime + deltaTime, resolution);

			maxElTime = nextMaxElTime;
			obs[0] = new SatelliteVisibilityPeriod(satellite, tu.CrossingPointTime, first.End, maxEl, maxElTime, first.ReferencePosition);
		}

		return obs;
	}

	/// <summary>
	///     Observes a satellite at an instant in time, relative to this GroundStation
	/// </summary>
	/// <param name="satellite">The satellite to observe</param>
	/// <param name="time">The time of observation</param>
	/// <returns>The topocentric observation of the satellite</returns>
	public TopocentricObservation Observe(Satellite satellite, DateTime time)
	{
		time = time.ToStrictUtc();
		var posEci = satellite.Predict(time);
		return Observe(posEci, time);
	}

	/// <summary>
	///     Observes an ECI coordinate at an instant in time, relative to this GroundStation
	/// </summary>
	/// <param name="target">The ECI coordinate to observe</param>
	/// <param name="time">The time of observation</param>
	/// <returns>The topocentric observation of the target</returns>
	public TopocentricObservation Observe(EciCoordinate target, DateTime time)
	{
		time = time.ToStrictUtc();
		var gst = time.ToGreenwichSiderealTime();

		TopocentricCalculator.ComputeTopocentric(
			ObserverLatRad, ObserverLonRad, ObserverAltKm,
			target.Position, target.Velocity, gst,
			out var az, out var el, out var range, out var rate);

		return new TopocentricObservation(Angle.FromRadians(az), Angle.FromRadians(el), range, rate, Location);
	}

	/// <summary>
	///     Tests whether a satellite is above a specified elevation
	/// </summary>
	/// <param name="pos">The position to check</param>
	/// <param name="minElevation">The minimum elevation required to be "visible"</param>
	/// <param name="time">The time the check is occurring</param>
	/// <returns>True if the satellite is above the specified elevation, false otherwise</returns>
	public bool IsVisible(Coordinate pos, Angle minElevation, DateTime time)
	{
		time = time.ToStrictUtc();

		var pGeo = pos.ToGeodetic();
		var footprint = pGeo.GetFootprintAngle();

		if (Location.AngleTo(pGeo) > footprint) return false;

		var eci = pos.ToEci(time);
		var gst = time.ToGreenwichSiderealTime();

		TopocentricCalculator.ComputeTopocentricStatic(
			ObserverLatRad, ObserverLonRad, ObserverAltKm,
			eci.Position, gst,
			out _, out var el, out _);

		return el >= minElevation.Radians;
	}

	/// <inheritdoc />
	protected bool Equals(GroundStation other)
	{
		return Equals(Location, other.Location);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		if (obj is null) return false;
		if (ReferenceEquals(this, obj)) return true;
		return obj is GroundStation gs && Equals(gs);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return Location.GetHashCode();
	}

	/// <inheritdoc />
	public static bool operator ==(GroundStation left, GroundStation right)
	{
		return Equals(left, right);
	}

	/// <inheritdoc />
	public static bool operator !=(GroundStation left, GroundStation right)
	{
		return !Equals(left, right);
	}

	/// <summary>
	///     Computes the elevation of a satellite at a given time
	/// </summary>
	/// <param name="satellite">The satellite to observe.</param>
	/// <param name="time">The time of observation (UTC).</param>
	/// <returns>The elevation angle in radians.</returns>
	private double GetElevation(Satellite satellite, DateTime time)
	{
		var posEci = satellite.Predict(time);
		var gst = time.ToGreenwichSiderealTime();

		TopocentricCalculator.ComputeTopocentricStatic(
			ObserverLatRad, ObserverLonRad, ObserverAltKm,
			posEci.Position, gst,
			out _, out var el, out _);

		return el;
	}

	/// <summary>
	///     Finds the next time when the satellite's elevation crosses from below the
	///     minimum elevation threshold to above it (acquisition of signal).
	///     If the satellite is already above the threshold at the start time, start is returned.
	/// </summary>
	/// <param name="satellite">The satellite to observe.</param>
	/// <param name="start">The time to begin searching from.</param>
	/// <param name="end">The latest time to search until.</param>
	/// <param name="deltaTime">The time step for the initial scan.</param>
	/// <param name="minElevation">The minimum elevation threshold.</param>
	/// <param name="resolution">The temporal resolution for binary search refinement.</param>
	/// <returns>The crossing time, or null if no crossing was found before end.</returns>
	private DateTime? FindNextBelowToAboveCrossingPoint(Satellite satellite, DateTime start, DateTime end, TimeSpan deltaTime, Angle minElevation, int resolution)
	{
		var t = start - deltaTime;
		DateTime prev;
		double el;

		do
		{
			prev = t;
			var next = t + deltaTime;
			t = next <= end ? next : end;
			el = GetElevation(satellite, t);
		} while (el < minElevation.Radians && t < end);

		if (prev == start)
		{
			return t;
		}

		if (el < minElevation.Radians)
		{
			return null;
		}

		DateTime tStart, tEnd;
		if (prev < t)
		{
			tStart = prev;
			tEnd = t;
		}
		else
		{
			tStart = t;
			tEnd = prev;
		}

		return FindCrossingTimeWithinInterval(satellite, tStart, tEnd, minElevation, resolution);
	}

	/// <summary>
	///     Finds the next time when the satellite's elevation crosses from above the
	///     minimum elevation threshold to below it (loss of signal).
	///     If the satellite is already below the threshold at the start time, start is returned.
	///     Also tracks the maximum elevation and its time during the search.
	/// </summary>
	/// <param name="satellite">The satellite to observe.</param>
	/// <param name="start">The time to begin searching from.</param>
	/// <param name="deltaTime">The time step for the scan (may be negative for backward search).</param>
	/// <param name="minElevation">The minimum elevation threshold.</param>
	/// <param name="resolution">The temporal resolution for binary search refinement.</param>
	/// <param name="end">Optional end time to clamp the search. If null, search continues until below threshold.</param>
	/// <returns>A CrossingPointInfo with the crossing time, max elevation, and max elevation time.</returns>
	private CrossingPointInfo FindNextAboveToBelowCrossingPoint(Satellite satellite, DateTime start, TimeSpan deltaTime, Angle minElevation, int resolution, DateTime? end = null)
	{
		var t = start - deltaTime;
		DateTime prev;
		var maxEl = Angle.Zero;
		var maxElTime = DateTime.MinValue;
		double el;

		if (end.HasValue)
		{
			do
			{
				prev = t;
				t += deltaTime;
				el = GetElevation(satellite, t);
				var elAngle = Angle.FromRadians(el);
				if (elAngle > maxEl)
				{
					maxEl = elAngle;
					maxElTime = t;
				}
			} while (el >= minElevation.Radians && t <= end);
		}
		else
		{
			do
			{
				prev = t;
				t += deltaTime;
				el = GetElevation(satellite, t);
				var elAngle = Angle.FromRadians(el);
				if (elAngle > maxEl)
				{
					maxEl = elAngle;
					maxElTime = t;
				}
			} while (el >= minElevation.Radians);
		}

		if (t == start)
		{
			return new CrossingPointInfo(t, maxElTime, maxEl);
		}

		DateTime tStart, tEnd;
		if (prev < t)
		{
			tStart = prev;
			tEnd = t;
		}
		else
		{
			tStart = t;
			tEnd = prev;
		}

		t = FindCrossingTimeWithinInterval(satellite, tStart, tEnd, minElevation, resolution);
		return new CrossingPointInfo(t, maxElTime, maxEl);
	}

	/// <summary>
	///     Refines an elevation crossing point to the requested temporal resolution using
	///     binary search within a known interval that contains exactly one crossing.
	/// </summary>
	/// <param name="satellite">The satellite to observe.</param>
	/// <param name="start">The start of the interval.</param>
	/// <param name="end">The end of the interval.</param>
	/// <param name="minElevation">The minimum elevation threshold.</param>
	/// <param name="resolution">The number of decimal places for the result time.</param>
	/// <returns>The refined crossing time.</returns>
	/// <exception cref="ArgumentException">Thrown if start equals end.</exception>
	private DateTime FindCrossingTimeWithinInterval(Satellite satellite, DateTime start, DateTime end, Angle minElevation, int resolution)
	{
		if (start == end)
		{
			throw new ArgumentException("start and end cannot be equal", nameof(start));
		}

		var startEl = GetElevation(satellite, start);
		var endEl = GetElevation(satellite, end);
		var isAscending = startEl < endEl;

		var tBelow = start;
		var tAbove = end;
		if (!isAscending)
		{
			tBelow = end;
			tAbove = start;
		}

		var minTicks = (long)(1e7 / Math.Pow(10, resolution));

		long dt;
		DateTime t;

		// continually halve the interval until the size of the interval is less than minTicks
		do
		{
			dt = (tAbove - tBelow).Ticks / 2;
			t = tBelow.AddTicks(dt);
			var el = GetElevation(satellite, t);
			if (el < minElevation.Radians)
			{
				tBelow = t;
			}
			else
			{
				tAbove = t;
			}
		} while (Math.Abs(dt) > minTicks);

		return t.Round(TimeSpan.FromTicks(minTicks));
	}

	/// <summary>
	///     Refines the time of maximum elevation using ternary search within the interval
	///     [before, after], centered around an initial peak estimate.
	/// </summary>
	/// <param name="satellite">The satellite to observe.</param>
	/// <param name="before">The lower bound of the search interval.</param>
	/// <param name="peakTime">The initial estimate of the peak time.</param>
	/// <param name="after">The upper bound of the search interval.</param>
	/// <param name="resolution">The number of decimal places for the result time.</param>
	/// <returns>A tuple of the maximum elevation angle and the time at which it occurs.</returns>
	private Tuple<Angle, DateTime> FindMaxElevation(Satellite satellite, DateTime before, DateTime peakTime, DateTime after, int resolution)
	{
		var minTicks = (long)(1e7 / Math.Pow(10, resolution));

		do
		{
			var elPeakTime = GetElevation(satellite, peakTime);

			var t1 = before + TimeSpan.FromTicks((peakTime - before).Ticks / 2);
			var t2 = peakTime + TimeSpan.FromTicks((after - peakTime).Ticks / 2);

			var elT1 = GetElevation(satellite, t1);
			var elT2 = GetElevation(satellite, t2);

			// temporal ordering is: before, t1, peakTime, t2, after

			// find max of {elT1, elPeakTime, elT2} and choose new (before, peakTime, after) appropriately
			if (elT1 > elPeakTime && elT1 > elT2)
			{
				after = peakTime;
				peakTime = t1;
			}
			else if (elPeakTime > elT1 && elPeakTime > elT2)
			{
				before = t1;
				after = t2;
			}
			else // elT2 is max
			{
				before = peakTime;
				peakTime = t2;
			}
		} while ((after - before).Ticks > minTicks);

		var finalEl = GetElevation(satellite, peakTime);
		return Tuple.Create(Angle.FromRadians(finalEl), peakTime.Round(TimeSpan.FromTicks(minTicks)));
	}
}
