using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.TLE;
using SGPdotNET.Util;

namespace SGPSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var tleUrl = new Url("https://celestrak.com/NORAD/elements/weather.txt");
            var provider = new RemoteTleProvider(true, tleUrl);
            var tles = provider.GetTles();
            var satellites = tles.Select(pair => new Satellite(pair.Value)).ToList();

            var minAngle = new AngleDegrees(10);
            var gs = new GroundStation(new GeodeticCoordinate(new AngleDegrees(30.229777), new AngleDegrees(-81.617525), 0));

            var state = TrackingState.ListingVisible;
            Satellite tracking = null;

            while (true)
            {
                switch (state)
                {
                    case TrackingState.ListingVisible:
                        tracking = SelectVisibleSatellite(satellites, gs, minAngle);
                        state = TrackingState.Tracking;
                        break;
                    case TrackingState.Tracking:
                        if (PressedKey(ConsoleKey.Q))
                            state = TrackingState.ListingVisible;
                        TrackSatellite(gs, tracking);
                        Thread.Sleep(250);
                        break;
                }
            }
        }

        private static bool PressedKey(ConsoleKey needle)
        {
            if (!Console.KeyAvailable) return false;

            var key = Console.ReadKey(true);
            return key.Key == needle;

        }

        private static void TrackSatellite(GroundStation gs, Satellite satellite)
        {
            Console.Clear();
            var observation = gs.Observe(satellite, DateTime.UtcNow);
            Console.WriteLine(satellite.Name);
            Console.WriteLine(observation);
        }

        private static Satellite SelectVisibleSatellite(List<Satellite> satellites, GroundStation gs, AngleDegrees minAngle)
        {
            var visible = new List<Satellite>();
            visible.Clear();
            Console.Clear();

            foreach (var satellite in satellites)
            {
                var satPos = satellite.Predict();
                if (gs.IsVisible(satPos, minAngle))
                    visible.Add(satellite);
            }

            for (var i = 0; i < visible.Count; i++)
            {
                var sat = visible[i];
                Console.WriteLine($"[{i}] {sat.Name}");
            }

            string input;
            int selectedSatellite;
            do
            {
                Console.Write("> ");
                input = Console.ReadLine();
            } while (!int.TryParse(input, out selectedSatellite));

            return visible[selectedSatellite];
        }

        public static TopocentricObservation LookAt(Coordinate from, Coordinate to, DateTime? time = null)
        {
            var t = DateTime.UtcNow;
            if (time.HasValue)
                t = time.Value;

            var geo = from.ToGeodetic();
            var eci = to.ToEci(t);
            var self = from.ToEci(t);

            var rangeRate = eci.Velocity - self.Velocity;
            var range = eci.Position - self.Position;

            var theta = eci.Time.ToLocalMeanSiderealTime(geo.Longitude);

            var sinLat = Math.Sin(geo.Latitude.Radians);
            var cosLat = Math.Cos(geo.Latitude.Radians);
            var sinTheta = Math.Sin(theta);
            var cosTheta = Math.Cos(theta);

            var topS = sinLat * cosTheta * range.X
                       + sinLat * sinTheta * range.Y - cosLat * range.Z;
            var topE = -sinTheta * range.X
                       + cosTheta * range.Y;
            var topZ = cosLat * cosTheta * range.X
                       + cosLat * sinTheta * range.Y + sinLat * range.Z;
            var az = Math.Atan(-topE / topS);

            if (topS > 0.0)
                az += Math.PI;

            if (az < 0.0)
                az += 2.0 * Math.PI;

            var el = Math.Asin(topZ / range.Length);
            var rate = (range.X * rangeRate.X + range.Y * rangeRate.Y + range.Z * rangeRate.Z) / range.Length;

            return new TopocentricObservation(new Angle(az), new Angle(el), range.Length, rate);
        }
    }

    enum TrackingState
    {
        ListingVisible,
        Tracking
    }
}
