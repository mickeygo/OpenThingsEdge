using BenchmarkDotNet.Attributes;
using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Exchange.BenchmarkTests.Benchmarks;

/// <summary>
/// ModbusTcp 基准测试
/// </summary>
[MemoryDiagnoser]
public class ModbusTcpBenchmark
{
    private ModbusTcpNet? _client;

    [GlobalSetup]
    public async Task Setup()
    {
        _client = new("127.0.0.1");
        await _client.ConnectServerAsync().ConfigureAwait(false);
    }

    [Benchmark]
    public async Task<int> ReadInt16Async()
    {
        var shortAddress = "s=1;x=3;5";
        var shortResult1 = await _client!.ReadInt16Async(shortAddress).ConfigureAwait(false);
        return shortResult1.Content;
    }

    [Benchmark]
    public async Task<bool> WriteInt16Async()
    {
        var shortAddress = "s=1;x=3;5";
        var shortValue = (short)254;
        var shortResult1 = await _client!.WriteAsync(shortAddress, shortValue).ConfigureAwait(false);
        return shortResult1.IsSuccess;
    }

    [Benchmark]
    public async Task<float> ReadFloatAsync()
    {
        var shortAddress = "s=1;x=3;12";
        var shortResult1 = await _client!.ReadFloatAsync(shortAddress).ConfigureAwait(false);
        return shortResult1.Content;
    }

    [Benchmark]
    public async Task<bool> WriteFloatAsync()
    {
        var shortAddress = "s=1;x=3;12";
        var shortValue = 127.12f;
        var shortResult1 = await _client!.WriteAsync(shortAddress, shortValue).ConfigureAwait(false);
        return shortResult1.IsSuccess;
    }
}
