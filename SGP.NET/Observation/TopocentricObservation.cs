using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Propagation;
using SGPdotNET.Util;

namespace SGPdotNET.Observation
{
	/// <summary>
	///     Stores a topocentric location (azimuth, elevation, range and range rate).
	/// </summary>
	/// <param name="Azimuth">Azimuth relative to the observer</param>
	/// <param name="Elevation">Elevation relative to the observer</param>
	/// <param name="Range">Range relative to the observer, in kilometers</param>
	/// <param name="RangeRate">Range rate relative to the observer, in kilometers/second</param>
	/// <param name="ReferencePosition">The position from which the satellite was observed to generate this observation</param>
	public record TopocentricObservation(Angle Azimuth, Angle Elevation, double Range, double RangeRate, Coordinate ReferencePosition = null)
	{
		/// <summary>
		///     Direction relative to the observer
		/// </summary>
		public RelativeDirection Direction => GetRelativeDirection();

		/// <summary>
		///     Time for an ideal radio signal to travel the distance between the observer and the satellite, in seconds
		/// </summary>
		public double SignalDelay => GetSignalDelay();

		/// <summary>
		///     Creates a new topocentric coordinate at the origin
		/// </summary>
		public TopocentricObservation() : this(Angle.Zero, Angle.Zero, 0, 0)
		{
		}

		/// <summary>
		///     Creates a new topocentric coordinate as a copy of the specified one
		/// </summary>
		/// <param name="topo">Object to copy from</param>
		public TopocentricObservation(TopocentricObservation topo)
		{
			Azimuth = topo.Azimuth;
			Elevation = topo.Elevation;
			Range = topo.Range;
			RangeRate = topo.RangeRate;
			ReferencePosition = topo.ReferencePosition;
		}

		private double GetSignalDelay()
		{
			return SgpConstants.SpeedOfLight / (Range * SgpConstants.MetersPerKilometer);
		}

		private RelativeDirection GetRelativeDirection()
		{
			if (Math.Abs(RangeRate) < double.Epsilon) return RelativeDirection.Fixed;
			return RangeRate < 0 ? RelativeDirection.Approaching : RelativeDirection.Receding;
		}

		/// <summary>
		///     Predicts the doppler shift of the satellite relative to the observer, in Hz
		/// </summary>
		/// <param name="inputFrequency">The base RX/TX frequency, in Hz</param>
		/// <returns>The doppler shift of the satellite</returns>
		public double GetDopplerShift(double inputFrequency)
		{
			var rr = RangeRate * SgpConstants.MetersPerKilometer;
			return -rr / SgpConstants.SpeedOfLight * inputFrequency;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return
				$"TopocentricObservation[Azimuth={Azimuth}, Elevation={Elevation}, Range={Range}km, RangeRate={RangeRate}km/s]";
		}
	}
}