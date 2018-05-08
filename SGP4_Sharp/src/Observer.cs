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
using System.IO;

namespace SGP4_Sharp
{

  /**
 * @brief Stores an observers location in Eci coordinates.
 */
  public class Observer
  {
    /** the observers position */
    private CoordGeodetic m_geo;

    /** the observers Eci for a particular time */
    private Eci m_eci;

    /**
     * ructor
     * @param[in] latitude observers latitude in degrees
     * @param[in] longitude observers longitude in degrees
     * @param[in] altitude observers altitude in kilometers
     */
    public Observer(double latitude, double longitude, double altitude)
    {
      m_geo = new CoordGeodetic(latitude, longitude, altitude);
      m_eci = new Eci(new DateTime(), m_geo);
    }

    /**
     * Constructor
     * @param[in] geo the observers position
     */
    public Observer(CoordGeodetic geo)
    {
      m_geo = new CoordGeodetic(geo);
      m_eci = new Eci(new DateTime(), geo);
    }

    /**
     * Set the observers location
     * @param[in] geo the observers position
     */
    public void SetLocation(CoordGeodetic geo)
    {
      m_geo = geo;
      m_eci.Update(m_eci.GetDateTime(), m_geo);
    }

    /**
     * Get the observers location
     * @returns the observers position
     */
    public CoordGeodetic GetLocation()
    {
      return m_geo;
    }

    /**
     * Get the look angle for the observers position to the object
     * @param[in] eci the object to find the look angle to
     * @returns the lookup angle
     */
    public CoordTopocentric GetLookAngle(Eci eci)
    {
      /*
     * update the observers Eci to match the time of the Eci passed in
     * if necessary
     */
      Update(eci.GetDateTime());

      /*
     * calculate differences
     */
      Vector range_rate = eci.Velocity() - m_eci.Velocity();
      Vector range = eci.Position() - m_eci.Position(); 

      range.w = range.Magnitude();

      /*
     * Calculate Local Mean Sidereal Time for observers longitude
     */
      double theta = eci.GetDateTime().ToLocalMeanSiderealTime(m_geo.longitude);
      
      double sin_lat = Math.Sin(m_geo.latitude);
      double cos_lat = Math.Cos(m_geo.latitude);
      double sin_theta = Math.Sin(theta);
      double cos_theta = Math.Cos(theta);

      double top_s = sin_lat * cos_theta * range.x
                     + sin_lat * sin_theta * range.y - cos_lat * range.z;
      double top_e = -sin_theta * range.x
                     + cos_theta * range.y;
      double top_z = cos_lat * cos_theta * range.x
                     + cos_lat * sin_theta * range.y + sin_lat * range.z;
      double az = Math.Atan(-top_e / top_s);

      if (top_s > 0.0)
      {
        az += Global.kPI;
      }

      if (az < 0.0)
      {
        az += 2.0 * Global.kPI;
      }
            
      double el = Math.Asin(top_z / range.w);
      double rate = range.Dot(range_rate) / range.w;

      /*
     * azimuth in radians
     * elevation in radians
     * range in km
     * range rate in km/s
     */
      return new CoordTopocentric(az, el, range.w, rate);
    }

    /**
     * @param[in] dt the date to update the observers position for
     */
    private void Update(DateTime dt)
    {
      if (m_eci != dt)
      {
        m_eci.Update(dt, m_geo);
      }
    }
       
  }
}
