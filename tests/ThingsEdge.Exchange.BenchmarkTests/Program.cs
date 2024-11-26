using BenchmarkDotNet.Running;
using ThingsEdge.Exchange.BenchmarkTests.Benchmarks;

_ = BenchmarkRunner.Run<Md5VsSha256>();

//_ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

Console.WriteLine("Hello, World!");
