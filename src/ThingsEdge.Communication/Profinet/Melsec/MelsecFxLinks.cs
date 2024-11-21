using System.Reflection.Metadata;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱计算机链接协议，适用FX3U系列，FX3G，FX3S等等系列，通常在PLC侧连接的是485的接线口。
/// </summary>
/// <remarks>
/// 关于在PLC侧的配置信息，协议：专用协议  传送控制步骤：格式一  站号设置：0
/// </remarks>
public class MelsecFxLinks : DeviceSerialPort, IReadWriteFxLinks, IReadWriteDevice, IReadWriteNet, IReadWriteDeviceStation
{
    private byte _watiingTime;

    public byte Station { get; set; }

    public byte WaittingTime
    {
        get => _watiingTime;
        set
        {
            if (_watiingTime > 15)
            {
                _watiingTime = 15;
            }
            else
            {
                _watiingTime = value;
            }
        }
    }

    public bool SumCheck { get; set; } = true;

    public int Format { get; set; } = 1;

    public MelsecFxLinks()
    {
        ByteTransform = new RegularByteTransform();
        WordLength = 1;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecFxLinksMessage(Format, SumCheck);
    }

    protected override byte[] PackCommandWithHeader(byte[] command)
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

    public override Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return MelsecFxLinksHelper.WriteAsync(this, address, data);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return MelsecFxLinksHelper.WriteAsync(this, address, values);
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
        return MelsecFxLinksHelper.ReadPlcTypeAsync (this, parameter);
    }

    public override string ToString()
    {
        return $"MelsecFxLinks[{PortName}:{BaudRate}]";
    }
}
