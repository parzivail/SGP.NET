using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Util;

namespace SGPdotNET.Propagation.Bodies;

/// <summary>
///     Provides methods to calculate the position of the Moon.
/// </summary>
public static class Moon
{
	private readonly struct LongitudeTerm(int d, int m, int mp, int f, int sigmaL, int sigmaR)
	{
		public readonly int D = d;
		public readonly int M = m;
		public readonly int MP = mp;
		public readonly int F = f;
		public readonly int SigmaL = sigmaL;
		public readonly int SigmaR = sigmaR;
	}

	private readonly struct LatitudeTerm(int d, int m, int mp, int f, int sigmaB)
	{
		public readonly int D = d;
		public readonly int M = m;
		public readonly int MP = mp;
		public readonly int F = f;
		public readonly int SigmaB = sigmaB;
	}

	/// <summary>
	///     Perturbation terms for lunar longitude and distance.
	///     Source: Meeus, "Astronomical Algorithms" 2nd Ed., Chapter 47, Table 47.A (p. 340).
	///     Coefficients are in 0.000001° for longitude and 0.001 km for distance.
	/// </summary>
	private static readonly LongitudeTerm[] LongitudeTerms =
	[
		new(0, 0, 1, 0, 6288774, -20905355),
		new(2, 0, -1, 0, 1274027, -3699111),
		new(2, 0, 0, 0, 658314, -2955968),
		new(0, 0, 2, 0, 213618, -569925),
		new(0, 1, 0, 0, -185116, 48888),
		new(0, 0, 0, 2, -114332, -3149),
		new(2, 0, -2, 0, 58793, 246158),
		new(2, -1, -1, 0, 57066, -152138),
		new(2, 0, 1, 0, 53322, -170733),
		new(2, -1, 0, 0, 45758, -204586),
		new(0, 1, -1, 0, -40923, -129620),
		new(1, 0, 0, 0, -34720, 108743),
		new(0, 1, 1, 0, -30383, 104755),
		new(2, 0, 0, -2, 15327, 10321),
		new(0, 0, 1, 2, -12528, 0),
		new(0, 0, 1, -2, 10980, 79661),
		new(4, 0, -1, 0, 10675, -34782),
		new(0, 0, 3, 0, 10034, -23210),
		new(4, 0, -2, 0, 8548, -21636),
		new(2, 1, -1, 0, -7888, 24208),
		new(2, 1, 0, 0, -6766, 30824),
		new(1, 0, -1, 0, -5163, -8379),
		new(1, 1, 0, 0, 4987, -16675),
		new(2, -1, 1, 0, 4036, -12831),
		new(2, 0, 2, 0, 3994, -10445),
		new(4, 0, 0, 0, 3861, -11650),
		new(2, 0, -3, 0, 3665, 14403),
		new(0, 1, -2, 0, -2689, -7003),
		new(2, 0, -1, 2, -2602, 0),
		new(2, -1, -2, 0, 2390, 10056),
		new(1, 0, 1, 0, -2348, 6322),
		new(2, -2, 0, 0, 2236, -9884),
		new(0, 1, 2, 0, -2120, 5751),
		new(0, 2, 0, 0, -2069, 0),
		new(2, -2, -1, 0, 2048, -4950),
		new(2, 0, 1, -2, -1773, 4130),
		new(2, 0, 0, 2, -1595, 0),
		new(4, -1, -1, 0, 1215, -3958),
		new(0, 0, 2, 2, -1110, 0),
		new(3, 0, -1, 0, -892, 3258),
		new(2, 1, 1, 0, -810, 2616),
		new(4, -1, -2, 0, 759, -1897),
		new(0, 2, -1, 0, -713, -2117),
		new(2, 2, -1, 0, -700, 2354),
		new(2, 1, -2, 0, 691, 0),
		new(2, -1, 0, -2, 596, 0),
		new(4, 0, 1, 0, 549, -1423),
		new(0, 0, 4, 0, 537, -1117),
		new(4, -1, 0, 0, 520, -1571),
		new(1, 0, -2, 0, -487, -1739),
		new(2, 1, 0, -2, -399, 0),
		new(0, 0, 2, -2, -381, -4421),
		new(1, 1, 1, 0, 351, 0),
		new(3, 0, -2, 0, -340, 0),
		new(4, 0, -3, 0, 330, 0),
		new(2, -1, 2, 0, 327, 0),
		new(0, 2, 1, 0, -323, 1165),
		new(1, 1, -1, 0, 299, 0),
		new(2, 0, 3, 0, 294, 0),
		new(2, 0, -1, -2, 0, 8752),
	];

	/// <summary>
	///     Perturbation terms for lunar latitude.
	///     Source: Meeus, "Astronomical Algorithms" 2nd Ed., Chapter 47, Table 47.B (p. 341).
	///     Coefficients are in 0.000001°.
	/// </summary>
	private static readonly LatitudeTerm[] LatitudeTerms =
	[
		new(0, 0, 0, 1, 5128122),
		new(0, 0, 1, 1, 280602),
		new(0, 0, 1, -1, 277693),
		new(2, 0, 0, -1, 173237),
		new(2, 0, -1, 1, 55413),
		new(2, 0, -1, -1, 46271),
		new(2, 0, 0, 1, 32573),
		new(0, 0, 2, 1, 17198),
		new(2, 0, 1, -1, 9266),
		new(0, 0, 2, -1, 8822),
		new(2, -1, 0, -1, 8216),
		new(2, 0, -2, -1, 4324),
		new(2, 0, 1, 1, 4200),
		new(2, 1, 0, -1, -3359),
		new(2, -1, -1, 1, 2463),
		new(2, -1, 0, 1, 2211),
		new(2, -1, -1, -1, 2065),
		new(0, 1, -1, -1, -1870),
		new(4, 0, -1, -1, 1828),
		new(0, 1, 0, 1, -1794),
		new(0, 0, 0, 3, -1749),
		new(0, 1, -1, 1, -1565),
		new(1, 0, 0, 1, -1491),
		new(0, 1, 1, 1, -1475),
		new(0, 1, 1, -1, -1410),
		new(0, 1, 0, -1, -1344),
		new(1, 0, 0, -1, -1335),
		new(0, 0, 3, 1, 1107),
		new(4, 0, 0, -1, 1021),
		new(4, 0, -1, 1, 833),
		new(0, 0, 1, -3, 777),
		new(4, 0, -2, 1, 671),
		new(2, 0, 0, -3, 607),
		new(2, 0, 2, -1, 596),
		new(2, -1, 1, -1, 491),
		new(2, 0, -2, 1, -451),
		new(0, 0, 3, -1, 439),
		new(2, 0, 2, 1, 422),
		new(2, 0, -3, -1, 421),
		new(2, 1, -1, 1, -366),
		new(2, 1, 0, 1, -351),
		new(4, 0, 0, 1, 331),
		new(2, -1, 1, 1, 315),
		new(2, -2, 0, -1, 302),
		new(0, 0, 1, 3, -283),
		new(2, 1, 1, -1, -229),
		new(1, 1, 0, -1, 223),
		new(1, 1, 0, 1, 223),
		new(0, 1, -2, -1, -220),
		new(2, 1, -1, -1, -220),
		new(1, 0, 1, 1, -185),
		new(2, -1, -2, -1, 181),
		new(0, 1, 2, 1, -177),
		new(4, 0, -2, -1, 176),
		new(4, -1, -1, -1, 166),
		new(1, 0, 1, -1, -164),
		new(4, 0, 1, -1, 132),
		new(1, 0, -1, -1, -119),
		new(4, -1, 0, -1, 115),
		new(2, -2, 0, 1, 107),
	];
	
    /// <summary>
    ///     Calculates the Moon's position in Earth-Centered Inertial (ECI) coordinates
    ///     using the truncated ELP 2000/82 lunar theory.
    /// </summary>
    /// <param name="time">The time of observation (UTC).</param>
    /// <returns>
    ///     An EciCoordinate representing the Moon's position. The Position vector is in kilometers
    ///     relative to Earth's center. Velocity is zero (not computed by this algorithm).
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This implementation follows Jean Meeus, "Astronomical Algorithms", 2nd Edition,
    ///         Chapter 47 "Position of the Moon" (pp. 337–342). The algorithm is a truncated
    ///         version of the ELP 2000/82 lunar theory by Chapront-Touzé and Chapront.
    ///     </para>
    ///     <para>
    ///         Accuracy is approximately 10 arcseconds in latitude and 4 arcseconds in longitude
    ///         for dates between 1900 and 2100. This does not account for nutation or the
    ///         conversion from Terrestrial Time (TT) to Universal Time (UT1), which introduces
    ///         an additional error of ~30 arcseconds when using UTC directly.
    ///     </para>
    ///     <para>
    ///         The returned ECI coordinate can be converted to geodetic coordinates via
    ///         <c>eci.ToGeodetic()</c> to obtain the sublunar point, or used with
    ///         <c>GroundStation.Observe()</c> for moonrise/moonset and lunar track calculations.
    ///     </para>
    /// </remarks>
    public static EciCoordinate Predict(DateTime time)
    {
        var jde = time.ToJulian();
        var t = (jde - 2451545.0) / 36525.0;

        var degToRad = Math.PI / 180.0;

        var lPrimeDeg = Horner(t, 218.3164477, 481267.88123421, -0.0015786, 1.0 / 538841.0, -1.0 / 65194000.0);
        var dDeg = Horner(t, 297.8501921, 445267.1114034, -0.0018819, 1.0 / 545868.0, -1.0 / 113065000.0);
        var mDeg = Horner(t, 357.5291092, 35999.0502909, -0.0001535, 1.0 / 24490000.0);
        var mPrimeDeg = Horner(t, 134.9633964, 477198.8675055, 0.0087414, 1.0 / 69699.0, -1.0 / 14712000.0);
        var fDeg = Horner(t, 93.2720950, 483202.0175233, -0.0036539, -1.0 / 3526000.0, 1.0 / 863310000.0);

        var lPrime = MathUtil.Wrap360(lPrimeDeg) * degToRad;
        var d = MathUtil.Wrap360(dDeg) * degToRad;
        var m = MathUtil.Wrap360(mDeg) * degToRad;
        var mPrime = MathUtil.Wrap360(mPrimeDeg) * degToRad;
        var f = MathUtil.Wrap360(fDeg) * degToRad;

        var a1 = (119.75 + 131.849 * t) * degToRad;
        var a2 = (53.09 + 479264.29 * t) * degToRad;
        var a3 = (313.45 + 481266.484 * t) * degToRad;

        var e = 1.0 - 0.002516 * t - 0.0000074 * t * t;
        var e2 = e * e;

        var sigmaL = 3958.0 * Math.Sin(a1)
                     + 1962.0 * Math.Sin(lPrime - f)
                     + 318.0 * Math.Sin(a2);
        var sigmaR = 0.0;
        var sigmaB = -2235.0 * Math.Sin(lPrime)
                     + 382.0 * Math.Sin(a3)
                     + 175.0 * Math.Sin(a1 - f)
                     + 175.0 * Math.Sin(a1 + f)
                     + 127.0 * Math.Sin(lPrime - mPrime)
                     - 115.0 * Math.Sin(lPrime + mPrime);

        for (var i = 0; i < LongitudeTerms.Length; i++)
        {
            ref var term = ref LongitudeTerms[i];
            var arg = d * term.D + m * term.M + mPrime * term.MP + f * term.F;
            var sa = Math.Sin(arg);
            var ca = Math.Cos(arg);

            var factor = term.M switch
            {
                0 => 1.0,
                1 or -1 => e,
                2 or -2 => e2,
                _ => 1.0
            };

            sigmaL += term.SigmaL * sa * factor;
            sigmaR += term.SigmaR * ca * factor;
        }

        for (var i = 0; i < LatitudeTerms.Length; i++)
        {
            ref var term = ref LatitudeTerms[i];
            var arg = d * term.D + m * term.M + mPrime * term.MP + f * term.F;
            var sb = Math.Sin(arg);

            var factor = term.M switch
            {
                0 => 1.0,
                1 or -1 => e,
                2 or -2 => e2,
                _ => 1.0
            };

            sigmaB += term.SigmaB * sb * factor;
        }

        var lambdaRad = WrapPi(lPrime + sigmaL * 1e-6 * degToRad);
        var betaRad = sigmaB * 1e-6 * degToRad;
        var distanceKm = 385000.56 + sigmaR * 1e-3;

        var cosLambda = Math.Cos(lambdaRad);
        var sinLambda = Math.Sin(lambdaRad);
        var cosBeta = Math.Cos(betaRad);
        var sinBeta = Math.Sin(betaRad);

        var epsilonDeg = 23.439 - 0.0000004 * (jde - 2451545.0);
        var epsilon = epsilonDeg * degToRad;
        var cosEpsilon = Math.Cos(epsilon);
        var sinEpsilon = Math.Sin(epsilon);

        var xKm = distanceKm * cosBeta * cosLambda;
        var yKm = distanceKm * (cosEpsilon * cosBeta * sinLambda - sinEpsilon * sinBeta);
        var zKm = distanceKm * (sinEpsilon * cosBeta * sinLambda + cosEpsilon * sinBeta);

        return new EciCoordinate(time, new Vector3(xKm, yKm, zKm));
    }

    private static double Horner(double t, params double[] coeffs)
	{
		var result = 0.0;
		for (var i = coeffs.Length - 1; i >= 0; i--)
			result = result * t + coeffs[i];
		return result;
	}

	private static double WrapPi(double angle)
	{
		while (angle > Math.PI) angle -= 2.0 * Math.PI;
		while (angle < -Math.PI) angle += 2.0 * Math.PI;
		return angle;
	}
}
