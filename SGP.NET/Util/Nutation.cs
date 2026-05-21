using System;

namespace SGPdotNET.Util;

/// <summary>
///     Computes nutation in longitude and obliquity using the IAU 1980 theory,
///     as presented in Meeus, "Astronomical Algorithms" 2nd Ed., Chapter 22.
/// </summary>
/// <remarks>
///     <para>
///         Nutation is the short-period oscillation of the Earth's axis caused by
///         gravitational perturbations from the Moon and Sun. It affects the apparent
///         position of celestial bodies by up to ~18 arcseconds.
///     </para>
///     <para>
///         The input Julian Date should be in TT (Terrestrial Time) for correct results.
///     </para>
/// </remarks>
public static class Nutation
{
    private readonly struct NutationTerm(int d, int m, int mp, int f, int omega, int sineCoef, int cosineCoef, double sineRate, double cosineRate)
    {
        public readonly int D = d;
        public readonly int M = m;
        public readonly int MP = mp;
        public readonly int F = f;
        public readonly int Omega = omega;
        public readonly int SineCoef = sineCoef;
        public readonly int CosineCoef = cosineCoef;
        public readonly double SineRate = sineRate;
        public readonly double CosineRate = cosineRate;
    }

    /// <summary>
    ///     Nutation terms from Meeus Ch. 22, Table 22.A (p. 145-146).
    ///     Sine coefficients are in 0.0001" for DeltaPsi, cosine coefficients in 0.0001" for DeltaEpsilon.
    ///     Rate terms are in 0.0001"/century.
    /// </summary>
    private static readonly NutationTerm[] Terms =
    [
        new( 0,  0,  0,  0,  1, -171996,  92025, -174.2,   8.9),
        new(-2,  0,  0,  2,  2,  -13187,   5736,   -1.6,  -3.1),
        new( 0,  0,  0,  2,  2,   -2274,    977,   -0.2,  -0.5),
        new( 0,  0,  0,  0,  2,    2062,   -895,    0.2,   0.5),
        new( 0,  1,  0,  0,  0,    1426,     54,   -3.4,  -0.1),
        new( 0,  0,  1,  0,  0,     712,     -7,    0.1,   0.0),
        new(-2,  1,  0,  2,  2,    -517,    224,    1.2,  -0.6),
        new( 0,  0,  0,  2,  1,    -386,    200,   -0.4,   0.0),
        new( 0,  0,  1,  2,  2,    -301,    129,    0.0,  -0.1),
        new(-2, -1,  0,  2,  2,     217,    -95,   -0.5,   0.3),
        new(-2,  0,  1,  0,  0,    -158,      0,    0.0,   0.0),
        new(-2,  0,  0,  2,  1,     129,    -70,    0.1,   0.0),
        new( 0,  0, -1,  2,  2,     123,    -53,    0.0,   0.0),
        new( 2,  0,  0,  0,  0,      63,      0,    0.0,   0.0),
        new( 0,  0,  1,  0,  1,      63,    -33,    0.1,   0.0),
        new( 2,  0, -1,  2,  2,     -59,     26,    0.0,   0.0),
        new( 0,  0, -1,  0,  1,     -58,     32,   -0.1,   0.0),
        new( 0,  0,  1,  2,  1,     -51,     27,    0.0,   0.0),
        new(-2,  0,  2,  0,  0,      48,      0,    0.0,   0.0),
        new( 0,  0, -2,  2,  1,      46,    -24,    0.0,   0.0),
        new( 2,  0,  0,  2,  2,     -38,     16,    0.0,   0.0),
        new( 0,  0,  2,  2,  2,     -31,     13,    0.0,   0.0),
        new( 0,  0,  2,  0,  0,      29,      0,    0.0,   0.0),
        new(-2,  0,  1,  2,  2,      29,    -12,    0.0,   0.0),
        new( 0,  0,  0,  2,  0,      26,      0,    0.0,   0.0),
        new(-2,  0,  0,  2,  0,     -22,      0,    0.0,   0.0),
        new( 0,  0, -1,  2,  1,      21,    -10,    0.0,   0.0),
        new( 0,  2,  0,  0,  0,      17,      0,   -0.1,   0.0),
        new( 2,  0, -1,  0,  1,      16,     -8,    0.0,   0.0),
        new(-2,  2,  0,  2,  2,     -16,      7,    0.1,   0.0),
        new( 0,  1,  0,  0,  1,     -15,      9,    0.0,   0.0),
        new(-2,  0,  1,  0,  1,     -13,      7,    0.0,   0.0),
        new( 0, -1,  0,  0,  1,     -12,      6,    0.0,   0.0),
        new( 0,  0,  2, -2,  0,      11,      0,    0.0,   0.0),
        new( 2,  0, -1,  2,  1,     -10,      5,    0.0,   0.0),
        new( 2,  0,  1,  2,  2,      -8,      3,    0.0,   0.0),
        new( 0,  1,  0,  2,  2,       7,     -3,    0.0,   0.0),
        new(-2,  1,  1,  0,  0,      -7,      0,    0.0,   0.0),
        new( 0, -1,  0,  2,  2,      -7,      3,    0.0,   0.0),
        new( 2,  0,  0,  2,  1,      -7,      3,    0.0,   0.0),
        new( 2,  0,  1,  0,  0,       6,      0,    0.0,   0.0),
        new(-2,  0,  2,  2,  2,       6,     -3,    0.0,   0.0),
        new(-2,  0,  1,  2,  1,       6,     -3,    0.0,   0.0),
        new( 2,  0, -2,  0,  1,      -6,      3,    0.0,   0.0),
        new( 2,  0,  0,  0,  1,      -6,      3,    0.0,   0.0),
        new( 0, -1,  1,  0,  0,       5,      0,    0.0,   0.0),
        new(-2, -1,  0,  2,  1,      -5,      3,    0.0,   0.0),
        new(-2,  0,  0,  0,  1,      -5,      3,    0.0,   0.0),
        new( 0,  0,  2,  2,  1,      -5,      3,    0.0,   0.0),
        new(-2,  0,  2,  0,  1,       4,      0,    0.0,   0.0),
        new(-2,  1,  0,  2,  1,       4,      0,    0.0,   0.0),
        new( 0,  0,  1, -2,  0,       4,      0,    0.0,   0.0),
        new(-1,  0,  1,  0,  0,      -4,      0,    0.0,   0.0),
        new(-2,  1,  0,  0,  0,      -4,      0,    0.0,   0.0),
        new( 1,  0,  0,  0,  0,      -4,      0,    0.0,   0.0),
        new( 0,  0,  1,  2,  0,       3,      0,    0.0,   0.0),
        new( 0,  0, -2,  2,  2,      -3,      0,    0.0,   0.0),
        new(-1, -1,  1,  0,  0,      -3,      0,    0.0,   0.0),
        new( 0,  1,  1,  0,  0,      -3,      0,    0.0,   0.0),
        new( 0, -1,  1,  2,  2,      -3,      0,    0.0,   0.0),
        new( 2, -1, -1,  2,  2,      -3,      0,    0.0,   0.0),
        new( 0,  0,  3,  2,  2,      -3,      0,    0.0,   0.0),
        new( 2, -1,  0,  2,  2,      -3,      0,    0.0,   0.0),
    ];

    /// <summary>
    ///     Computes the nutation in longitude (DeltaPsi) in degrees.
    /// </summary>
    /// <param name="jdeTt">The Julian Date in Terrestrial Time (TT).</param>
    /// <returns>The nutation in longitude in degrees (typically a few arcseconds).</returns>
    public static double LongitudeDeg(double jdeTt)
    {
        var t = (jdeTt - 2451545.0) / 36525.0;

        // Mean arguments in degrees
        var d = MathUtil.Wrap360(297.85036 + t * (445267.111480 + t * (-0.0019142 + t / 189474.0)));
        var m = MathUtil.Wrap360(357.52772 + t * (35999.050340 + t * (-0.0001603 - t / 300000.0)));
        var mp = MathUtil.Wrap360(134.96298 + t * (477198.867398 + t * (0.0086972 + t / 56250.0)));
        var f = MathUtil.Wrap360(93.27191 + t * (483202.017538 + t * (-0.0036825 + t / 327270.0)));
        var omega = MathUtil.Wrap360(125.04452 + t * (-1934.136261 + t * (0.0020708 + t / 450000.0)));

        var dRad = MathUtil.DegreesToRadians(d);
        var mRad = MathUtil.DegreesToRadians(m);
        var mpRad = MathUtil.DegreesToRadians(mp);
        var fRad = MathUtil.DegreesToRadians(f);
        var omegaRad = MathUtil.DegreesToRadians(omega);

        var args = new double[5] { dRad, mRad, mpRad, fRad, omegaRad };

        var deltaPsiArcsec = 0.0;
        for (var i = 0; i < Terms.Length; i++)
        {
            ref var term = ref Terms[i];
            var arg = term.D * args[0] + term.M * args[1] + term.MP * args[2] + term.F * args[3] + term.Omega * args[4];
            var coeff = term.SineCoef + term.SineRate * t;
            deltaPsiArcsec += coeff * Math.Sin(arg);
        }

        // Convert from 0.0001" to degrees
        return deltaPsiArcsec / 36000000.0;
    }

    /// <summary>
    ///     Computes the nutation in obliquity (DeltaEpsilon) in degrees.
    /// </summary>
    /// <param name="jdeTt">The Julian Date in Terrestrial Time (TT).</param>
    /// <returns>The nutation in obliquity in degrees (typically a few arcseconds).</returns>
    public static double ObliquityDeg(double jdeTt)
    {
        var t = (jdeTt - 2451545.0) / 36525.0;

        // Mean arguments in degrees
        var d = MathUtil.Wrap360(297.85036 + t * (445267.111480 + t * (-0.0019142 + t / 189474.0)));
        var m = MathUtil.Wrap360(357.52772 + t * (35999.050340 + t * (-0.0001603 - t / 300000.0)));
        var mp = MathUtil.Wrap360(134.96298 + t * (477198.867398 + t * (0.0086972 + t / 56250.0)));
        var f = MathUtil.Wrap360(93.27191 + t * (483202.017538 + t * (-0.0036825 + t / 327270.0)));
        var omega = MathUtil.Wrap360(125.04452 + t * (-1934.136261 + t * (0.0020708 + t / 450000.0)));

        var dRad = MathUtil.DegreesToRadians(d);
        var mRad = MathUtil.DegreesToRadians(m);
        var mpRad = MathUtil.DegreesToRadians(mp);
        var fRad = MathUtil.DegreesToRadians(f);
        var omegaRad = MathUtil.DegreesToRadians(omega);

        var args = new double[5] { dRad, mRad, mpRad, fRad, omegaRad };

        var deltaEpsilonArcsec = 0.0;
        for (var i = 0; i < Terms.Length; i++)
        {
            ref var term = ref Terms[i];
            var arg = term.D * args[0] + term.M * args[1] + term.MP * args[2] + term.F * args[3] + term.Omega * args[4];
            var coeff = term.CosineCoef + term.CosineRate * t;
            deltaEpsilonArcsec += coeff * Math.Cos(arg);
        }

        // Convert from 0.0001" to degrees
        return deltaEpsilonArcsec / 36000000.0;
    }
}
