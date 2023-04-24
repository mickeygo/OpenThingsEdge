using Ops.Communication.Profinet.Siemens;
using System.Text;

namespace ThingsEdge.Providers.Ops.Tests.Profinets;

public class SiemensS7Net_Tests
{
    [Fact]
    public async Task Should_Read_Multiple_Address_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, "192.168.0.1");
        await s7.ConnectServerAsync();
        
        var ret1 = s7.ReadInt16("DB101.316");

        string[] addresses = { "DB101.316", "DB101.318", "DB101.322", "DB101.364", "DB101.116", "DB101.0" };
        ushort[] lengths = { 2, 4, 42, 42, 200, 128/8 }; // 类型宽度

        var ret2 = await s7.ReadAsync(addresses, lengths);
        var ret01 = s7.ByteTransform.TransInt16(ret2.Content, 0); // 0
        var ret02 = s7.ByteTransform.TransSingle(ret2.Content, 2); // 0+2
        var ret03 = s7.ByteTransform.TransString(ret2.Content, 8, 40, Encoding.ASCII); // 0+2+4+2
        var ret04 = s7.ByteTransform.TransString(ret2.Content, 50, 40, Encoding.ASCII).TrimEnd('\0'); //0+2+4+42+2

        var ret05 = s7.ByteTransform.TransInt16(ret2.Content, 90); // 90
        var ret06 = s7.ByteTransform.TransInt16(ret2.Content, 92); // 90 + 2

        var ret07 = ret2.Content[290];
        var ret07_1 = (ret07 & 0x01) == 0x01; // 0000_0001
        var ret07_2 = (ret07 & 0x02) == 0x02; // 0000_0010
        var ret07_3 = (ret07 & 0x04) == 0x04; // 0000_0100


        Assert.True(ret1.IsSuccess);
    }
}
