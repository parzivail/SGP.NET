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
using System.Text;


namespace SGP4_Sharp
{
  /**
 * @brief Stores a geodetic location (latitude, longitude, altitude).
 *
 * Internally the values are stored in radians and kilometres.
 */
  public class CoordGeodetic
  {
    /**
     * Default constructor
     */
    public CoordGeodetic()
    {
      latitude = 0.0;
      longitude = 0.0;
      altitude = 0.0;
    }

    /**
     * Constructor
     * @param[in] lat the latitude (degrees by default)
     * @param[in] lon the longitude (degrees by default)
     * @param[in] alt the altitude in kilometers
     * @param[in] is_radians whether the latitude/longitude is in radians
     */
    public CoordGeodetic(
      double lat,
      double lon,
      double alt,
      bool is_radians = false)
    {
      if (is_radians)
      {
        latitude = lat;
        longitude = lon;
      }
      else
      {
        latitude = Util.DegreesToRadians(lat);
        longitude = Util.DegreesToRadians(lon);
      }
      altitude = alt;
    }

    /**
     * Copy constructor
     * @param[in] geo object to copy from
     */
    public CoordGeodetic(CoordGeodetic geo)
    {
      latitude = geo.latitude;
      longitude = geo.longitude;
      altitude = geo.altitude;
    }

    /**
     * Assignment operator
     * @param[in] geo object to copy from
     */
    //    public CoordGeodetic operator = (CoordGeodetic geo)
    //    {
    //        if (this != &geo)
    //        {
    //            latitude = geo.latitude;
    //            longitude = geo.longitude;
    //            altitude = geo.altitude;
    //        }
    //        return *this;
    //    }

    /**
     * Equality operator
     * @param[in] geo the object to compare with
     * @returns whether the object is equal
     */
    public static bool operator ==(CoordGeodetic geo, CoordGeodetic b)
    {
      return b.IsEqual(geo);
    }

    /**
     * Inequality operator
     * @param[in] geo the object to compare with
     * @returns whether the object is not equal
     */
    public static bool operator !=(CoordGeodetic geo, CoordGeodetic b)
    {
      return !b.IsEqual(geo);
    }

    /**
     * Dump this object to a string
     * @returns string
     */
    public override string ToString()
    {
//        StringBuilder builder;
//        builder.Append()
//        ss << std::right << std::fixed << std::setprecision(3);
//        ss << "Lat: " << std::setw(7) << Util::RadiansToDegrees(latitude);
//        ss << ", Lon: " << std::setw(7) << Util::RadiansToDegrees(longitude);
//        ss << ", Alt: " << std::setw(9) << altitude;
//        return ss.str();
// TODO Update this

      return "ToString not implemented";
    }

    /** latitude in radians (-PI >= latitude < PI) */
    public double latitude;
    /** latitude in radians (-PI/2 >= latitude <= PI/2) */
    public double longitude;
    /** altitude in kilometers */
    public double altitude;

    private bool IsEqual(CoordGeodetic geo)
    {
      bool equal = false;
      if (latitude == geo.latitude &&
          longitude == geo.longitude &&
          altitude == geo.altitude)
      {
        equal = false;
      }
      return equal;
    }
  }
}
