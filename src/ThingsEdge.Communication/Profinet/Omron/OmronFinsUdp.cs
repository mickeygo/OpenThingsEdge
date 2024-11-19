using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的Udp协议的实现类，地址类型和Fins-TCP一致，无连接的实现，可靠性不如<see cref="OmronFinsNet" />。
/// </summary>
public class OmronFinsUdp : DeviceUdpNet, IOmronFins, IReadWriteDevice, IReadWriteNet
{
    public byte ICF { get; set; } = 128;

    public byte RSV { get; private set; }

    public byte GCT { get; set; } = 2;

    public byte DNA { get; set; }

    public byte DA1 { get; set; }

    public byte DA2 { get; set; }

    public byte SNA { get; set; }

    public byte SA1 { get; set; } = 13;

    public byte SA2 { get; set; }

    public byte SID { get; set; }

    public int ReadSplits { get; set; } = 500;

    public OmronPlcType PlcType { get; set; } = OmronPlcType.CSCJ;

    public OmronFinsUdp(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    public OmronFinsUdp()
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform(DataFormat.CDAB)
        {
            IsStringReverseByteWord = true
        };
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new FinsUdpMessage();
    }

    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return PackCommand(command);
    }

    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return OmronFinsNetHelper.UdpResponseValidAnalysis(response);
    }

    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return OmronFinsNetHelper.ReadAsync(this, address, length, ReadSplits);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return OmronFinsNetHelper.ReadBoolAsync(this, address, length, ReadSplits);
    }

    public override Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
    {
        return base.ReadStringAsync(address, length, Encoding.UTF8);
    }

    /// <summary>
    /// 批量读取数据。
    /// </summary>
    /// <param name="addresses">地址集合。</param>
    /// <returns></returns>
    public Task<OperateResult<byte[]>> ReadAsync(string[] addresses)
    {
        return OmronFinsNetHelper.ReadAsync(this, addresses);
    }

    public override Task<OperateResult> WriteAsync(string address, byte[] values)
    {
        return OmronFinsNetHelper.WriteAsync(this, address, values);
    }

    public override Task<OperateResult> WriteAsync(string address, string value)
    {
        return base.WriteAsync(address, value, Encoding.UTF8);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return OmronFinsNetHelper.WriteAsync(this, address, values);
    }

    public Task<OperateResult> RunAsync()
    {
        return OmronFinsNetHelper.RunAsync(this);
    }

    public Task<OperateResult> StopAsync()
    {
        return OmronFinsNetHelper.StopAsync(this);
    }

    public Task<OperateResult<OmronCpuUnitData>> ReadCpuUnitDataAsync()
    {
        return OmronFinsNetHelper.ReadCpuUnitDataAsync(this);
    }

    public Task<OperateResult<OmronCpuUnitStatus>> ReadCpuUnitStatusAsync()
    {
        return OmronFinsNetHelper.ReadCpuUnitStatusAsync(this);
    }

    public Task<OperateResult<DateTime>> ReadCpuTimeAsync()
    {
        return OmronFinsNetHelper.ReadCpuTimeAsync(this);
    }

    private byte[] PackCommand(byte[] cmd)
    {
        var array = new byte[10 + cmd.Length];
        array[0] = ICF;
        array[1] = RSV;
        array[2] = GCT;
        array[3] = DNA;
        array[4] = DA1;
        array[5] = DA2;
        array[6] = SNA;
        array[7] = SA1;
        array[8] = SA2;
        array[9] = SID;
        cmd.CopyTo(array, 10);
        return array;
    }

    public override string ToString()
    {
        return $"OmronFinsUdp[{IpAddress}:{Port}]";
    }
}
