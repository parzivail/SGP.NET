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
    /// The exception that the SGP4 class throws when a satellite decays.
    /// </summary>
    public class DecayedException : Exception
    {
        private readonly DateTime _dt;
        private readonly Vector _pos;
        private readonly Vector _vel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dt">time of the event</param>
        /// <param name="pos">position of the satellite at dt</param>
        /// <param name="vel">velocity of the satellite at dt</param>
        public DecayedException(DateTime dt, Vector pos, Vector vel) : base("Error: Satellite decayed")
        {
            _dt = dt;
            _pos = pos;
            _vel = vel;
        }

        /// <summary>
        /// Gets the DateTime at the time of decay
        /// </summary>
        /// <returns></returns>
        public DateTime Decayed()
        {
            return _dt;
        }

        /// <summary>
        /// Gets the position at the time of decay
        /// </summary>
        /// <returns></returns>
        public Vector Position()
        {
            return _pos;
        }

        /// <summary>
        /// Returns the velocity at the time of decay
        /// </summary>
        /// <returns></returns>
        public Vector Velocity()
        {
            return _vel;
        }
    }
}
