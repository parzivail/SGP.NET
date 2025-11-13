using System;

namespace SGPdotNET.Util
{
    /// <summary>
    /// Feature flags for enabling/disabling performance optimizations and bug fixes.
    /// These flags allow A/B testing and gradual rollout of improvements.
    /// </summary>
    public static class FeatureFlags
    {
        /// <summary>
        /// Enable optimized Math.Pow replacements with direct multiplication.
        /// When enabled, replaces Math.Pow(x, n) for small integer powers with faster multiplication.
        /// </summary>
        public static bool UseOptimizedPowerOperations { get; set; } = false;

        /// <summary>
        /// Enable caching of trigonometric values in hot paths.
        /// When enabled, caches sin/cos values that are recalculated frequently.
        /// </summary>
        public static bool UseTrigonometricCaching { get; set; } = false;

        /// <summary>
        /// Enable fast path for circular orbits in Kepler solver.
        /// When enabled, uses direct solution for near-circular orbits instead of Newton-Raphson.
        /// </summary>
        public static bool UseFastPathCircularOrbits { get; set; } = false;

        /// <summary>
        /// Enable caching of ECI coordinate conversions.
        /// When enabled, caches ground station ECI positions for repeated calculations.
        /// </summary>
        public static bool UseEciConversionCaching { get; set; } = false;

        /// <summary>
        /// Enable caching of geodetic conversion values.
        /// When enabled, caches prime vertical radius and other geodetic constants.
        /// </summary>
        public static bool UseGeodeticCaching { get; set; } = false;

        /// <summary>
        /// Use optimized dictionary lookups (TryGetValue instead of ContainsKey + indexer).
        /// When enabled, uses more efficient dictionary access patterns.
        /// </summary>
        public static bool UseOptimizedDictionaryLookups { get; set; } = false;

        /// <summary>
        /// Enable fixed Julian date calculation.
        /// When enabled, uses corrected Julian date algorithm instead of the buggy implementation.
        /// </summary>
        public static bool FixJulianDateCalculation { get; set; } = false;

        /// <summary>
        /// Enable fixed signal delay calculation (BUG FIX).
        /// The original implementation had an inverted formula (speed/distance instead of distance/speed).
        /// When enabled, uses corrected signal delay formula: (Range * MetersPerKm) / SpeedOfLight.
        /// </summary>
        public static bool UseFixedSignalDelay { get; set; } = false;

        /// <summary>
        /// Use Atan2 for azimuth calculations instead of Atan (BUG FIX).
        /// The original Atan-based implementation has a bug that causes incorrect quadrant handling,
        /// resulting in 180° errors in some cases. Atan2 is mathematically correct.
        /// When enabled, fixes the azimuth calculation bug.
        /// </summary>
        public static bool UseAtan2ForAzimuth { get; set; } = false;

        /// <summary>
        /// Enable all optimizations (for testing/benchmarking).
        /// </summary>
        public static void EnableAllOptimizations()
        {
            UseOptimizedPowerOperations = true;
            UseTrigonometricCaching = true;
            UseFastPathCircularOrbits = true;
            UseEciConversionCaching = true;
            UseGeodeticCaching = true;
            UseOptimizedDictionaryLookups = true;
        }

        /// <summary>
        /// Enable all bug fixes.
        /// </summary>
        public static void EnableAllBugFixes()
        {
            FixJulianDateCalculation = true;
            UseFixedSignalDelay = true;
            UseAtan2ForAzimuth = true;
        }

        /// <summary>
        /// Enable all features (optimizations + bug fixes).
        /// </summary>
        public static void EnableAll()
        {
            EnableAllOptimizations();
            EnableAllBugFixes();
        }

        /// <summary>
        /// Disable all features (revert to original behavior).
        /// </summary>
        public static void DisableAll()
        {
            UseOptimizedPowerOperations = false;
            UseTrigonometricCaching = false;
            UseFastPathCircularOrbits = false;
            UseEciConversionCaching = false;
            UseGeodeticCaching = false;
            UseOptimizedDictionaryLookups = false;
            FixJulianDateCalculation = false;
            UseFixedSignalDelay = false;
            UseAtan2ForAzimuth = false;
        }
    }
}

