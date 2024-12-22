using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 基于Qna 兼容3C帧的格式一的通讯，具体的地址需要参照三菱的基本地址，本类是基于tcp通讯的实现。
/// </summary>
/// <remarks>
/// 地址可以携带站号信息，例如：s=2;D100
/// </remarks>
public class MelsecA3CNetOverTcp : DeviceTcpNet, IReadWriteA3C, IReadWriteDevice, IReadWriteNet
{
    public byte Station { get; set; }

    public bool SumCheck { get; set; } = true;

    public int Format { get; set; } = 1;

    public bool EnableWriteBitToWordRegister { get; set; }

    /// <summary>
    /// 指定ip地址和端口号来实例化对象。
    /// </summary>
    /// <param name="ipAddress">Ip地址信息</param>
    /// <param name="port">端口号信息</param>
    /// <param name="options">配置选项</param>
    public MelsecA3CNetOverTcp(string ipAddress, int port, DeviceTcpNetOptions? options = null) : base(ipAddress, port, options)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    public Task<OperateResult<string>> ReadPlcTypeAsync()
    {
        return MelsecA3CNetHelper.ReadPlcTypeAsync(this);
    }

    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return MelsecA3CNetHelper.ReadAsync(this, address, length);
    }

    /// <summary>
    /// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型。
    /// </summary>
    /// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
    /// <param name="length">读取的长度</param>
    /// <returns>读取结果信息</returns>
    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return MelsecA3CNetHelper.ReadBoolAsync(this, address, length);
    }

    /// <summary>
    /// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持 X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="data">数据值</param>
    /// <returns>是否写入成功</returns>
    public override Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return MelsecA3CNetHelper.WriteAsync(this, address, data);
    }

    /// <summary>
    /// 批量写入bool类型的数组，支持的类型为 X,Y,S,T,C，具体的地址范围取决于PLC的类型。
    /// </summary>
    /// <remarks>
    /// 当需要写入D寄存器的位时，可以开启<see cref="EnableWriteBitToWordRegister" />为<c>True</c>，然后地址使用 D100.2 等格式进行批量写入位操作，该操作有一定风险。
    /// </remarks>
    /// <param name="address">PLC的地址信息</param>
    /// <param name="values">数据信息</param>
    /// <returns>是否写入成功</returns>
    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return MelsecA3CNetHelper.WriteAsync(this, address, values);
    }

    public Task<OperateResult> RemoteRunAsync()
    {
        return MelsecA3CNetHelper.RemoteRunAsync(this);
    }

    public Task<OperateResult> RemoteStopAsync()
    {
        return MelsecA3CNetHelper.RemoteStopAsync(this);
    }

    public override string ToString()
    {
        return $"MelsecA3CNetOverTcp[{Host}:{Port}]";
    }
}
