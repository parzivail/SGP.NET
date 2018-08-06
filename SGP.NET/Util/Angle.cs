using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGPdotNET.Util
{
    /// <summary>
    /// Stores an angle
    /// </summary>
    public class Angle
    {
        /// <summary>
        /// The Angle that represents zero radians/degrees
        /// </summary>
        public static readonly Angle Zero = new Angle(0);

        /// <summary>
        /// The angle represented by this object, in degrees
        /// </summary>
        public double Degrees => MathUtil.RadiansToDegrees(Radians);

        /// <summary>
        /// The angle represented by this object, in radians
        /// </summary>
        public double Radians { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="angle">The angle to be stored in the object</param>
        /// <param name="degrees">True if the supplied angle is in degrees, otherwise radians is assumed</param>
        public Angle(double angle, bool degrees = false)
        {
            Radians = degrees ? angle / 180 * Math.PI : angle;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Angle angle && Radians == angle.Radians;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return 1530437289 + Radians.GetHashCode();
        }

        /// <summary>
        /// Checks two angles for equality
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <returns>True if the supplied angles are exactly equal</returns>
        public static bool operator ==(Angle angle1, Angle angle2)
        {
            return EqualityComparer<Angle>.Default.Equals(angle1, angle2);
        }

        /// <summary>
        /// Checks two angles for inequality
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <returns>True if the supplied angles are not exactly equal</returns>
        public static bool operator !=(Angle angle1, Angle angle2)
        {
            return !(angle1 == angle2);
        }

        /// <summary>
        /// Compares two angles
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <returns>True if the first angle is greater than the second</returns>
        public static bool operator >(Angle angle1, Angle angle2)
        {
            return angle1.Radians > angle2.Radians;
        }

        /// <summary>
        /// Compares two angles
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <returns>True if the first angle is less than the second</returns>
        public static bool operator <(Angle angle1, Angle angle2)
        {
            return angle1.Radians < angle2.Radians;
        }

        /// <summary>
        /// Compares two angles
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <returns>True if the first angle is greater than or equal to the second</returns>
        public static bool operator >=(Angle angle1, Angle angle2)
        {
            return angle1.Radians >= angle2.Radians;
        }

        /// <summary>
        /// Compares two angles
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <returns>True if the first angle is less than or equal to the second</returns>
        public static bool operator <=(Angle angle1, Angle angle2)
        {
            return angle1.Radians <= angle2.Radians;
        }

        /// <summary>
        /// Adds two angles
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <returns>The result of adding the first angle to the second angle</returns>
        public static Angle operator +(Angle angle1, Angle angle2)
        {
            return new Angle(angle1.Radians + angle2.Radians);
        }

        /// <summary>
        /// Subtracts two angles
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <returns>The result of subtracting the second angle from the first angle</returns>
        public static Angle operator -(Angle angle1, Angle angle2)
        {
            return new Angle(angle1.Radians - angle2.Radians);
        }
    }
}
