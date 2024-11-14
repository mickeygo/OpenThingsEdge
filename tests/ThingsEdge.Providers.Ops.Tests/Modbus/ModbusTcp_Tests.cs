using System.Diagnostics;
using Ops.Communication.Modbus;

namespace ThingsEdge.Providers.Ops.Tests.Modbus;

public sealed class ModbusTcp_Tests
{
    [Fact]
    public async Task Should_Get_Float_Data_Test()
    {
        using var modbus = new ModbusTcpNet("127.0.0.1");
        await modbus.ConnectServerAsync();

        var d1 = await modbus.ReadFloatAsync("s=1;x=3;420");
        Assert.True(d1.IsSuccess);
        Assert.True(d1.Content > 0.1);
    }

    [Fact]
    public async Task Should_Stopwatch_Test()
    {
        using var modbus = new ModbusTcpNet("127.0.0.1");
        await modbus.ConnectServerAsync();

        var sw = Stopwatch.StartNew();

        _ = await modbus.ReadStringAsync("s=1;x=3;352", 20);
        _ = await modbus.ReadInt16Async("s=1;x=3;372");
        _ = await modbus.ReadInt16Async("s=1;x=3;374");
        _ = await modbus.ReadStringAsync("s=1;x=3;376", 20);
        _ = await modbus.ReadStringAsync("s=1;x=3;386", 10);
        _ = await modbus.ReadStringAsync("s=1;x=3;396", 10);
        _ = await modbus.ReadFloatAsync("s=1;x=3;410");
        _ = await modbus.ReadFloatAsync("s=1;x=3;420");
        _ = await modbus.ReadFloatAsync("s=1;x=3;424");

        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds > 0, sw.ElapsedMilliseconds.ToString());
    }
}
