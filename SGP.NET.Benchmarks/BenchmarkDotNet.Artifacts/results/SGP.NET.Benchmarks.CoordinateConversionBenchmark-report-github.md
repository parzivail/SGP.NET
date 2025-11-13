```

BenchmarkDotNet v0.13.12, Ubuntu 25.10 (Questing Quokka)
AMD Ryzen 9 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.111
  [Host]     : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-OFXTHN : .NET 9.0.10 (9.0.1025.47515), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

IterationCount=5  LaunchCount=1  WarmupCount=2  

```
| Method                  | Mean      | Error    | StdDev   | Ratio | Gen0    | Allocated | Alloc Ratio |
|------------------------ |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| GeodeticToEci_Original  |  69.10 μs | 0.479 μs | 0.124 μs |  1.00 | 14.2822 | 117.19 KB |        1.00 |
| GeodeticToEci_Optimized |  56.47 μs | 1.122 μs | 0.174 μs |  0.82 | 14.3433 | 117.19 KB |        1.00 |
| EciToGeodetic_Original  | 186.47 μs | 3.660 μs | 0.950 μs |  2.70 |  4.6387 |  39.18 KB |        0.33 |
| EciToGeodetic_Optimized | 185.37 μs | 1.586 μs | 0.245 μs |  2.68 |  4.6387 |  39.18 KB |        0.33 |
