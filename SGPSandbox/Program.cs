using System;
using System.Collections.Generic;
using System.IO.Ports;
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
            var tleUrl = new Uri("https://celestrak.com/NORAD/elements/visual.txt");
            var provider = new RemoteTleProvider(true, tleUrl);
            var tles = provider.GetTles();
            var satellites = tles.Select(pair => new Satellite(pair.Value)).ToList();

            var minAngle = (Angle)10;
            var gs = new GroundStation(new GeodeticCoordinate(30.229777, -81.617525, 0));

            var state = TrackingState.ListingComPorts;
            Satellite tracking = null;
            SerialPort comPort = null;

            while (true)
            {
                switch (state)
                {
                    case TrackingState.ListingComPorts:
                        comPort = SelectComPort();
                        if (comPort != null)
                        {
                            if (comPort.IsOpen)
                                comPort.Close();
                            comPort.Open();
                        }
                        state = TrackingState.ListingVisible;
                        break;
                    case TrackingState.ListingVisible:
                        tracking = SelectVisibleSatellite(satellites, gs, minAngle);
                        state = TrackingState.Tracking;
                        break;
                    case TrackingState.Tracking:
                        if (PressedKey(ConsoleKey.V))
                            state = TrackingState.ListingVisible;
                        if (PressedKey(ConsoleKey.Q))
                            state = TrackingState.Quitting;

                        Console.Clear();
                        var observation = gs.Observe(tracking, DateTime.UtcNow);
                        Console.WriteLine(tracking.Name);
                        Console.WriteLine($"{observation.Elevation.Degrees:F2};{observation.Azimuth.Degrees:F2}");

                        comPort?.Write($"{observation.Elevation.Degrees:F2};{observation.Azimuth.Degrees:F2}");

                        Thread.Sleep(250);
                        break;
                }

                if (state == TrackingState.Quitting)
                    break;
            }

            comPort?.Close();
        }

        private static SerialPort SelectComPort()
        {
            Console.Clear();

            const string none = "None";
            var ports = SerialPort.GetPortNames();
            var portsL = ports.ToList();
            portsL.Insert(0, "None");
            ports = portsL.ToArray();

            for (var i = 0; i < ports.Length; i++)
            {
                var sat = ports[i];
                Console.WriteLine($"[{i}] {ports[i]}");
            }

            string input;
            int selectedPort;
            do
            {
                Console.Write("> ");
                input = Console.ReadLine();
            } while (!int.TryParse(input, out selectedPort));

            return ports[selectedPort] == none ? null : new SerialPort(ports[selectedPort], 115200);
        }

        private static bool PressedKey(ConsoleKey needle)
        {
            if (!Console.KeyAvailable) return false;

            var key = Console.ReadKey(true);
            return key.Key == needle;

        }

        private static Satellite SelectVisibleSatellite(List<Satellite> satellites, GroundStation gs, Angle minAngle)
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
    }

    enum TrackingState
    {
        ListingComPorts,
        ListingVisible,
        Tracking,
        Quitting
    }
}
