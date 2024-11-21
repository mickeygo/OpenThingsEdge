using ThingsEdge.Communication.Profinet.Melsec;

namespace ThingsEdge.Communication.Tests.Profinet.Melsec;

/// <summary>
/// Melsec 测试
/// </summary>
public class MelsecMcNetTests
{
    /// <summary>
    /// 读写测试
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Should_WriteAndRead_Test()
    {
        using MelsecMcNet client = new("127.0.0.1", 6000);
        var connectResult = await client.ConnectServerAsync();
        Assert.True(connectResult.IsSuccess, connectResult.Message);

        // short
        var shortAddress = "D100";
        var shortValue = (short)254;
        var shortResult1 = await client.WriteAsync(shortAddress, shortValue);
        Assert.True(shortResult1.IsSuccess, shortResult1.Message);
        var shortResult2 = await client.ReadInt16Async(shortAddress);
        Assert.True(shortResult2.IsSuccess, shortResult2.Message);
        Assert.Equal(shortValue, shortResult2.Content);
    }
}
