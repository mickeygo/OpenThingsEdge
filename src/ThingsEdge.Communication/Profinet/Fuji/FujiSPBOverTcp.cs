using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.Fuji;

/// <summary>
/// 富士PLC的SPB协议，详细的地址信息见api文档说明，地址可以携带站号信息，例如：s=2;D100，PLC侧需要配置无BCC计算，包含0D0A结束码。
/// </summary>
public class FujiSPBOverTcp : DeviceTcpNet
{
    /// <summary>
    /// PLC的站号信息。
    /// </summary>
    public byte Station { get; set; } = 1;

    /// <summary>
    /// 使用指定的ip地址和端口来实例化一个对象。
    /// </summary>
    /// <param name="ipAddress">设备的Ip地址</param>
    /// <param name="port">设备的端口号</param>
    /// <param name="options">配置选项</param>
    public FujiSPBOverTcp(string ipAddress, int port, DeviceTcpNetOptions? options = null) : base(ipAddress, port, options)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await FujiSPBHelper.ReadAsync(this, Station, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await FujiSPBHelper.ReadBoolAsync(this, Station, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return await FujiSPBHelper.WriteAsync(this, Station, address, data).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await FujiSPBHelper.WriteAsync(this, Station, address, value).ConfigureAwait(false);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        // TODO: [NotImplemented] FujiSPBOverTcp -> WriteAsync
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new FujiSPBMessage();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"FujiSPBOverTcp[{Host}:{Port}]";
    }
}
