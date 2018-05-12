using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SGP4_Sharp;
using DateTime = SGP4_Sharp.DateTime;

namespace Sandbox
{
    class Satellite
    {
        private readonly SGP4 _sgp4;

        public string Name { get; }

        public Satellite(string name, string tle1, string tle2)
        {
            Name = name;

            _sgp4 = new SGP4(new Tle(name, tle1, tle2));
        }

        public Eci Predict()
        {
            return Predict(DateTime.Now());
        }

        public Eci Predict(DateTime time)
        {
            return _sgp4.FindPosition(time);
        }

        public List<CoordGeodetic> GetFootprint()
        {
            return GetFootprint(DateTime.Now());
        }

        public List<CoordGeodetic> GetFootprint(DateTime time)
        {
            var center = Predict(time).ToGeodetic();
            var coords = new List<CoordGeodetic>();
            var size = center.CalculateFootprintRadiusRad();

            for (var i = 0; i < 60; i++)
            {
                var perc = i / 60f * 2 * Math.PI;

                var lat = Math.PI / 2f - size;
                var lon = perc;

                coords.Add(new CoordGeodetic(lat, lon, 10, true));
            }

            return coords;
        }

        private double WrapLon(double d)
        {
            return (d + Math.PI) % (2 * Math.PI) - Math.PI;
        }

        private double WrapLat(double d)
        {
            return (d + Math.PI / 2) % Math.PI - Math.PI / 2;
        }
    }
}
