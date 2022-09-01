using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Observation
{
	/// <summary>
	///     Stores a period during which a satellite is visible to a ground station
	/// </summary>
	public class SatelliteVisibilityPeriod
	{
		public Satellite Satellite { get; }
		public DateTime Start { get; }
		public DateTime End { get; }
		public Angle MaxElevation { get; }
		public DateTime MaxElevationTime { get; }
		public Coordinate ReferencePosition { get; }

		/// <summary>
		///     Stores a period during which a satellite is visible to a ground station
		/// </summary>
		/// <param name="Satellite">The satellite that is being observed</param>
		/// <param name="Start">The start time of the observation</param>
		/// <param name="End">The end time of the observation</param>
		/// <param name="MaxElevation">The max elevation reached during observation</param>
		/// <param name="MaxElevationTime">The time at which max elevation is reached during observation</param>
		/// <param name="ReferencePosition">The position from which the satellite was observed to generate this observation</param>
		public SatelliteVisibilityPeriod(Satellite Satellite, DateTime Start, DateTime End, Angle MaxElevation, DateTime MaxElevationTime, Coordinate ReferencePosition = null)
		{
			this.Satellite = Satellite;
			this.Start = Start;
			this.End = End;
			this.MaxElevation = MaxElevation;
			this.MaxElevationTime = MaxElevationTime;
			this.ReferencePosition = ReferencePosition;
		}

		protected bool Equals(SatelliteVisibilityPeriod other)
		{
			return Equals(Satellite, other.Satellite) && Start.Equals(other.Start) && End.Equals(other.End) && MaxElevation.Equals(other.MaxElevation) &&
			       MaxElevationTime.Equals(other.MaxElevationTime) && Equals(ReferencePosition, other.ReferencePosition);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((SatelliteVisibilityPeriod)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Satellite != null ? Satellite.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Start.GetHashCode();
				hashCode = (hashCode * 397) ^ End.GetHashCode();
				hashCode = (hashCode * 397) ^ MaxElevation.GetHashCode();
				hashCode = (hashCode * 397) ^ MaxElevationTime.GetHashCode();
				hashCode = (hashCode * 397) ^ (ReferencePosition != null ? ReferencePosition.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(SatelliteVisibilityPeriod left, SatelliteVisibilityPeriod right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(SatelliteVisibilityPeriod left, SatelliteVisibilityPeriod right)
		{
			return !Equals(left, right);
		}
	}
}