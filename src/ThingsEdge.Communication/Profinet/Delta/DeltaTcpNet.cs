using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.ModBus;
using ThingsEdge.Communication.Profinet.Delta.Helper;

namespace ThingsEdge.Communication.Profinet.Delta;

/// <summary>
/// 台达PLC的网口通讯类，基于Modbus-Rtu协议开发，按照台达的地址进行实现。
/// </summary>
/// <remarks>
/// 适用于DVP-ES/EX/EC/SS型号，DVP-SA/SC/SX/EH型号以及AS300型号，地址参考API文档，同时地址可以携带站号信息，举例：[s=2;D100],[s=3;M100]，可以动态修改当前报文的站号信息。
/// </remarks>
public class DeltaTcpNet : ModbusTcpNet, IDelta, IReadWriteDevice, IReadWriteNet
{
    public DeltaSeries Series { get; set; } = DeltaSeries.Dvp;

    public DeltaTcpNet(string ipAddress, int port = 502, byte station = 1, DeviceTcpNetOptions? options = null)
        : base(ipAddress, port, station, options)
    {
        ByteTransform.DataFormat = DataFormat.CDAB;
    }

    /// <inheritdoc />
    public override OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
    {
        return DeltaHelper.TranslateToModbusAddress(this, address, modbusCode);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await DeltaHelper.ReadBoolAsync(this, base.ReadBoolAsync, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return await DeltaHelper.WriteAsync(this, base.WriteAsync, address, values).ConfigureAwait(false);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await DeltaHelper.ReadAsync(this, base.ReadAsync, address, length).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return await DeltaHelper.WriteAsync(this, base.WriteAsync, address, data).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"DeltaTcpNet[{Host}:{Port}]";
    }
}