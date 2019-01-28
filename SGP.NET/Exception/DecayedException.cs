using System;
using SGPdotNET.Util;

namespace SGPdotNET.Exception
{
    /// <inheritdoc />
    /// <summary>
    ///     Exception thrown by the propagator when a satellite decays
    /// </summary>
    public class DecayedException : System.Exception
    {
        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="time">Time of the event</param>
        /// <param name="position">Position of the satellite at time</param>
        /// <param name="velocity">Velocity of the satellite at time</param>
        public DecayedException(DateTime time, Vector3 position, Vector3 velocity) : base("Satellite decayed")
        {
            Time = time;
            Position = position;
            Velocity = velocity;
        }

        /// <summary>
        ///     Time of the event
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        ///     Position of the satellite at time
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        ///     Velocity of the satellite at time
        /// </summary>
        public Vector3 Velocity { get; }
    }
}