# SGP.NET Tests

This project contains correctness tests and performance benchmarks for SGP.NET.

## Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~CorrectnessTests"
```

## Test Structure

### CorrectnessTests.cs
Tests that verify mathematical correctness by comparing:
- Old vs new implementations (when feature flags are toggled)
- Results against known reference values
- Round-trip conversions
- Edge case handling

### TestData.cs
Contains reference test data including:
- Sample TLEs for different orbit types (ISS, NOAA-18, Molniya, Geostationary)
- Reference Julian dates
- Reference Greenwich Sidereal Times
- Test ground station locations

## Feature Flags

All optimizations and bug fixes are controlled by feature flags in `SGPdotNET.Util.FeatureFlags`:

- `UseOptimizedPowerOperations` - Replace Math.Pow with direct multiplication
- `UseTrigonometricCaching` - Cache sin/cos values
- `UseFastPathCircularOrbits` - Fast path for circular orbits
- `UseEciConversionCaching` - Cache ECI conversions
- `UseGeodeticCaching` - Cache geodetic conversion values
- `UseOptimizedDictionaryLookups` - Use TryGetValue instead of ContainsKey
- `UseFixedJulianDate` - Use corrected Julian date calculation
- `UseFixedSignalDelay` - Use corrected signal delay formula
- `UseAtan2ForAzimuth` - Use Atan2 for azimuth calculations

## Running Benchmarks

```bash
cd SGP.NET.Benchmarks
dotnet run -c Release
```

This will run all benchmarks and generate a report comparing:
- Original implementation (baseline)
- Optimized implementation
- Memory allocations
- Execution time

## Expected Results

### Correctness
All tests should pass with both old and new implementations. The new implementation should produce identical results (within numerical precision).

### Performance
Expected improvements:
- **3-5x faster** for Math.Pow replacements
- **2-3x faster** for trigonometric operations (with caching)
- **2x faster** for dictionary lookups
- **1.5-2x faster** for coordinate conversions (with caching)
- **Overall: 5-10x faster** for typical workloads

