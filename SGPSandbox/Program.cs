using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
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
            // Create a set of TLE strings
            var tle1 = "NOAA 18";
            var tle2 = "1 28654U 05018A   21211.15637809  .00000079  00000-0  67126-4 0  9997";
            var tle3 = "2 28654  98.9915 276.1858 0014784 158.1505 202.0300 14.12625913834561";

// Create a satellite from the TLEs
            var sat = new Satellite(tle1, tle2, tle3);

// Set up our ground station location
            var location = new GeodeticCoordinate(Angle.FromDegrees(50.74), Angle.FromDegrees(-1.31), 0);

// Create a ground station
            var groundStation = new GroundStation(location);

// Observe the satellite
            // var observation = groundStation.Observe(sat, new DateTime(2019, 3, 5, 3, 45, 12, DateTimeKind.Utc));
            var observation = groundStation.Observe(sat, DateTime.UtcNow);

            Console.WriteLine(observation);
            
            /*
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

                            comPort.WriteLine("$G");
                            comPort.WriteLine("G10P0L20X0Y0Z0");
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

                        comPort?.WriteLine($"G1X{-observation.Elevation.Degrees:F2}Y{observation.Azimuth.Degrees:F2}F1000");

                        Thread.Sleep(250);
                        break;
                }

                if (state == TrackingState.Quitting)
                    break;
            }

            comPort?.Close();
            */
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
                if (gs.IsVisible(satPos, minAngle, satPos.Time))
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
