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
    /// <summary>
    /// Stores an observers location in Eci coordinates.
    /// </summary>
    public class Observer
    {
        /// <summary>
        /// The observer Eci for a particular time
        /// </summary>
        public Eci Location;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="latitude">observer latitude in degrees</param>
        /// <param name="longitude">observer longitude in degrees</param>
        /// <param name="altitude">observer altitude in kilometers</param>
        public Observer(double latitude, double longitude, double altitude)
        {
            Location = new Eci(DateTime.UtcNow, new CoordGeodetic(latitude, longitude, altitude));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="geo">observer position</param>
        public Observer(CoordGeodetic geo)
        {
            Location = new Eci(DateTime.UtcNow, geo);
        }

        /// <summary>
        /// Sets the observer's location
        /// </summary>
        /// <param name="geo">observer location</param>
        public void SetLocation(CoordGeodetic geo)
        {
            Location.FromGeodetic(Location.Time, geo);
        }

        /// <summary>
        /// Sets the time of this location
        /// </summary>
        /// <param name="dt"></param>
        private void Update(DateTime dt)
        {
            if (Location is null)
                return;
            Location.Time = dt;
        }
    }
}
