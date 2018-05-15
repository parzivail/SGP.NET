using System;

namespace SGP4_Sharp
{
    public class Global
    {
        public const double KAe = 1.0;
        public const double Kq0 = 120.0;
        public const double Ks0 = 78.0;
        public const double KMu = 398600.8;
        public const double KXj2 = 1.082616e-3;
        public const double KXj3 = -2.53881e-6;
        public const double KXj4 = -1.65597e-6;

        public static double KXke = 60.0 / Math.Sqrt(EarthRadiusKm * EarthRadiusKm * EarthRadiusKm / KMu);
        public const double KCk2 = 0.5 * KXj2 * KAe * KAe;
        public const double KCk4 = -0.375 * KXj4 * KAe * KAe * KAe * KAe;

        public static double KQoms2T = Math.Pow((Kq0 - Ks0) / EarthRadiusKm, 4.0);

        public const double Ks = KAe * (1.0 + Ks0 / EarthRadiusKm);
        public const double KPi = 3.14159265358979323846264338327950288419716939937510582;
        public const double KTwopi = 2.0 * KPi;
        public const double KTwothird = 2.0 / 3.0;
        public const double KThdt = 4.37526908801129966e-3;

        public const double KAu = 1.49597870691e8;

        public const double EarthRadiusKm = 6378.135;
        public const double EarthFlatteningConstant = 1.0 / 298.26;
        public const double EarthRotationPerSiderealDay = 1.00273790934;

        public const double KSecondsPerDay = 86400.0;
        public const double KMinutesPerDay = 1440.0;
        public const double KHoursPerDay = 24.0;

        /// <summary>
        /// Jan 1.0 1900 = Jan 1 1900 00h UTC
        /// </summary>
        public const double KEpochJan100H1900 = 2415019.5;

        /// <summary>
        /// Jan 1.5 1900 = Jan 1 1900 12h UTC
        /// </summary>
        public const double KEpochJan112H1900 = 2415020.0;

        /// <summary>
        /// Jan 1.5 2000 = Jan 1 2000 12h UTC
        /// </summary>
        public const double KEpochJan112H2000 = 2451545.0;
    }
}

