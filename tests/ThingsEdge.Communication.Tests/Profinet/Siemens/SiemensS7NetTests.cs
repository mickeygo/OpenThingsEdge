using System.Text;
using ThingsEdge.Communication.Profinet.Siemens;

namespace ThingsEdge.Communication.Tests.Profinet.Siemens;

/// <summary>
/// SiemensS7Net 测试
/// </summary>
public class SiemensS7NetTests
{
    /// <summary>
    /// S7 读写测试
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Should_WriteAndRead_Test()
    {
        SiemensS7Net client = new(SiemensPLCS.S1500, "127.0.0.1");
        var connectResult = await client.ConnectServerAsync();
        Assert.True(connectResult.IsSuccess, connectResult.Message);

        //// bool
        //var boolAddres = "DB100.1";
        //var boolResult1 = await client.WriteAsync(boolAddres, true);
        //Assert.True(boolResult1.IsSuccess, boolResult1.Message);
        //var boolResult2 = await client.ReadBoolAsync(boolAddres);
        //Assert.True(boolResult2.IsSuccess, boolResult2.Message);
        //Assert.True(boolResult2.Content);

        // short
        var shortAddress = "DB10.5";
        var shortValue = (short)254;
        var shortResult1 = await client.WriteAsync(shortAddress, shortValue);
        Assert.True(shortResult1.IsSuccess, shortResult1.Message);
        var shortResult2 = await client.ReadInt16Async(shortAddress);
        Assert.True(shortResult2.IsSuccess, shortResult2.Message);
        Assert.Equal(shortValue, shortResult2.Content);

        // int
        var intAddress = "DB10.7";
        var intValue = 1288;
        var intResult1 = await client.WriteAsync(intAddress, intValue);
        Assert.True(intResult1.IsSuccess, intResult1.Message);
        var intResult2 = await client.ReadInt32Async(intAddress);
        Assert.True(intResult2.IsSuccess, intResult2.Message);
        Assert.Equal(intValue, intResult2.Content);

        // float
        var floatAddress = "DB10.11";
        var floatValue = 127.12f;
        var floatResult1 = await client.WriteAsync(floatAddress, floatValue);
        Assert.True(floatResult1.IsSuccess, floatResult1.Message);
        var floatResult2 = await client.ReadFloatAsync(floatAddress);
        Assert.True(floatResult2.IsSuccess, floatResult2.Message);
        Assert.True((floatValue - 0.01) < floatResult2.Content && floatResult2.Content < (floatValue + 0.01));

        // long
        var longAddress = "DB10.15";
        var longValue = 1234567890123L;
        var longResult1 = await client.WriteAsync(longAddress, longValue);
        Assert.True(longResult1.IsSuccess, longResult1.Message);
        var longResult2 = await client.ReadInt64Async(longAddress);
        Assert.True(longResult2.IsSuccess, longResult2.Message);
        Assert.Equal(longValue, longResult2.Content);

        // double
        var doubleAddress = "DB10.24";
        var doubleValue = 1234567890.12d;
        var doubleResult1 = await client.WriteAsync(doubleAddress, doubleValue);
        Assert.True(doubleResult1.IsSuccess, doubleResult1.Message);
        var doubleResult2 = await client.ReadDoubleAsync(doubleAddress);
        Assert.True(doubleResult2.IsSuccess, doubleResult2.Message);
        Assert.True((doubleValue - 0.01) < doubleResult2.Content && doubleResult2.Content < (doubleValue + 0.01));

        // string
        var stringAddress1 = "DB10.32";
        var strV1 = "ABCDE12345"; // 10
        var strResult1 = await client.WriteAsync(stringAddress1, strV1);
        Assert.True(strResult1.IsSuccess, strResult1.Message);
        var strResult2 = await client.ReadStringAsync(stringAddress1);
        Assert.True(strResult2.IsSuccess, strResult2.Message);
        Assert.Equal(strV1, strResult2.Content.TrimEnd('\0'));

        // WString
        var wstringAddress1 = "DB10.44";
        var wstrV1 = "西门子WString"; // 10
        var wstrResult1 = await client.WriteWStringAsync(wstringAddress1, wstrV1);
        Assert.True(wstrResult1.IsSuccess, wstrResult1.Message);
        var wstrResult2 = await client.ReadWStringAsync(stringAddress1);
        Assert.True(wstrResult2.IsSuccess, wstrResult2.Message);
        Assert.Equal(wstrV1, wstrResult2.Content.TrimEnd('\0'));
    }

    /// <summary>
    /// 批量读取测试
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Should_Read_Multiple_Address_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, "192.168.0.1");
        await s7.ConnectServerAsync();

        string[] addresses = { "DB10.5", "DB10.7", "DB10.11", "DB10.15", "DB10.24", "DB10.32", "DB10.44" };
        ushort[] lengths = { 2, 4, 4, 8, 8, 12, 24 }; // 类型宽度（430 byte）

        var ret2 = await s7.ReadAsync(addresses, lengths);
        Assert.True(ret2.IsSuccess, ret2.Message);

        var ret01 = s7.ByteTransform.TransString(ret2.Content, 2, 40, Encoding.ASCII).TrimEnd('\0'); // 0+2
        var ret02 = s7.ByteTransform.TransInt16(ret2.Content, 42); // 42
        var ret03 = s7.ByteTransform.TransString(ret2.Content, 46, 40, Encoding.ASCII); // 42+2+2
        var ret04 = s7.ByteTransform.TransSingle(ret2.Content, 86); // 44+42
        var ret05 = s7.ByteTransform.TransInt16(ret2.Content, 90); // 86+4
        var ret06 = s7.ByteTransform.TransSingle(ret2.Content, 92); // 90 + 2

        //var ret07 = s7.ByteTransform.TransString(ret2.Content, 98, 40, Encoding.ASCII).TrimEnd('\0'); // 92+4+2
        //var ret08 = s7.ByteTransform.TransString(ret2.Content, 2, 40, Encoding.ASCII).TrimEnd('\0'); // 96+
        //var ret09 = s7.ByteTransform.TransString(ret2.Content, 2, 40, Encoding.ASCII).TrimEnd('\0');

        //var ret07 = ret2.Content[290];
        //var ret07_1 = (ret07 & 0x01) == 0x01; // 0000_0001
        //var ret07_2 = (ret07 & 0x02) == 0x02; // 0000_0010
        //var ret07_3 = (ret07 & 0x04) == 0x04; // 0000_0100

        Assert.True(ret2.IsSuccess);
    }
}
