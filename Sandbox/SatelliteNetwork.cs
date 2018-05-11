using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGP4_Sharp;

namespace Sandbox
{
    class SatelliteNetwork
    {
        public CoordGeodetic GroundStation { get; }
        public List<Satellite> Satellites { get; }

        public SatelliteNetwork(CoordGeodetic groundStation)
        {
            GroundStation = groundStation;
            Satellites = new List<Satellite>();
        }
    }
}
