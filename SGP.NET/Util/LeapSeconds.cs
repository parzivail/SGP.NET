using System;

namespace SGPdotNET.Util;

/// <summary>
///     Provides conversion from UTC to Terrestrial Time (TT) using the IERS leap second table.
///     TT = TAI + 32.184 s, and TAI - UTC is the cumulative leap second offset for a given date.
/// </summary>
/// <remarks>
///     <para>
///         The leap second table is sourced from IERS Bulletin C and NIST.
///         As of January 2017, TAI - UTC = 37 seconds. No leap seconds have been added since.
///     </para>
///     <para>
///         See: https://www.nist.gov/pml/time-and-frequency-division/time-realization/leap-seconds
///         See: https://aa.usno.navy.mil/faq/TT
///     </para>
/// </remarks>
public static class LeapSeconds
{
    /// <summary>
    ///     TT offset from TAI in seconds. This is a fixed definition: TT = TAI + 32.184 s.
    /// </summary>
    public const double TtMinusTai = 32.184;

    /// <summary>
    ///     Returns the cumulative leap second offset (TAI - UTC) for a given UTC date.
    ///     The offset increases by 1 second each time a leap second is inserted.
    /// </summary>
    /// <param name="utc">The UTC date/time.</param>
    /// <returns>The number of leap seconds (TAI - UTC) applicable at the given UTC instant.</returns>
    public static int GetTaiMinusUtc(DateTime utc)
    {
        // Leap second insertion dates (all at 23:59:60 UTC on June 30 or December 31).
        // Each entry is the Julian Date at which the new offset takes effect.
        // The offset is the cumulative number of leap seconds from that date onward.
        // Source: IERS Bulletin C, NIST.
        // 1972-01-01: TAI-UTC was set to 10s (retroactive), then +1s on each listed date.
        var jd = utc.ToJulian();

        // Offsets are cumulative; the last entry before or at jd gives the correct offset.
        if (jd < 2441317.5) return 0;   // Before 1972-01-01: no leap seconds defined
        if (jd < 2441499.5) return 10;  // 1972-01-01
        if (jd < 2441683.5) return 11;  // 1972-07-01
        if (jd < 2442048.5) return 12;  // 1973-01-01
        if (jd < 2442413.5) return 13;  // 1974-01-01
        if (jd < 2442778.5) return 14;  // 1975-01-01
        if (jd < 2443144.5) return 15;  // 1976-01-01
        if (jd < 2443509.5) return 16;  // 1977-01-01
        if (jd < 2443874.5) return 17;  // 1978-01-01
        if (jd < 2444239.5) return 18;  // 1979-01-01
        if (jd < 2444786.5) return 19;  // 1980-07-01
        if (jd < 2445151.5) return 20;  // 1981-07-01
        if (jd < 2445516.5) return 21;  // 1982-07-01
        if (jd < 2446247.5) return 22;  // 1985-07-01
        if (jd < 2447161.5) return 23;  // 1988-01-01
        if (jd < 2447892.5) return 24;  // 1990-01-01
        if (jd < 2448257.5) return 25;  // 1991-01-01
        if (jd < 2448804.5) return 26;  // 1992-07-01
        if (jd < 2449169.5) return 27;  // 1993-07-01
        if (jd < 2449534.5) return 28;  // 1994-07-01
        if (jd < 2450083.5) return 29;  // 1996-01-01
        if (jd < 2450630.5) return 30;  // 1997-07-01
        if (jd < 2451179.5) return 31;  // 1999-01-01
        if (jd < 2453736.5) return 32;  // 2006-01-01
        if (jd < 2454832.5) return 33;  // 2009-01-01
        if (jd < 2456109.5) return 34;  // 2012-07-01
        if (jd < 2456652.5) return 35;  // 2014-07-01
        if (jd < 2457204.5) return 36;  // 2015-07-01
        if (jd < 2457754.5) return 37;  // 2017-01-01
        return 37;                       // No leap seconds added since 2017
    }

    /// <summary>
    ///     Converts a UTC Julian Date to a TT (Terrestrial Time) Julian Date.
    ///     TT = UTC + (TAI - UTC) / 86400 + 32.184 / 86400
    /// </summary>
    /// <param name="jdUtc">The Julian Date in UTC.</param>
    /// <returns>The Julian Date in Terrestrial Time (TT).</returns>
    public static double UtcToTt(double jdUtc)
    {
        // Convert JD back to DateTime for leap second lookup.
        // JD 1721425.5 = 0001-01-01 00:00:00 (proleptic Gregorian)
        var ticks = (long)((jdUtc - 1721425.5) * TimeSpan.TicksPerDay);
        var utc = new DateTime(ticks, DateTimeKind.Utc);
        var taiMinusUtc = GetTaiMinusUtc(utc);
        return jdUtc + (taiMinusUtc + TtMinusTai) / 86400.0;
    }
}
