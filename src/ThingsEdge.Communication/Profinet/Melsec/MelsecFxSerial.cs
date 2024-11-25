using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱的串口通信的对象，适用于读取FX系列的串口数据。
/// </summary>
/// <remarks>
/// 一般老旧的型号，例如FX2N之类的，需要将<see cref="IsNewVersion" />设置为<c>False</c>，
/// 如果是FX3U新的型号，则需要将<see cref="IsNewVersion" />设置为<c>True</c>。
/// </remarks>
public class MelsecFxSerial : DeviceSerialPort, IMelsecFxSerial, IReadWriteNet
{
    public bool IsNewVersion { get; set; }

    /// <summary>
    /// 获取或设置是否动态修改PLC的波特率，如果为 <c>True</c>，那么如果本对象设置了波特率 115200，
    /// 就会自动修改PLC的波特率到 115200，因为三菱PLC在重启后都会使用默认的波特率9600。
    /// </summary>
    public bool AutoChangeBaudRate { get; set; }

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

    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecFxSerialMessage();
    }

    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        if (AutoChangeBaudRate)
        {
            return await NetworkPipe.ReadFromCoreServerAsync(GetNewNetMessage(), [5], hasResponseData: true).ConfigureAwait(false);
        }
        return await base.InitializationOnConnectAsync().ConfigureAwait(false);
    }

    public override async Task<OperateResult> OpenAsync()
    {
        if (NetworkPipe is not PipeSerialPort pipeSerialPort)
        {
            return new OperateResult("PipeSerialPort get failed");
        }

        // 动态修改PLC的波特率，那么如果本对象设置了波特率 115200，就会自动修改PLC的波特率到 115200，因为三菱PLC在重启后都会使用默认的波特率9600。
        var baudRate = pipeSerialPort.GetPipe().BaudRate;
        if (AutoChangeBaudRate && baudRate != 9600)
        {
            // 先切换到波特率 9600 进行处理。
            pipeSerialPort.GetPipe().BaudRate = 9600;
            OperateResult operateResult = await NetworkPipe.CreateAndConnectPipeAsync().ConfigureAwait(false);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }
            for (var i = 0; i < 3; i++)
            {
                var operateResult2 = await NetworkPipe.ReadFromCoreServerAsync(GetNewNetMessage(), [5], hasResponseData: true).ConfigureAwait(false);
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

            byte[] sendValue = baudRate switch
            {
                115200 => [2, 65, 53, 3, 55, 57],
                57600 => [2, 65, 51, 3, 55, 55],
                38400 => [2, 65, 50, 3, 55, 54],
                19200 => [2, 65, 49, 3, 55, 53],
                _ => [2, 65, 53, 3, 55, 57],
            };
            var operateResult3 = await NetworkPipe.ReadFromCoreServerAsync(GetNewNetMessage(), sendValue, hasResponseData: true).ConfigureAwait(false);
            if (!operateResult3.IsSuccess)
            {
                return operateResult3;
            }
            if (operateResult3.Content.Length < 1 || operateResult3.Content[0] != 6)
            {
                return new OperateResult("check 0x06 back after send data failed!");
            }
            NetworkPipe.ClosePipe();

            // 处理后关闭连接，再切换到原有的波特率。
            pipeSerialPort.GetPipe().BaudRate = baudRate;
        }

        return await base.OpenAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 激活PLC的接收状态，需要再和PLC交互之前进行调用。
    /// </summary>
    /// <returns>是否激活成功</returns>
    public async Task<OperateResult> ActivePlcAsync()
    {
        return await MelsecFxSerialHelper.ActivePlcAsync(this).ConfigureAwait(false);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await MelsecFxSerialHelper.ReadAsync(this, address, length, IsNewVersion).ConfigureAwait(false);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await MelsecFxSerialHelper.ReadBoolAsync(this, address, length, IsNewVersion).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return await MelsecFxSerialHelper.WriteAsync(this, address, data, IsNewVersion).ConfigureAwait(false);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        // DOTO: [NotImplemented] MelsecFxSerial -> WriteAsync
        throw new NotImplementedException();
    }

    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await MelsecFxSerialHelper.WriteAsync(this, address, value).ConfigureAwait(false);
    }

    public override string ToString()
    {
        return $"MelsecFxSerial[{NetworkPipe}]";
    }
}
