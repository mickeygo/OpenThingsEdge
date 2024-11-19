using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱计算机链接协议的网口版本，适用FX3U系列，FX3G，FX3S等等系列，通常在PLC侧连接的是485的接线口。
/// </summary>
/// <remarks>
/// 关于在PLC侧的配置信息，协议：专用协议、传送控制步骤：格式一  站号设置：0。
/// </remarks>
public class MelsecFxLinksOverTcp : DeviceTcpNet, IReadWriteFxLinks, IReadWriteDevice, IReadWriteNet, IReadWriteDeviceStation
{
    private byte _waittingTime;

    public byte Station { get; set; }

    public byte WaittingTime
    {
        get
        {
            return _waittingTime;
        }
        set
        {
            if (value > 15)
            {
                _waittingTime = 15;
            }
            else
            {
                _waittingTime = value;
            }
        }
    }

    public bool SumCheck { get; set; } = true;

    public int Format { get; set; } = 1;

    /// <summary>
    /// 实例化默认的对象。
    /// </summary>
    public MelsecFxLinksOverTcp()
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    /// <summary>
    /// 指定ip地址和端口号来实例化默认的对象。
    /// </summary>
    /// <param name="ipAddress">Ip地址信息</param>
    /// <param name="port">端口号</param>
    public MelsecFxLinksOverTcp(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecFxLinksMessage(Format, SumCheck);
    }

    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return MelsecFxLinksHelper.PackCommandWithHeader(this, command);
    }

    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return MelsecFxLinksHelper.ReadAsync(this, address, length);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return MelsecFxLinksHelper.ReadBoolAsync(this, address, length);
    }

    public override Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return MelsecFxLinksHelper.WriteAsync(this, address, value);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] value)
    {
        return MelsecFxLinksHelper.WriteAsync(this, address, value);
    }

    public Task<OperateResult> StartPLCAsync(string parameter = "")
    {
        return MelsecFxLinksHelper.StartPLCAsync(this, parameter);
    }

    public Task<OperateResult> StopPLCAsync(string parameter = "")
    {
        return MelsecFxLinksHelper.StopPLCAsync(this, parameter);
    }

    public Task<OperateResult<string>> ReadPlcTypeAsync(string parameter = "")
    {
        return MelsecFxLinksHelper.ReadPlcTypeAsync(this, parameter);
    }

    public override string ToString()
    {
        return $"MelsecFxLinksOverTcp[{IpAddress}:{Port}]";
    }
}
