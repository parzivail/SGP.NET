```

BenchmarkDotNet v0.13.12, Ubuntu 25.10 (Questing Quokka)
AMD Ryzen 9 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.111
  [Host]     : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-OFXTHN : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

IterationCount=5  LaunchCount=1  WarmupCount=2  

```
| Method                       | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|----------------------------- |----------:|----------:|----------:|------:|----------:|------------:|
| MathPow_Squared              | 73.728 μs | 0.9698 μs | 0.1501 μs |  1.00 |         - |          NA |
| DirectMultiplication_Squared |  5.968 μs | 0.0231 μs | 0.0036 μs |  0.08 |         - |          NA |
| MathPow_Cubed                | 73.472 μs | 0.1381 μs | 0.0214 μs |  1.00 |         - |          NA |
| DirectMultiplication_Cubed   |  5.976 μs | 0.0217 μs | 0.0034 μs |  0.08 |         - |          NA |
| MathPow_To1_5                | 73.452 μs | 0.8985 μs | 0.1390 μs |  1.00 |         - |          NA |
| Optimized_To1_5              | 41.501 μs | 0.1260 μs | 0.0195 μs |  0.56 |         - |          NA |
