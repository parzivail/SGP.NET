```

BenchmarkDotNet v0.13.12, Ubuntu 25.10 (Questing Quokka)
AMD Ryzen 9 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.111
  [Host]     : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-OFXTHN : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

IterationCount=5  LaunchCount=1  WarmupCount=2  

```
| Method                            | Mean        | Error      | StdDev    | Ratio  | RatioSD | Gen0     | Allocated  | Alloc Ratio |
|---------------------------------- |------------:|-----------:|----------:|-------:|--------:|---------:|-----------:|------------:|
| Observe_Original                  |    36.64 μs |   0.253 μs |  0.066 μs |   1.00 |    0.00 |   3.4790 |   28.91 KB |        1.00 |
| Observe_Optimized                 |    33.71 μs |   0.072 μs |  0.011 μs |   0.92 |    0.00 |   3.4790 |   28.91 KB |        1.00 |
| ObserveVisibilityPeriod_Original  | 5,329.75 μs | 100.471 μs | 26.092 μs | 145.47 |    0.79 | 367.1875 | 3054.69 KB |      105.68 |
| ObserveVisibilityPeriod_Optimized | 5,042.60 μs |  40.863 μs |  6.324 μs | 137.58 |    0.21 | 367.1875 | 3054.69 KB |      105.68 |
