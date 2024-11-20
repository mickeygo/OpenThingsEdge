using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Tests.Modbus;

/// <summary>
/// Modbus 测试
/// </summary>
public class ModbusTcpNetTests
{
    /// <summary>
    /// Modbus 读测试
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Should_Read_Test()
    {
        ModbusTcpNet client = new("");
        var connectResult = await client.ConnectServerAsync();
        Assert.True(connectResult.IsSuccess, connectResult.Message);

        var int16Result = await client.ReadInt16Async("");
        Assert.True(int16Result.IsSuccess, connectResult.Message);
        Assert.Equal(0, int16Result.Content);
    }

    /// <summary>
    /// Modbus 写测试
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Should_Write_Test()
    {
        ModbusTcpNet client = new("127.0.0.1");
        await using (client.ConfigureAwait(false))
        {
            var connectResult = await client.ConnectServerAsync();
            Assert.True(connectResult.IsSuccess, connectResult.Message);

            // byte
            var byteAddres = "";
            var byteValue = (byte)251;
            var byteResult1 = await client.WriteAsync(byteAddres, [byteValue]);
            Assert.True(byteResult1.IsSuccess, byteResult1.Message);
            var byteResult2 = await client.ReadAsync(byteAddres, 1);
            Assert.True(byteResult2.IsSuccess, byteResult2.Message);
            Assert.Equal(byteValue, byteResult2.Content[0]);

            // bool
            var boolAddres = "";
            var boolResult1 = await client.WriteAsync(boolAddres, true);
            Assert.True(boolResult1.IsSuccess, boolResult1.Message);
            var boolResult2 = await client.ReadBoolAsync(boolAddres);
            Assert.True(boolResult2.IsSuccess, boolResult2.Message);
            Assert.True(boolResult2.Content);

            // short
            var shortAddress = "";
            var shortValue = (short)254;
            var shortResult1 = await client.WriteAsync(shortAddress, shortValue);
            Assert.True(shortResult1.IsSuccess, shortResult1.Message);
            var shortResult2 = await client.ReadInt16Async(shortAddress);
            Assert.True(shortResult2.IsSuccess, shortResult2.Message);
            Assert.Equal(shortValue, shortResult2.Content);

            // int
            var intAddress = "";
            var intValue = 1288;
            var intResult1 = await client.WriteAsync(intAddress, intValue);
            Assert.True(intResult1.IsSuccess, intResult1.Message);
            var intResult2 = await client.ReadInt32Async(intAddress);
            Assert.True(intResult2.IsSuccess, intResult2.Message);
            Assert.Equal(intValue, intResult2.Content);

            // float
            var floatAddress = "";
            var floatValue = 127.12f;
            var floatResult1 = await client.WriteAsync(floatAddress, floatValue);
            Assert.True(floatResult1.IsSuccess, floatResult1.Message);
            var floatResult2 = await client.ReadInt32Async(floatAddress);
            Assert.True(floatResult2.IsSuccess, floatResult2.Message);
            Assert.True((floatValue - 0.01) < floatResult2.Content && floatResult2.Content < (floatValue + 0.01));

            // long
            var longAddress = "";
            var longValue = 1234567890123L;
            var longResult1 = await client.WriteAsync(longAddress, longValue);
            Assert.True(longResult1.IsSuccess, longResult1.Message);
            var longResult2 = await client.ReadInt64Async(longAddress);
            Assert.True(longResult2.IsSuccess, longResult2.Message);
            Assert.Equal(longValue, longResult2.Content);

            // double
            var doubleAddress = "";
            var doubleValue = 1234567890.12d;
            var doubleResult1 = await client.WriteAsync(doubleAddress, doubleValue);
            Assert.True(doubleResult1.IsSuccess, doubleResult1.Message);
            var doubleResult2 = await client.ReadInt32Async(doubleAddress);
            Assert.True(doubleResult2.IsSuccess, doubleResult2.Message);
            Assert.True((doubleValue - 0.01) < doubleResult2.Content && doubleResult2.Content < (doubleValue + 0.01));

            // string
            var stringAddress1 = "";
            var strV1 = "Modubs_Test_Write";
            var strResult1 = await client.WriteAsync(stringAddress1, strV1, (ushort)strV1.Length);
            Assert.True(strResult1.IsSuccess, strResult1.Message);
            var strResult2 = await client.ReadStringAsync(stringAddress1, (ushort)strV1.Length);
            Assert.True(strResult2.IsSuccess, strResult2.Message);
            Assert.Equal(strV1, strResult2.Content);
        }
    }
}
