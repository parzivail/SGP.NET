using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Propagation
{
    public class CelestialBodies
    {
        /// <summary>
        ///     Predicts the sun's location at a specific time
        /// </summary>
        /// <param name="time">The time of observation</param>
        /// <returns>A geodetic coordinate set representing the body's position at the given time</returns>
        public static Coordinate PredictSunCelestial(DateTime time)
        {
            // See https://en.wikipedia.org/wiki/Position_of_the_Sun

            var n = time.ToJulian() - 2451545.0;
            var l = MathUtil.Wrap360(280.460 + 0.9856474 * n);
            var g = MathUtil.DegreesToRadians(MathUtil.Wrap360(357.528 + 0.9856003 * n));

            var lambda = MathUtil.DegreesToRadians(l + 1.915 * Math.Sin(g) + 0.02 * Math.Sin(2 * g));
            var epsilon = 23.4;

            var sinLambda = Math.Sin(lambda);

            var alpha = Math.Atan2(Math.Cos(epsilon) * sinLambda, Math.Cos(lambda));
            var delta = Math.Asin(Math.Sin(epsilon) * sinLambda);

            var distAu = 1.00014 - 0.01671 * Math.Cos(g) - 0.00014 * Math.Cos(2 * g);
            var distKm = SgpConstants.KmPerAu * distAu;

            // TODO: convert from celestial to ecliptic/geodetic
            return new GeodeticCoordinate(Angle.FromRadians(delta), Angle.FromRadians(alpha), distKm);
        }
    }
}
