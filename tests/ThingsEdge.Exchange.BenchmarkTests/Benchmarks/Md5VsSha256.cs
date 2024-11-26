using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;

namespace ThingsEdge.Exchange.BenchmarkTests.Benchmarks;

/// <summary>
/// 示例
/// </summary>
public class Md5VsSha256
{
    private const int N = 10000;
    private readonly byte[] _data;

    private readonly SHA256 _sha256 = SHA256.Create();
    private readonly MD5 _md5 = MD5.Create();

    /// <summary>
    /// 构造函数
    /// </summary>
    public Md5VsSha256()
    {
        _data = new byte[N];
        new Random(42).NextBytes(_data);
    }

    /// <summary>
    /// Sha256
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    public byte[] Sha256() => _sha256.ComputeHash(_data);

    /// <summary>
    /// Md5
    /// </summary>
    /// <returns></returns>
    [Benchmark]
    public byte[] Md5() => _md5.ComputeHash(_data);
}
