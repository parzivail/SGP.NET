using System;
using SGPdotNET.Propagation;

namespace SGPdotNET.Util
{
    /// <summary>
    /// Optimized math operations that can be toggled via feature flags
    /// </summary>
    internal static class OptimizedMath
    {
        /// <summary>
        /// Optimized power operation: x^2
        /// </summary>
        public static double Pow2(double x)
        {
            if (FeatureFlags.UseOptimizedPowerOperations)
                return x * x;
            return Math.Pow(x, 2.0);
        }

        /// <summary>
        /// Optimized power operation: x^3
        /// </summary>
        public static double Pow3(double x)
        {
            if (FeatureFlags.UseOptimizedPowerOperations)
                return x * x * x;
            return Math.Pow(x, 3.0);
        }

        /// <summary>
        /// Optimized power operation: x^4
        /// </summary>
        public static double Pow4(double x)
        {
            if (FeatureFlags.UseOptimizedPowerOperations)
            {
                var x2 = x * x;
                return x2 * x2;
            }
            return Math.Pow(x, 4.0);
        }

        /// <summary>
        /// Optimized power operation: x^1.5 (x * sqrt(x))
        /// </summary>
        public static double Pow1_5(double x)
        {
            if (FeatureFlags.UseOptimizedPowerOperations)
                return x * Math.Sqrt(x);
            return Math.Pow(x, 1.5);
        }

        /// <summary>
        /// Optimized power operation: x^(2/3)
        /// </summary>
        public static double Pow2_3(double x)
        {
            if (FeatureFlags.UseOptimizedPowerOperations)
            {
                // x^(2/3) = (x^(1/3))^2 = cbrt(x)^2
                var cbrt = Math.Pow(x, 1.0 / 3.0); // No good optimization for cube root
                return cbrt * cbrt;
            }
            return Math.Pow(x, SgpConstants.TwoThirds);
        }

        /// <summary>
        /// Optimized power operation: x^3.5 (x^3 * sqrt(x))
        /// </summary>
        public static double Pow3_5(double x)
        {
            if (FeatureFlags.UseOptimizedPowerOperations)
            {
                var x3 = x * x * x;
                return x3 * Math.Sqrt(x);
            }
            return Math.Pow(x, 3.5);
        }

        /// <summary>
        /// Optimized power operation: x^n where n is a small integer
        /// </summary>
        public static double Pow(double x, double n)
        {
            if (!FeatureFlags.UseOptimizedPowerOperations)
                return Math.Pow(x, n);

            // Handle common integer powers
            if (n == 2.0) return Pow2(x);
            if (n == 3.0) return Pow3(x);
            if (n == 4.0) return Pow4(x);
            if (n == 1.5) return Pow1_5(x);
            if (n == 3.5) return Pow3_5(x);
            if (Math.Abs(n - SgpConstants.TwoThirds) < 1e-10) return Pow2_3(x);

            // Fall back to Math.Pow for other cases
            return Math.Pow(x, n);
        }
    }
}

