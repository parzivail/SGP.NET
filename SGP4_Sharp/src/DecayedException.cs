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
 * @brief The exception that the SGP4 class throws when a satellite decays.
 */
  public class DecayedException : Exception
  {
    /**
     * Constructor
     * @param[in] dt time of the event
     * @param[in] pos position of the satellite at dt
     * @param[in] vel velocity of the satellite at dt
     */
    public DecayedException(DateTime dt, Vector pos, Vector vel)
    {
      _dt = (dt);
      _pos = (pos);
      _vel = (vel);
    }

    /**
     * @returns the error string
     */
    public string what()
    {
      return "Error: Satellite decayed";
    }

    /**
     * @returns the date
     */
    public DateTime Decayed()
    {
      return _dt;
    }

    /**
     * @returns the position
     */
    public Vector Position()
    {
      return _pos;
    }

    /**
     * @returns the velocity
     */
    public Vector Velocity()
    {
      return _vel;
    }

    private DateTime _dt;
    private Vector _pos;
    private Vector _vel;
  };

}
