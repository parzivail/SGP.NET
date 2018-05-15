using System;

namespace SGP4_Sharp
{
    public static class TimeExtensions
    {
        /// <summary>
        ///     Converts a DateTime to a Julian date
        /// </summary>
        /// <param name="dt">the time to convert</param>
        /// <returns>a Julian date</returns>
        public static double ToJulian(this DateTime dt)
        {
            var ts = new TimeSpan(dt.Ticks);
            return ts.TotalDays + 1721425.5;
        }

        /// <summary>
        ///     Converts a DateTime to Greenwich Sidereal Time
        /// </summary>
        /// <param name="dt">the time to convert</param>
        /// <returns>the Greenwich Sidereal Time</returns>
        public static double ToGreenwichSiderealTime(this DateTime dt)
        {
            // t = Julian centuries from 2000 Jan. 1 12h UT1
            var t = (dt.ToJulian() - 2451545.0) / 36525.0;

            // Rotation angle in arcseconds
            var theta = 67310.54841
                        + (876600.0 * 3600.0 + 8640184.812866) * t
                        + 0.093104 * t * t
                        - 0.0000062 * t * t * t;

            // 360.0 / 86400.0 = 1.0 / 240.0
            return Util.WrapTwoPi(Util.DegreesToRadians(theta / 240.0));
        }

        /// <summary>
        ///     Converts a DateTime to Local Mean Sidereal Time
        /// </summary>
        /// <param name="dt">the time to convert</param>
        /// <param name="longitude">the longitude of observation</param>
        /// <returns>the Local Mean Sidereal Time</returns>
        public static double ToLocalMeanSiderealTime(this DateTime dt, double longitude)
        {
            return Util.WrapTwoPi(dt.ToGreenwichSiderealTime() + longitude);
        }
    }
}