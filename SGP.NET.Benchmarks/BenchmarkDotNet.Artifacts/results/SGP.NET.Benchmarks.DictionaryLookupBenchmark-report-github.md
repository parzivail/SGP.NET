```

BenchmarkDotNet v0.13.12, Ubuntu 25.10 (Questing Quokka)
AMD Ryzen 9 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.111
  [Host]     : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-OFXTHN : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

IterationCount=5  LaunchCount=1  WarmupCount=2  

```
| Method                   | Mean     | Error    | StdDev   | Ratio | Allocated | Alloc Ratio |
|------------------------- |---------:|---------:|---------:|------:|----------:|------------:|
| ContainsKey_Then_Indexer | 51.65 μs | 0.053 μs | 0.014 μs |  1.00 |         - |          NA |
| TryGetValue              | 27.01 μs | 0.213 μs | 0.055 μs |  0.52 |         - |          NA |
