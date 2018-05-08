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
using System.Text;
using System.Runtime.InteropServices;


namespace SGP4_Sharp
{
  /**
   * @brief Represents an instance in time.
   */
  public class DateTime
  {
    public const Int64 TicksPerDay = 86400000000L;
    public const Int64 TicksPerHour = 3600000000L;
    public const Int64 TicksPerMinute = 60000000L;
    public const Int64 TicksPerSecond = 1000000L;
    public const Int64 TicksPerMillisecond = 1000L;
    public const Int64 TicksPerMicrosecond = 1L;

    public const Int64 UnixEpoch = 62135596800000000L;

    public const Int64 MaxValueTicks = 315537897599999999L;


    public static int[,] daysInMonth = new int[2, 13]
    {
      //  1   2   3   4   5   6   7   8   9   10  11  12
      { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 },
      { 0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 }
    };
    public static int[,] cumulDaysInMonth = new int[2, 13]
    {
      //  1  2   3   4   5    6    7    8    9    10   11   12
      { 0, 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 },
      { 0, 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335 }
    };

    /**
     * Default contructor
     * Initialise to 0001/01/01 00:00:00.000000
     */
    public DateTime()
    {
      Initialise(1, 1, 1, 0, 0, 0, 0);
    }

    /**
     * Constructor
     * @param[in] ticks raw tick value
     */
    public DateTime(UInt64 ticks)
    {
      m_encoded = ticks;
    }

    /**
     * Constructor
     * @param[in] year the year
     * @param[in] doy the day of the year
     */
    public DateTime(uint year, double doy)
    {
      double absDays = AbsoluteDays((int)year, doy);

      TimeSpan t = new TimeSpan((Int64)(absDays * TicksPerDay));

      m_encoded = (ulong)(t.Ticks());
    }

    /**
     * Constructor
     * @param[in] year the year
     * @param[in] month the month
     * @param[in] day the day
     */
    public DateTime(int year, int month, int day)
    {
      Initialise(year, month, day, 0, 0, 0, 0);
    }

    /**
     * Constructor
     * @param[in] year the year
     * @param[in] month the month
     * @param[in] day the day
     * @param[in] hour the hour
     * @param[in] minute the minute
     * @param[in] second the second
     */
    public DateTime(int year, int month, int day, int hour, int minute, int second)
    {
      Initialise(year, month, day, hour, minute, second, 0);
    }

    public DateTime(System.DateTime dt)
    {
      Initialise(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond * 1000);
    }

    /**
     * Constructor
     * @param[in] year the year
     * @param[in] month the month
     * @param[in] day the day
     * @param[in] hour the hour
     * @param[in] minute the minute
     * @param[in] second the second
     * @param[in] microsecond the microsecond
     */
    public void Initialise(int year,
                           int month,
                           int day,
                           int hour,
                           int minute,
                           int second,
                           int microsecond)
    {
      if (!IsValidYearMonthDay(year, month, day) ||
          hour < 0 || hour > 23 ||
          minute < 0 || minute > 59 ||
          second < 0 || second > 59 ||
          microsecond < 0 || microsecond > 999999)
      {
        throw new Exception();
      }
      m_encoded = (ulong)(new TimeSpan(
        AbsoluteDays(year, month, day),
        hour,
        minute,
        second,
        microsecond).Ticks());
    }

    /**
     * Return the current time
     * @param[in] microseconds whether to set the microsecond component
     * @returns a DateTime object set to the current date and time
     */
    public static DateTime Now(bool microseconds = false)
    {
      DateTime dt = new DateTime(System.DateTime.UtcNow);
//      System.DateTime sdt = System.DateTime.UtcNow;
//      DateTime dt = null;
//
//      if (microseconds) {
//        dt = DateTime (UnixEpoch
//        + ts.tv_sec * TicksPerSecond
//        + ts.tv_nsec / 1000LL * TicksPerMicrosecond);
//      } else {
//        dt = DateTime (UnixEpoch
//        + ts.tv_sec * TicksPerSecond);
//      }
       

      return dt;
    }

    /**
     * Find whether a year is a leap year
     * @param[in] year the year to check
     * @returns whether the year is a leap year
     */
    public static bool IsLeapYear(int year)
    {
      if (!IsValidYear(year))
      {
        throw new Exception();
      }

      return (((year % 4) == 0 && (year % 100) != 0) || (year % 400) == 0);
    }

    /**
     * Checks whether the given year is valid
     * @param[in] year the year to check
     * @returns whether the year is valid
     */
    public static bool IsValidYear(int year)
    {
      bool valid = true;
      if (year < 1 || year > 9999)
      {
        valid = false;
      }
      return valid;
    }

    /**
     * Check whether the year/month is valid
     * @param[in] year the year to check
     * @param[in] month the month to check
     * @returns whether the year/month is valid
     */
    public static bool IsValidYearMonth(int year, int month)
    {
      bool valid = true;
      if (IsValidYear(year))
      {
        if (month < 1 || month > 12)
        {
          valid = false;
        }
      }
      else
      {
        valid = false;
      }
      return valid;
    }

    /**
     * Check whether the year/month/day is valid
     * @param[in] year the year to check
     * @param[in] month the month to check
     * @param[in] day the day to check
     * @returns whether the year/month/day is valid
     */
    public static bool IsValidYearMonthDay(int year, int month, int day)
    {
      bool valid = true;
      if (IsValidYearMonth(year, month))
      {
        if (day < 1 || day > DaysInMonth(year, month))
        {
          valid = false;
        }
      }
      else
      {
        valid = false;
      }
      return valid;
    }

    /**
     * Find the number of days in a month given the year/month
     * @param[in] year the year
     * @param[in] month the month
     * @returns the days in the given month
     */
    public static int DaysInMonth(int year, int month)
    {
      if (!IsValidYearMonth(year, month))
      {
        throw new Exception();
      }

      int result;

      if (IsLeapYear(year))
      {
        result = daysInMonth[1, month];
      }
      else
      {
        result = daysInMonth[0, month];
      }

      return result;
    }

    /**
     * Find the day of the year given the year/month/day
     * @param[in] year the year
     * @param[in] month the month
     * @param[in] day the day
     * @returns the day of the year
     */
    public int DayOfYear(int year, int month, int day)
    {
      if (!IsValidYearMonthDay(year, month, day))
      {
        throw new Exception();
      }

      int daysThisYear = day;

      if (IsLeapYear(year))
      {
        daysThisYear += cumulDaysInMonth[1, month];
      }
      else
      {
        daysThisYear += cumulDaysInMonth[0, month];
      }

      return daysThisYear;
    }

    /**
     *
     */
    public double AbsoluteDays(int year, double doy)
    {

      int previousYear = year - 1;

      /*
       * + days in previous years ignoring leap days
       * + Julian leap days before this year
       * - minus prior century years
       * + plus prior years divisible by 400 days
       */
      Int64 daysSoFar = 365 * previousYear
                        + previousYear / 4
                        - previousYear / 100
                        + previousYear / 400;

      return (double)(daysSoFar) + doy - 1.0;
    }

    public int AbsoluteDays(int year, int month, int day)
    {
      int previousYear = year - 1;

      /*
       * days this year (0 - ...)
       * + days in previous years ignoring leap days
       * + Julian leap days before this year
       * - minus prior century years
       * + plus prior years divisible by 400 days
       */
      int result = DayOfYear(year, month, day) - 1
                   + 365 * previousYear
                   + previousYear / 4
                   - previousYear / 100
                   + previousYear / 400;

      return result;
    }

    public TimeSpan TimeOfDay()
    {
      return new TimeSpan((long)(Ticks() % TimeSpan.TicksPerDay));
    }

    public int DayOfWeek()
    {
      /*
           * The fixed day 1 (January 1, 1 Gregorian) is Monday.
           * 0 Sunday
           * 1 Monday
           * 2 Tuesday
           * 3 Wednesday
           * 4 Thursday
           * 5 Friday
           * 6 Saturday
           */
      return (int)(((m_encoded / TimeSpan.TicksPerDay) + 1L) % 7L);
    }

    public bool Equals(DateTime dt)
    {
      return (m_encoded == dt.m_encoded);
    }

    public int Compare(DateTime dt)
    {
      int ret = 0;

      if (m_encoded < dt.m_encoded)
      {
        return -1;
      }
      else if (m_encoded > dt.m_encoded)
      {
        return 1;
      }

      return ret;
    }

    public DateTime AddYears(int years)
    {
      return AddMonths(years * 12);
    }

    public DateTime AddMonths(int months)
    {
      int year = 0;
      int month = 0;
      int day = 0;
      FromTicks(ref year, ref month, ref day);
      month += months % 12;
      year += months / 12;

      if (month < 1)
      {
        month += 12;
        --year;
      }
      else if (month > 12)
      {
        month -= 12;
        ++year;
      }

      int maxday = DaysInMonth(year, month);
      day = Math.Min(day, maxday);

      return new DateTime(year, month, day).Add(TimeOfDay());
    }

    /**
     * Add a TimeSpan to this DateTime
     * @param[in] t the TimeSpan to add
     * @returns a DateTime which has the given TimeSpan added
     */
    public DateTime Add(TimeSpan t)
    {
      return AddTicks(t.Ticks());
    }

    public DateTime AddDays(double days)
    {
      return AddMicroseconds(days * 86400000000.0);
    }

    public DateTime AddHours(double hours)
    {
      return AddMicroseconds(hours * 3600000000.0);
    }

    public DateTime AddMinutes(double minutes)
    {
      return AddMicroseconds(minutes * 60000000.0);
    }

    public DateTime AddSeconds(double seconds)
    {
      return AddMicroseconds(seconds * 1000000.0);
    }

    public DateTime AddMicroseconds(double microseconds)
    {
      Int64 ticks = (Int64)(microseconds * TicksPerMicrosecond);
      return AddTicks(ticks);
    }

    public DateTime AddTicks(Int64 ticks)
    {
      return new DateTime(m_encoded + (ulong)ticks);
    }

    /**
     * Get the number of ticks
     * @returns the number of ticks
     */
    public UInt64 Ticks()
    {
      return m_encoded;
    }

    public void FromTicks(ref int year, ref int month, ref int day)
    {
      int totalDays = (int)(m_encoded / TicksPerDay);

      /*
       * number of 400 year cycles
       */
      int num400 = totalDays / 146097;
      totalDays -= num400 * 146097;
      /*
       * number of 100 year cycles
       */
      int num100 = totalDays / 36524;
      if (num100 == 4)
      {
        /*
         * last day of the last leap century
         */
        num100 = 3;
      }
      totalDays -= num100 * 36524;
      /*
       * number of 4 year cycles
       */
      int num4 = totalDays / 1461;
      totalDays -= num4 * 1461;
      /*
       * number of years
       */
      int num1 = totalDays / 365;
      if (num1 == 4)
      {
        /*
         * last day of the last leap olympiad
         */
        num1 = 3;
      }
      totalDays -= num1 * 365;

      /*
       * find year
       */
      year = (num400 * 400) + (num100 * 100) + (num4 * 4) + num1 + 1;

      /*
       * convert day of year to month/day
       */
      int daysInMonthIndex;
      if (IsLeapYear(year))
      {
        daysInMonthIndex = 1;
      }
      else
      {
        daysInMonthIndex = 0;
      }

      month = 1;
      while (totalDays >= daysInMonth[daysInMonthIndex, month] && month <= 12)
      {
        totalDays -= daysInMonth[daysInMonthIndex, month++];
      }

      day = totalDays + 1;
    }

    public int Year()
    {
      int year = 0;
      int month = 0;
      int day = 0;
      FromTicks(ref year, ref month, ref day);
      return year;
    }

    public int Month()
    {
      int year = 0;
      int month = 0;
      int day = 0;
      FromTicks(ref year, ref month, ref day);
      return month;
    }

    public int Day()
    {
      int year = 0;
      int month = 0;
      int day = 0;
      FromTicks(ref year, ref month, ref day);
      return day;
    }

    /**
     * Hour component
     * @returns the hour component
     */
    public int Hour()
    {
      return (int)(m_encoded % TicksPerDay / TicksPerHour);
    }

    /**
     * Minute component
     * @returns the minute component
     */
    public int Minute()
    {
      return (int)(m_encoded % TicksPerHour / TicksPerMinute);
    }

    /**
     * Second component
     * @returns the Second component
     */
    public int Second()
    {
      return (int)(m_encoded % TicksPerMinute / TicksPerSecond);
    }

    /**
     * Microsecond component
     * @returns the microsecond component
     */
    public int Microsecond()
    {
      return (int)(m_encoded % TicksPerSecond / TicksPerMicrosecond);
    }

    /**
     * Convert to a julian date
     * @returns the julian date
     */
    public double ToJulian()
    {
      TimeSpan ts = new TimeSpan((long)Ticks());
      return ts.TotalDays() + 1721425.5;
    }

    /**
     * Convert to greenwich sidereal time
     * @returns the greenwich sidereal time
     */
    public double ToGreenwichSiderealTime()
    {
      // t = Julian centuries from 2000 Jan. 1 12h UT1
      double t = (ToJulian() - 2451545.0) / 36525.0;

      // Rotation angle in arcseconds
      double theta = 67310.54841
                     + (876600.0 * 3600.0 + 8640184.812866) * t
                     + 0.093104 * t * t
                     - 0.0000062 * t * t * t;

      // 360.0 / 86400.0 = 1.0 / 240.0
      return Util.WrapTwoPI(Util.DegreesToRadians(theta / 240.0));
    }

    /**
     * Convert to local mean sidereal time (GMST plus the observer's longitude)
     * @param[in] lon observers longitude
     * @returns the local mean sidereal time
     */
    public double ToLocalMeanSiderealTime(double lon)
    {
      return Util.WrapTwoPI(ToGreenwichSiderealTime() + lon);
    }

    public System.DateTime ToSystemDateTime()
    {
      System.DateTime dt = new System.DateTime(this.Year(), this.Month(), this.Day(), this.Hour(), this.Minute(), this.Second());

      return dt;
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();
      int year = 0;
      int month = 0;
      int day = 0;
      FromTicks(ref year, ref month, ref day);

      builder.Append(String.Format("{0:0000}-", year));
      builder.Append(String.Format("{0:00}-", month));
      builder.Append(String.Format("{0:00} ", day));
      builder.Append(String.Format("{0:00}:", Hour()));
      builder.Append(String.Format("{0:00}:", Minute()));
      builder.Append(String.Format("{0:00}.", Second()));
      builder.Append(String.Format("{0:0000} UTC", Microsecond()));

      return builder.ToString();
    }

    public static DateTime Parse(string dateTimeText)
    {
      //2015-4-13 0:5:39.0 UTC
      DateTime dt = new DateTime(System.DateTime.ParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.ffff", null));

      return dt;
    }


    private UInt64 m_encoded;

    // TODO stream operator commented
    //    public std::ostream& operator<<(std::ostream& strm, const DateTime& dt)
    //    {
    //        return strm << dt.ToString();
    //    }

    public static DateTime operator +(DateTime dt, TimeSpan ts)
    {
      Int64 res = (long)dt.Ticks() + ts.Ticks();
      if (res < 0 || res > MaxValueTicks)
      {
        throw new Exception();
      }

      return new DateTime((ulong)res);
    }

    public static DateTime operator -(DateTime dt, TimeSpan ts)
    {
      Int64 res = (long)dt.Ticks() - ts.Ticks();
      if (res < 0 || res > MaxValueTicks)
      {
        throw new Exception();
      }

      return new DateTime((ulong)res);
    }

    public static TimeSpan operator -(DateTime dt1, DateTime dt2)
    {
      return new TimeSpan((long)(dt1.Ticks() - dt2.Ticks()));
    }

    public static bool operator ==(DateTime dt1, DateTime dt2)
    {
      return dt1.Equals(dt2);
    }

    public static bool operator >(DateTime dt1, DateTime dt2)
    {
      return (dt1.Compare(dt2) > 0);
    }

    public static bool operator >=(DateTime dt1, DateTime dt2)
    {
      return (dt1.Compare(dt2) >= 0);
    }

    public static bool operator !=(DateTime dt1, DateTime dt2)
    {
      return !dt1.Equals(dt2);
    }

    public static bool operator <(DateTime dt1, DateTime dt2)
    {
      return (dt1.Compare(dt2) < 0);
    }

    public static bool operator <=(DateTime dt1, DateTime dt2)
    {
      return (dt1.Compare(dt2) <= 0);
    }
  }
}