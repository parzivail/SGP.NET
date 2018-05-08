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
  /**
 * @brief Stores an Earth-centered inertial position for a particular time.
 */
  public class Eci
  {
    /**
     * @param[in] dt the date to be used for this position
     * @param[in] latitude the latitude in degrees
     * @param[in] longitude the longitude in degrees
     * @param[in] altitude the altitude in kilometers
     */
    public Eci(DateTime dt, double latitude, double longitude, double altitude)
    {
      ToEci(dt, new CoordGeodetic(latitude, longitude, altitude));
    }

    /**
     * @param[in] dt the date to be used for this position
     * @param[in] geo the position
     */
    public Eci(DateTime dt, CoordGeodetic geo)
    {
      ToEci(dt, geo);
    }

    /**
     * @param[in] dt the date to be used for this position
     * @param[in] position
     */
    public Eci(DateTime dt, Vector position)
    {
      m_dt = dt;
      m_position = position;
    }

    /**
     * @param[in] dt the date to be used for this position
     * @param[in] position the position
     * @param[in] velocity the velocity
     */
    public Eci(DateTime dt, Vector position, Vector velocity)
    {
      m_dt = dt;
      m_position = position;
      m_velocity = velocity;
    }

    /**
     * Equality operator
     * @param dt the date to compare
     * @returns true if the object matches
     */
    public static bool operator==(Eci e, DateTime dt)
    {
      return e.m_dt == dt;
    }

    /**
     * Inequality operator
     * @param dt the date to compare
     * @returns true if the object doesn't match
     */
    public static bool operator!=(Eci e, DateTime dt)
    {
      return e.m_dt != dt;
    }

    /**
     * Update this object with a new date and geodetic position
     * @param dt new date
     * @param geo new geodetic position
     */
    public void Update(DateTime dt, CoordGeodetic geo)
    {
      ToEci(dt, geo);
    }

    /**
     * @returns the position
     */
    public Vector Position()
    {
      return m_position;
    }

    /**
     * @returns the velocity
     */
    public Vector Velocity()
    {
      return m_velocity;
    }

    /**
     * @returns the date
     */
    public DateTime GetDateTime()
    {
      return m_dt;
    }

    /**
     * @returns the position in geodetic form
     */
    public CoordGeodetic ToGeodetic()
    {
      double theta = Util.AcTan(m_position.y, m_position.x);
     
      double lon = Util.WrapNegPosPI(theta
                     - m_dt.ToGreenwichSiderealTime());
     
      double r = Math.Sqrt((m_position.x * m_position.x)
                   + (m_position.y * m_position.y));
     
      double e2 = Global.kF * (2.0 - Global.kF);
      
      double lat = Util.AcTan(m_position.z, r);
      double phi = 0.0;
      double c = 0.0;
      int cnt = 0;

      do
      {
        phi = lat;
        double sinphi = Math.Sin(phi);
        c = 1.0 / Math.Sqrt(1.0 - e2 * sinphi * sinphi);
        lat = Util.AcTan(m_position.z + Global.kXKMPER * c * e2 * sinphi, r);
        cnt++;
      } while (Math.Abs(lat - phi) >= 1e-10 && cnt < 10);

      double alt = r / Math.Cos(lat) - Global.kXKMPER * c;

      return new CoordGeodetic(lat, lon, alt, true);
    }

     
    private void ToEci(DateTime dt, CoordGeodetic geo)
    {
      /*
     * set date
     */
      m_dt = dt;

      const double mfactor = Global.kTWOPI * (Global.kOMEGA_E / Global.kSECONDS_PER_DAY);

      /*
     * Calculate Local Mean Sidereal Time for observers longitude
     */
      double theta = m_dt.ToLocalMeanSiderealTime(geo.longitude);

      /*
     * take into account earth flattening
     */
      double c = 1.0
                 / Math.Sqrt(1.0 + Global.kF * (Global.kF - 2.0) * Math.Pow(Math.Sin(geo.latitude), 2.0));
      double s = Math.Pow(1.0 - Global.kF, 2.0) * c;
      double achcp = (Global.kXKMPER * c + geo.altitude) * Math.Cos(geo.latitude);

      /*
     * X position in km
     * Y position in km
     * Z position in km
     * W magnitude in km
     */
      m_position.x = achcp * Math.Cos(theta);
      m_position.y = achcp * Math.Sin(theta);
      m_position.z = (Global.kXKMPER * s + geo.altitude) * Math.Sin(geo.latitude);
      m_position.w = m_position.Magnitude();

      /*
     * X velocity in km/s
     * Y velocity in km/s
     * Z velocity in km/s
     * W magnitude in km/s
     */
      m_velocity.x = -mfactor * m_position.y;
      m_velocity.y = mfactor * m_position.x;
      m_velocity.z = 0.0;
      m_velocity.w = m_velocity.Magnitude();
    }


    private DateTime m_dt;
    private Vector m_position;
    private Vector m_velocity;
  }
}
