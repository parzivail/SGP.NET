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
using System.Runtime.Remoting.Messaging;

namespace SGP4_Sharp
{



  /**
 * @brief Represents a time interval.
 *
 * Represents a time interval (duration/elapsed) that is measured as a positive
 * or negative number of days, hours, minutes, seconds, and fractions
 * of a second.
 */
  public class TimeSpan
  {
    public const Int64 TicksPerDay = 86400000000L;
    public const Int64 TicksPerHour = 3600000000L;
    public const Int64 TicksPerMinute = 60000000L;
    public const Int64 TicksPerSecond = 1000000L;
    public const Int64 TicksPerMillisecond = 1000L;
    public const Int64 TicksPerMicrosecond = 1L;

    public const Int64 UnixEpoch = 62135596800000000L;

    public const Int64 MaxValueTicks = 315537897599999999L;

    // 1582-Oct-15
    public const Int64 GregorianStart = 49916304000000000L;

    public TimeSpan(Int64 ticks)
    {
      m_ticks = ticks;
    }

    public TimeSpan(int hours, int minutes, int seconds)
    {
      CalculateTicks(0, hours, minutes, seconds, 0);
    }

    public TimeSpan(int days, int hours, int minutes, int seconds)
    {
      CalculateTicks(days, hours, minutes, seconds, 0);
    }

    public TimeSpan(int days, int hours, int minutes, int seconds, int microseconds)
    {
      CalculateTicks(days, hours, minutes, seconds, microseconds);
    }

    public TimeSpan Add(TimeSpan ts)
    {
      return new TimeSpan(m_ticks + ts.m_ticks);
    }

    public TimeSpan Subtract(TimeSpan ts)
    {
      return new TimeSpan(m_ticks - ts.m_ticks);
    }

    public int Compare(TimeSpan ts)
    {
      int ret = 0;

      if (m_ticks < ts.m_ticks)
      {
        ret = -1;
      }
      if (m_ticks > ts.m_ticks)
      {
        ret = 1;
      }
      return ret;
    }

    public override bool Equals(Object o)
    {
      bool result = false;
      TimeSpan ts = o as TimeSpan;
      
      if (ts != null)
      {
        result = this.Equals(ts);
      }

      return result;
    }

    public override int GetHashCode()
    {
      System.TimeSpan ts = new System.TimeSpan(m_ticks);
      return ts.GetHashCode();
    }

    public bool Equals(TimeSpan ts)
    {
      return m_ticks == ts.m_ticks;
    }

    public int Days()
    {
      return (int)(m_ticks / TicksPerDay);
    }

    public int Hours()
    {
      return (int)(m_ticks % TicksPerDay / TicksPerHour);
    }

    public int Minutes()
    {
      return (int)(m_ticks % TicksPerHour / TicksPerMinute);
    }

    public int Seconds()
    {
      return (int)(m_ticks % TicksPerMinute / TicksPerSecond);
    }

    public int Milliseconds()
    {
      return (int)(m_ticks % TicksPerSecond / TicksPerMillisecond);
    }

    public int Microseconds()
    {
      return (int)(m_ticks % TicksPerSecond / TicksPerMicrosecond);
    }

    public Int64 Ticks()
    {
      return m_ticks;
    }

    public double TotalDays()
    {
      return (double)(m_ticks) / TicksPerDay;
    }

    public double TotalHours()
    {
      return (double)(m_ticks) / TicksPerHour;
    }

    public double TotalMinutes()
    {
      return (double)(m_ticks) / TicksPerMinute;
    }

    public double TotalSeconds()
    {
      return (double)(m_ticks) / TicksPerSecond;
    }

    public double TotalMilliseconds()
    {
      return (double)(m_ticks) / TicksPerMillisecond;
    }

    public double TotalMicroseconds()
    {
      return (double)(m_ticks) / TicksPerMicrosecond;
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();



      if (m_ticks < 0)
      {
        builder.Append("-");
      }

      if (Days() != 0)
      {
        builder.Append(String.Format("{0}.", Days()));
      }

      builder.Append(String.Format("{0:00}:", Hours()));
      builder.Append(String.Format("{0:00}:", Minutes()));
      builder.Append(String.Format("{0:00}", Seconds()));

      if (Microseconds() != 0)
      {
        builder.Append(String.Format(".{0}", Microseconds()));
      }

      return builder.ToString();
    }

    private Int64 m_ticks;

    private void CalculateTicks(int days,
                                int hours,
                                int minutes,
                                int seconds,
                                int microseconds)
    {
      m_ticks = days * TicksPerDay +
      (hours * 3600L + minutes * 60L + seconds) * TicksPerSecond +
      microseconds * TicksPerMicrosecond;
    }

    //  private std::ostream& operator<<(std::ostream& strm, const TimeSpan& t)
    //{
    //  return strm << t.ToString();
    //}

    public static TimeSpan operator +(TimeSpan ts1, TimeSpan ts2)
    {
      return ts1.Add(ts2);
    }

    public static TimeSpan operator-(TimeSpan ts1, TimeSpan ts2)
    {
      return ts1.Subtract(ts2);
    }

    public  static bool operator==(TimeSpan ts1, TimeSpan ts2)
    {
      return ts1.Equals(ts2);
    }

    public  static bool operator>(TimeSpan ts1, TimeSpan ts2)
    {
      return (ts1.Compare(ts2) > 0);
    }

    public static  bool operator>=(TimeSpan ts1, TimeSpan ts2)
    {
      return (ts1.Compare(ts2) >= 0);
    }

    public static  bool operator!=(TimeSpan ts1, TimeSpan ts2)
    {
      return !ts1.Equals(ts2);
    }

    public static  bool operator<(TimeSpan ts1, TimeSpan ts2)
    {
      return (ts1.Compare(ts2) < 0);
    }

    public static  bool operator<=(TimeSpan ts1, TimeSpan ts2)
    {
      return (ts1.Compare(ts2) <= 0);
    }
  }
}