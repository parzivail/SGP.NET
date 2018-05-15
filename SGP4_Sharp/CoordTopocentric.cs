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
    /// <summary>
    /// Stores a topocentric location (azimuth, elevation, range and range rate).
    /// Azimuth and elevation are stored in radians. Range in kilometers. Range rate in kilometers/second.
    /// </summary>
    public class CoordTopocentric
    {
        /// <summary>
        /// Azimuth in radians
        /// </summary>
        public double Azimuth;
        /// <summary>
        /// Elevation in radians
        /// </summary>
        public double Elevation;
        /// <summary>
        /// Range in kilometers
        /// </summary>
        public double Range;
        /// <summary>
        /// Range rate in kilometers/second
        /// </summary>
        public double RangeRate;

        public CoordTopocentric()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="az">azimuth in radians</param>
        /// <param name="el">elevation in radians</param>
        /// <param name="rnge">range in kilometers</param>
        /// <param name="rngeRate">range rate in kilometers per second</param>
        public CoordTopocentric(double az, double el, double rnge, double rngeRate)
        {
            Azimuth = az;
            Elevation = el;
            Range = rnge;
            RangeRate = rngeRate;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="topo">object to copy from</param>
        public CoordTopocentric(CoordTopocentric topo)
        {
            Azimuth = topo.Azimuth;
            Elevation = topo.Elevation;
            Range = topo.Range;
            RangeRate = topo.RangeRate;
        }

        public override bool Equals(object obj)
        {
            return obj is CoordGeodetic geodetic &&
                   Equals(geodetic);
        }

        protected bool Equals(CoordTopocentric other)
        {
            return Azimuth.Equals(other.Azimuth) && Elevation.Equals(other.Elevation) && Range.Equals(other.Range) && RangeRate.Equals(other.RangeRate);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Azimuth.GetHashCode();
                hashCode = (hashCode * 397) ^ Elevation.GetHashCode();
                hashCode = (hashCode * 397) ^ Range.GetHashCode();
                hashCode = (hashCode * 397) ^ RangeRate.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"CoordTopocentric{{Azimuth={Azimuth}, Elevation={Elevation}, Range={Range}, Range={RangeRate}}}";
        }
    }
}
