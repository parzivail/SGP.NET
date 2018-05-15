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
        private void SetTime(DateTime dt)
        {
            if (Location is null)
                return;
            Location.Time = dt;
        }
    }
}
