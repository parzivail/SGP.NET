# SGP.NET Benchmarks

Performance benchmarks comparing original vs optimized implementations.

## Running Benchmarks

```bash
# Run all benchmarks (Release mode recommended)
dotnet run -c Release

# Run specific benchmark class
dotnet run -c Release -- --filter "*SatellitePropagationBenchmark*"
```

## Benchmark Classes

### SatellitePropagationBenchmark
Tests satellite position prediction performance:
- `Predict_Original` - Baseline implementation
- `Predict_Optimized` - With all optimizations enabled
- `Predict_MultipleSatellites_Original` - Multiple satellites, baseline
- `Predict_MultipleSatellites_Optimized` - Multiple satellites, optimized

### CoordinateConversionBenchmark
Tests coordinate conversion performance:
- `GeodeticToEci_Original` - Baseline geodetic to ECI conversion
- `GeodeticToEci_Optimized` - Optimized with caching
- `EciToGeodetic_Original` - Baseline ECI to geodetic conversion
- `EciToGeodetic_Optimized` - Optimized implementation

### ObservationBenchmark
Tests ground station observation performance:
- `Observe_Original` - Single observation, baseline
- `Observe_Optimized` - Single observation, optimized
- `ObserveVisibilityPeriod_Original` - Full visibility period calculation, baseline
- `ObserveVisibilityPeriod_Optimized` - Full visibility period, optimized

### MathOperationsBenchmark
Tests low-level math operation performance:
- `MathPow_Squared` vs `DirectMultiplication_Squared`
- `MathPow_Cubed` vs `DirectMultiplication_Cubed`
- `MathPow_To1_5` vs `Optimized_To1_5`

### DictionaryLookupBenchmark
Tests dictionary access patterns:
- `ContainsKey_Then_Indexer` - Baseline (two lookups)
- `TryGetValue` - Optimized (single lookup)

## Interpreting Results

BenchmarkDotNet reports:
- **Mean** - Average execution time
- **Error** - Standard error
- **StdDev** - Standard deviation
- **Ratio** - Performance ratio vs baseline (1.00 = same, <1.00 = faster, >1.00 = slower)
- **Gen 0/1/2** - Garbage collection counts
- **Allocated** - Memory allocated per operation

### Expected Improvements

- **Math.Pow replacements**: 3-5x faster
- **Dictionary lookups**: 2x faster
- **Coordinate conversions**: 1.5-2x faster (with caching)
- **Satellite propagation**: 2-3x faster (with all optimizations)
- **Observation calculations**: 1.5-2x faster

## Output

Results are displayed in the console and saved to:
- `BenchmarkDotNet.Artifacts/results/*.html` - HTML report
- `BenchmarkDotNet.Artifacts/results/*.log` - Detailed log

## Notes

- Benchmarks should be run in Release mode for accurate results
- First run may be slower due to JIT compilation
- Results may vary based on hardware and system load
- Use multiple iterations for stable results

