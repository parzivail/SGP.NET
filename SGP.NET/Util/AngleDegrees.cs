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

        public static explicit operator AngleDegrees(double d) => new AngleDegrees(d);
	}
}