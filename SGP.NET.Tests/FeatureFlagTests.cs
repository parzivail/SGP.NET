using System;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Util;
using Xunit;

namespace SGP.NET.Tests
{
    /// <summary>
    /// Tests to verify feature flags work correctly and can be toggled
    /// </summary>
    public class FeatureFlagTests
    {
        [Fact]
        public void FeatureFlags_CanBeToggled()
        {
            // Test that flags can be set and read
            FeatureFlags.UseOptimizedPowerOperations = true;
            Assert.True(FeatureFlags.UseOptimizedPowerOperations);

            FeatureFlags.UseOptimizedPowerOperations = false;
            Assert.False(FeatureFlags.UseOptimizedPowerOperations);
        }

        [Fact]
        public void FeatureFlags_EnableAllOptimizations()
        {
            FeatureFlags.DisableAll();
            Assert.False(FeatureFlags.UseOptimizedPowerOperations);
            Assert.False(FeatureFlags.UseTrigonometricCaching);

            FeatureFlags.EnableAllOptimizations();
            Assert.True(FeatureFlags.UseOptimizedPowerOperations);
            Assert.True(FeatureFlags.UseTrigonometricCaching);
            Assert.True(FeatureFlags.UseFastPathCircularOrbits);
            Assert.True(FeatureFlags.UseEciConversionCaching);
            Assert.True(FeatureFlags.UseGeodeticCaching);
            Assert.True(FeatureFlags.UseOptimizedDictionaryLookups);
        }

        [Fact]
        public void FeatureFlags_EnableAllBugFixes()
        {
            FeatureFlags.DisableAll();
            Assert.False(FeatureFlags.FixJulianDateCalculation);
            Assert.False(FeatureFlags.UseFixedSignalDelay);

            FeatureFlags.EnableAllBugFixes();
            Assert.True(FeatureFlags.FixJulianDateCalculation);
            Assert.True(FeatureFlags.UseFixedSignalDelay);
            Assert.True(FeatureFlags.UseAtan2ForAzimuth);
        }

        [Fact]
        public void FeatureFlags_EnableAll()
        {
            FeatureFlags.DisableAll();
            FeatureFlags.EnableAll();

            // Check optimizations
            Assert.True(FeatureFlags.UseOptimizedPowerOperations);
            Assert.True(FeatureFlags.UseTrigonometricCaching);

            // Check bug fixes
            Assert.True(FeatureFlags.FixJulianDateCalculation);
            Assert.True(FeatureFlags.UseFixedSignalDelay);
        }

        [Fact]
        public void FeatureFlags_DisableAll()
        {
            FeatureFlags.EnableAll();
            FeatureFlags.DisableAll();

            Assert.False(FeatureFlags.UseOptimizedPowerOperations);
            Assert.False(FeatureFlags.UseTrigonometricCaching);
            Assert.False(FeatureFlags.FixJulianDateCalculation);
            Assert.False(FeatureFlags.UseFixedSignalDelay);
        }

        [Fact]
        public void FeatureFlags_IndependentToggling()
        {
            FeatureFlags.DisableAll();

            // Enable one flag
            FeatureFlags.UseOptimizedPowerOperations = true;
            Assert.True(FeatureFlags.UseOptimizedPowerOperations);
            Assert.False(FeatureFlags.UseTrigonometricCaching);

            // Enable another flag
            FeatureFlags.UseTrigonometricCaching = true;
            Assert.True(FeatureFlags.UseOptimizedPowerOperations);
            Assert.True(FeatureFlags.UseTrigonometricCaching);

            // Disable first flag
            FeatureFlags.UseOptimizedPowerOperations = false;
            Assert.False(FeatureFlags.UseOptimizedPowerOperations);
            Assert.True(FeatureFlags.UseTrigonometricCaching);
        }

        [Fact]
        public void FeatureFlags_CodeExecutesWithFlagsEnabled()
        {
            // Verify that code actually runs with flags enabled/disabled
            var satellite = TestData.CreateIssSatellite();
            var testTime = new DateTime(2019, 2, 3, 12, 0, 0, DateTimeKind.Utc);

            // Should not throw with flags enabled
            FeatureFlags.EnableAll();
            var pos1 = satellite.Predict(testTime);
            Assert.NotNull(pos1);

            // Should not throw with flags disabled
            FeatureFlags.DisableAll();
            var pos2 = satellite.Predict(testTime);
            Assert.NotNull(pos2);
        }
    }
}

