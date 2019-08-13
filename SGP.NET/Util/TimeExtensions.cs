using System;

namespace SGPdotNET.Util
{
    /// <summary>
    ///     Adds extension methods to the <see cref="System.DateTime" /> class that are useful for astronomical calculations
    /// </summary>
    public static class TimeExtensions
    {
        /// <summary>
        ///     Converts a DateTime to a Julian date
        /// </summary>
        /// <param name="dt">The time to convert</param>
        /// <returns>The Julian representation the DateTime</returns>
        public static double ToJulian(this DateTime dt)
        {
            var ts = new TimeSpan(dt.Ticks);
            return ts.TotalDays + 1721425.5;
        }

        /// <summary>
        ///     Converts a DateTime to Greenwich Sidereal Time
        /// </summary>
        /// <param name="dt">The time to convert</param>
        /// <returns>The Greenwich Sidereal Time representation the DateTime</returns>
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
            return MathUtil.WrapTwoPi(MathUtil.DegreesToRadians(theta / 240.0));
        }

        /// <summary>
        ///     Converts a DateTime to Local Mean Sidereal Time
        /// </summary>
        /// <param name="dt">The time to convert</param>
        /// <param name="longitude">The longitude of observation</param>
        /// <returns>The Local Mean Sidereal Time representation the DateTime</returns>
        public static double ToLocalMeanSiderealTime(this DateTime dt, Angle longitude)
        {
            return MathUtil.WrapTwoPi(dt.ToGreenwichSiderealTime() + longitude.Radians);
        }

        /// <summary>
        ///     Safely converts the time to UTC only if the Kind of the input is known
        /// </summary>
        /// <param name="time">The time to convert</param>
        /// <returns>The UTC representation the DateTime</returns>
        /// <exception cref="ArgumentException">Thrown when the Kind property of the DateTime is Unspecified</exception>
        internal static DateTime ToStrictUtc(this DateTime time)
        {
            if (time.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException(
                    $"{nameof(time)}: Kind is unspecified and cannot be converted safely to UTC.");
            return time.ToUniversalTime();
        }

        /// <summary>
        ///     Rounds a DateTime to the nearest TimeSpan unit
        /// </summary>
        /// <param name="date">The time to round</param>
        /// <param name="span">The unit to round towards</param>
        /// <returns>The rounded DateTime</returns>
        internal static DateTime Round(this DateTime date, TimeSpan span)
        {
            var ticks = (date.Ticks + span.Ticks / 2 + 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks, date.Kind);
        }
    }
}