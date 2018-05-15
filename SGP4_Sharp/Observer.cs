using System;

namespace SGP4_Sharp
{
    /// <summary>
    ///     Stores an observers location in Eci coordinates.
    /// </summary>
    public class Observer
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="latitude">observer latitude in degrees</param>
        /// <param name="longitude">observer longitude in degrees</param>
        /// <param name="altitude">observer altitude in kilometers</param>
        public Observer(double latitude, double longitude, double altitude)
        {
            Location = new Eci(DateTime.UtcNow, new CoordGeodetic(latitude, longitude, altitude));
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="geo">observer position</param>
        public Observer(CoordGeodetic geo)
        {
            Location = new Eci(DateTime.UtcNow, geo);
        }

        /// <summary>
        ///     The observer Eci for a particular time
        /// </summary>
        public Eci Location { get; }

        public override bool Equals(object obj)
        {
            return obj is Observer observer && Location.Equals(observer.Location);
        }

        public override int GetHashCode()
        {
            return 1369928374 + Location.GetHashCode();
        }
    }
}