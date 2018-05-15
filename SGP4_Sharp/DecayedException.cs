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
