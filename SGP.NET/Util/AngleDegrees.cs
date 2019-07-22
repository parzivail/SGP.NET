namespace SGPdotNET.Util
{
    /// <summary>
    ///     Stores an angle, specified in degrees but backed in radians
    /// </summary>
    public class AngleDegrees : Angle
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="angle">The angle to be stored in the object, in degrees</param>
        public AngleDegrees(double angle) : base(MathUtil.DegreesToRadians(angle))
        {
        }

        /// <summary>
        /// Converts an explicit number to an angle in degrees via a cast
        /// </summary>
        /// <param name="d">The angle, in degrees</param>
        /// <returns>An AngleDegrees object representing the angle</returns>
        public static explicit operator AngleDegrees(double d) => new AngleDegrees(d);
	}
}