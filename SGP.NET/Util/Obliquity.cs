using System;

namespace SGPdotNET.Util;

/// <summary>
///     Computes the obliquity of the ecliptic using the IAU polynomial from Meeus,
///     "Astronomical Algorithms" 2nd Ed., Chapter 22.
/// </summary>
/// <remarks>
///     <para>
///         The mean obliquity is the angle between the Earth's equatorial plane and the
///         ecliptic plane, excluding the short-period nutation corrections.
///     </para>
/// </remarks>
public static class Obliquity
{
    /// <summary>
    ///     Computes the mean obliquity of the ecliptic in degrees for a given Julian Date.
    /// </summary>
    /// <param name="jde">The Julian Date (typically in TT for ephemeris calculations).</param>
    /// <returns>The mean obliquity in degrees.</returns>
    /// <remarks>
    ///     Polynomial from Meeus Ch. 22, p. 143. Valid for several millennia around J2000.
    ///     At J2000.0 (JDE 2451545.0), the mean obliquity is 23°26'21.448" = 23.4392911°.
    /// </remarks>
    public static double MeanObliquityDeg(double jde)
    {
        // u in units of 100 Julian centuries from J2000.0
        var u = (jde - 2451545.0) / 3652500.0;

        // Polynomial in arcseconds (Horner's method)
        // epsilon0 = 23°26'21.448" at J2000.0
        var deltaArcsec = u * (-4680.93
            + u * (-1.55
                + u * (1999.25
                    + u * (-51.38
                        + u * (-249.67
                            + u * (-39.05
                                + u * (7.12
                                    + u * (27.87
                                        + u * (5.79
                                            + u * 2.45)))))))));

        // 23°26'21.448" in degrees
        const double epsilon0Deg = 23.0 + 26.0 / 60.0 + 21.448 / 3600.0;
        return epsilon0Deg + deltaArcsec / 3600.0;
    }
}
