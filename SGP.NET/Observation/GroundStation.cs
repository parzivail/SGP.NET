using System;
using System.Collections.Generic;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Observation
{
	/// <summary>
	///     A representation of a ground station that can observe satellites
	/// </summary>
	public class GroundStation
	{
		/// <summary>
		///     The location of the ground station
		/// </summary>
		public Coordinate Location { get; }

		/// <summary>
		///     Creates a new ground station at the specified location
		/// </summary>
		/// <param name="location">The location of the ground station. Cannot be null</param>
		public GroundStation(Coordinate location)
		{
			Location = location ?? throw new ArgumentNullException(nameof(location));
		}

		/// <summary>
		///     Creates a list of all of the predicted observations within the specified time period for this GroundStation.
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
			Angle minElevation = default, // default is Angle.Zero
			bool clipToStartTime = true, // default is true as it is assumed typical use case will be for future propagation, not searching into the past
			bool clipToEndTime = false, // default is false as it is assumed typical use case will be to capture entire future pass
			int resolution = 3)
		{
			// check input constraints
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

			DateTime aosTime;
			var t = start;

			do
			{
				// find the AOS Time of the next pass
				var aosCrossingPoint = FindNextBelowToAboveCrossingPoint(satellite, t, end, deltaTime, minElevation, resolution);
				if (!aosCrossingPoint.HasValue)
					// we're done if no crossing point was found
					break;

				aosTime = aosCrossingPoint.Value;
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

				if (clipToStartTime) // ensure before is clipped for max elevation search 
				{
					before = start > before ? start : before;
				}

				var after = maxElTime + deltaTime;
				if (clipToEndTime) // ensure after is clipped for max elevation search
				{
					after = end < after ? end : after;
				}

				// add the visibility period for the pass
				var refinedMaxElResult = FindMaxElevation(satellite, before, maxElTime, after, resolution);
				var maxEl = refinedMaxElResult.Item1;
				maxElTime = refinedMaxElResult.Item2;
				obs.Add(new SatelliteVisibilityPeriod(satellite, aosTime, losTime, maxEl, maxElTime, Location));

				t = losTime + deltaTime;
			} while (t <= end);

			// if clipToStartTime is false and the start time has been clipped, walk back in time until previous AOS crossing point has been found
			if (!clipToStartTime && obs.Count > 0 && obs[0].Start <= start)
			{
				var first = obs[0];
				var tu = FindNextAboveToBelowCrossingPoint(satellite, first.Start, deltaTime.Negate(), minElevation, resolution);
				var maxElTime = first.MaxElevation > tu.MaxElevation ? first.MaxElevationTime : tu.MaxElevationTime;
				var tuple = FindMaxElevation(satellite, maxElTime - deltaTime, maxElTime, maxElTime + deltaTime, resolution);

				var maxEl = tuple.Item1;
				var nextMaxElTime = tuple.Item2;
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
		/// <returns>A list of observations where an AOS is seen at or after the start parameter</returns>
		public TopocentricObservation Observe(Satellite satellite, DateTime time)
		{
			time = time.ToStrictUtc();

			var posEci = satellite.Predict(time);
			return Location.Observe(posEci, time);
		}

		/// <summary>
		///     Tests whether or not a satellite is above a specified elevation
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

			var aer = Location.Observe(pos, time);
			return aer.Elevation >= minElevation;
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


		// convenience function to get a topocentric observation for a given satellite and time
		private TopocentricObservation GetTopo(Satellite satellite, DateTime time)
		{
			var posEci = satellite.Predict(time);
			return Location.ToEci(time).Observe(posEci, posEci.Time);
		}

		// finds the next crossing point in time when the observer's elevation changes from below minElevation to above.
		// if the observer's elevation at the start time is above or equal to minElevation, start is returned.
		private DateTime? FindNextBelowToAboveCrossingPoint(Satellite satellite, DateTime start, DateTime end, TimeSpan deltaTime, Angle minElevation, int resolution)
		{
			var eciLocation = Location.ToEci(start);
			var posEci = satellite.Predict(start);

			var t = start - deltaTime;
			DateTime prev;
			Angle el;

			do
			{
				prev = t;
				var next = t + deltaTime;
				t = next <= end ? next : end; // clamp t to end
				el = GetTopo(satellite, t).Elevation;
			} while (el < minElevation && t < end);

			if (prev == start)
			{
				return t;
			}

			if (el < minElevation)
			{
				return null;
			} // if we haven't found a crossing point

			// sort out tStart and tEnd
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

		// a POD structure that contains time of crossing point, max elevation, and time of max elevation
		private struct CrossingPointInfo
		{
			public CrossingPointInfo(DateTime crossingPointTime, DateTime maxElevationTime, Angle maxElevation)
			{
				CrossingPointTime = crossingPointTime;
				MaxElevationTime = maxElevationTime;
				MaxElevation = maxElevation;
			}

			public DateTime CrossingPointTime { get; }
			public DateTime MaxElevationTime { get; }
			public Angle MaxElevation { get; }
		}

		// finds the next crossing point in time when the observer's elevation changes from above minElevation to below.
		// if the observer's elevation at time start is below minElevation, the start time is returned.
		// note that deltaTime may be negative, i.e. this function can walk backwards in time as well as forwards.
		private CrossingPointInfo FindNextAboveToBelowCrossingPoint(Satellite satellite, DateTime start, TimeSpan deltaTime, Angle minElevation, int resolution, DateTime? end = null)
		{
			var eciLocation = Location.ToEci(start);
			var posEci = satellite.Predict(start);

			var t = start - deltaTime;
			DateTime prev;
			var maxEl = Angle.Zero;
			var maxElTime = DateTime.MinValue;
			Angle el;

			// we write two loops to make the check condition a little easier to read (and slightly more efficient)
			if (end.HasValue) // if an definite end time is specified
			{
				do
				{
					prev = t;
					t += deltaTime;
					el = GetTopo(satellite, t).Elevation;
					if (el > maxEl)
					{
						maxEl = el;
						maxElTime = t;
					}
				} while (el >= minElevation && t <= end);
			}
			else // if no definite end time is specified
			{
				do
				{
					prev = t;
					t += deltaTime;
					el = GetTopo(satellite, t).Elevation;
					if (el > maxEl)
					{
						maxEl = el;
						maxElTime = t;
					}
				} while (el >= minElevation);
			}

			if (t == start)
			{
				return new CrossingPointInfo(t, maxElTime, maxEl);
			} // bail out early if t==start

			DateTime tStart, tEnd;
			// sort out tStart and tEnd
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

		// given a interval of time [start, end] with an crossing point within, determine the crossing point time 
		// it is assumed that the crossing point exists and is singular.
		private DateTime FindCrossingTimeWithinInterval(Satellite satellite, DateTime start, DateTime end, Angle minElevation, int resolution)
		{
			if (start == end)
			{
				throw new ArgumentException("start and end cannot be equal", "start");
			}

			var startEl = GetTopo(satellite, start).Elevation;
			var endEl = GetTopo(satellite, end).Elevation;
			var isAscending = startEl < endEl;

			var tBelow = start;
			var tAbove = end;
			if (!isAscending)
			{
				tBelow = end;
				tAbove = start;
			}

			var minTicks = (long)(1e7 / Math.Pow(10, resolution)); // convert resolution (num decimals) to minimum ticks

			long dt;
			DateTime t;

			// continually halve the interval until the size of the interval is less than minTicks
			do
			{
				dt = (tAbove - tBelow).Ticks / 2;
				t = tBelow.AddTicks(dt);
				var el = GetTopo(satellite, t).Elevation;
				if (el < minElevation)
				{
					tBelow = t;
				}
				else
				{
					tAbove = t;
				}
			} while (Math.Abs(dt) > minTicks);

			return t.Round(TimeSpan.FromTicks(minTicks)); // remove the trailing decimals
		}

		// finds the max elevation and time for max elevation, to a given temporal resolution
		private Tuple<Angle, DateTime> FindMaxElevation(Satellite satellite, DateTime before, DateTime peakTime, DateTime after, int resolution)
		{
			var minTicks = (long)(1e7 / Math.Pow(10, resolution)); // convert resolution (num decimals) to minimum ticks

			do
			{
				var elBefore = GetTopo(satellite, before).Elevation;
				var elAfter = GetTopo(satellite, after).Elevation;
				var elPeakTime = GetTopo(satellite, peakTime).Elevation;

				var t1 = before + TimeSpan.FromTicks((peakTime - before).Ticks / 2);
				var t2 = peakTime + TimeSpan.FromTicks((after - peakTime).Ticks / 2);

				var elT1 = GetTopo(satellite, t1).Elevation;
				var elT2 = GetTopo(satellite, t2).Elevation;

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

			return Tuple.Create(GetTopo(satellite, peakTime).Elevation, peakTime.Round(TimeSpan.FromTicks(minTicks))); // remove the trailing decimals);
		}
	}
}