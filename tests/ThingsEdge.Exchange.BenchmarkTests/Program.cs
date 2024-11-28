using BenchmarkDotNet.Running;
using ThingsEdge.Exchange.BenchmarkTests.Benchmarks;

//Mean: 所有测试运行的平均时间。
//Error: 测试运行的标准误差，标准误差是测试结果的离散程度的度量，标准误差越小，表示测试结果越稳定。
//StdDev: 所有测试运行的标准偏差，标准偏差是测试结果的离散程度的度量，标准偏差越小，表示测试结果越接近平均值。
//Median: 所有测试运行的中位数。中位数是测试结果的中间值，如果测试结果的个数为奇数，则中位数为中间的那个值；如果测试结果的个数为偶数，则中位数为中间两个值的平均值。
//Ratio: 每个测试运行的平均时间与基准测试运行的平均时间的比值。基准测试是性能最好的测试，它的比值为 1.0。其他测试的比值表示它们相对于基准测试的性能表现，比值越小，表示性能越好。
//RatioSD: 所有测试运行的比值的标准偏差。标准偏差越小，表示比值的离散程度越小，测试结果更稳定。
//Gen 0: 所有测试运行期间生成的第 0 代垃圾回收的次数。垃圾回收是 .NET 运行时自动回收不再使用的内存的机制，Generational Garbage Collection 是 .NET 中的一种垃圾回收算法。
//Gen 1: 所有测试运行期间生成的第 1 代垃圾回收的次数。
//Gen 2: 所有测试运行期间生成的第 2 代垃圾回收的次数。
//Allocated: 所有测试运行期间分配的内存总量。

_ = BenchmarkRunner.Run<ModbusTcpBenchmark>();

//_ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

Console.WriteLine("Hello, World!");
