using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.TLE;
using SGPdotNET.Util;

namespace Sandbox
{
    class GettingStarted
    {
        public static void Go()
        {
            // Create a provider
            var provider = new LocalTleProvider("tles.txt", true);

            // Get every TLE
            var tles = provider.GetTles();

            // Alternatively get a specific satellite's TLE
            var issTle = provider.GetTle(25544);
        }
    }
}
