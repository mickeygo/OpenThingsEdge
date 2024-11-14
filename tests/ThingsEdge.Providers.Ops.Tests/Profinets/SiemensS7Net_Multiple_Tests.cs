using Ops.Communication.Profinet.Siemens;
using ThingsEdge.Contracts.Variables;
using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops.Tests.Profinets;

public class SiemensS7Net_Multiple_Tests
{
    [Fact]
    public async Task Should_Read_Multiple_Address_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, "192.168.120.10");
        await s7.ConnectServerAsync();
        
        string[] addresses = [ "DB3012.12", "DB3012.14" ];
        ushort[] lengths = [ 2, 52]; // 类型宽度

        var ret1 = await s7.ReadAsync(addresses, lengths);
        Assert.True(ret1.IsSuccess, ret1.Message);

        int index = 0;
        var ret01 = s7.ByteTransform.TransInt16(ret1.Content, index); // 0
        Assert.Equal(0, ret01);

        index = 2;
        var ret02 = s7.ByteTransform.TransString(ret1.Content, index + 2, 50, Encoding.ASCII);
        Assert.Equal("ABC123", ret02.Trim('\0'));

        // 按实际长度读取
        index = 2;
        var ret03 = s7.ByteTransform.TransString(ret1.Content, index + 2, ret1.Content[index + 1], Encoding.ASCII);
        Assert.Equal("ABC1", ret03);

        // 假设为长度为 0 的测试
        index = 2;
        var ret04 = s7.ByteTransform.TransString(ret1.Content, index + 2, 0, Encoding.ASCII);
        Assert.Equal("", ret04);
    }

    [Fact]
    public async Task Should_Read_Multiple_Tags_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, "192.168.120.10");
        await s7.ConnectServerAsync();

        Tag[] tags = [
            new Tag { TagId = "0", Address = "DB3012.12", DataType = TagDataType.Int },
            new Tag { TagId = "1", Address = "DB3012.14", DataType = TagDataType.String, Length = 50 }
            ];
        var (ok, payloads, err) = await s7.ReadMultiAsync(tags);
        Assert.True(ok, err);
        Assert.True(payloads?.Count > 0);
        Assert.Equal(0, (short)payloads[0].Value);
        Assert.Equal("ABC1", payloads[1].Value);
    }
}
