namespace SGPdotNET.Util
{
    /// <summary>
    ///     Stores an angle, specified in radians and backed in radians. Used for explicit casting.
    /// </summary>
    public class AngleRadians : Angle
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="angle">The angle to be stored in the object, in radians</param>
        public AngleRadians(double angle) : base(angle)
        {
        }
        
        /// <summary>
        /// Converts an explicit number to an angle in radians via a cast
        /// </summary>
        /// <param name="d">The angle, in radians</param>
        /// <returns>An AngleRadians object representing the angle</returns>
        public static explicit operator AngleRadians(double d) => new AngleRadians(d);
	}
}