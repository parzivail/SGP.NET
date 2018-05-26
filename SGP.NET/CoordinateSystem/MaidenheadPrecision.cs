using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGPdotNET.CoordinateSystem
{
    /// <summary>
    /// Defines the avilable granularities for the conversion to the Maidenhead system
    /// </summary>
    public enum MaidenheadPrecision
    {
        /// <summary>
        /// One pair, accurate to 1111.2 kilometers
        /// </summary>
        ThousandKilometers,
        /// <summary>
        /// Two pairs, accurate to 111.12 kilometers
        /// </summary>
        HunderedKilometers,
        /// <summary>
        /// Three pairs, accurate to 4.630 kilometers
        /// </summary>
        FiveKilometers,
        /// <summary>
        /// Four pairs, accurate to 463 meters
        /// </summary>
        FiveHundredMeters,
        /// <summary>
        /// Five pairs, accurate to 19.2917 meters
        /// </summary>
        TwentyMeters,
        /// <summary>
        /// Six pairs, accurate to 1.9292 meters
        /// </summary>
        TwoMeters
    }
}
