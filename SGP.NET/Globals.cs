using System;

namespace SGPdotNET
{
    /// <summary>
    ///     Stores various numerical constants used in propogation
    /// </summary>
    public class SgpConstants
    {
        public const double TwoPi = 2.0 * Math.PI;
        public const double TwoThirds = 2.0 / 3.0;

        public const double SecondsPerDay = 86400.0;
        public const double MinutesPerDay = 1440.0;
        public const double HoursPerDay = 24.0;

        public const double Ck2 = 0.5 * ZonalHarmonicJ2 * DistanceUnitsPerEarthRadii * DistanceUnitsPerEarthRadii;

        public const double Ck4 =
            -0.375 * ZonalHarmonicJ4 * DistanceUnitsPerEarthRadii * DistanceUnitsPerEarthRadii *
            DistanceUnitsPerEarthRadii * DistanceUnitsPerEarthRadii;

        public const double Q0 = 120.0;
        public const double S0 = 78.0;
        public const double S = DistanceUnitsPerEarthRadii * (1.0 + S0 / EarthRadiusKm);

        /// <summary>
        ///     Also called Ae
        /// </summary>
        public const double DistanceUnitsPerEarthRadii = 1.0;

        /// <summary>
        ///     Also called mu
        /// </summary>
        public const double EarthGravitation = 398600.8;

        /// <summary>
        ///     Also called XJ2
        /// </summary>
        public const double ZonalHarmonicJ2 = 1.082616e-3;

        /// <summary>
        ///     Also called XJ3
        /// </summary>
        public const double ZonalHarmonicJ3 = -2.53881e-6;

        /// <summary>
        ///     Also called XJ4
        /// </summary>
        public const double ZonalHarmonicJ4 = -1.65597e-6;

        /// <summary>
        ///     Also called THDT or rptim
        /// </summary>
        public const double EarthRotationPerMinRad = 4.37526908801129966e-3;

        /// <summary>
        ///     Also called Au
        /// </summary>
        public const double KmPerAu = 1.49597870691e8;

        /// <summary>
        ///     Also called KmPer
        /// </summary>
        public const double EarthRadiusKm = 6378.135;

        /// <summary>
        ///     Also called kF
        /// </summary>
        public const double EarthFlatteningConstant = 1.0 / 298.26;

        /// <summary>
        ///     Also called OmegaE
        /// </summary>
        public const double EarthRotationPerSiderealDay = 1.00273790934;

        /// <summary>
        ///     Jan 1.0 1900 = Jan 1 1900 00h UTC
        /// </summary>
        public const double EpochJan100H1900 = 2415019.5;

        /// <summary>
        ///     Jan 1.5 1900 = Jan 1 1900 12h UTC
        /// </summary>
        public const double EpochJan112H1900 = 2415020.0;

        /// <summary>
        ///     Jan 1.5 2000 = Jan 1 2000 12h UTC
        /// </summary>
        public const double EpochJan112H2000 = 2451545.0;

        public static double Qoms2T = Math.Pow((Q0 - S0) / EarthRadiusKm, 4.0);

        /// <summary>
        ///     Also called XKE
        /// </summary>
        public static double ReciprocalOfMinutesPerTimeUnit = 60.0 /
                                                              Math.Sqrt(EarthRadiusKm * EarthRadiusKm * EarthRadiusKm /
                                                                        EarthGravitation);
    }
}