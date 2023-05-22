using Ops.Communication.Profinet.Melsec;
using ThingsEdge.Contracts.Devices;
using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops.Tests.Profinets;

public sealed class MelsecMcNet_Tests
{
    [Fact]
    public async void Should_Read_Address_Test()
    {
        using var mc = new MelsecMcNet("192.168.3.39", 4096);
        await mc.ConnectCloseAsync();
        var ret1 = await mc.ReadInt16Async("D0");
        Assert.True(ret1.IsSuccess, ret1.Message);
    }

    [Fact]
    public async Task Should_Read_Array_Address_Test()
    {
        using var mc = new MelsecMcNet("192.168.3.39", 4096);
        await mc.ConnectServerAsync();
        var ret1 = await mc.ReadInt16Async("D0", 10);
        Assert.True(ret1.IsSuccess, ret1.Message);

        var ret2 = await mc.ReadBoolAsync("M0", 100);
        Assert.True(ret2.IsSuccess, ret2.Message);
    }

    [Fact]
    public async void Should_Write_Address_Test()
    {
        using var mc = new MelsecMcNet("192.168.3.39", 4096);
        await mc.ConnectCloseAsync();
        var ret1 = await mc.WriteAsync("D1000", (short)12); // byte=>0 + 2|word=>1
        var ret2 = await mc.WriteAsync("D1001", 135); // byte=>2 + 4; word=>2
        var ret3 = await mc.WriteAsync("D1003", 12.34f); // byte=>6 + 4; word=>2
        var ret4 = await mc.WriteAsync("D1005", "abc12345", 10); // byte=>10 + 10*2; word=>10
        var ret5 = await mc.WriteAsync("D1015", 15.6f); // byte=>30 + 4; word=>2
        var ret6 = await mc.WriteAsync("D1017", (byte)122); // byte=>34+2; word=>1
        var ret7 = await mc.WriteAsync("D1018", true); // byte=>36+2; word=>1
        var ret8 = await mc.WriteAsync("D1019", new float[] { 12.3f, 13.4f, 15.6f, 17.8f, 21.2f }); // byte=>38+20; word=>10
        Assert.True(ret1.IsSuccess, ret1.Message);
    }

    [Fact]
    public async void Should_Read_Continuation_Address_Test()
    {
        // 与 ModbusTcp 类似
        // int16/uint16 => 2
        // int32/uint32 => 4
        // int64/uint64 => 8
        // float => 4
        // double => 8
        // string[len] => len * 2 

        using var mc = new MelsecMcNet("192.168.3.39", 4096);
        await mc.ConnectServerAsync();

        var ret0 = await mc.ReadAsync("D1000", 34); // 34个word
        var ret0_d1 = mc.ByteTransform.TransInt16(ret0.Content, 0);
        var ret0_d2 = mc.ByteTransform.TransInt32(ret0.Content, 2);
        var ret0_d3 = mc.ByteTransform.TransSingle(ret0.Content, 6);
        var ret0_d4 = mc.ByteTransform.TransString(ret0.Content, 10, 10, Encoding.ASCII).TrimEnd('\0');
        var ret0_d5 = mc.ByteTransform.TransSingle(ret0.Content, 30);
        var ret0_d6 = mc.ByteTransform.TransByte(ret0.Content, 34);
        var ret0_d7 = mc.ByteTransform.TransBool(ret0.Content, 36);
        var ret0_d8 = mc.ByteTransform.TransSingle(ret0.Content, 38, 5);
        Assert.True(ret0.IsSuccess, ret0.Message);

        List<Tag> tags = new()
        {
            new Tag { Address = "D1000", Length = 0, DataType = TagDataType.Int, },
            new Tag { Address = "D1001", Length = 0, DataType = TagDataType.DInt, },
            new Tag { Address = "D1003", Length = 0, DataType = TagDataType.Real, },
            new Tag { Address = "D1005", Length = 10, DataType = TagDataType.String, },
            new Tag { Address = "D1015", Length = 0, DataType = TagDataType.Real, },
            new Tag { Address = "D1017", Length = 0, DataType = TagDataType.Byte, },
            new Tag { Address = "D1018", Length = 0, DataType = TagDataType.Bit, },
            new Tag { Address = "D1016", Length = 5, DataType = TagDataType.Real, },
        };
        var (ok, data, err) = await mc.ReadContinuationAsync(tags);
        Assert.True(ok, err);
    }

    [Fact]
    public async void Should_Read_Multiple_Address_Test()
    {
        // 与 ModbusTcp 类似
        // int16/uint16 => 1
        // int32/uint32 => 2
        // int64/uint64 => 4
        // float => 2
        // double => 4
        // string[len] => len

        string[] addresses = { "D0", "D1", "D2", "D3", "D4", "D5" };
        ushort[] lengths = { 1, 1, 1, 1, 1, 1 }; // 类型宽度，单个宽度长度为 2个byte）

        using var mc = new MelsecMcNet("192.168.3.39", 4096);
        await mc.ConnectServerAsync();

        var ret0 = await mc.ReadRandomAsync(addresses);
        var ret0_d1 = mc.ByteTransform.TransInt16(ret0.Content, 0);
        var ret0_d2 = mc.ByteTransform.TransInt16(ret0.Content, 2);
        var ret0_d3 = mc.ByteTransform.TransInt16(ret0.Content, 4);
        var ret0_d4 = mc.ByteTransform.TransInt16(ret0.Content, 6);
        var ret0_d5 = mc.ByteTransform.TransInt16(ret0.Content, 8);
        var ret0_d6 = mc.ByteTransform.TransInt16(ret0.Content, 10);
        Assert.True(ret0.IsSuccess, ret0.Message);

        var ret1 = await mc.ReadRandomAsync(addresses, lengths);
        var ret1_d1 = mc.ByteTransform.TransInt16(ret1.Content, 0);
        var ret1_d2 = mc.ByteTransform.TransInt16(ret1.Content, 2);
        var ret1_d3 = mc.ByteTransform.TransInt16(ret1.Content, 4);
        var ret1_d4 = mc.ByteTransform.TransInt16(ret1.Content, 6);
        var ret1_d5 = mc.ByteTransform.TransInt16(ret1.Content, 8);
        var ret1_d6 = mc.ByteTransform.TransInt16(ret1.Content, 10);
        Assert.True(ret1.IsSuccess, ret1.Message);
    }
}
