using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGPdotNET.Util
{
    /// <summary>
    /// Stores an angle, specified in degrees but backed in radians
    /// </summary>
    public class AngleDegrees : Angle
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="angle">The angle to be stored in the object, in degrees</param>
        public AngleDegrees(double angle) : base(MathUtil.DegreesToRadians(angle))
        {
        }
    }
}
