using System;

namespace SGPdotNET
{
    /// <summary>
    ///     Stores an observers location in ECI coordinates
    /// </summary>
    public class Observer
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="latitude">Observer latitude in degrees</param>
        /// <param name="longitude">Observer longitude in degrees</param>
        /// <param name="altitude">Observer altitude in kilometers</param>
        public Observer(double latitude, double longitude, double altitude) : this(new CoordGeodetic(latitude, longitude, altitude))
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="geo">Observer position</param>
        public Observer(CoordGeodetic geo)
        {
            Location = new Eci(DateTime.UtcNow, geo);
        }

        /// <summary>
        ///     The observer ECI for a particular time
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