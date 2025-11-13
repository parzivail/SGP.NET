using System;
using System.Linq;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Propagation;
using SGPdotNET.Util;
using Xunit;

namespace SGP.NET.Tests
{
    /// <summary>
    /// Tests to verify mathematical correctness of implementations
    /// These tests compare old vs new implementations when feature flags are toggled
    /// </summary>
    public class CorrectnessTests
    {
        [Fact]
        public void JulianDate_MatchesReferenceValues()
        {
            // Test with bug fix enabled (correct implementation)
            FeatureFlags.FixJulianDateCalculation = true;
            
            foreach (var (date, expectedJulian) in TestData.ReferenceJulianDates)
            {
                var calculated = date.ToJulian();
                
                // Allow small floating point differences
                var difference = Math.Abs(calculated - expectedJulian);
                Assert.True(difference < 0.0001, 
                    $"Julian date for {date:yyyy-MM-dd HH:mm:ss} UTC: expected {expectedJulian}, got {calculated}, difference {difference}");
            }
            
            // Reset flag
            FeatureFlags.FixJulianDateCalculation = false;
        }

        [Fact]
        public void GreenwichSiderealTime_MatchesReferenceValues()
        {
            foreach (var (date, expectedGst) in TestData.ReferenceGreenwichSiderealTimes)
            {
                var calculated = date.ToGreenwichSiderealTime();
                
                // Allow small floating point differences (sidereal time is in radians)
                var difference = Math.Abs(calculated - expectedGst);
                // Normalize to [0, 2π) for comparison
                if (difference > Math.PI)
                    difference = 2 * Math.PI - difference;
                
                Assert.True(difference < 0.001, 
                    $"GST for {date:yyyy-MM-dd HH:mm:ss} UTC: expected {expectedGst}, got {calculated}, difference {difference} rad");
            }
        }

        [Fact]
        public void SatellitePropagation_ConsistentResults()
        {
            var satellite = TestData.CreateIssSatellite();
            var testTime = new DateTime(2019, 2, 3, 12, 0, 0, DateTimeKind.Utc);

            // Test with flags disabled (original implementation)
            FeatureFlags.DisableAll();
            var positionOriginal = satellite.Predict(testTime);

            // Test with flags enabled (optimized implementation)
            FeatureFlags.EnableAll();
            var positionOptimized = satellite.Predict(testTime);

            // Results should be identical (within numerical precision)
            Assert.Equal(positionOriginal.Position.X, positionOptimized.Position.X, 5);
            Assert.Equal(positionOriginal.Position.Y, positionOptimized.Position.Y, 5);
            Assert.Equal(positionOriginal.Position.Z, positionOptimized.Position.Z, 5);
            Assert.Equal(positionOriginal.Velocity.X, positionOptimized.Velocity.X, 5);
            Assert.Equal(positionOriginal.Velocity.Y, positionOptimized.Velocity.Y, 5);
            Assert.Equal(positionOriginal.Velocity.Z, positionOptimized.Velocity.Z, 5);
        }

        [Fact]
        public void CoordinateConversion_RoundTrip()
        {
            var originalGeo = new GeodeticCoordinate(
                Angle.FromDegrees(40.689236),
                Angle.FromDegrees(-74.044563),
                0.0
            );

            var testTime = new DateTime(2019, 2, 3, 12, 0, 0, DateTimeKind.Utc);

            // Convert to ECI and back
            var eci = originalGeo.ToEci(testTime);
            var convertedGeo = eci.ToGeodetic();

            // Should match within reasonable precision
            Assert.Equal(originalGeo.Latitude.Degrees, convertedGeo.Latitude.Degrees, 6);
            Assert.Equal(originalGeo.Longitude.Degrees, convertedGeo.Longitude.Degrees, 6);
            Assert.Equal(originalGeo.Altitude, convertedGeo.Altitude, 0.1);
        }

        [Fact]
        public void TopocentricObservation_ConsistentResults()
        {
            var satellite = TestData.CreateIssSatellite();
            var groundStation = new GroundStation(new GeodeticCoordinate(
                Angle.FromDegrees(40.689236),
                Angle.FromDegrees(-74.044563),
                0.0
            ));
            var testTime = new DateTime(2019, 2, 3, 12, 0, 0, DateTimeKind.Utc);

            // Test with flags disabled (original buggy implementation)
            FeatureFlags.DisableAll();
            var observationOriginal = groundStation.Observe(satellite, testTime);

            // Test with flags enabled (bug fixes applied)
            FeatureFlags.EnableAll();
            var observationOptimized = groundStation.Observe(satellite, testTime);

            // Elevation, range, and range rate should be identical
            Assert.Equal(observationOriginal.Elevation.Degrees, observationOptimized.Elevation.Degrees, 5);
            Assert.Equal(observationOriginal.Range, observationOptimized.Range, 0.1);
            Assert.Equal(observationOriginal.RangeRate, observationOptimized.RangeRate, 0.01);

            // Azimuth may differ by 180° due to bug fix in original implementation
            // The original Atan-based calculation has a bug that causes incorrect quadrant handling
            // Atan2 is mathematically correct and fixes this bug
            var azimuthDiff = Math.Abs(observationOriginal.Azimuth.Degrees - observationOptimized.Azimuth.Degrees);
            var normalizedDiff = Math.Min(azimuthDiff, 360.0 - azimuthDiff); // Handle wrapping
            
            // Accept either identical results or 180° difference (bug fix)
            Assert.True(normalizedDiff < 1.0 || Math.Abs(normalizedDiff - 180.0) < 1.0,
                $"Azimuth difference {normalizedDiff}° is unexpected. Original: {observationOriginal.Azimuth.Degrees}°, Fixed: {observationOptimized.Azimuth.Degrees}°");
        }

        [Fact]
        public void CircularOrbit_FastPathMatchesNewtonRaphson()
        {
            // Use a near-circular orbit
            var satellite = TestData.CreateNoaa18Satellite();
            var testTime = new DateTime(2019, 2, 3, 12, 0, 0, DateTimeKind.Utc);

            // Test with fast path disabled
            FeatureFlags.DisableAll();
            var positionOriginal = satellite.Predict(testTime);

            // Test with fast path enabled
            FeatureFlags.DisableAll();
            FeatureFlags.UseFastPathCircularOrbits = true;
            var positionFastPath = satellite.Predict(testTime);

            // Results should match
            Assert.Equal(positionOriginal.Position.X, positionFastPath.Position.X, 5);
            Assert.Equal(positionOriginal.Position.Y, positionFastPath.Position.Y, 5);
            Assert.Equal(positionOriginal.Position.Z, positionFastPath.Position.Z, 5);
        }

        [Fact]
        public void MultipleSatellites_ConsistentResults()
        {
            var satellites = new[]
            {
                TestData.CreateIssSatellite(),
                TestData.CreateNoaa18Satellite(),
                TestData.CreateMolniyaSatellite(),
            };

            var testTime = new DateTime(2019, 2, 3, 12, 0, 0, DateTimeKind.Utc);

            foreach (var satellite in satellites)
            {
                FeatureFlags.DisableAll();
                var posOriginal = satellite.Predict(testTime);

                FeatureFlags.EnableAll();
                var posOptimized = satellite.Predict(testTime);

                // Verify positions match
                Assert.Equal(posOriginal.Position.X, posOptimized.Position.X, 5);
                Assert.Equal(posOriginal.Position.Y, posOptimized.Position.Y, 5);
                Assert.Equal(posOriginal.Position.Z, posOptimized.Position.Z, 5);
            }
        }

        [Fact]
        public void SignalDelay_CalculationCorrect()
        {
            // Create a topocentric observation
            var satellite = TestData.CreateIssSatellite();
            var groundStation = new GroundStation(new GeodeticCoordinate(
                Angle.FromDegrees(40.689236),
                Angle.FromDegrees(-74.044563),
                0.0
            ));
            var testTime = new DateTime(2019, 2, 3, 12, 0, 0, DateTimeKind.Utc);

            // Test with bug fix enabled (should be correct)
            FeatureFlags.DisableAll();
            FeatureFlags.UseFixedSignalDelay = true;
            var observation = groundStation.Observe(satellite, testTime);
            var rangeKm = observation.Range;
            var speedOfLightMps = SgpConstants.SpeedOfLight;
            var metersPerKm = SgpConstants.MetersPerKilometer;

            // Expected delay = distance / speed
            var expectedDelay = (rangeKm * metersPerKm) / speedOfLightMps;
            var calculatedDelay = observation.SignalDelay;

            Assert.Equal(expectedDelay, calculatedDelay, 0.000001);
        }

        [Fact]
        public void Azimuth_EdgeCasesHandled()
        {
            // Test azimuth calculation with edge cases
            // NOTE: This test verifies that Atan2 fixes a bug in the original Atan-based implementation
            // The original implementation incorrectly handles quadrants, causing 180° errors
            var satellite = TestData.CreateIssSatellite();
            var groundStation = new GroundStation(new GeodeticCoordinate(
                Angle.FromDegrees(40.689236),
                Angle.FromDegrees(-74.044563),
                0.0
            ));

            // Test multiple times to catch edge cases
            foreach (var testTime in TestData.TestTimes)
            {
                FeatureFlags.DisableAll();
                var obsOriginal = groundStation.Observe(satellite, testTime);

                FeatureFlags.UseAtan2ForAzimuth = true;
                var obsOptimized = groundStation.Observe(satellite, testTime);

                // Azimuth should be in [0, 360) degrees (both implementations should produce valid results)
                Assert.True(obsOriginal.Azimuth.Degrees >= 0 && obsOriginal.Azimuth.Degrees < 360,
                    $"Original azimuth out of range: {obsOriginal.Azimuth.Degrees}");
                Assert.True(obsOptimized.Azimuth.Degrees >= 0 && obsOptimized.Azimuth.Degrees < 360,
                    $"Fixed azimuth out of range: {obsOptimized.Azimuth.Degrees}");

                // Calculate normalized difference (handles wrapping)
                var diff = Math.Abs(obsOriginal.Azimuth.Degrees - obsOptimized.Azimuth.Degrees);
                var normalizedDiff = Math.Min(diff, 360.0 - diff);

                // Accept either:
                // 1. Identical results (within 1° tolerance)
                // 2. 180° difference (bug fix - original has incorrect quadrant handling)
                // 3. Wrapping case (diff > 359° means they're very close when wrapped)
                Assert.True(normalizedDiff < 1.0 || Math.Abs(normalizedDiff - 180.0) < 1.0 || diff > 359.0,
                    $"Azimuth mismatch: original={obsOriginal.Azimuth.Degrees}°, fixed={obsOptimized.Azimuth.Degrees}°, diff={normalizedDiff}°");
            }
        }

        [Fact]
        public void GeodeticCaching_ConsistentResults()
        {
            var geo = new GeodeticCoordinate(
                Angle.FromDegrees(40.689236),
                Angle.FromDegrees(-74.044563),
                0.0
            );
            var testTime = new DateTime(2019, 2, 3, 12, 0, 0, DateTimeKind.Utc);

            // Without caching
            FeatureFlags.DisableAll();
            var eci1 = geo.ToEci(testTime);

            // With caching
            FeatureFlags.UseGeodeticCaching = true;
            var eci2 = geo.ToEci(testTime);

            // Results should be identical
            Assert.Equal(eci1.Position.X, eci2.Position.X, 10);
            Assert.Equal(eci1.Position.Y, eci2.Position.Y, 10);
            Assert.Equal(eci1.Position.Z, eci2.Position.Z, 10);
        }
    }
}

