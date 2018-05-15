/*
 * Copyright 2013 Daniel Warner <contact@danrw.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;


namespace SGP4_Sharp
{
    public class Util
    {
        /*
         * always positive result
         * Mod(-3,4)= 1   fmod(-3,4)= -3
         */
        public static double Mod(double x, double y)
        {
            if (y == 0)
            {
                return x;
            }

            return x - y * Math.Floor(x / y);
        }

        public static double WrapNegPosPi(double a)
        {
            return Mod(a + Global.KPi, Global.KTwopi) - Global.KPi;
        }

        public static double WrapTwoPi(double a)
        {
            return Mod(a, Global.KTwopi);
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
            return degrees * Global.KPi / 180.0;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Global.KPi;
        }

        public static double AcTan(double sinx, double cosx)
        {
            if (cosx == 0.0)
            {
                if (sinx > 0.0)
                {
                    return Global.KPi / 2.0;
                }

                return 3.0 * Global.KPi / 2.0;
            }

            if (cosx > 0.0)
            {
                return Math.Atan(sinx / cosx);
            }

            return Global.KPi + Math.Atan(sinx / cosx);
        }
    }
}

