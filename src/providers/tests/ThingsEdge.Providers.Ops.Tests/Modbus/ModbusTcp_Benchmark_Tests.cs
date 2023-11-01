using System.Diagnostics;
using Ops.Communication.Modbus;
using ThingsEdge.Contracts.Variables;
using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops.Tests.Modbus;

public class ModbusTcp_Benchmark_Tests
{
    [Fact]
    public async Task Benchmark_Driver_Test()
    {
        using var modbus = new ModbusTcpNet("127.0.0.1");
        var driver = new DriverConnector("1", "127.0.0.1", 0, modbus);
        await modbus.ConnectServerAsync();

        driver.Available = true;
        driver.ConnectedStatus = ConnectionStatus.Connected;

        var sw1 = Stopwatch.StartNew();

        _ = await modbus.ReadStringAsync("s=1;x=3;352", 20);
        _ = await modbus.ReadInt16Async("s=1;x=3;372");
        _ = await modbus.ReadInt16Async("s=1;x=3;374");
        _ = await modbus.ReadStringAsync("s=1;x=3;376", 20);
        _ = await modbus.ReadStringAsync("s=1;x=3;386", 10);
        _ = await modbus.ReadStringAsync("s=1;x=3;396", 10);
        _ = await modbus.ReadFloatAsync("s=1;x=3;410");
        _ = await modbus.ReadFloatAsync("s=1;x=3;420");
        _ = await modbus.ReadFloatAsync("s=1;x=3;424");

        sw1.Stop();
        var elapsed1 = sw1.ElapsedMilliseconds;

        var sw2 = Stopwatch.StartNew();

        _ = await driver.ReadAsync(new Tag { Address = "s=1;x=3;352", DataType = TagDataType.String, Length = 20 });
        _ = await driver.ReadAsync(new Tag { Address = "s=1;x=3;372", DataType = TagDataType.Int });
        _ = await driver.ReadAsync(new Tag { Address = "s=1;x=3;374", DataType = TagDataType.Int });
        _ = await driver.ReadAsync(new Tag { Address = "s=1;x=3;376", DataType = TagDataType.String, Length = 20 });
        _ = await driver.ReadAsync(new Tag { Address = "s=1;x=3;386", DataType = TagDataType.String, Length = 10 });
        _ = await driver.ReadAsync(new Tag { Address = "s=1;x=3;396", DataType = TagDataType.String, Length = 10 });
        _ = await driver.ReadAsync(new Tag { Address = "s=1;x=3;410", DataType = TagDataType.Real });
        _ = await driver.ReadAsync(new Tag { Address = "s=1;x=3;420", DataType = TagDataType.Real });
        _ = await driver.ReadAsync(new Tag { Address = "s=1;x=3;424", DataType = TagDataType.Real });

        sw2.Stop();
        var elapsed2 = sw2.ElapsedMilliseconds;

        Assert.True(elapsed1 < elapsed2, $"elapsed1:{elapsed1}ms, elapsed2:{elapsed2}ms");
    }
}
