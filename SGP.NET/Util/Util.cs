using System;
using SGPdotNET.Propogation;

namespace SGPdotNET.Util
{
    internal static class Util
    {
        /*
         * always positive result
         * Mod(-3,4)= 1   fmod(-3,4)= -3
         */

        public static double Mod(double x, double y)
        {
            if (y == 0)
                return x;

            return x - y * Math.Floor(x / y);
        }

        public static double WrapNegPosPi(double a)
        {
            return Mod(a + Math.PI, SgpConstants.TwoPi) - Math.PI;
        }

        public static double WrapTwoPi(double a)
        {
            return Mod(a, SgpConstants.TwoPi);
        }

        public static double WrapNegPos180(double a)
        {
            return Mod(a + 180.0, 360.0) - 180.0;
        }

        public static double Wrap360(double a)
        {
            return Mod(a, 360.0);
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        public static double AcTan(double sinx, double cosx)
        {
            if (cosx == 0.0)
            {
                if (sinx > 0.0)
                    return Math.PI / 2.0;

                return 3.0 * Math.PI / 2.0;
            }

            if (cosx > 0.0)
                return Math.Atan(sinx / cosx);

            return Math.PI + Math.Atan(sinx / cosx);
        }

        public static DateTime Round(this DateTime date, TimeSpan span)
        {
            var ticks = (date.Ticks + span.Ticks / 2 + 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
    }
}