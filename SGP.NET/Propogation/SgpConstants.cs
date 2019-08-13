using System;

namespace SGPdotNET.Propogation
{
    /// <summary>
    ///     Stores various numerical constants used in propogation
    /// </summary>
    public class SgpConstants
    {
        /// <summary>
        ///     Twice the value of Pi
        /// </summary>
        public const double TwoPi = 2 * Math.PI;

        /// <summary>
        ///     Two divided by three (2/3)
        /// </summary>
        public const double TwoThirds = 2.0 / 3;

        /// <summary>
        ///     The number of seconds per day
        /// </summary>
        public const double SecondsPerDay = 86400;

        /// <summary>
        ///     The number of minutes per day
        /// </summary>
        public const double MinutesPerDay = 1440;

        /// <summary>
        ///     The number of hours per day
        /// </summary>
        public const double HoursPerDay = 24;

        /// <summary>
        ///     The number of minutes per degree
        /// </summary>
        public const double MinutesPerDegree = 60;

        /// <summary>
        ///     The number of seconds per minute
        /// </summary>
        public const double SecondsPerMinute = 60;

        /// <summary>
        ///     The speef of light, in meters/second
        /// </summary>
        public const double SpeedOfLight = 299792458;

        /// <summary>
        ///     CK2 propogation constant
        /// </summary>
        public const double Ck2 = 0.5 * ZonalHarmonicJ2 * DistanceUnitsPerEarthRadii * DistanceUnitsPerEarthRadii;

        /// <summary>
        ///     CK4 propogation constant
        /// </summary>
        public const double Ck4 =
            -0.375 * ZonalHarmonicJ4 * DistanceUnitsPerEarthRadii * DistanceUnitsPerEarthRadii *
            DistanceUnitsPerEarthRadii * DistanceUnitsPerEarthRadii;

        /// <summary>
        ///     Q-zero propogation constant
        /// </summary>
        public const double Q0 = 120;

        /// <summary>
        ///     S-zero propogation constant
        /// </summary>
        public const double S0 = 78;

        /// <summary>
        ///     S propogation constant
        /// </summary>
        public const double S = DistanceUnitsPerEarthRadii * (1 + S0 / EarthRadiusKm);

        /// <summary>
        ///     Also called Ae
        /// </summary>
        public const double DistanceUnitsPerEarthRadii = 1;

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
        public const double EarthFlatteningConstant = 1 / 298.26;

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
        public const double EpochJan112H1900 = 2415020;

        /// <summary>
        ///     Jan 1.5 2000 = Jan 1 2000 12h UTC
        /// </summary>
        public const double EpochJan112H2000 = 2451545;

        /// <summary>
        ///     The number of meters in a kilometer
        /// </summary>
        public const double MetersPerKilometer = 1000;

        /// <summary>
        ///     QOMS2T propogation constant
        /// </summary>
        public static readonly double Qoms2T = Math.Pow((Q0 - S0) / EarthRadiusKm, 4);

        /// <summary>
        ///     Also called XKE
        /// </summary>
        public static readonly double ReciprocalOfMinutesPerTimeUnit = 60 /
                                                                       Math.Sqrt(EarthRadiusKm * EarthRadiusKm *
                                                                                 EarthRadiusKm /
                                                                                 EarthGravitation);
    }
}