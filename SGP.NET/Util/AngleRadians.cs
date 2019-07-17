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

        public static explicit operator AngleRadians(double d) => new AngleRadians(d);
	}
}