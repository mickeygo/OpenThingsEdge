using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Extensions;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.Fuji;

/// <summary>
/// 富士PLC的SPH通信协议，可以和富士PLC进行通信，默认CPU0，需要根据实际进行调整。
/// </summary>
/// <remarks>
/// 地址支持 M1.0, M3.0, M10.0 以及I0, Q0。
/// </remarks>
public class FujiSPHNet : DeviceTcpNet
{
    /// <summary>
    /// 对于 CPU0-CPU7来说是CPU的站号，分为对应 0xFE-0xF7，对于P/PE link, FL-net是模块站号，分别对应0xF6-0xEF。
    /// </summary>
    public byte ConnectionID { get; set; } = 254;

    /// <summary>
    /// 指定IP地址和端口号来实例化一个对象。
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    public FujiSPHNet(string ipAddress, int port = 18245) : base(ipAddress, port)
    {
        ByteTransform = new RegularByteTransform();
        WordLength = 1;
    }

    /// <summary>
    /// 批量读取PLC的地址数据，长度单位为字。地址支持M1.1000，M3.1000，M10.1000，返回读取的原始字节数组。
    /// </summary>
    /// <param name="address">PLC的地址，支持M1.1000，M3.1000，M10.1000</param>
    /// <param name="length">读取的长度信息，按照字为单位</param>
    /// <returns>包含byte[]的原始字节数据内容</returns>
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var analysis = FujiSPHAddress.ParseFrom(address);
        if (!analysis.IsSuccess)
        {
            return analysis.ConvertFailed<byte[]>();
        }
        return await ReadFujiSPHAddressAsync(analysis.Content, length).ConfigureAwait(false);
    }

    /// <summary>
    /// 批量读取位数据的方法，需要传入位地址，读取的位长度，地址示例：M1.100.5，M3.1000.12，M10.1000.0。
    /// </summary>
    /// <param name="address">PLC的地址，示例：M1.100.5，M3.1000.12，M10.1000.0</param>
    /// <param name="length">读取的bool长度信息</param>
    /// <returns>包含bool[]的结果对象</returns>
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        var analysis = FujiSPHAddress.ParseFrom(address);
        if (!analysis.IsSuccess)
        {
            return analysis.ConvertFailed<bool[]>();
        }
        var bitCount = analysis.Content.BitIndex + length;
        var read = await ReadFujiSPHAddressAsync(length: (ushort)(bitCount % 16 == 0 ? bitCount / 16 : bitCount / 16 + 1), address: analysis.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read.ConvertFailed<bool[]>();
        }
        return OperateResult.CreateSuccessResult(read.Content.ToBoolArray().SelectMiddle(analysis.Content.BitIndex, length));
    }

    /// <summary>
    /// 批量写入字节数组到PLC的地址里，地址支持M1.1000，M3.1000，M10.1000，返回是否写入成功。
    /// </summary>
    /// <param name="address">PLC的地址，支持M1.1000，M3.1000，M10.1000</param>
    /// <param name="data">要写入的数据</param>
    /// <returns>是否写入成功</returns>
    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        var command = BuildWriteCommand(ConnectionID, address, data);
        if (!command.IsSuccess)
        {
            return command.ConvertFailed<byte[]>();
        }
        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var extra = ExtractActualData(read.Content);
        if (!extra.IsSuccess)
        {
            return extra;
        }
        return OperateResult.CreateSuccessResult();
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        var analysis = FujiSPHAddress.ParseFrom(address);
        if (!analysis.IsSuccess)
        {
            return analysis.ConvertFailed<bool[]>();
        }

        var bitCount = analysis.Content.BitIndex + values.Length;
        var read = await ReadFujiSPHAddressAsync(length: (ushort)(bitCount % 16 == 0 ? bitCount / 16 : bitCount / 16 + 1), address: analysis.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read.ConvertFailed<bool[]>();
        }

        var writeBoolArray = read.Content.ToBoolArray();
        values.CopyTo(writeBoolArray, analysis.Content.BitIndex);
        var command = BuildWriteCommand(ConnectionID, address, writeBoolArray.ToByteArray());
        if (!command.IsSuccess)
        {
            return command.ConvertFailed<byte[]>();
        }

        var write = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!write.IsSuccess)
        {
            return write;
        }
        var extra = ExtractActualData(write.Content);
        if (!extra.IsSuccess)
        {
            return extra;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// This command is used to start all the CPUs that exist in a configuration in a batch. 
    /// Each CPU is cold-started or warm-started,depending on its condition. If a CPU is already started up, 
    /// or if the key switch is set at "RUN" position, the CPU does not perform processing for startup, 
    /// which, however, does not result in an error, and a response is returned normally
    /// </summary>
    /// <returns>是否启动成功</returns>
    public async Task<OperateResult> CpuBatchStartAsync()
    {
        return (await ReadFromCoreServerAsync(PackCommand(ConnectionID, 4, 0, null)).ConfigureAwait(false)).Check(ExtractActualData);
    }

    /// <summary>
    /// This command is used to initialize and start all the CPUs that exist in a configuration in a batch. Each CPU is cold-started.
    /// If a CPU is already started up, or if the key switch is set at "RUN" position, the CPU does not perform processing for initialization 
    /// and startup, which, however, does not result in an error, and a response is returned normally.
    /// </summary>
    /// <returns>是否启动成功</returns>
    public async Task<OperateResult> CpuBatchInitializeAndStartAsync()
    {
        return (await ReadFromCoreServerAsync(PackCommand(ConnectionID, 4, 1, null)).ConfigureAwait(false)).Check(ExtractActualData);
    }

    /// <summary>
    /// This command is used to stop all the CPUs that exist in a configuration in a batch.
    /// If a CPU is already stopped, or if the key switch is set at "RUN" position, the CPU does not perform processing for stop, which,
    /// however, does not result in an error, and a response is returned normally.
    /// </summary>
    /// <returns>是否停止成功</returns>
    public async Task<OperateResult> CpuBatchStopAsync()
    {
        return (await ReadFromCoreServerAsync(PackCommand(ConnectionID, 4, 2, null)).ConfigureAwait(false)).Check(ExtractActualData);
    }

    /// <summary>
    /// This command is used to stop all the CPUs that exist in a configuration in a batch.
    /// If a CPU is already stopped, or if the key switch is set at "RUN" position, the CPU does not perform processing for stop, which,
    /// however, does not result in an error, and a response is returned normally.
    /// </summary>
    /// <returns>是否复位成功</returns>
    public async Task<OperateResult> CpuBatchResetAsync()
    {
        return (await ReadFromCoreServerAsync(PackCommand(ConnectionID, 4, 3, null)).ConfigureAwait(false)).Check(ExtractActualData);
    }

    /// <summary>
    /// This command is used to start an arbitrary CPU existing in a configuration by specifying it. The CPU may be cold-started or
    /// warm-started, depending on its condition. An error occurs if the CPU is already started. A target CPU is specified by a connection
    /// mode and connection ID.
    /// </summary>
    /// <returns>是否启动成功</returns>
    public async Task<OperateResult> CpuIndividualStartAsync()
    {
        return (await ReadFromCoreServerAsync(PackCommand(ConnectionID, 4, 4, null)).ConfigureAwait(false)).Check(ExtractActualData);
    }

    /// <summary>
    /// This command is used to initialize and start an arbitrary CPU existing in a configuration by specifying it. The CPU is cold-started.
    /// An error occurs if the CPU is already started or if the key switch is set at "RUN" or "STOP" position. A target CPU is specified by
    /// a connection mode and connection ID.
    /// </summary>
    /// <returns>是否启动成功</returns>
    public async Task<OperateResult> CpuIndividualInitializeAndStartAsync()
    {
        return (await ReadFromCoreServerAsync(PackCommand(ConnectionID, 4, 5, null)).ConfigureAwait(false)).Check(ExtractActualData);
    }

    /// <summary>
    /// This command is used to stop an arbitrary CPU existing in a configuration by specifying it. An error occurs if the CPU is already
    /// stopped or if the key switch is set at "RUN" or "STOP" position. A target CPU is specified by a connection mode and connection ID.
    /// </summary>
    /// <returns>是否停止成功</returns>
    public async Task<OperateResult> CpuIndividualStopAsync()
    {
        return (await ReadFromCoreServerAsync(PackCommand(ConnectionID, 4, 6, null)).ConfigureAwait(false)).Check(ExtractActualData);
    }

    /// <summary>
    /// This command is used to reset an arbitrary CPU existing in a configuration by specifying it. An error occurs if the key switch is
    /// set at "RUN" or "STOP" position. A target CPU is specified by a connection mode and connection ID.
    /// </summary>
    /// <returns>是否复位成功</returns>
    public async Task<OperateResult> CpuIndividualResetAsync()
    {
        return (await ReadFromCoreServerAsync(PackCommand(ConnectionID, 4, 7, null)).ConfigureAwait(false)).Check(ExtractActualData);
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new FujiSPHMessage();
    }

    private async Task<OperateResult<byte[]>> ReadFujiSPHAddressAsync(FujiSPHAddress address, ushort length)
    {
        var command = BuildReadCommand(ConnectionID, address, length);
        if (!command.IsSuccess)
        {
            return command.ConvertFailed<byte[]>();
        }

        var array = new List<byte>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return read;
            }
            var extra = ExtractActualData(read.Content);
            if (!extra.IsSuccess)
            {
                return extra;
            }
            array.AddRange(extra.Content);
        }
        return OperateResult.CreateSuccessResult(array.ToArray());
    }

    private static byte[] PackCommand(byte connectionId, byte command, byte mode, byte[]? data)
    {
        data ??= [];
        var array = new byte[20 + data.Length];
        array[0] = 251;
        array[1] = 128;
        array[2] = 128;
        array[3] = 0;
        array[4] = byte.MaxValue;
        array[5] = 123;
        array[6] = connectionId;
        array[7] = 0;
        array[8] = 17;
        array[9] = 0;
        array[10] = 0;
        array[11] = 0;
        array[12] = 0;
        array[13] = 0;
        array[14] = command;
        array[15] = mode;
        array[16] = 0;
        array[17] = 1;
        array[18] = BitConverter.GetBytes(data.Length)[0];
        array[19] = BitConverter.GetBytes(data.Length)[1];
        if (data.Length != 0)
        {
            data.CopyTo(array, 20);
        }
        return array;
    }

    /// <summary>
    /// 构建读取数据的命令报文
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="address">读取的PLC的地址</param>
    /// <param name="length">读取的长度信息，按照字为单位</param>
    /// <returns>构建成功的读取报文命令</returns>
    private static OperateResult<List<byte[]>> BuildReadCommand(byte connectionId, FujiSPHAddress address, ushort length)
    {
        var list = new List<byte[]>();
        var array = CollectionUtils.SplitIntegerToArray(length, 230);
        for (var i = 0; i < array.Length; i++)
        {
            list.Add(PackCommand(connectionId, 0, 0,
            [
                address.TypeCode,
                BitConverter.GetBytes(address.AddressStart)[0],
                BitConverter.GetBytes(address.AddressStart)[1],
                BitConverter.GetBytes(address.AddressStart)[2],
                BitConverter.GetBytes(array[i])[0],
                BitConverter.GetBytes(array[i])[1]
            ]));
            address.AddressStart += array[i];
        }
        return OperateResult.CreateSuccessResult(list);
    }

    /// <summary>
    /// 构建写入数据的命令报文
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="address">写入的PLC的地址</param>
    /// <param name="data">原始数据内容</param>
    /// <returns>报文信息</returns>
    private static OperateResult<byte[]> BuildWriteCommand(byte connectionId, string address, byte[] data)
    {
        var operateResult = FujiSPHAddress.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return operateResult.ConvertFailed<byte[]>();
        }

        var value = data.Length / 2;
        var array = new byte[6 + data.Length];
        array[0] = operateResult.Content.TypeCode;
        array[1] = BitConverter.GetBytes(operateResult.Content.AddressStart)[0];
        array[2] = BitConverter.GetBytes(operateResult.Content.AddressStart)[1];
        array[3] = BitConverter.GetBytes(operateResult.Content.AddressStart)[2];
        array[4] = BitConverter.GetBytes(value)[0];
        array[5] = BitConverter.GetBytes(value)[1];
        data.CopyTo(array, 6);
        return OperateResult.CreateSuccessResult(PackCommand(connectionId, 1, 0, array));
    }

    /// <summary>
    /// 从PLC返回的报文里解析出实际的数据内容，如果发送了错误，则返回失败信息
    /// </summary>
    /// <param name="response">PLC返回的报文信息</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<byte[]> ExtractActualData(byte[] response)
    {
        try
        {
            if (response[4] != 0)
            {
                return new OperateResult<byte[]>(response[4], GetErrorDescription(response[4]));
            }
            if (response.Length > 26)
            {
                return OperateResult.CreateSuccessResult(response.RemoveBegin(26));
            }
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>(ex.Message + " Source: " + response.ToHexString(' '));
        }
    }


    /// <summary>
    /// 根据错误代号获取详细的错误描述信息
    /// </summary>
    /// <param name="code">错误码</param>
    /// <returns>错误的描述文本</returns>
    private static string GetErrorDescription(byte code)
    {
        return code switch
        {
            16 => "Command cannot be executed because an error occurred in the CPU.",
            17 => "Command cannot be executed because the CPU is running.",
            18 => "Command cannot be executed due to the key switch condition of the CPU.",
            32 => "CPU received undefined command or mode.",
            34 => "Setting error was found in command header part.",
            35 => "Transmission is interlocked by a command from another device.",
            40 => "Requested command cannot be executed because another command is now being executed.",
            43 => "Requested command cannot be executed because the loader is now performing another processing( including program change).",
            47 => "Requested command cannot be executed because the system is now being initialized.",
            64 => "Invalid data type or number was specified.",
            65 => "Specified data cannot be found.",
            68 => "Specified address exceeds the valid range.",
            69 => "Address + the number of read/write words exceed the valid range.",
            160 => "No module exists at specified destination station No.",
            162 => "No response data is returned from the destination module.",
            164 => "Command cannot be communicated because an error occurred in the SX bus.",
            165 => "Command cannot be communicated because NAK occurred while sending data via the SX bus.",
            _ => StringResources.Language.UnknownError,
        };
    }

    public override string ToString()
    {
        return $"FujiSPHNet[{IpAddress}:{Port}]";
    }
}
