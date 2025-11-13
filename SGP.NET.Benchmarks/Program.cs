using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Util;

namespace SGP.NET.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SGP.NET Performance Benchmarks");
            Console.WriteLine("==============================\n");

            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

            Console.WriteLine("\nBenchmark complete!");
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
    public class SatellitePropagationBenchmark
    {
        private Satellite _issSatellite;
        private Satellite _noaa18Satellite;
        private Satellite _molniyaSatellite;
        private DateTime[] _testTimes;

        [GlobalSetup]
        public void Setup()
        {
            _issSatellite = new Satellite(
                "ISS (ZARYA)",
                "1 25544U 98067A   19034.73310439  .00001974  00000-0  38215-4 0  9991",
                "2 25544  51.6436 304.9146 0005074 348.4622  36.8575 15.53228055154526"
            );

            _noaa18Satellite = new Satellite(
                "NOAA-18",
                "1 28654U 05018A   19034.12345678  .00000001  00000-0  00000+0 0  9998",
                "2 28654  98.7449 123.4567 0012345 234.5678 125.4321 14.12345678901234"
            );

            _molniyaSatellite = new Satellite(
                "MOLNIYA 1-1",
                "1 00001U 65001A   19034.12345678  .00000001  00000-0  00000+0 0  9998",
                "2 00001  63.4000 270.0000 7200000 270.0000  90.0000   2.0000000000000"
            );

            _testTimes = Enumerable.Range(0, 100)
                .Select(i => new DateTime(2019, 2, 3, 0, 0, 0, DateTimeKind.Utc).AddHours(i))
                .ToArray();
        }

        [Benchmark(Baseline = true)]
        public void Predict_Original()
        {
            FeatureFlags.DisableAll();
            foreach (var time in _testTimes)
            {
                _issSatellite.Predict(time);
            }
        }

        [Benchmark]
        public void Predict_Optimized()
        {
            FeatureFlags.EnableAllOptimizations();
            foreach (var time in _testTimes)
            {
                _issSatellite.Predict(time);
            }
        }

        [Benchmark]
        public void Predict_MultipleSatellites_Original()
        {
            FeatureFlags.DisableAll();
            var satellites = new[] { _issSatellite, _noaa18Satellite, _molniyaSatellite };
            foreach (var satellite in satellites)
            {
                foreach (var time in _testTimes)
                {
                    satellite.Predict(time);
                }
            }
        }

        [Benchmark]
        public void Predict_MultipleSatellites_Optimized()
        {
            FeatureFlags.EnableAllOptimizations();
            var satellites = new[] { _issSatellite, _noaa18Satellite, _molniyaSatellite };
            foreach (var satellite in satellites)
            {
                foreach (var time in _testTimes)
                {
                    satellite.Predict(time);
                }
            }
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
    public class CoordinateConversionBenchmark
    {
        private GeodeticCoordinate _groundStation;
        private DateTime[] _testTimes;

        [GlobalSetup]
        public void Setup()
        {
            _groundStation = new GeodeticCoordinate(
                Angle.FromDegrees(40.689236),
                Angle.FromDegrees(-74.044563),
                0.0
            );

            _testTimes = Enumerable.Range(0, 1000)
                .Select(i => new DateTime(2019, 2, 3, 0, 0, 0, DateTimeKind.Utc).AddMinutes(i))
                .ToArray();
        }

        [Benchmark(Baseline = true)]
        public void GeodeticToEci_Original()
        {
            FeatureFlags.DisableAll();
            foreach (var time in _testTimes)
            {
                _groundStation.ToEci(time);
            }
        }

        [Benchmark]
        public void GeodeticToEci_Optimized()
        {
            FeatureFlags.EnableAllOptimizations();
            foreach (var time in _testTimes)
            {
                _groundStation.ToEci(time);
            }
        }

        [Benchmark]
        public void EciToGeodetic_Original()
        {
            FeatureFlags.DisableAll();
            var eci = _groundStation.ToEci(_testTimes[0]);
            for (int i = 0; i < 1000; i++)
            {
                eci.ToGeodetic();
            }
        }

        [Benchmark]
        public void EciToGeodetic_Optimized()
        {
            FeatureFlags.EnableAllOptimizations();
            var eci = _groundStation.ToEci(_testTimes[0]);
            for (int i = 0; i < 1000; i++)
            {
                eci.ToGeodetic();
            }
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
    public class ObservationBenchmark
    {
        private Satellite _satellite;
        private GroundStation _groundStation;
        private DateTime[] _testTimes;

        [GlobalSetup]
        public void Setup()
        {
            _satellite = new Satellite(
                "ISS (ZARYA)",
                "1 25544U 98067A   19034.73310439  .00001974  00000-0  38215-4 0  9991",
                "2 25544  51.6436 304.9146 0005074 348.4622  36.8575 15.53228055154526"
            );

            _groundStation = new GroundStation(new GeodeticCoordinate(
                Angle.FromDegrees(40.689236),
                Angle.FromDegrees(-74.044563),
                0.0
            ));

            _testTimes = Enumerable.Range(0, 100)
                .Select(i => new DateTime(2019, 2, 3, 0, 0, 0, DateTimeKind.Utc).AddMinutes(i * 10))
                .ToArray();
        }

        [Benchmark(Baseline = true)]
        public void Observe_Original()
        {
            FeatureFlags.DisableAll();
            foreach (var time in _testTimes)
            {
                _groundStation.Observe(_satellite, time);
            }
        }

        [Benchmark]
        public void Observe_Optimized()
        {
            FeatureFlags.EnableAllOptimizations();
            foreach (var time in _testTimes)
            {
                _groundStation.Observe(_satellite, time);
            }
        }

        [Benchmark]
        public void ObserveVisibilityPeriod_Original()
        {
            FeatureFlags.DisableAll();
            var start = new DateTime(2019, 2, 3, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddDays(1);
            _groundStation.Observe(_satellite, start, end, TimeSpan.FromSeconds(10));
        }

        [Benchmark]
        public void ObserveVisibilityPeriod_Optimized()
        {
            FeatureFlags.EnableAllOptimizations();
            var start = new DateTime(2019, 2, 3, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddDays(1);
            _groundStation.Observe(_satellite, start, end, TimeSpan.FromSeconds(10));
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
    public class MathOperationsBenchmark
    {
        private double[] _testValues;

        [GlobalSetup]
        public void Setup()
        {
            _testValues = Enumerable.Range(0, 10000)
                .Select(i => (double)i / 100.0)
                .ToArray();
        }

        [Benchmark(Baseline = true)]
        public double MathPow_Squared()
        {
            double sum = 0;
            foreach (var x in _testValues)
            {
                sum += Math.Pow(x, 2.0);
            }
            return sum;
        }

        [Benchmark]
        public double DirectMultiplication_Squared()
        {
            double sum = 0;
            foreach (var x in _testValues)
            {
                sum += x * x;
            }
            return sum;
        }

        [Benchmark]
        public double MathPow_Cubed()
        {
            double sum = 0;
            foreach (var x in _testValues)
            {
                sum += Math.Pow(x, 3.0);
            }
            return sum;
        }

        [Benchmark]
        public double DirectMultiplication_Cubed()
        {
            double sum = 0;
            foreach (var x in _testValues)
            {
                sum += x * x * x;
            }
            return sum;
        }

        [Benchmark]
        public double MathPow_To1_5()
        {
            double sum = 0;
            foreach (var x in _testValues)
            {
                sum += Math.Pow(x, 1.5);
            }
            return sum;
        }

        [Benchmark]
        public double Optimized_To1_5()
        {
            double sum = 0;
            foreach (var x in _testValues)
            {
                sum += x * Math.Sqrt(x);
            }
            return sum;
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
    public class DictionaryLookupBenchmark
    {
        private Dictionary<int, string> _dictionary;
        private int[] _keys;

        [GlobalSetup]
        public void Setup()
        {
            _dictionary = Enumerable.Range(0, 1000)
                .ToDictionary(i => i, i => $"Value{i}");

            _keys = Enumerable.Range(0, 10000)
                .Select(i => i % 1000)
                .ToArray();
        }

        [Benchmark(Baseline = true)]
        public int ContainsKey_Then_Indexer()
        {
            int count = 0;
            foreach (var key in _keys)
            {
                if (_dictionary.ContainsKey(key))
                {
                    var value = _dictionary[key];
                    if (value != null) count++;
                }
            }
            return count;
        }

        [Benchmark]
        public int TryGetValue()
        {
            int count = 0;
            foreach (var key in _keys)
            {
                if (_dictionary.TryGetValue(key, out var value))
                {
                    if (value != null) count++;
                }
            }
            return count;
        }
    }
}
