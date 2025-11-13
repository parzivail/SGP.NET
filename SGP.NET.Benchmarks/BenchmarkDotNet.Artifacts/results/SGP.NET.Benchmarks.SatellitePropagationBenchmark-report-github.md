```

BenchmarkDotNet v0.13.12, Ubuntu 25.10 (Questing Quokka)
AMD Ryzen 9 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.111
  [Host]     : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-OFXTHN : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

IterationCount=5  LaunchCount=1  WarmupCount=2  

```
| Method                               | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Predict_Original                     | 21.54 μs | 0.190 μs | 0.049 μs |  1.00 |    0.00 | 1.4343 |  11.72 KB |        1.00 |
| Predict_Optimized                    | 19.73 μs | 0.075 μs | 0.019 μs |  0.92 |    0.00 | 1.4343 |  11.72 KB |        1.00 |
| Predict_MultipleSatellites_Original  | 73.99 μs | 0.223 μs | 0.035 μs |  3.44 |    0.01 | 4.2725 |   35.2 KB |        3.00 |
| Predict_MultipleSatellites_Optimized | 68.12 μs | 2.848 μs | 0.441 μs |  3.16 |    0.02 | 4.2725 |   35.2 KB |        3.00 |
