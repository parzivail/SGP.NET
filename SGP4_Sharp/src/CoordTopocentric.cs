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


namespace SGP4_Sharp
{
  /**
 * @brief Stores a topocentric location (azimuth, elevation, range and range
 * rate).
 *
 * Azimuth and elevation are stored in radians. Range in kilometres. Range
 * rate in kilometres/second.
 */
  public class CoordTopocentric
  {
    /**
     * Default constructor
     */
    public CoordTopocentric()
    {
      azimuth = (0.0); 
      elevation = (0.0);
      range = (0.0);
      range_rate = (0.0);
    }

    /**
     * Constructor
     * @param[in] az azimuth in radians
     * @param[in] el elevation in radians
     * @param[in] rnge range in kilometers
     * @param[in] rnge_rate range rate in kilometers per second
     */
    public CoordTopocentric(
      double az,
      double el,
      double rnge,
      double rnge_rate)
    {
      azimuth = (az);
      elevation = (el);
      range = (rnge);
      range_rate = (rnge_rate);
    }

    /**
     * Copy constructor
     * @param[in] topo object to copy from
     */
    public CoordTopocentric(CoordTopocentric topo)
    {
      azimuth = topo.azimuth;
      elevation = topo.elevation;
      range = topo.range;
      range_rate = topo.range_rate;
    }

    /**
     * Assignment operator
     * @param[in] topo object to copy from
     */
    //    public static CoordTopocentric operator=(CoordTopocentric topo)
    //    {
    //        if (this != &topo)
    //        {
    //            azimuth = topo.azimuth;
    //            elevation = topo.elevation;
    //            range = topo.range;
    //            range_rate = topo.range_rate;
    //        }
    //        return *this;
    //    }

    /**
     * Equality operator
     * @param[in] topo value to check
     * @returns whether the object is equal
     */
    public static bool operator==(CoordTopocentric topo, CoordTopocentric b)
    {
      return b.IsEqual(topo);
    }

    /**
     * Inequality operator
     * @param[in] topo the object to compare with
     * @returns whether the object is not equal
     */    
    public static bool operator !=(CoordTopocentric topo, CoordTopocentric b)
    {
      return !b.IsEqual(topo);
    }

    /**
     * Dump this object to a string
     * @returns string
     */
    public string ToString()
    {
//        std::stringstream ss;
//        ss << std::right << std::fixed << std::setprecision(3);
//        ss << "Az: " << std::setw(8) << Util::RadiansToDegrees(azimuth);
//        ss << ", El: " << std::setw(8) << Util::RadiansToDegrees(elevation);
//        ss << ", Rng: " << std::setw(10) << range;
//        ss << ", Rng Rt: " << std::setw(7) << range_rate;
//        return ss.str();
      return "ToString not yet implemented";
    }

    /** azimuth in radians */
    public double azimuth;
    /** elevations in radians */
    public double elevation;
    /** range in kilometers */
    public double range;
    /** range rate in kilometers per second */
    public double range_rate;

    private bool IsEqual(CoordTopocentric topo)
    {
      bool equal = false;
      if (azimuth == topo.azimuth &&
          elevation == topo.elevation &&
          range == topo.range &&
          range_rate == topo.range_rate)
      {
        equal = true;
      }
      return equal;
    }
  }

}
