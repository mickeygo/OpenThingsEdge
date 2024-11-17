using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱的串口通信的对象，适用于读取FX系列的串口数据，支持的类型参考文档说明<br />
/// Mitsubishi's serial communication object is suitable for reading serial data of the FX series. Refer to the documentation for the supported types.
/// </summary>
/// <remarks>
/// 一般老旧的型号，例如FX2N之类的，需要将<see cref="P:HslCommunication.Profinet.Melsec.MelsecFxSerial.IsNewVersion" />设置为<c>False</c>，如果是FX3U新的型号，则需要将<see cref="P:HslCommunication.Profinet.Melsec.MelsecFxSerial.IsNewVersion" />设置为<c>True</c>
/// </remarks>
/// <example>
/// <inheritdoc cref="T:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp" path="remarks" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="Usage" title="简单的使用" />
/// </example>
public class MelsecFxSerial : DeviceSerialPort, IMelsecFxSerial, IReadWriteNet
{
    /// <inheritdoc cref="P:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.IsNewVersion" />
    public bool IsNewVersion { get; set; }

    /// <summary>
    /// 获取或设置是否动态修改PLC的波特率，如果为 <c>True</c>，那么如果本对象设置了波特率 115200，就会自动修改PLC的波特率到 115200，因为三菱PLC再重启后都会使用默认的波特率9600 <br />
    /// Get or set whether to dynamically modify the baud rate of the PLC. If it is <c>True</c>, then if the baud rate of this object is set to 115200, 
    /// the baud rate of the PLC will be automatically modified to 115200, because the Mitsubishi PLC is not After restart, the default baud rate of 9600 will be used
    /// </summary>
    public bool AutoChangeBaudRate { get; set; } = false;


    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public MelsecFxSerial()
    {
        ByteTransform = new RegularByteTransform();
        WordLength = 1;
        IsNewVersion = true;
        ByteTransform.IsStringReverseByteWord = true;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecFxSerialMessage();
    }

    /// <inheritdoc />
    public override OperateResult Open()
    {
        if (!(CommunicationPipe is PipeSerialPort pipeSerialPort))
        {
            return new OperateResult("PipeSerialPort get failed");
        }
        var baudRate = pipeSerialPort.GetPipe().BaudRate;
        if (AutoChangeBaudRate && baudRate != 9600)
        {
            pipeSerialPort.GetPipe().BaudRate = 9600;
            OperateResult operateResult = pipeSerialPort.OpenCommunication();
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }
            for (var i = 0; i < 3; i++)
            {
                var operateResult2 = pipeSerialPort.ReadFromCoreServer(GetNewNetMessage(), new byte[1] { 5 }, hasResponseData: true);
                if (!operateResult2.IsSuccess)
                {
                    return operateResult2;
                }
                if (operateResult2.Content.Length >= 1 && operateResult2.Content[0] == 6)
                {
                    break;
                }
                if (i == 2)
                {
                    return new OperateResult("check 0x06 back before send data failed!");
                }
            }
            var sendValue = baudRate switch
            {
                115200 => new byte[6] { 2, 65, 53, 3, 55, 57 },
                57600 => new byte[6] { 2, 65, 51, 3, 55, 55 },
                38400 => new byte[6] { 2, 65, 50, 3, 55, 54 },
                19200 => new byte[6] { 2, 65, 49, 3, 55, 53 },
                _ => new byte[6] { 2, 65, 53, 3, 55, 57 },
            };
            var operateResult3 = pipeSerialPort.ReadFromCoreServer(GetNewNetMessage(), sendValue, hasResponseData: true);
            if (!operateResult3.IsSuccess)
            {
                return operateResult3;
            }
            if (operateResult3.Content.Length < 1 || operateResult3.Content[0] != 6)
            {
                return new OperateResult("check 0x06 back after send data failed!");
            }
            pipeSerialPort.CloseCommunication();
            pipeSerialPort.GetPipe().BaudRate = baudRate;
        }
        return base.Open();
    }

    /// <inheritdoc />
    protected override OperateResult InitializationOnConnect()
    {
        if (AutoChangeBaudRate)
        {
            return CommunicationPipe.ReadFromCoreServer(GetNewNetMessage(), new byte[1] { 5 }, hasResponseData: true);
        }
        return base.InitializationOnConnect();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.Read(System.String,System.UInt16)" />
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return MelsecFxSerialHelper.Read(this, address, length, IsNewVersion);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.Write(System.String,System.Byte[])" />
    public override OperateResult Write(string address, byte[] value)
    {
        return MelsecFxSerialHelper.Write(this, address, value, IsNewVersion);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.ReadBool(System.String,System.UInt16)" />
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return MelsecFxSerialHelper.ReadBool(this, address, length, IsNewVersion);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.Write(System.String,System.Boolean)" />
    public override OperateResult Write(string address, bool value)
    {
        return MelsecFxSerialHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.ActivePlc(HslCommunication.Core.IReadWriteDevice)" />
    public OperateResult ActivePlc()
    {
        return MelsecFxSerialHelper.ActivePlc(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.ActivePlc(HslCommunication.Core.IReadWriteDevice)" />
    public async Task<OperateResult> ActivePlcAsync()
    {
        return await MelsecFxSerialHelper.ActivePlcAsync(this);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await Task.Run(() => Write(address, value));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"MelsecFxSerial[{CommunicationPipe}]";
    }
}
